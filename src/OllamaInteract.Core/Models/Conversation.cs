namespace OllamaInteract.Core.Models;

public class Conversation
{
    public uint? ID { get; private set; } = null;
    public string Name { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    public Conversation()
    {
        Name = "New conversation";
    }

    public Conversation(uint? id)
    {
        ID = id;
    }
}