"""
FX Agent Service – FastAPI entry point.

Endpoints:
  GET  /                        → serve static index.html
  GET  /api/status              → service health + current config
  POST /api/workflow/run        → trigger a full agent workflow
  GET  /api/workflow/history    → past run summaries
  GET  /api/approvals           → pending broker approvals
  POST /api/approvals/{id}      → broker approve / reject
  GET  /api/notifications       → sent customer notifications
  GET  /api/fx/rate             → proxy current FX rate
  WS   /ws                      → real-time agent execution stream
"""
from __future__ import annotations

import asyncio
import logging
from pathlib import Path

from fastapi import FastAPI, HTTPException, WebSocket, WebSocketDisconnect
from fastapi.responses import FileResponse, JSONResponse
from fastapi.staticfiles import StaticFiles
from pydantic import BaseModel

from agents.orchestrator import Orchestrator
from config import settings
from tools.comm_tools import (
    list_notifications,
    list_pending_approvals,
    respond_to_approval,
)
from tools.market_tools import get_fx_rate

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(name)s: %(message)s")
logger = logging.getLogger(__name__)

app = FastAPI(title="FX Agent Service", version="1.0.0")

# Single global orchestrator instance
orchestrator = Orchestrator()

# ── Static files ─────────────────────────────────────────────────────────────
STATIC_DIR = Path(__file__).parent / "static"
app.mount("/static", StaticFiles(directory=str(STATIC_DIR)), name="static")


@app.get("/", include_in_schema=False)
async def index():
    return FileResponse(str(STATIC_DIR / "index.html"))


# ── Health / status ───────────────────────────────────────────────────────────

@app.get("/api/status")
async def get_status():
    return {
        "service": "fx-agent",
        "version": "1.0.0",
        "mode": "azure-ai-foundry" if settings.azure_ai_connection_string else "demo",
        "model": settings.azure_ai_model,
        "broker_backoffice_url": settings.broker_backoffice_url,
        "trading_platform_url": settings.trading_platform_url,
        "news_feed_url": settings.news_feed_url,
        "workflow_running": orchestrator.running,
    }


# ── Workflow ──────────────────────────────────────────────────────────────────

@app.post("/api/workflow/run")
async def run_workflow():
    if orchestrator.running:
        raise HTTPException(status_code=409, detail="A workflow is already running")
    # Fire-and-forget so HTTP response returns immediately
    asyncio.create_task(orchestrator.run_workflow())
    return {"status": "started", "message": "Workflow started – connect to /ws for live events"}


@app.get("/api/workflow/history")
async def workflow_history():
    return {"runs": orchestrator.run_history}


# ── Broker approvals ──────────────────────────────────────────────────────────

@app.get("/api/approvals")
async def get_approvals():
    return {"approvals": list_pending_approvals()}


class ApprovalResponse(BaseModel):
    decision: str  # "approve" or "reject"
    notes: str = ""


@app.post("/api/approvals/{approval_id}")
async def respond_approval(approval_id: str, body: ApprovalResponse):
    if body.decision not in ("approve", "reject"):
        raise HTTPException(status_code=400, detail="decision must be 'approve' or 'reject'")
    ok = respond_to_approval(approval_id, body.decision, body.notes)
    if not ok:
        raise HTTPException(status_code=404, detail="Approval not found or already decided")
    return {"status": "ok", "approval_id": approval_id, "decision": body.decision}


# ── Customer notifications ────────────────────────────────────────────────────

@app.get("/api/notifications")
async def get_notifications():
    return {"notifications": list_notifications()}


# ── FX rate proxy ─────────────────────────────────────────────────────────────

@app.get("/api/fx/rate")
async def fx_rate():
    import json
    raw = await get_fx_rate()
    return JSONResponse(content=json.loads(raw))


# ── WebSocket – real-time event stream ───────────────────────────────────────

@app.websocket("/ws")
async def websocket_endpoint(ws: WebSocket):
    await ws.accept()
    conn_id, queue = orchestrator.subscribe()
    logger.info("WebSocket connected: %s", conn_id)
    try:
        while True:
            # Wait for the next event (put_nowait by orchestrator._broadcast)
            msg = await asyncio.wait_for(queue.get(), timeout=30)
            if msg is None:
                break
            await ws.send_text(msg)
    except asyncio.TimeoutError:
        # Send a keepalive ping
        try:
            await ws.send_text('{"type":"ping"}')
        except Exception as ping_exc:
            logger.debug("WebSocket ping failed (%s): %s", conn_id, ping_exc)
    except WebSocketDisconnect:
        pass
    except Exception as exc:
        logger.warning("WebSocket error (%s): %s", conn_id, exc)
    finally:
        orchestrator.unsubscribe(conn_id)
        logger.info("WebSocket disconnected: %s", conn_id)


# ── Entry point ───────────────────────────────────────────────────────────────

if __name__ == "__main__":
    import uvicorn

    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
