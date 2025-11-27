namespace OllamaInteract.Core.Models;

public class AvailableModelsResponse : IResponse
{
    public bool Success { get; set; }
    public long ResponseTime { get; set; }
    public string Error { get; set; } = string.Empty;
    public List<AvailableModel> Models { get; set; } = new List<AvailableModel>();
}

public class AvailableModel
{
    public string Name { get; set; } = string.Empty;
    public string[] ParameterS { get; set; } = new string[1];
}