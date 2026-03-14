"""
Analysis Agent – uses Azure AI Foundry (Microsoft Agent Framework) to research
current FX market conditions, interpret news sentiment, review the portfolio,
and produce a clear BUY / SELL / HOLD recommendation for the broker.
"""
from __future__ import annotations

import json
import logging
from typing import AsyncIterator

from config import settings
from tools.market_tools import MARKET_TOOL_DEFINITIONS, MARKET_TOOL_HANDLERS

logger = logging.getLogger(__name__)

_SYSTEM_PROMPT = """You are the FX Analysis Agent for an institutional foreign-exchange desk.

Your responsibilities:
1. Retrieve the current AUD/USD rate using get_fx_rate.
2. Fetch the latest market news using get_market_news.
3. Check the portfolio summary using get_portfolio_summary.
4. Synthesise all data into a concise market analysis.
5. Produce a clear trading recommendation: BUY, SELL, or HOLD.
   Include:
   - Recommended action (BUY / SELL / HOLD)
   - Suggested notional amount (in AUD units, e.g. 100000)
   - Key reasons (2-3 bullet points)
   - Risk factors to consider

Always respond in structured JSON with the schema:
{
  "recommendation": "BUY" | "SELL" | "HOLD",
  "amount": <number>,
  "currency_pair": "AUD/USD",
  "reasons": ["...", "..."],
  "risks": ["...", "..."],
  "current_rate": <number>,
  "summary": "<one-sentence summary>"
}
"""


class AnalysisAgent:
    """
    Wraps an Azure AI Foundry agent that analyses the FX market.

    When Azure AI Foundry credentials are available (AZURE_AI_CONNECTION_STRING
    is set) the agent runs on the managed service.  Otherwise a local demo mode
    is used that calls the same tool functions and returns a structured result.
    """

    def __init__(self) -> None:
        self._agent_id: str | None = settings.analysis_agent_id
        self._client = None

    # ── Lazy Foundry client ──────────────────────────────────────────────────

    def _get_client(self):
        if self._client is not None:
            return self._client
        if not settings.azure_ai_connection_string:
            return None
        try:
            from azure.ai.projects import AIProjectClient
            from azure.identity import DefaultAzureCredential

            self._client = AIProjectClient.from_connection_string(
                credential=DefaultAzureCredential(),
                conn_str=settings.azure_ai_connection_string,
            )
        except Exception as exc:
            logger.warning("Could not create AIProjectClient: %s", exc)
        return self._client

    def _ensure_agent(self, client) -> str:
        """Create the Foundry agent on first use and cache its ID."""
        if self._agent_id:
            return self._agent_id

        from azure.ai.projects.models import FunctionTool, ToolSet

        functions = FunctionTool(
            functions=[
                defn["function"] for defn in MARKET_TOOL_DEFINITIONS
            ]
        )
        toolset = ToolSet()
        toolset.add(functions)

        agent = client.agents.create_agent(
            model=settings.azure_ai_model,
            name="fx-analysis-agent",
            instructions=_SYSTEM_PROMPT,
            toolset=toolset,
        )
        self._agent_id = agent.id
        logger.info("Created Analysis Agent: %s", agent.id)
        return agent.id

    # ── Public API ────────────────────────────────────────────────────────────

    async def run(self, context: str = "") -> AsyncIterator[dict]:
        """
        Run the analysis agent and yield execution events.

        Each yielded dict has at minimum: {"type": str, "content": str}.
        """
        client = self._get_client()
        if client:
            async for event in self._run_foundry(client, context):
                yield event
        else:
            async for event in self._run_demo(context):
                yield event

    # ── Azure AI Foundry path ────────────────────────────────────────────────

    async def _run_foundry(self, client, context: str) -> AsyncIterator[dict]:
        from azure.ai.projects.models import MessageRole

        agent_id = self._ensure_agent(client)
        thread = client.agents.create_thread()
        user_message = (
            f"Analyse the current AUD/USD FX market and provide a trading "
            f"recommendation. {context}".strip()
        )
        client.agents.create_message(
            thread_id=thread.id,
            role=MessageRole.USER,
            content=user_message,
        )

        yield {"type": "status", "content": "Analysis Agent started (Azure AI Foundry)"}

        with client.agents.create_and_process_run(
            thread_id=thread.id, agent_id=agent_id
        ) as run:
            if run.status == "failed":
                yield {"type": "error", "content": f"Run failed: {run.last_error}"}
                return

            # Handle tool calls
            if hasattr(run, "required_action") and run.required_action:
                for tool_call in run.required_action.submit_tool_outputs.tool_calls:
                    fn_name = tool_call.function.name
                    fn_args = json.loads(tool_call.function.arguments or "{}")
                    yield {
                        "type": "tool_call",
                        "content": f"Calling tool: {fn_name}",
                        "tool": fn_name,
                    }
                    handler = MARKET_TOOL_HANDLERS.get(fn_name)
                    if handler:
                        result = await handler(**fn_args)
                        yield {
                            "type": "tool_result",
                            "content": f"Tool {fn_name} returned data",
                            "tool": fn_name,
                            "result": result,
                        }

        messages = client.agents.list_messages(thread_id=thread.id)
        for msg in messages:
            if msg.role == MessageRole.ASSISTANT:
                text = msg.content[0].text.value if msg.content else ""
                yield {"type": "result", "content": text}
                break

    # ── Local demo path (no Azure credentials needed) ────────────────────────

    async def _run_demo(self, context: str) -> AsyncIterator[dict]:
        yield {"type": "status", "content": "Analysis Agent started (demo mode – no Azure AI credentials)"}

        # Step 1 – get FX rate
        yield {"type": "tool_call", "content": "Calling tool: get_fx_rate", "tool": "get_fx_rate"}
        rate_json = await MARKET_TOOL_HANDLERS["get_fx_rate"]()
        rate_data = json.loads(rate_json)
        yield {"type": "tool_result", "content": f"Current rate: {rate_data.get('rate')}", "tool": "get_fx_rate", "result": rate_json}

        # Step 2 – get news
        yield {"type": "tool_call", "content": "Calling tool: get_market_news", "tool": "get_market_news"}
        news_json = await MARKET_TOOL_HANDLERS["get_market_news"]()
        news_data = json.loads(news_json)
        yield {"type": "tool_result", "content": f"Fetched {len(news_data.get('news', []))} articles", "tool": "get_market_news", "result": news_json}

        # Step 3 – get portfolio
        yield {"type": "tool_call", "content": "Calling tool: get_portfolio_summary", "tool": "get_portfolio_summary"}
        portfolio_json = await MARKET_TOOL_HANDLERS["get_portfolio_summary"]()
        yield {"type": "tool_result", "content": "Portfolio data retrieved", "tool": "get_portfolio_summary", "result": portfolio_json}

        # Step 4 – synthesise recommendation
        rate = rate_data.get("rate", 0.655)
        news_items = news_data.get("news", [])
        negative_count = sum(1 for n in news_items if n.get("sentiment") == "negative")
        positive_count = sum(1 for n in news_items if n.get("sentiment") == "positive")

        if positive_count > negative_count and rate < 0.665:
            action = "BUY"
            reasons = [
                f"Rate {rate:.4f} is below recent average – buying opportunity",
                f"{positive_count} positive news items support AUD strength",
                "Portfolio has capacity for additional exposure",
            ]
        elif negative_count > positive_count or rate > 0.675:
            action = "SELL"
            reasons = [
                f"Rate {rate:.4f} is elevated – locking in gains",
                f"{negative_count} negative news items indicate AUD weakness ahead",
                "Risk reduction aligned with fund mandate",
            ]
        else:
            action = "HOLD"
            reasons = [
                f"Rate {rate:.4f} is within neutral range",
                "Mixed news sentiment – insufficient directional signal",
                "Maintaining current position pending clearer catalyst",
            ]

        recommendation = {
            "recommendation": action,
            "amount": 100000,
            "currency_pair": "AUD/USD",
            "current_rate": rate,
            "reasons": reasons,
            "risks": [
                "Unexpected central bank announcement could reverse direction",
                "Low liquidity window – wider spreads possible",
            ],
            "summary": (
                f"{action} AUD/USD at {rate:.4f}. "
                f"{'Positive' if positive_count >= negative_count else 'Negative'} "
                "news sentiment combined with current rate level drives this call."
            ),
        }

        yield {
            "type": "result",
            "content": json.dumps(recommendation, indent=2),
            "recommendation": recommendation,
        }
