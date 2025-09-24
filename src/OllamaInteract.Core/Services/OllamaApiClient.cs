using System.Collections.Generic;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
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
        var response = await _httpClient.GetAsync("http://localhost:8000/api/v1/models");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadFromJsonAsync<AvailableModelsResponse>();
            if(json != null)
            {
                return json.Models;
            }
        }
        return new List<AvailableModel>();
    }

    /*
    public Task<ChatResponse> SendChatAsync(ChatRequest chatRequest)
    {
    }

    */
    public async Task<bool> IsServerHealthyAsync()
    {
        var response = await _httpClient.GetAsync("http://localhost:8000/health");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadFromJsonAsync<ServerHealthyCheck>();
            if(json != null && json.Status == "healthy")
            {
                return true;
            }
        }
        return false;
    }
}