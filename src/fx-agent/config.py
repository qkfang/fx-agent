"""
Configuration settings loaded from environment variables / .env file.
"""
from __future__ import annotations

from typing import Optional
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    # Azure AI Foundry – set AZURE_AI_CONNECTION_STRING to enable live agents.
    # Format: "endpoint=https://…;subscription_id=…;resource_group=…;project_name=…"
    azure_ai_connection_string: Optional[str] = None
    azure_ai_model: str = "gpt-4o"

    # Backend service base URLs (the existing .NET services).
    # Ports match the default launchSettings.json profiles for each service:
    #   broker-backoffice  → http://localhost:5269
    #   research-analytics → http://localhost:5003
    #   news-feed          → http://localhost:5142
    #   trading-platform   → http://localhost:5249
    broker_backoffice_url: str = "http://localhost:5269"
    research_analytics_url: str = "http://localhost:5003"
    news_feed_url: str = "http://localhost:5142"
    trading_platform_url: str = "http://localhost:5249"

    # Persisted Azure AI Foundry agent IDs (populated on first run)
    analysis_agent_id: Optional[str] = None
    trader_agent_id: Optional[str] = None
    comm_agent_id: Optional[str] = None

    model_config = {"env_file": ".env", "env_file_encoding": "utf-8"}


settings = Settings()

# ── Scenario keys ─────────────────────────────────────────────────────────────
# Supported scenario identifiers for the E2E simulation workflow.
SCENARIO_MIDDLE_EAST_WAR = "middle_east_war"
