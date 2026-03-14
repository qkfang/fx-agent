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

    # Backend service base URLs (the existing .NET services)
    broker_backoffice_url: str = "http://localhost:5001"
    trading_platform_url: str = "http://localhost:5000"
    news_feed_url: str = "http://localhost:5002"

    # Persisted Azure AI Foundry agent IDs (populated on first run)
    analysis_agent_id: Optional[str] = None
    trader_agent_id: Optional[str] = None
    comm_agent_id: Optional[str] = None

    model_config = {"env_file": ".env", "env_file_encoding": "utf-8"}


settings = Settings()
