using System.Text.Json.Serialization;

namespace OllamaInteract.Core.Models;

public class AppConfig
{
    [JsonPropertyName("python_server_host")]
    public string PythonHost {get; set; }= "http://localhost";

    [JsonPropertyName("python_server_port")]
    public int PythonPort {get; set; }= 8000;

    [JsonPropertyName("ollama_host")]
    public string OllamaHost {get; set; }= "http://localhost";

    [JsonPropertyName("ollama_port")]
    public int OllamaPort {get; set; }= 8000;
    
}