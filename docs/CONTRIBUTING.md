# Contributing

## Workflow

1. Open an issue or use a existing one
2. Fork repository
2. Create (fix/feature/docs)/thing branch
3. Make changes
4. Submit PR

## Branch naming

- fix/something - for fixing bugs
- feature/something - for adding new features
- docs/something - for working on docs directory
- refactor/something - for code structure changes with no or minimal impact on functionality

## Development

```bash
## In terminal 1: # It will run python server
# Go into the cloned folder
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
cd src/OllamaInteract.PythonServer
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000 ## it will hotreload python changes

## In terminal 2: # It will build and run the client app
# Go into the cloned folder
cd OllamaInteract/src/OllamaInteract.CLI # for CLI

cd OllamaInteract/src/OllamaInteract.GUI # for GUI

dotnet watch # it will hot reload C# changes
```