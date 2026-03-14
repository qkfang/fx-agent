"""
Trading tools – execute buy/sell orders via the broker back-office MCP endpoint
and query transaction history from the trading platform.
These functions are registered as callable tools for the Trader Agent.
"""
from __future__ import annotations

import json
from datetime import datetime, timezone
from typing import Any

import httpx

from config import settings


async def execute_buy(amount: float, currency_pair: str = "AUD/USD") -> str:
    """
    Execute a BUY order through the broker back-office MCP endpoint.

    Args:
        amount: The notional amount to buy (in base currency units).
        currency_pair: The currency pair to trade (default AUD/USD).
    """
    try:
        async with httpx.AsyncClient(timeout=10) as client:
            resp = await client.post(
                f"{settings.broker_backoffice_url}/mcp/fx",
                json={"action": "buy", "amount": amount},
            )
            resp.raise_for_status()
            data = resp.json()
            return json.dumps(
                {
                    "status": "executed",
                    "action": "buy",
                    "amount": amount,
                    "currency_pair": currency_pair,
                    "rate": data.get("data", {}).get("rate") if data.get("data") else None,
                    "message": data.get("message"),
                    "timestamp": datetime.now(timezone.utc).isoformat(),
                }
            )
    except Exception as exc:
        return json.dumps(
            {
                "status": "failed",
                "action": "buy",
                "amount": amount,
                "currency_pair": currency_pair,
                "error": str(exc),
                "timestamp": datetime.now(timezone.utc).isoformat(),
            }
        )


async def execute_sell(amount: float, currency_pair: str = "AUD/USD") -> str:
    """
    Execute a SELL order through the broker back-office MCP endpoint.

    Args:
        amount: The notional amount to sell (in base currency units).
        currency_pair: The currency pair to trade (default AUD/USD).
    """
    try:
        async with httpx.AsyncClient(timeout=10) as client:
            resp = await client.post(
                f"{settings.broker_backoffice_url}/mcp/fx",
                json={"action": "sell", "amount": amount},
            )
            resp.raise_for_status()
            data = resp.json()
            return json.dumps(
                {
                    "status": "executed",
                    "action": "sell",
                    "amount": amount,
                    "currency_pair": currency_pair,
                    "rate": data.get("data", {}).get("rate") if data.get("data") else None,
                    "message": data.get("message"),
                    "timestamp": datetime.now(timezone.utc).isoformat(),
                }
            )
    except Exception as exc:
        return json.dumps(
            {
                "status": "failed",
                "action": "sell",
                "amount": amount,
                "currency_pair": currency_pair,
                "error": str(exc),
                "timestamp": datetime.now(timezone.utc).isoformat(),
            }
        )


async def get_transaction_history() -> str:
    """Retrieve recent FX transaction history from the trading platform."""
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.get(
                f"{settings.trading_platform_url}/api/transactions"
            )
            resp.raise_for_status()
            return json.dumps(resp.json())
    except Exception as exc:
        return json.dumps(
            {
                "transactions": [
                    {
                        "id": "tx-001",
                        "type": "Buy",
                        "amount": 100000,
                        "rate": 0.6520,
                        "timestamp": "2024-01-15T09:30:00Z",
                    },
                    {
                        "id": "tx-002",
                        "type": "Sell",
                        "amount": 50000,
                        "rate": 0.6580,
                        "timestamp": "2024-01-15T14:00:00Z",
                    },
                ],
                "note": f"Trading platform unavailable ({exc}); using sample data",
            }
        )


async def get_mcp_status() -> str:
    """Check the status of the broker back-office MCP service."""
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.get(f"{settings.broker_backoffice_url}/mcp/status")
            resp.raise_for_status()
            data = resp.json()
            return json.dumps({"status": "online", "detail": data})
    except Exception as exc:
        return json.dumps({"status": "offline", "error": str(exc)})


# ── Tool definitions (OpenAI function-calling schema) ────────────────────────

TRADING_TOOL_DEFINITIONS: list[dict[str, Any]] = [
    {
        "type": "function",
        "function": {
            "name": "execute_buy",
            "description": (
                "Execute a BUY order for the specified amount through the broker "
                "back-office MCP trading endpoint."
            ),
            "parameters": {
                "type": "object",
                "properties": {
                    "amount": {
                        "type": "number",
                        "description": "Notional amount to buy in base currency units.",
                    },
                    "currency_pair": {
                        "type": "string",
                        "description": "Currency pair to trade, e.g. AUD/USD.",
                        "default": "AUD/USD",
                    },
                },
                "required": ["amount"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "execute_sell",
            "description": (
                "Execute a SELL order for the specified amount through the broker "
                "back-office MCP trading endpoint."
            ),
            "parameters": {
                "type": "object",
                "properties": {
                    "amount": {
                        "type": "number",
                        "description": "Notional amount to sell in base currency units.",
                    },
                    "currency_pair": {
                        "type": "string",
                        "description": "Currency pair to trade, e.g. AUD/USD.",
                        "default": "AUD/USD",
                    },
                },
                "required": ["amount"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "get_transaction_history",
            "description": "Retrieve the recent FX transaction history from the trading platform.",
            "parameters": {"type": "object", "properties": {}, "required": []},
        },
    },
    {
        "type": "function",
        "function": {
            "name": "get_mcp_status",
            "description": "Check whether the broker back-office MCP service is online.",
            "parameters": {"type": "object", "properties": {}, "required": []},
        },
    },
]

TRADING_TOOL_HANDLERS: dict[str, Any] = {
    "execute_buy": execute_buy,
    "execute_sell": execute_sell,
    "get_transaction_history": get_transaction_history,
    "get_mcp_status": get_mcp_status,
}
