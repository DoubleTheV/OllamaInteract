using System.Text.Json.Serialization;

namespace OllamaInteract.Core.Models;

public class AppConfig
{
    [JsonPropertyName("python_run_on_startup")]
    public bool PythonRunOnStartup { get; set; } = true;

    [JsonPropertyName("python_server_directory")] // relative to .exe // release primary and fallback to production
    public string[] PythonServerDirectory { get; set; } = {"/Python/", "../OllamaInteract.PythonServer/"};

    [JsonPropertyName("python_executable")]
    public string PythonExectuable { get; set; } = "python";

    [JsonPropertyName("python_server_host")]
    public string PythonHost { get; set; } = "http://localhost";

    [JsonPropertyName("python_server_port")]
    public int PythonPort { get; set; } = 8000;

    [JsonPropertyName("ollama_host")]
    public string OllamaHost { get; set; } = "http://localhost";

    [JsonPropertyName("ollama_port")]
    public int OllamaPort { get; set; } = 11434;

}