namespace OllamaInteract.Core.Models;

public class Conversation
{
    private  uint? ID { get; set; } = null;
    public string Name { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public string Model { get; set; } = string.Empty;
}