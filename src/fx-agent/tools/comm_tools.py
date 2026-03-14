"""
Communication tools – manage human-broker approvals and customer notifications.
These functions are registered as callable tools for the Comm Agent.

In a production deployment these would integrate with messaging services
(email, SMS, Teams, etc.).  Here we use an in-process approval queue that
the web frontend can poll / respond to via REST, enabling human-in-the-loop
decision making without an external dependency.
"""
from __future__ import annotations

import asyncio
import json
import uuid
from datetime import datetime, timezone
from typing import Any

# ── In-process approval / notification store ─────────────────────────────────
# Keyed by approval_id; values track state and waiter futures.

_pending_approvals: dict[str, dict[str, Any]] = {}
_approval_waiters: dict[str, asyncio.Future[str]] = {}
_notifications: list[dict[str, Any]] = []


# ── Public helpers (used by the REST API) ────────────────────────────────────

def list_pending_approvals() -> list[dict[str, Any]]:
    """Return all approvals that are still waiting for a human decision."""
    return [
        v for v in _pending_approvals.values() if v["status"] == "pending"
    ]


def list_notifications() -> list[dict[str, Any]]:
    """Return all customer notifications (most recent first)."""
    return list(reversed(_notifications))


def respond_to_approval(approval_id: str, decision: str, notes: str = "") -> bool:
    """
    Record a broker's decision (approve / reject) for a pending trade.

    Args:
        approval_id: ID returned by request_broker_approval.
        decision: "approve" or "reject".
        notes: Optional broker notes.

    Returns:
        True if the approval existed and was updated; False otherwise.
    """
    record = _pending_approvals.get(approval_id)
    if record is None or record["status"] != "pending":
        return False

    record["status"] = decision
    record["notes"] = notes
    record["decided_at"] = datetime.now(timezone.utc).isoformat()

    future = _approval_waiters.pop(approval_id, None)
    if future and not future.done():
        future.set_result(decision)

    return True


# ── Tool implementations ──────────────────────────────────────────────────────

async def request_broker_approval(
    recommendation: str,
    action: str,
    amount: float,
    currency_pair: str = "AUD/USD",
    timeout_seconds: int = 120,
) -> str:
    """
    Submit a trade recommendation to the human broker for approval and wait
    for their decision (up to *timeout_seconds*).

    Args:
        recommendation: The analysis agent's recommendation summary.
        action: Proposed trade action – "buy" or "sell".
        amount: Proposed notional amount.
        currency_pair: Currency pair, e.g. "AUD/USD".
        timeout_seconds: How long to wait for the broker's decision.
    """
    approval_id = str(uuid.uuid4())[:8]
    loop = asyncio.get_event_loop()
    future: asyncio.Future[str] = loop.create_future()

    record: dict[str, Any] = {
        "approval_id": approval_id,
        "status": "pending",
        "recommendation": recommendation,
        "action": action,
        "amount": amount,
        "currency_pair": currency_pair,
        "created_at": datetime.now(timezone.utc).isoformat(),
        "decided_at": None,
        "notes": "",
    }
    _pending_approvals[approval_id] = record
    _approval_waiters[approval_id] = future

    try:
        decision = await asyncio.wait_for(future, timeout=timeout_seconds)
    except asyncio.TimeoutError:
        record["status"] = "timeout"
        _approval_waiters.pop(approval_id, None)
        return json.dumps(
            {
                "approval_id": approval_id,
                "decision": "timeout",
                "message": (
                    f"No broker response within {timeout_seconds}s; "
                    "trade NOT executed."
                ),
            }
        )

    return json.dumps(
        {
            "approval_id": approval_id,
            "decision": decision,
            "notes": record.get("notes", ""),
            "message": (
                f"Broker {decision}d the {action} trade for "
                f"{amount:,.0f} {currency_pair}."
            ),
        }
    )


async def send_customer_notification(
    subject: str,
    message: str,
    notification_type: str = "trade_alert",
) -> str:
    """
    Send a notification to customers about a completed trade or market event.

    Args:
        subject: Short notification subject / title.
        message: Notification body text.
        notification_type: Category label (e.g. trade_alert, market_update).
    """
    notification = {
        "id": str(uuid.uuid4())[:8],
        "type": notification_type,
        "subject": subject,
        "message": message,
        "sent_at": datetime.now(timezone.utc).isoformat(),
    }
    _notifications.append(notification)

    return json.dumps(
        {
            "status": "sent",
            "notification_id": notification["id"],
            "subject": subject,
            "message": f"Notification sent to customers: {subject}",
        }
    )


async def get_notification_history() -> str:
    """Retrieve the list of previously sent customer notifications."""
    return json.dumps({"notifications": list(reversed(_notifications))})


# ── Tool definitions (OpenAI function-calling schema) ────────────────────────

COMM_TOOL_DEFINITIONS: list[dict[str, Any]] = [
    {
        "type": "function",
        "function": {
            "name": "request_broker_approval",
            "description": (
                "Submit a trade recommendation to the human broker and wait for "
                "their approval or rejection before proceeding."
            ),
            "parameters": {
                "type": "object",
                "properties": {
                    "recommendation": {
                        "type": "string",
                        "description": "Summary of the analysis agent's recommendation.",
                    },
                    "action": {
                        "type": "string",
                        "enum": ["buy", "sell"],
                        "description": "Proposed trade direction.",
                    },
                    "amount": {
                        "type": "number",
                        "description": "Proposed notional trade amount.",
                    },
                    "currency_pair": {
                        "type": "string",
                        "description": "Currency pair, e.g. AUD/USD.",
                        "default": "AUD/USD",
                    },
                    "timeout_seconds": {
                        "type": "integer",
                        "description": "Seconds to wait before timing out.",
                        "default": 120,
                    },
                },
                "required": ["recommendation", "action", "amount"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "send_customer_notification",
            "description": (
                "Send a notification to customers about an executed trade "
                "or market event."
            ),
            "parameters": {
                "type": "object",
                "properties": {
                    "subject": {
                        "type": "string",
                        "description": "Short title / subject for the notification.",
                    },
                    "message": {
                        "type": "string",
                        "description": "Notification body text.",
                    },
                    "notification_type": {
                        "type": "string",
                        "description": "Category label.",
                        "default": "trade_alert",
                    },
                },
                "required": ["subject", "message"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "get_notification_history",
            "description": "Retrieve the list of previously sent customer notifications.",
            "parameters": {"type": "object", "properties": {}, "required": []},
        },
    },
]

COMM_TOOL_HANDLERS: dict[str, Any] = {
    "request_broker_approval": request_broker_approval,
    "send_customer_notification": send_customer_notification,
    "get_notification_history": get_notification_history,
}
