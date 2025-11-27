namespace OllamaInteract.Core.Models;

public interface IResponse
{
    public bool Success { get; set; }
    public long ResponseTime { get; set; }
    public string Error { get; set; }
}

public class Response
{
    public bool Success { get; set; }
    public long ResponseTime { get; set; }
    public string Error { get; set; } = string.Empty;
}