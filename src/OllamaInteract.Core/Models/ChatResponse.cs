namespace OllamaInteract.Core.Models;

public class ChatResponse
{
    public bool Success { get; set; }
    public string Response { get; set; } = string.Empty;
    public long ResponseTime { get; set; }
    public string Error { get; set; } = string.Empty;
}