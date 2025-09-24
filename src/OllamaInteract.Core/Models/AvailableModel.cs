namespace OllamaInteract.Core.Models;

public class AvailableModelsResponse
{
    public bool Success { get; set; }
    public List<AvailableModel> Models { get; set; } = new List<AvailableModel>();
    public string? Error { get; set; }
}

public class AvailableModel
{
    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
}