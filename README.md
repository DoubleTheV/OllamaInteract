# Ollama Interact

Ollama Interact is a cross-platform CLI / Desktop app for interaction, management and memory of [Ollama](https://github.com/ollama/ollama) models.

## Fatures

- Chatting with AI models through an CLI or Desktop app.
- Memory creation and retention about user.
- Easy customization of the models.

## Requirements

- [Ollama](https://github.com/ollama/ollama)
- [.NET](https://dotnet.microsoft.com/en-us/download)

## Running the app

**Option 1 (the easier one):**

Download the [release](https://github.com/DoubleTheV/OllamaInteract/releases) (on the right of the current website).

**Option 2 (if the release you are looking for is not listed):**

Use [git](https://git-scm.com/downloads) to clone the repository and build the project using [dotnet](https://dotnet.microsoft.com/en-us/download).

```bash
git clone https://github.com/DoubleTheV/OllamaInteract.git

## In terminal 1: # It will run python server
# Go into the cloned folder
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
cd src/OllamaInteract.PythonServer
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000

## In terminal 2: # It will build and run the client app
# Go into the cloned folder

cd OllamaInteract/src/OllamaInteract.CLI # for CLI

cd OllamaInteract/src/OllamaInteract.GUI # for GUI

dotnet build
# Run the .exe in the /bin/Debug/net9.0

# or

dotnet run
```

## Contributing

See [CONTRIBUTING.md](https://github.com/DoubleTheV/OllamaInteract/blob/master/docs/CONTRIBUTING.md) for details

## License

[MPL-2.0](https://choosealicense.com/licenses/mpl-2.0)
