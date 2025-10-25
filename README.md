# Ollama Interact

<div align="center" style="display: flex; flex-direction: row; align-items: center; gap: 10px;">
    <a href="https://github.com/DoubleTheV/OllamaInteract/blob/main/LICENSE">
        <img src="https://img.shields.io/badge/License-MPL%202.0-brightgreen.svg" alt="License: MPL 2.0">
    </a>
    <a href="https://github.com/DoubleTheV/OllamaInteract/stargazers">
        <img src="https://img.shields.io/github/stars/DoubleTheV/OllamaInteract?style=social" alt="GitHub Stars">
    </a>
    <a href="https://github.com/DoubleTheV/OllamaInteract/network/members">
        <img src="https://img.shields.io/github/forks/DoubleTheV/OllamaInteract?style=social" alt="GitHub Forks">
    </a>
    <a href="https://github.com/DoubleTheV/OllamaInteract/issues">
        <img src="https://img.shields.io/github/issues/DoubleTheV/OllamaInteract" alt="GitHub Issues">
    </a>
    <a href="https://github.com/DoubleTheV/OllamaInteract/releases">
        <img src="https://img.shields.io/github/v/release/DoubleTheV/OllamaInteract" alt="GitHub Release">
    </a>
    <a href="https://dotnet.microsoft.com/en-us/download/dotnet/9.0">
        <img src="https://img.shields.io/badge/.NET-9.0-blueviolet" alt=".NET Version">
    </a>
</div>

## Description

Ollama Interact is a cross-platform CLI / Desktop app for interaction, management and memory of [Ollama](https://github.com/ollama/ollama) models.

## Features

- Chatting with AI models through an CLI or Desktop app.
- Memory creation and retention about user.
- Easy customization of the models.
- Cross platform application

## Requirements

- [Python](https://www.python.org/downloads/)
- [Pip](https://pypi.org/project/pip/)
- [Ollama](https://github.com/ollama/ollama)
- [.NET 9.0](https://dotnet.microsoft.com/en-us/download)
- [Git](https://git-scm.com/install/) - if building manually

## Running the app

**Option 1 (the easier one):**

Download the [release](https://github.com/DoubleTheV/OllamaInteract/releases) (on the right of the current website).

**Option 2 (if the release you are looking for is not listed):**

Use [git](https://git-scm.com/downloads) to clone the repository and build the project using [dotnet](https://dotnet.microsoft.com/en-us/download).

```bash
# Cloning the repository
git clone https://github.com/DoubleTheV/OllamaInteract.git

cd OllamaInteract/src/OllamaInteract.CLI # for CLI

cd OllamaInteract/src/OllamaInteract.GUI # for GUI

# Running the software

dotnet run

# or

dotnet build
# Run the executable in the /bin/Debug/net9.0
```

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to submit pull requests, report issues, or suggest features.

## License

This project is licensed under the [Mozilla Public License 2.0 (MPL-2.0)](LICENSE).
