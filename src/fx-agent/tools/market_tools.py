"""
Market tools – fetch FX rates and news headlines from the existing .NET services.
These functions are registered as callable tools for the Analysis Agent.
"""
from __future__ import annotations

import json
from datetime import datetime, timezone
from typing import Any

import httpx

from config import settings


async def get_fx_rate() -> str:
    """Return the current AUD/USD FX rate from the broker back-office service."""
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.get(f"{settings.broker_backoffice_url}/api/fx/rate")
            resp.raise_for_status()
            data = resp.json()
            return json.dumps(
                {
                    "currency_pair": data.get("currencyPair", "AUD/USD"),
                    "rate": data.get("rate"),
                    "timestamp": data.get("timestamp"),
                }
            )
    except Exception as exc:
        # Fallback with a simulated rate when the service is unavailable
        return json.dumps(
            {
                "currency_pair": "AUD/USD",
                "rate": 0.655,
                "timestamp": datetime.now(timezone.utc).isoformat(),
                "note": f"Service unavailable ({exc}); using simulated value",
            }
        )


async def get_market_news() -> str:
    """Fetch the latest FX news headlines from the news-feed service."""
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.get(f"{settings.news_feed_url}/api/news")
            resp.raise_for_status()
            articles = resp.json()
            headlines = [
                {
                    "title": a.get("title", ""),
                    "sentiment": a.get("sentiment", "neutral"),
                    "publishedAt": a.get("publishedAt", ""),
                }
                for a in (articles[:5] if isinstance(articles, list) else [])
            ]
            return json.dumps({"news": headlines})
    except Exception as exc:
        # Return mock news when service is unavailable
        mock_news = [
            {
                "title": "RBA holds interest rates steady amid inflation concerns",
                "sentiment": "negative",
                "publishedAt": datetime.now(timezone.utc).isoformat(),
            },
            {
                "title": "USD strengthens on positive US jobs data",
                "sentiment": "negative",
                "publishedAt": datetime.now(timezone.utc).isoformat(),
            },
            {
                "title": "AUD recovers on strong commodity exports",
                "sentiment": "positive",
                "publishedAt": datetime.now(timezone.utc).isoformat(),
            },
        ]
        return json.dumps(
            {
                "news": mock_news,
                "note": f"News service unavailable ({exc}); using sample data",
            }
        )


async def get_portfolio_summary() -> str:
    """Retrieve the current fund / portfolio summary from the trading platform."""
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.get(f"{settings.trading_platform_url}/api/portfolio")
            resp.raise_for_status()
            return json.dumps(resp.json())
    except Exception as exc:
        return json.dumps(
            {
                "fund_name": "FX Alpha Fund",
                "aud_usd_position": 500000,
                "unrealised_pnl": 3250.0,
                "available_cash": 120000.0,
                "note": f"Trading platform unavailable ({exc}); using sample data",
            }
        )


# ── Tool definitions (OpenAI function-calling schema) ────────────────────────

MARKET_TOOL_DEFINITIONS: list[dict[str, Any]] = [
    {
        "type": "function",
        "function": {
            "name": "get_fx_rate",
            "description": (
                "Get the current AUD/USD foreign exchange rate from the live broker "
                "back-office service."
            ),
            "parameters": {"type": "object", "properties": {}, "required": []},
        },
    },
    {
        "type": "function",
        "function": {
            "name": "get_market_news",
            "description": (
                "Retrieve the latest FX market news headlines and their sentiment "
                "(positive / negative / neutral)."
            ),
            "parameters": {"type": "object", "properties": {}, "required": []},
        },
    },
    {
        "type": "function",
        "function": {
            "name": "get_portfolio_summary",
            "description": (
                "Get the current portfolio / fund summary including positions, "
                "unrealised P&L, and available cash."
            ),
            "parameters": {"type": "object", "properties": {}, "required": []},
        },
    },
]

MARKET_TOOL_HANDLERS: dict[str, Any] = {
    "get_fx_rate": get_fx_rate,
    "get_market_news": get_market_news,
    "get_portfolio_summary": get_portfolio_summary,
}
