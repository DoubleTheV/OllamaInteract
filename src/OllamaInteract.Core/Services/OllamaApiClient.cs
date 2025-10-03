using System.Collections.Generic;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using OllamaInteract.Core.Models;

namespace OllamaInteract.Core.Services;

public class OllamaApiClient : IOllamaApiClient
{
    private readonly HttpClient _httpClient;

    private ConfigManager _configManager = new ConfigManager();

    public OllamaApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AvailableModel>> GetAvailableModelsAsync()
    {
        var response = await _httpClient.GetAsync($"{_configManager.Config.PythonHost}:{_configManager.Config.PythonPort}/api/v1/models");
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


    public async Task<ChatResponse> SendChatAsync(ChatRequest chatRequest)
    {
        try
        {
            var startTime = DateTime.Now;

            var response = await _httpClient.PostAsJsonAsync($"{_configManager.Config.PythonHost}:{_configManager.Config.PythonPort}/api/v1/chat", chatRequest);

            if (response.IsSuccessStatusCode)
            {
                var chatResponse = await response.Content.ReadFromJsonAsync<ChatResponse>();
                if (chatResponse == null)
                {
                    chatResponse = new ChatResponse { Success = false, Error = "Invalid response format" };
                }
                chatResponse.ResponseTime = (long)(DateTime.Now - startTime).TotalMilliseconds;

                return chatResponse;
            }
            else
            {
                return new ChatResponse
                {
                    Success = false,
                    Error = $"HTTP error: {response.StatusCode}"
                };
            }
        }
        catch (Exception e)
        {
            return new ChatResponse
            {
                Success = false,
                Error = $"Communication error: {e.Message}"
            };
        }
    }

    
    public async Task<bool> IsServerHealthyAsync()
    {
        var response = await _httpClient.GetAsync($"http://{_configManager.Config.PythonHost}:{_configManager.Config.PythonPort}/health");

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