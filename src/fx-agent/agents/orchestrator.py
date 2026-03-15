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
from config import SCENARIO_MIDDLE_EAST_WAR

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

    async def run_workflow(self, scenario: str = "") -> dict[str, Any]:
        """
        Execute the full FX trading workflow.

        Args:
            scenario: Optional scenario key to inject specific market context.
                      Supported values: "middle_east_war"
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

        # Apply scenario context for demo-mode tools
        if scenario:
            from tools.market_tools import set_scenario
            set_scenario(scenario)

        def emit(event: dict[str, Any]) -> None:
            event.setdefault("run_id", run_id)
            event.setdefault("timestamp", datetime.now(timezone.utc).isoformat())
            events.append(event)
            self._broadcast(event)
            logger.debug("Orchestrator event: %s", event.get("type"))

        try:
            emit({"type": "workflow_start", "content": f"Starting FX trading workflow (run {run_id})", "agent": "orchestrator"})

            # ── Phase 0: News Feed – publish article to news-feed service ─────
            if scenario == SCENARIO_MIDDLE_EAST_WAR:
                emit({"type": "phase", "content": "Phase 0: News Feed", "agent": "news_feed"})
                from tools.market_tools import publish_news_article
                news_title = "War erupts in Middle East: oil prices surge on supply fears"
                news_content = (
                    "A major armed conflict has broken out in the Middle East, sending crude oil "
                    "prices sharply higher on fears of supply disruptions. The Australian dollar "
                    "came under initial selling pressure as risk-off sentiment swept markets, but "
                    "analysts note that Australia's commodity export revenue stands to benefit "
                    "materially from elevated energy prices. AUD/USD is seen as a buying "
                    "opportunity on any dip below 0.6580."
                )
                news_result_json = await publish_news_article(
                    title=news_title,
                    content=news_content,
                    article_type="Good",
                    category="FX",
                    author="Global FX Wire",
                    summary="Middle East war drives oil prices higher; AUD seen as buy on dips.",
                )
                news_result = json.loads(news_result_json)
                emit({
                    "type": "news_published",
                    "content": f"Breaking: {news_title}",
                    "agent": "news_feed",
                    "article": {
                        "id": news_result.get("article_id"),
                        "title": news_title,
                        "service": "news-feed",
                        "status": news_result.get("status"),
                    },
                })
                emit({
                    "type": "status",
                    "content": "News article published to news-feed service and forwarded to Research Analytics",
                    "agent": "news_feed",
                })

            # ── Phase 1: Research Analytics / Market Analysis ──────────────────
            analysis_label = "Phase 1: Research Analytics & Market Analysis" if scenario else "Phase 1: Market Analysis"
            emit({"type": "phase", "content": analysis_label, "agent": "analysis"})
            # Pass scenario as human-readable context for the Azure AI Foundry LLM prompt.
            # The demo-mode news tool uses the globally set _scenario_context separately.
            llm_context = f"Scenario: {scenario}. " if scenario else ""
            async for event in self._analysis_agent.run(context=llm_context):
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

            # ── Scenario: Publish research note + track customer view ──────────
            research_article_id: int | None = None
            if scenario == SCENARIO_MIDDLE_EAST_WAR:
                from tools.research_tools import publish_research_note, track_article_view

                rec_action = recommendation.get("recommendation", "BUY")
                rec_summary = recommendation.get("summary", "")
                note_title = "AUD/USD – Buying Opportunity Amid Middle East Uncertainty"
                note_content = (
                    f"Research Note – {datetime.now(timezone.utc).strftime('%d %b %Y')}\n\n"
                    f"**Recommendation: {rec_action} AUD/USD**\n\n"
                    f"{rec_summary}\n\n"
                    "**Key Drivers:**\n"
                    + "\n".join(f"- {r}" for r in recommendation.get("reasons", []))
                    + "\n\n**Risk Factors:**\n"
                    + "\n".join(f"- {r}" for r in recommendation.get("risks", []))
                )
                note_result_json = await publish_research_note(
                    title=note_title,
                    summary=rec_summary or "AUD/USD buy opportunity driven by Middle East commodity tailwinds.",
                    content=note_content,
                    category="AUD/USD",
                    sentiment="Bullish" if rec_action == "BUY" else ("Bearish" if rec_action == "SELL" else "Neutral"),
                    author="FX Research Team",
                    tags="AUD/USD,Middle East,Commodities,Geopolitical",
                )
                note_result = json.loads(note_result_json)
                research_article_id = note_result.get("article_id")
                emit({
                    "type": "research_note",
                    "content": f"Research note published to research-analytics portal: '{note_title}'",
                    "agent": "analysis",
                    "note": {
                        "id": research_article_id,
                        "title": note_title,
                        "sentiment": note_result.get("sentiment"),
                        "published_to": "Research Analytics Portal",
                        "service": "research-analytics",
                        "status": note_result.get("status"),
                    },
                })

                # Phase 2: Customer views the research note on the portal
                emit({"type": "phase", "content": "Phase 2: Customer Engagement", "agent": "comm"})
                view_result_json = await track_article_view(
                    article_id=research_article_id or 0,
                    user_name="John Smith",
                    user_email="john.smith@smithcapital.com",
                    user_company="Smith Capital",
                    time_spent_seconds=187,
                )
                view_result = json.loads(view_result_json)
                emit({
                    "type": "customer_view",
                    "content": "Customer 'John Smith' viewed research note on Research Analytics Portal – engagement recorded",
                    "agent": "comm",
                    "customer": {
                        "name": "John Smith",
                        "email": "john.smith@smithcapital.com",
                        "company": "Smith Capital",
                        "session_id": view_result.get("session_id"),
                        "time_spent_seconds": view_result.get("time_spent_seconds"),
                        "service": "research-analytics",
                    },
                })
                emit({
                    "type": "broker_outreach",
                    "content": "Customer engagement logged in research-analytics. Broker notified to reach out to John Smith (Smith Capital).",
                    "agent": "comm",
                })

            # ── Phase 3 (or Phase 2): Broker Approval (skip for HOLD) ─────────
            approval_phase_label = "Phase 3: Broker Approval" if scenario else "Phase 2: Broker Approval"
            action = recommendation.get("recommendation", "HOLD")
            broker_decision = "not_required"
            approval_id: str | None = None

            if action in ("BUY", "SELL"):
                emit({"type": "phase", "content": approval_phase_label, "agent": "comm"})
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

            # ── Phase 4 (or Phase 3): Trade Execution ────────────────────────
            trade_phase_label = "Phase 4: Trade Execution" if scenario else "Phase 3: Trade Execution"
            notification_phase_label = "Phase 5: Customer Notification" if scenario else "Phase 4: Customer Notification"
            if action in ("BUY", "SELL") and broker_decision in ("approve", "not_required"):
                emit({"type": "phase", "content": trade_phase_label, "agent": "trader"})
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

                # ── Phase 5 (or Phase 4): Customer Notification ───────────────
                if trade_result:
                    emit({"type": "phase", "content": notification_phase_label, "agent": "comm"})
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
            if scenario:
                from tools.market_tools import reset_scenario
                reset_scenario()

        emit({"type": "workflow_end", "content": f"Workflow {run_id} finished", "agent": "orchestrator", "summary": summary})
        self.run_history.append(summary)
        return summary
