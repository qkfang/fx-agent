"""
Research tools – create research notes and track customer article views via the
research-analytics service (FxWebPortal).

Service endpoints used:
  research-analytics (http://localhost:5003)
    GET  /api/articles?category=<cat>  – list published research articles
    POST /api/articles                 – create and publish a research article
    POST /api/track                    – record a visitor / customer view event
"""
from __future__ import annotations

import json
import uuid
from datetime import datetime, timezone
from typing import Any

import httpx

from config import settings


async def get_research_articles(category: str = "") -> str:
    """
    Fetch published research articles from the research-analytics service.

    Args:
        category: Optional category filter, e.g. "AUD/USD".
    """
    try:
        params = {"category": category} if category else {}
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.get(
                f"{settings.research_analytics_url}/api/articles",
                params=params,
            )
            resp.raise_for_status()
            articles = resp.json()
            return json.dumps(
                {
                    "articles": [
                        {
                            "id": a.get("id"),
                            "title": a.get("title"),
                            "summary": a.get("summary"),
                            "category": a.get("category"),
                            "sentiment": a.get("sentiment"),
                            "author": a.get("author"),
                            "publishedDate": a.get("publishedDate"),
                            "tags": a.get("tags"),
                        }
                        for a in (articles if isinstance(articles, list) else [])
                    ],
                    "source": "research-analytics",
                }
            )
    except Exception as exc:
        return json.dumps(
            {
                "articles": [],
                "note": f"Research analytics unavailable ({exc}); no articles returned",
            }
        )


async def publish_research_note(
    title: str,
    summary: str,
    content: str,
    category: str = "AUD/USD",
    sentiment: str = "Bullish",
    author: str = "FX Research Team",
    tags: str = "",
) -> str:
    """
    Create and immediately publish a research note on the research-analytics portal.

    Args:
        title: Article headline shown to customers.
        summary: One-sentence summary displayed on the article list.
        content: Full research note body.
        category: e.g. "AUD/USD", "Technical Analysis", "Market Outlook".
        sentiment: "Bullish", "Bearish", or "Neutral".
        author: Research analyst byline.
        tags: Comma-separated keyword tags.
    """
    payload = {
        "title": title,
        "summary": summary,
        "content": content,
        "category": category,
        "sentiment": sentiment,
        "author": author,
        "tags": tags,
        "status": "Published",
    }
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.post(
                f"{settings.research_analytics_url}/api/articles",
                json=payload,
            )
            resp.raise_for_status()
            data = resp.json()
            return json.dumps(
                {
                    "status": "published",
                    "article_id": data.get("id"),
                    "title": data.get("title"),
                    "category": data.get("category"),
                    "sentiment": data.get("sentiment"),
                    "service": "research-analytics",
                }
            )
    except Exception as exc:
        return json.dumps(
            {
                "status": "simulated",
                "article_id": None,
                "title": title,
                "category": category,
                "sentiment": sentiment,
                "note": f"Research analytics unavailable ({exc}); note simulated",
            }
        )


async def track_article_view(
    article_id: int | None = None,
    user_name: str = "John Smith",
    user_email: str = "john.smith@example.com",
    user_company: str = "Smith Capital",
    time_spent_seconds: int = 120,
    page_url: str = "",
) -> str:
    """
    Record a customer article view in the research-analytics tracking service.
    This simulates a customer reading a research note – the event is persisted
    in visitors.json and surfaced in the research-analytics admin dashboard.

    Args:
        article_id: ID of the article the customer viewed (None if not yet published).
        user_name: Customer's full name.
        user_email: Customer's email address (used as a lead).
        user_company: Customer's company name.
        time_spent_seconds: How long the customer spent reading (seconds).
        page_url: URL of the article page.
    """
    session_id = str(uuid.uuid4())
    resolved_id = article_id or 0
    payload = {
        "sessionId": session_id,
        "articleId": resolved_id,
        "pageUrl": page_url or (f"{settings.research_analytics_url}/article/{resolved_id}" if resolved_id else settings.research_analytics_url),
        "timeSpentSeconds": time_spent_seconds,
        "clickCount": 3,
        "userName": user_name,
        "userEmail": user_email,
        "userCompany": user_company,
        "language": "en-AU",
        "timezone": "Australia/Sydney",
        "screenSize": "1920x1080",
        "referrer": "",
    }
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.post(
                f"{settings.research_analytics_url}/api/track",
                json=payload,
            )
            resp.raise_for_status()
            return json.dumps(
                {
                    "status": "tracked",
                    "session_id": session_id,
                    "article_id": article_id,
                    "user_name": user_name,
                    "user_email": user_email,
                    "time_spent_seconds": time_spent_seconds,
                    "service": "research-analytics",
                    "timestamp": datetime.now(timezone.utc).isoformat(),
                }
            )
    except Exception as exc:
        return json.dumps(
            {
                "status": "simulated",
                "session_id": session_id,
                "article_id": article_id,
                "user_name": user_name,
                "note": f"Research analytics unavailable ({exc}); view simulated",
                "timestamp": datetime.now(timezone.utc).isoformat(),
            }
        )


# ── Tool definitions (OpenAI function-calling schema) ────────────────────────

RESEARCH_TOOL_DEFINITIONS: list[dict[str, Any]] = [
    {
        "type": "function",
        "function": {
            "name": "get_research_articles",
            "description": (
                "Fetch published research articles from the research-analytics portal. "
                "Optionally filter by category (e.g. AUD/USD)."
            ),
            "parameters": {
                "type": "object",
                "properties": {
                    "category": {
                        "type": "string",
                        "description": "Optional category filter, e.g. AUD/USD.",
                    }
                },
                "required": [],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "publish_research_note",
            "description": (
                "Create and publish a research note on the research-analytics portal "
                "so customers can read it."
            ),
            "parameters": {
                "type": "object",
                "properties": {
                    "title": {"type": "string", "description": "Article headline."},
                    "summary": {"type": "string", "description": "One-sentence summary."},
                    "content": {"type": "string", "description": "Full research note body."},
                    "category": {
                        "type": "string",
                        "description": "e.g. AUD/USD, Technical Analysis, Market Outlook.",
                    },
                    "sentiment": {
                        "type": "string",
                        "enum": ["Bullish", "Bearish", "Neutral"],
                        "description": "Research note market sentiment.",
                    },
                    "author": {"type": "string", "description": "Research analyst byline."},
                    "tags": {"type": "string", "description": "Comma-separated tags."},
                },
                "required": ["title", "summary", "content"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "track_article_view",
            "description": (
                "Record a customer reading a research article on the portal. "
                "Used to simulate customer engagement that triggers broker outreach."
            ),
            "parameters": {
                "type": "object",
                "properties": {
                    "article_id": {
                        "type": "integer",
                        "description": "ID of the article the customer viewed.",
                    },
                    "user_name": {"type": "string", "description": "Customer full name."},
                    "user_email": {"type": "string", "description": "Customer email."},
                    "user_company": {"type": "string", "description": "Customer company."},
                    "time_spent_seconds": {
                        "type": "integer",
                        "description": "Seconds spent reading.",
                    },
                    "page_url": {"type": "string", "description": "Article page URL."},
                },
                "required": ["article_id"],
            },
        },
    },
]

RESEARCH_TOOL_HANDLERS: dict[str, Any] = {
    "get_research_articles": get_research_articles,
    "publish_research_note": publish_research_note,
    "track_article_view": track_article_view,
}
