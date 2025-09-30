using System.Text.Json.Serialization;

namespace OllamaInteract.Core.Models;

public class AppConfig
{
    [JsonPropertyName("python_server_port")]
    public int PythonServerPort = 8000;
    [JsonPropertyName("ollama_host")]
    public string OllamaHost = "localhost:11434";
    
}