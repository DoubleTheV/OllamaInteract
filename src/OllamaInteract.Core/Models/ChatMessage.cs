using System.Text.Json.Serialization;

namespace OllamaInteract.Core.Models;

public class ChatMessage
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    [JsonIgnore]
    public string TimeStamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    [JsonPropertyName("role")]
    public string Role { get; protected set; } = "system";

    public ChatMessage() { }
    
    public ChatMessage(string content, string role, string timestamp)
    {
        Content = content;
        Role = role;
        TimeStamp = timestamp;
    }

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
    [JsonPropertyName("messages")]
    public ChatMessage[] Messages { get; set; } = [];

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