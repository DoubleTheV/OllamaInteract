using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaInteract.Core.Models;

namespace OllamaInteract.Core.Services;

public interface IOllamaApiClient
{
    Task<List<AvailableModel>> GetAvailableModelsAsync();
    // Task<ChatResponse> SendChatAsync(ChatRequest request);
    Task<bool> IsServerHealthyAsync();
}