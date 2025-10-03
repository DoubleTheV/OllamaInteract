from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.config import config
from app.api.routes import router as api_router

def create_app() -> FastAPI:
    app = FastAPI(
        title="Ollama Bridge Server",
        description="HTTP bridge between C# and Ollama",
        version="0.1.0"
    )

    app.add_middleware(
        CORSMiddleware,
        allow_origins=["localhost"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"]
    )

    app.include_router(api_router, prefix="/api/v1")

    return app

app = create_app()

@app.get("/")
async def root():
    return {
        "message": "Ollama Bridge Server is running",
        "health": "/health"
    }

@app.get("/health")
async def health_check():
    return {"status": "healthy", "service": "ollama-bridge"}