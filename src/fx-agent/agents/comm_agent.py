"""
Comm Agent – uses Azure AI Foundry (Microsoft Agent Framework) to:
  1. Present a trade recommendation to the human broker and collect approval.
  2. Send notifications to customers after a trade has been executed.
"""
from __future__ import annotations

import json
import logging
from typing import AsyncIterator

from config import settings
from tools.comm_tools import COMM_TOOL_DEFINITIONS, COMM_TOOL_HANDLERS

logger = logging.getLogger(__name__)

_SYSTEM_PROMPT = """You are the FX Communications Agent for an institutional foreign-exchange desk.

You act as the bridge between the analysis team and the human broker, and between
the desk and its customers.

Your responsibilities:
1. Present trade recommendations to the human broker via request_broker_approval
   and wait for their decision.
2. After a trade is executed, notify customers with a clear, professional message
   using send_customer_notification.
3. Always be concise, professional, and accurate in your communications.

When requesting broker approval, include:
- The recommended action (BUY / SELL / HOLD)
- The suggested amount
- 2-3 key reasons
- Any significant risk factors

When notifying customers, include:
- What action was taken
- The rate at which the trade was executed
- Brief market context
"""


class CommAgent:
    """
    Wraps an Azure AI Foundry agent that handles broker/customer communications.

    Falls back to a local demo mode when Azure AI credentials are not configured.
    """

    def __init__(self) -> None:
        self._agent_id: str | None = settings.comm_agent_id
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
            functions=[defn["function"] for defn in COMM_TOOL_DEFINITIONS]
        )
        toolset = ToolSet()
        toolset.add(functions)

        agent = client.agents.create_agent(
            model=settings.azure_ai_model,
            name="fx-comm-agent",
            instructions=_SYSTEM_PROMPT,
            toolset=toolset,
        )
        self._agent_id = agent.id
        logger.info("Created Comm Agent: %s", agent.id)
        return agent.id

    # ── Public API ────────────────────────────────────────────────────────────

    async def request_approval(
        self, recommendation: dict
    ) -> AsyncIterator[dict]:
        """
        Seek human broker approval for the given trade recommendation.
        Yields execution events; the final event contains the broker decision.
        """
        client = self._get_client()
        if client:
            async for event in self._run_foundry_approval(client, recommendation):
                yield event
        else:
            async for event in self._run_demo_approval(recommendation):
                yield event

    async def send_trade_notification(
        self, trade_result: dict
    ) -> AsyncIterator[dict]:
        """Send a post-trade customer notification."""
        client = self._get_client()
        if client:
            async for event in self._run_foundry_notification(client, trade_result):
                yield event
        else:
            async for event in self._run_demo_notification(trade_result):
                yield event

    # ── Azure AI Foundry path ────────────────────────────────────────────────

    async def _run_foundry_approval(
        self, client, recommendation: dict
    ) -> AsyncIterator[dict]:
        from azure.ai.projects.models import MessageRole

        agent_id = self._ensure_agent(client)
        thread = client.agents.create_thread()
        prompt = (
            f"Please request broker approval for this trade recommendation: "
            f"{json.dumps(recommendation)}"
        )
        client.agents.create_message(
            thread_id=thread.id, role=MessageRole.USER, content=prompt
        )
        yield {"type": "status", "content": "Comm Agent requesting broker approval (Azure AI Foundry)"}

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
                    handler = COMM_TOOL_HANDLERS.get(fn_name)
                    if handler:
                        result = await handler(**fn_args)
                        yield {"type": "tool_result", "content": f"{fn_name} completed", "tool": fn_name, "result": result}

        messages = client.agents.list_messages(thread_id=thread.id)
        for msg in messages:
            if msg.role == MessageRole.ASSISTANT:
                text = msg.content[0].text.value if msg.content else ""
                yield {"type": "result", "content": text}
                break

    async def _run_foundry_notification(
        self, client, trade_result: dict
    ) -> AsyncIterator[dict]:
        from azure.ai.projects.models import MessageRole

        agent_id = self._ensure_agent(client)
        thread = client.agents.create_thread()
        prompt = (
            f"A trade has been executed. Please send a customer notification: "
            f"{json.dumps(trade_result)}"
        )
        client.agents.create_message(
            thread_id=thread.id, role=MessageRole.USER, content=prompt
        )
        yield {"type": "status", "content": "Comm Agent sending customer notification (Azure AI Foundry)"}

        with client.agents.create_and_process_run(
            thread_id=thread.id, agent_id=agent_id
        ) as run:
            if run.status == "failed":
                yield {"type": "error", "content": f"Run failed: {run.last_error}"}
                return

        messages = client.agents.list_messages(thread_id=thread.id)
        for msg in messages:
            if msg.role == MessageRole.ASSISTANT:
                text = msg.content[0].text.value if msg.content else ""
                yield {"type": "result", "content": text}
                break

    # ── Local demo path ───────────────────────────────────────────────────────

    async def _run_demo_approval(
        self, recommendation: dict
    ) -> AsyncIterator[dict]:
        yield {
            "type": "status",
            "content": "Comm Agent requesting broker approval (demo mode)",
        }

        action = recommendation.get("recommendation", "HOLD")
        amount = recommendation.get("amount", 100000)
        currency_pair = recommendation.get("currency_pair", "AUD/USD")
        summary = recommendation.get("summary", "")
        reasons = recommendation.get("reasons", [])

        approval_text = (
            f"Requesting approval to {action} {amount:,.0f} {currency_pair}. "
            f"Summary: {summary}. "
            f"Reasons: {'; '.join(reasons)}."
        )

        yield {
            "type": "tool_call",
            "content": "Calling tool: request_broker_approval",
            "tool": "request_broker_approval",
        }

        if action == "HOLD":
            yield {
                "type": "tool_result",
                "content": "No approval required for HOLD recommendation",
                "tool": "request_broker_approval",
                "result": json.dumps({"decision": "not_required", "action": "HOLD"}),
            }
            yield {
                "type": "approval_result",
                "content": "No broker approval required (HOLD)",
                "decision": "not_required",
                "recommendation": recommendation,
            }
            return

        # For BUY/SELL – create a real pending approval that the UI can respond to
        result_json = await COMM_TOOL_HANDLERS["request_broker_approval"](
            recommendation=approval_text,
            action=action.lower(),
            amount=float(amount),
            currency_pair=currency_pair,
            timeout_seconds=300,
        )
        result = json.loads(result_json)

        yield {
            "type": "tool_result",
            "content": f"Broker decision: {result.get('decision')}",
            "tool": "request_broker_approval",
            "result": result_json,
        }
        yield {
            "type": "approval_result",
            "content": f"Broker {result.get('decision', 'pending')}",
            "decision": result.get("decision"),
            "approval_id": result.get("approval_id"),
            "recommendation": recommendation,
        }

    async def _run_demo_notification(
        self, trade_result: dict
    ) -> AsyncIterator[dict]:
        yield {
            "type": "status",
            "content": "Comm Agent sending customer notification (demo mode)",
        }
        yield {
            "type": "tool_call",
            "content": "Calling tool: send_customer_notification",
            "tool": "send_customer_notification",
        }

        action = trade_result.get("action", "executed")
        amount = trade_result.get("amount", 0)
        rate = trade_result.get("rate", 0)
        currency_pair = trade_result.get("currency_pair", "AUD/USD")

        subject = f"Trade Alert: {action.upper()} {amount:,.0f} {currency_pair}"
        message = (
            f"Dear Customer,\n\n"
            f"We have executed a {action.upper()} trade on your behalf:\n"
            f"  Pair:   {currency_pair}\n"
            f"  Amount: {amount:,.0f}\n"
            f"  Rate:   {rate:.4f}\n\n"
            f"This trade was executed following thorough market analysis and with "
            f"broker approval.\n\nKind regards,\nFX Desk"
        )

        result_json = await COMM_TOOL_HANDLERS["send_customer_notification"](
            subject=subject,
            message=message,
            notification_type="trade_alert",
        )
        result = json.loads(result_json)

        yield {
            "type": "tool_result",
            "content": f"Notification sent: {subject}",
            "tool": "send_customer_notification",
            "result": result_json,
        }
        yield {
            "type": "result",
            "content": f"Customer notification sent successfully (ID: {result.get('notification_id')})",
        }
