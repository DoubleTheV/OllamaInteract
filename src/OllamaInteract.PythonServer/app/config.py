### Future to add env variable config, allowing for scaled up utilization

class Config:
    ### Configuration settings for Ollama ###
    OLLAMA_HOST = 'http://localhost:11434'
    SERVER_HOST = '0.0.0.0'
    SERVER_PORT = '8000'
    DEBUG = 'true'

config = Config()