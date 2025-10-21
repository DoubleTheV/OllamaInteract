from fastapi import APIRouter, HTTPException, Request
from app.services.ollama_client import OllamaClient

router = APIRouter()
ollama_client = OllamaClient()

@router.post("/chat")
async def chat_endpoint(request: Request):
    try:
        body = await request.json()
        response = await ollama_client.chat(body.get('content'), body.get('model'), body.get('role'), body.get('messages'))
        return {
            "success": True,
            "model": body.get('model'),
            "content": response,
            "error": None
        }
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Ollama communication failed: {str(e)}"
        )

@router.get("/models")
async def models_endpoint():
    try:
        awaitedModels = await ollama_client.list_models()
        formattedModels = [ # format them to match Model class in C#               
            {
                "name": m['model'],
                "parameters": m['details']['parameter_size']
            }
            for m in awaitedModels
        ]
        return {
            "success": True,
            "models": formattedModels,
            "error": None
        }
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Failed to fetch models: {str(e)}"
        )

@router.get("/generate")
async def generate_endopoint(prompt: str, model: str):
    try:
        response = await ollama_client.generate(prompt, model)
        return {
            "success": True,
            "model": model,
            "response": response,
            "error": None
        }
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Generation failed: {str(e)}"
        )