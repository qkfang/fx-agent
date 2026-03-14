"""
Orchestrator – coordinates the three agents in a full FX trading workflow:

  1. Analysis Agent  → research market & produce BUY/SELL/HOLD recommendation
  2. Comm Agent      → seek human broker approval (for BUY/SELL)
  3. Trader Agent    → execute approved trade
  4. Comm Agent      → notify customers of executed trade

All execution events are streamed to registered WebSocket clients so the
web frontend can display real-time progress.
"""
from __future__ import annotations

import asyncio
import json
import logging
import uuid
from datetime import datetime, timezone
from typing import Any

from agents.analysis_agent import AnalysisAgent
from agents.comm_agent import CommAgent
from agents.trader_agent import TraderAgent

logger = logging.getLogger(__name__)


class Orchestrator:
    """
    Manages agent lifecycle and broadcasts execution events to WebSocket
    subscribers.
    """

    def __init__(self) -> None:
        self._analysis_agent = AnalysisAgent()
        self._comm_agent = CommAgent()
        self._trader_agent = TraderAgent()

        # Active WebSocket queues – keyed by connection id
        self._subscribers: dict[str, asyncio.Queue[str | None]] = {}

        # History of completed runs
        self.run_history: list[dict[str, Any]] = []

        # Whether a workflow is currently running
        self.running: bool = False

    # ── Subscriber management ─────────────────────────────────────────────────

    def subscribe(self) -> tuple[str, asyncio.Queue[str | None]]:
        """Register a new WebSocket listener; returns (connection_id, queue)."""
        conn_id = str(uuid.uuid4())[:8]
        q: asyncio.Queue[str | None] = asyncio.Queue()
        self._subscribers[conn_id] = q
        return conn_id, q

    def unsubscribe(self, conn_id: str) -> None:
        self._subscribers.pop(conn_id, None)

    def _broadcast(self, event: dict[str, Any]) -> None:
        """Push an event to every connected WebSocket subscriber."""
        payload = json.dumps(event)
        for q in self._subscribers.values():
            q.put_nowait(payload)

    # ── Main workflow ─────────────────────────────────────────────────────────

    async def run_workflow(self) -> dict[str, Any]:
        """
        Execute the full FX trading workflow.
        Returns a summary dict when complete.
        """
        if self.running:
            return {"status": "busy", "message": "A workflow is already running"}

        self.running = True
        run_id = str(uuid.uuid4())[:8]
        started_at = datetime.now(timezone.utc).isoformat()
        events: list[dict[str, Any]] = []
        recommendation: dict[str, Any] = {}
        trade_result: dict[str, Any] = {}

        def emit(event: dict[str, Any]) -> None:
            event.setdefault("run_id", run_id)
            event.setdefault("timestamp", datetime.now(timezone.utc).isoformat())
            events.append(event)
            self._broadcast(event)
            logger.debug("Orchestrator event: %s", event.get("type"))

        try:
            emit({"type": "workflow_start", "content": f"Starting FX trading workflow (run {run_id})", "agent": "orchestrator"})

            # ── Phase 1: Analysis ─────────────────────────────────────────────
            emit({"type": "phase", "content": "Phase 1: Market Analysis", "agent": "analysis"})
            async for event in self._analysis_agent.run():
                event["agent"] = "analysis"
                emit(event)
                if event.get("type") == "result" and event.get("recommendation"):
                    recommendation = event["recommendation"]

            if not recommendation:
                # Try to parse JSON from the last result event
                for ev in reversed(events):
                    if ev.get("type") == "result" and ev.get("agent") == "analysis":
                        try:
                            recommendation = json.loads(ev.get("content", "{}"))
                        except json.JSONDecodeError:
                            pass
                        break

            if not recommendation:
                recommendation = {
                    "recommendation": "HOLD",
                    "amount": 0,
                    "currency_pair": "AUD/USD",
                    "summary": "Could not parse analysis recommendation",
                    "reasons": [],
                    "risks": [],
                    "current_rate": 0.655,
                }

            emit({
                "type": "recommendation",
                "content": f"Recommendation: {recommendation.get('recommendation')} {recommendation.get('currency_pair')} {recommendation.get('amount', 0):,.0f}",
                "agent": "analysis",
                "recommendation": recommendation,
            })

            # ── Phase 2: Broker Approval (skip for HOLD) ──────────────────────
            action = recommendation.get("recommendation", "HOLD")
            broker_decision = "not_required"
            approval_id: str | None = None

            if action in ("BUY", "SELL"):
                emit({"type": "phase", "content": "Phase 2: Broker Approval", "agent": "comm"})
                async for event in self._comm_agent.request_approval(recommendation):
                    event["agent"] = "comm"
                    emit(event)
                    if event.get("type") == "approval_result":
                        broker_decision = event.get("decision", "pending")
                        approval_id = event.get("approval_id")

                if broker_decision == "pending":
                    # Approval submitted but not yet responded to – pause here
                    emit({
                        "type": "awaiting_approval",
                        "content": "Waiting for broker approval via the dashboard…",
                        "agent": "comm",
                        "approval_id": approval_id,
                    })
                    # Wait up to 5 minutes for a broker decision
                    deadline = asyncio.get_event_loop().time() + 300
                    while broker_decision == "pending" and asyncio.get_event_loop().time() < deadline:
                        await asyncio.sleep(2)
                        from tools.comm_tools import _pending_approvals
                        record = _pending_approvals.get(approval_id or "")
                        if record and record["status"] != "pending":
                            broker_decision = record["status"]

            emit({
                "type": "approval_decision",
                "content": f"Broker decision: {broker_decision}",
                "agent": "comm",
                "decision": broker_decision,
            })

            # ── Phase 3: Trade Execution ──────────────────────────────────────
            if action in ("BUY", "SELL") and broker_decision in ("approve", "not_required"):
                emit({"type": "phase", "content": "Phase 3: Trade Execution", "agent": "trader"})
                amount = float(recommendation.get("amount", 100000))
                currency_pair = recommendation.get("currency_pair", "AUD/USD")

                async for event in self._trader_agent.execute_trade(
                    action=action.lower(), amount=amount, currency_pair=currency_pair
                ):
                    event["agent"] = "trader"
                    emit(event)
                    if event.get("type") == "result" and event.get("trade"):
                        trade_result = event["trade"]

                if not trade_result:
                    for ev in reversed(events):
                        if ev.get("type") == "result" and ev.get("agent") == "trader":
                            try:
                                trade_result = json.loads(ev.get("content", "{}"))
                            except json.JSONDecodeError:
                                pass
                            break

                # ── Phase 4: Customer Notification ────────────────────────────
                if trade_result:
                    emit({"type": "phase", "content": "Phase 4: Customer Notification", "agent": "comm"})
                    async for event in self._comm_agent.send_trade_notification(trade_result):
                        event["agent"] = "comm"
                        emit(event)
            else:
                emit({
                    "type": "status",
                    "content": (
                        "Workflow complete – no trade executed "
                        f"(action={action}, decision={broker_decision})"
                    ),
                    "agent": "orchestrator",
                })

            summary = {
                "run_id": run_id,
                "started_at": started_at,
                "completed_at": datetime.now(timezone.utc).isoformat(),
                "status": "completed",
                "recommendation": recommendation,
                "broker_decision": broker_decision,
                "trade_result": trade_result,
                "event_count": len(events),
            }

        except Exception as exc:
            logger.exception("Workflow error: %s", exc)
            emit({"type": "error", "content": f"Workflow error: {exc}", "agent": "orchestrator"})
            summary = {
                "run_id": run_id,
                "started_at": started_at,
                "completed_at": datetime.now(timezone.utc).isoformat(),
                "status": "error",
                "error": str(exc),
                "event_count": len(events),
            }
        finally:
            self.running = False

        emit({"type": "workflow_end", "content": f"Workflow {run_id} finished", "agent": "orchestrator", "summary": summary})
        self.run_history.append(summary)
        return summary
