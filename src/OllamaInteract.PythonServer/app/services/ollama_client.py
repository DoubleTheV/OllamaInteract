import ollama
from typing import List, Dict, Any
import logging

logger = logging.getLogger(__name__)

class OllamaClient:
    def __init__(self):
        self.client = ollama.Client()
        self._test_connection()
    
    def _test_connection(self):
        try:
            ollama.list() ## Check if ollama responds
            logger.info("Successfully connected to Ollama")
        except Exception as e:
            logger.error(f"Failed to connect to Ollama: {e}")
            raise ConnectionError(f"Ollama server not available: {e}")
    
    async def chat(self, message: str, model: str) -> str:
        response = self.client.chat(
            model=model,
            messages=[{"role": "user", "content": message}]
        )
        return response['message']['content']

    async def generate(self, prompt: str, model: str) -> str:
        response = self.client.generate(model=model, prompt=prompt)
        return response['response']

    async def list_models(self) -> List[Dict[str, Any]]:
        response = self.client.list()
        return response['models']