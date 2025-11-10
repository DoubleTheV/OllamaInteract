from fastapi import APIRouter, HTTPException, Request
from app.services.ollama_client import OllamaClient
from app.services.ollama_scraper import OllamaScraper
from typing import Optional

router = APIRouter()
ollama_client = OllamaClient()
ollama_scraper = OllamaScraper()

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
async def models_endpoint(): ## to-do: change the structure to not repeat names, matching the Model class in C#
    try:
        awaitedModels = await ollama_client.list_models()
        temp_dict = {}
        for m in awaitedModels:
            name, parameter = m['model'].split(':')

            if name in temp_dict:
                temp_dict[name].append(parameter)
            else:
                temp_dict[name] = [parameter]

        formattedModels = []

        for name, parameters in temp_dict.items():
            formattedModels.append({
                "name": name,
                "parameters": parameters
            })

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

@router.get("/search")
async def search_models(prompt: Optional[str] = None):
    models, status_code = await ollama_scraper.scrapeModels(prompt)

    if(status_code != 200):
        raise HTTPException(
            status_code= status_code,
            detail="Searching models failed."
        )
    
    return {
        "success": True,
        "models": models,
        "error": None
    }
