"""
Market tools – fetch FX rates and news headlines from the existing .NET services.
These functions are registered as callable tools for the Analysis Agent.

Service endpoints used:
  broker-backoffice  (http://localhost:5269) – FX rate, account summaries
  news-feed          (http://localhost:5142) – news articles (GET/POST /api/news)
"""
from __future__ import annotations

import json
from datetime import datetime, timezone
from typing import Any

import httpx

from config import settings, SCENARIO_MIDDLE_EAST_WAR

# ── Scenario context (used in demo/fallback mode) ────────────────────────────
# Set via set_scenario() before running a workflow to inject specific news data.
# Safe for concurrent use because the Orchestrator enforces a single active
# workflow at a time (orchestrator.running guard).

_scenario_context: str = ""


def set_scenario(scenario: str) -> None:
    """Set the current scenario context so demo-mode news reflects the event."""
    global _scenario_context
    _scenario_context = scenario


def reset_scenario() -> None:
    """Clear the scenario context after a workflow completes."""
    global _scenario_context
    _scenario_context = ""


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


def _map_news_article(a: dict) -> dict:
    """
    Map a news-feed NewsArticle to the sentiment format expected by the analysis agent.

    The news-feed service uses Type: "Good" / "Bad".  We normalise that to
    sentiment: "positive" / "negative" / "neutral" so the analysis agent can
    interpret the news regardless of which service provides it.
    """
    raw_type = a.get("type", "")
    if raw_type == "Good":
        sentiment = "positive"
    elif raw_type == "Bad":
        sentiment = "negative"
    else:
        # Fallback: accept pre-mapped sentiment if present
        sentiment = a.get("sentiment", "neutral")
    return {
        "title": a.get("title", ""),
        "sentiment": sentiment,
        "category": a.get("category", "FX"),
        "author": a.get("author", ""),
        "publishedAt": a.get("publishedAt") or a.get("publishedDate", ""),
    }


async def get_market_news() -> str:
    """Fetch the latest FX news headlines from the news-feed service (GET /api/news)."""
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.get(f"{settings.news_feed_url}/api/news")
            resp.raise_for_status()
            articles = resp.json()
            headlines = [
                _map_news_article(a)
                for a in (articles[:5] if isinstance(articles, list) else [])
            ]
            return json.dumps({"news": headlines})
    except Exception as exc:
        # Return fallback news when service is unavailable
        if _scenario_context == SCENARIO_MIDDLE_EAST_WAR:
            mock_news = [
                {
                    "title": "War erupts in Middle East: oil prices surge on supply fears",
                    "sentiment": "positive",
                    "publishedAt": datetime.now(timezone.utc).isoformat(),
                },
                {
                    "title": "Australia commodity exports to benefit from oil price spike",
                    "sentiment": "positive",
                    "publishedAt": datetime.now(timezone.utc).isoformat(),
                },
                {
                    "title": "AUD/USD dips on initial risk-off – analysts see buying opportunity",
                    "sentiment": "positive",
                    "publishedAt": datetime.now(timezone.utc).isoformat(),
                },
                {
                    "title": "USD safe-haven demand temporarily weighs on AUD/USD",
                    "sentiment": "negative",
                    "publishedAt": datetime.now(timezone.utc).isoformat(),
                },
            ]
        else:
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


async def publish_news_article(
    title: str,
    content: str,
    article_type: str = "Good",
    category: str = "FX",
    author: str = "FX News Team",
    summary: str = "",
) -> str:
    """
    Publish a news article to the news-feed service (POST /api/news).

    Args:
        title: Article headline.
        content: Full article body.
        article_type: "Good" or "Bad" – the news-feed service's sentiment marker.
        category: Article category, e.g. "FX", "Commodities", "Macro".
        author: Byline.
        summary: Short summary (optional).
    """
    payload = {
        "title": title,
        "summary": summary or title,
        "content": content,
        "type": article_type,
        "category": category,
        "author": author,
        "isPublished": True,
        "publishedAt": datetime.now(timezone.utc).isoformat(),
    }
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.post(
                f"{settings.news_feed_url}/api/news",
                json=payload,
            )
            resp.raise_for_status()
            data = resp.json()
            return json.dumps(
                {
                    "status": "published",
                    "article_id": data.get("id"),
                    "title": data.get("title"),
                    "service": "news-feed",
                }
            )
    except Exception as exc:
        return json.dumps(
            {
                "status": "simulated",
                "article_id": None,
                "title": title,
                "note": f"News-feed service unavailable ({exc}); article simulated",
            }
        )


async def get_portfolio_summary() -> str:
    """
    Retrieve customer account summaries from the broker back-office service.
    Falls back to the trading platform's fund summary, then to sample data.
    """
    # Primary: broker back-office has live account data with real positions
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.get(f"{settings.broker_backoffice_url}/api/accounts")
            resp.raise_for_status()
            accounts = resp.json()
            total_balance = sum(a.get("balance", 0) for a in accounts)
            total_pnl = sum(a.get("openPnL", 0) for a in accounts)
            return json.dumps(
                {
                    "fund_name": "FX Alpha Fund",
                    "accounts": len(accounts),
                    "total_balance": total_balance,
                    "total_open_pnl": total_pnl,
                    "available_cash": total_balance,
                    "source": "broker-backoffice",
                }
            )
    except Exception:
        pass

    # Secondary: trading platform has a static fund summary
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.get(f"{settings.trading_platform_url}/api/portfolio")
            resp.raise_for_status()
            data = resp.json()
            return json.dumps(
                {
                    "fund_name": "FX Alpha Fund",
                    "total_balance": data.get("totalBalance", 0),
                    "aud_balance": data.get("audBalance", 0),
                    "usd_balance": data.get("usdBalance", 0),
                    "total_pnl": data.get("totalProfitLoss", 0),
                    "source": "trading-platform",
                }
            )
    except Exception as exc:
        return json.dumps(
            {
                "fund_name": "FX Alpha Fund",
                "aud_usd_position": 500000,
                "unrealised_pnl": 3250.0,
                "available_cash": 120000.0,
                "note": f"Services unavailable ({exc}); using sample data",
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
                "(positive / negative / neutral) from the news-feed service."
            ),
            "parameters": {"type": "object", "properties": {}, "required": []},
        },
    },
    {
        "type": "function",
        "function": {
            "name": "get_portfolio_summary",
            "description": (
                "Get the current portfolio / fund summary including account balances, "
                "open positions, unrealised P&L, and available cash from the "
                "broker back-office service."
            ),
            "parameters": {"type": "object", "properties": {}, "required": []},
        },
    },
    {
        "type": "function",
        "function": {
            "name": "publish_news_article",
            "description": (
                "Publish a news article to the news-feed service so it is visible "
                "to research analysts and customers."
            ),
            "parameters": {
                "type": "object",
                "properties": {
                    "title": {"type": "string", "description": "Article headline."},
                    "content": {"type": "string", "description": "Full article body."},
                    "article_type": {
                        "type": "string",
                        "enum": ["Good", "Bad"],
                        "description": "Positive or negative market news.",
                    },
                    "category": {
                        "type": "string",
                        "description": "Category, e.g. FX, Commodities, Macro.",
                    },
                    "author": {"type": "string", "description": "Byline."},
                    "summary": {"type": "string", "description": "Short summary."},
                },
                "required": ["title", "content"],
            },
        },
    },
]

MARKET_TOOL_HANDLERS: dict[str, Any] = {
    "get_fx_rate": get_fx_rate,
    "get_market_news": get_market_news,
    "get_portfolio_summary": get_portfolio_summary,
    "publish_news_article": publish_news_article,
}

