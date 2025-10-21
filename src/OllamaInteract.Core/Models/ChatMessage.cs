namespace OllamaInteract.Core.Models;

public class ChatMessage
{
    public string Content { get; set; } = string.Empty;
    public string TimeStamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string Role { get; protected set; } = "system";

    public ChatMessage(){}

    public ChatMessage(ChatRequest chatRequest)
    {
        Content = chatRequest.Content;
        TimeStamp = chatRequest.TimeStamp;
        Role = chatRequest.Role;
    }
    public ChatMessage(ChatResponse chatResponse)
    {
        Content = chatResponse.Content;
        TimeStamp = chatResponse.TimeStamp;
        Role = chatResponse.Role;
    }
}

public class ChatRequest : ChatMessage
{
    public string Model { get; set; } = string.Empty;

    public ChatRequest()
    {
        Role = "user";
    }
}

public class ChatResponse : ChatMessage
{
    public bool Success { get; set; }
    public long ResponseTime { get; set; }
    public string Error { get; set; } = string.Empty;

    public ChatResponse()
    {
        Role = "assistant";
    }

}