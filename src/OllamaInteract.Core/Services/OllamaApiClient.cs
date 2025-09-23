using System.Collections.Generic;
using System.Net.Http.Json;
using OllamaInteract.Core.Models;

namespace OllamaInteract.Core.Services;

public class OllamaApiClient : IOllamaApiClient
{
    private readonly HttpClient _httpClient;

    public OllamaApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AvailableModel>> GetAvailableModelsAsync()
    {
        await Task.Delay(1000);
        return new List<AvailableModel>();
    }

    /*
    public Task<ChatResponse> SendChatAsync(ChatRequest chatRequest)
    {
    }

    public Task<bool> IsServerHealthyAsync()
    {
    }
    */
}