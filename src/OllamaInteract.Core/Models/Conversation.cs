namespace OllamaInteract.Core.Models;

public class Conversation
{
    public uint ID { get; private set; }
    public string Name { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    public Conversation(uint id)
    {
        ID = id;
        Name = "New conversation";
    }
}