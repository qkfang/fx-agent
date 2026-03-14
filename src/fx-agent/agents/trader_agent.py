"""
Trader Agent – uses Azure AI Foundry (Microsoft Agent Framework) to interact
with the broker back-office and trading platform, executing approved trades.
"""
from __future__ import annotations

import json
import logging
from typing import AsyncIterator

from config import settings
from tools.trading_tools import TRADING_TOOL_DEFINITIONS, TRADING_TOOL_HANDLERS

logger = logging.getLogger(__name__)

_SYSTEM_PROMPT = """You are the FX Trader Agent for an institutional foreign-exchange desk.

You are responsible for executing approved trade orders via the broker back-office
and confirming execution with the trading platform.

Your responsibilities:
1. Check the MCP service status with get_mcp_status before trading.
2. Execute the approved trade using execute_buy or execute_sell.
3. Verify execution by calling get_transaction_history.
4. Return a structured execution report.

Always respond with a JSON execution report:
{
  "execution_status": "success" | "failed",
  "action": "buy" | "sell",
  "amount": <number>,
  "currency_pair": "AUD/USD",
  "rate": <number or null>,
  "transaction_id": "<string or null>",
  "message": "<summary>",
  "timestamp": "<ISO timestamp>"
}
"""


class TraderAgent:
    """
    Wraps an Azure AI Foundry agent that executes FX trades.

    Falls back to a local demo mode when Azure AI credentials are not configured.
    """

    def __init__(self) -> None:
        self._agent_id: str | None = settings.trader_agent_id
        self._client = None

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
        if self._agent_id:
            return self._agent_id

        from azure.ai.projects.models import FunctionTool, ToolSet

        functions = FunctionTool(
            functions=[defn["function"] for defn in TRADING_TOOL_DEFINITIONS]
        )
        toolset = ToolSet()
        toolset.add(functions)

        agent = client.agents.create_agent(
            model=settings.azure_ai_model,
            name="fx-trader-agent",
            instructions=_SYSTEM_PROMPT,
            toolset=toolset,
        )
        self._agent_id = agent.id
        logger.info("Created Trader Agent: %s", agent.id)
        return agent.id

    # ── Public API ────────────────────────────────────────────────────────────

    async def execute_trade(
        self, action: str, amount: float, currency_pair: str = "AUD/USD"
    ) -> AsyncIterator[dict]:
        """
        Execute a trade and yield execution events.

        Args:
            action: "buy" or "sell".
            amount: Notional amount.
            currency_pair: e.g. "AUD/USD".
        """
        client = self._get_client()
        if client:
            async for event in self._run_foundry(client, action, amount, currency_pair):
                yield event
        else:
            async for event in self._run_demo(action, amount, currency_pair):
                yield event

    # ── Azure AI Foundry path ────────────────────────────────────────────────

    async def _run_foundry(
        self, client, action: str, amount: float, currency_pair: str
    ) -> AsyncIterator[dict]:
        from azure.ai.projects.models import MessageRole

        agent_id = self._ensure_agent(client)
        thread = client.agents.create_thread()
        prompt = (
            f"Please execute an approved {action.upper()} order for "
            f"{amount:,.0f} {currency_pair}."
        )
        client.agents.create_message(
            thread_id=thread.id, role=MessageRole.USER, content=prompt
        )
        yield {"type": "status", "content": f"Trader Agent executing {action.upper()} (Azure AI Foundry)"}

        with client.agents.create_and_process_run(
            thread_id=thread.id, agent_id=agent_id
        ) as run:
            if run.status == "failed":
                yield {"type": "error", "content": f"Run failed: {run.last_error}"}
                return

            if hasattr(run, "required_action") and run.required_action:
                for tool_call in run.required_action.submit_tool_outputs.tool_calls:
                    fn_name = tool_call.function.name
                    fn_args = json.loads(tool_call.function.arguments or "{}")
                    yield {"type": "tool_call", "content": f"Calling tool: {fn_name}", "tool": fn_name}
                    handler = TRADING_TOOL_HANDLERS.get(fn_name)
                    if handler:
                        result = await handler(**fn_args)
                        yield {"type": "tool_result", "content": f"{fn_name} completed", "tool": fn_name, "result": result}

        messages = client.agents.list_messages(thread_id=thread.id)
        for msg in messages:
            if msg.role == MessageRole.ASSISTANT:
                text = msg.content[0].text.value if msg.content else ""
                yield {"type": "result", "content": text}
                break

    # ── Local demo path ───────────────────────────────────────────────────────

    async def _run_demo(
        self, action: str, amount: float, currency_pair: str
    ) -> AsyncIterator[dict]:
        yield {
            "type": "status",
            "content": f"Trader Agent executing {action.upper()} {amount:,.0f} {currency_pair} (demo mode)",
        }

        # Step 1 – check MCP status
        yield {"type": "tool_call", "content": "Calling tool: get_mcp_status", "tool": "get_mcp_status"}
        status_json = await TRADING_TOOL_HANDLERS["get_mcp_status"]()
        status_data = json.loads(status_json)
        yield {
            "type": "tool_result",
            "content": f"MCP service: {status_data.get('status')}",
            "tool": "get_mcp_status",
            "result": status_json,
        }

        # Step 2 – execute the trade
        fn_name = "execute_buy" if action.lower() == "buy" else "execute_sell"
        yield {
            "type": "tool_call",
            "content": f"Calling tool: {fn_name}",
            "tool": fn_name,
        }
        trade_fn = TRADING_TOOL_HANDLERS[fn_name]
        trade_json = await trade_fn(amount=amount, currency_pair=currency_pair)
        trade_data = json.loads(trade_json)
        yield {
            "type": "tool_result",
            "content": f"Trade {trade_data.get('status')}: {trade_data.get('message', '')}",
            "tool": fn_name,
            "result": trade_json,
        }

        # Step 3 – verify via transaction history
        yield {
            "type": "tool_call",
            "content": "Calling tool: get_transaction_history",
            "tool": "get_transaction_history",
        }
        history_json = await TRADING_TOOL_HANDLERS["get_transaction_history"]()
        yield {
            "type": "tool_result",
            "content": "Transaction history retrieved",
            "tool": "get_transaction_history",
            "result": history_json,
        }

        # Emit final result
        yield {
            "type": "result",
            "content": json.dumps(
                {
                    "execution_status": trade_data.get("status", "unknown"),
                    "action": action,
                    "amount": amount,
                    "currency_pair": currency_pair,
                    "rate": trade_data.get("rate"),
                    "message": trade_data.get("message", ""),
                    "timestamp": trade_data.get("timestamp"),
                }
            ),
            "trade": trade_data,
        }
