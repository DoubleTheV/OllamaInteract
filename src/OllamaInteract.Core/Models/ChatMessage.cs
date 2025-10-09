namespace OllamaInteract.Core.Models;

public class ChatMessage
{
    public string Message { get; set; } = string.Empty;
    public string TimeStamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public bool fromUser { get; set; }
}

public class ChatRequest : ChatMessage
{
    public string Model { get; set; } = string.Empty;

    public ChatRequest()
    {
        fromUser = true;
    }
}

public class ChatResponse : ChatMessage
{
    public bool Success { get; set; }
    public long ResponseTime { get; set; }
    public string Error { get; set; } = string.Empty;

    public ChatResponse()
    {
        fromUser = false;
    }

}