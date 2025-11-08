import httpx
from bs4 import BeautifulSoup
import logging

logger = logging.getLogger(__name__)

class OllamaScraper:
    def __init__(self):
        self.client = httpx.AsyncClient()

    async def scrapeModels(self, query):
        response = await self.client.get(f"https://ollama.com/search?q={query}")
        if(response.status_code != 200):
            logger.error()
            return None, response.status_code

        soup = BeautifulSoup(response.text, 'html.parser')
    
        models = []
        for item in soup.select('div#searchresults ul li'):
            parameter_sizes = []

            spans = item.select('a > div:nth-of-type(2) > div > span')
            for span in spans:                    
                if(span.text.strip()[0].isdigit() or span.text.strip() == "cloud"):
                    parameter_sizes.append(span.text)
            if len(parameter_sizes) == 0 or (len(parameter_sizes) != 1 and parameter_sizes[0] != "cloud"):
                if(len(parameter_sizes) != 0 and parameter_sizes[0] == "cloud"):
                    parameter_sizes.pop(0)
                parameter_sizes.append("latest")
                models.append({
                    "name": item.select_one('a > div > h2 > span').text,
                    "parameters": parameter_sizes
                })

        return models, response.status_code