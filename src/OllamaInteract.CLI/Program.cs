using OllamaInteract.Core.Models;
using OllamaInteract.Core.Services;

Console.WriteLine("OllamaInteract test CLI");
Console.WriteLine("=======================");

var httpClient = new HttpClient();
var ollamaClient = new OllamaApiClient(httpClient);

var configManager = new ConfigManager();

try
{
    Console.WriteLine($"Server healthy? - {await ollamaClient.IsServerHealthyAsync()}");
    Console.WriteLine("Available Models:");
    var models = await ollamaClient.GetAvailableModelsAsync();
    foreach (var m in models)
    {
        Console.WriteLine($"    Name: {m.Name}; ParameterS: {m.ParameterS};");
    }
    if (models.Count >= 1)
    {
        Console.WriteLine("Communication test:");
        var request = new ChatRequest();
        request.Message = "Hi! I'm testing communication! Please answer shortly!";
        request.Model = models[0].Name;

        var answer = await ollamaClient.SendChatAsync(request);
        Console.WriteLine($"    TestMessage: {request.Message} \n   Answer: {answer.Response} \n    Time elapsed: {answer.ResponseTime / 1000}s");
    }
    Console.WriteLine("Config check");
    try
    {
        Console.WriteLine($"    Config: {configManager.Config}");
        configManager.UpdateConfig(c => c.OllamaPort = 696969);
        Console.WriteLine($"    Update check: {configManager.Config.OllamaPort == 696969}");
        configManager.SaveConfig();
        configManager.LoadConfig();
        Console.WriteLine($"    Update check: {configManager.Config.OllamaPort == 696969}");
        configManager.ResetToDefault();
        Console.WriteLine($"    Update check: {configManager.Config.OllamaPort != 696969}");
        configManager.SaveConfig();
        configManager.LoadConfig();
        Console.WriteLine($"    Update check: {configManager.Config.OllamaPort != 696969}");
        configManager.SaveConfig();

    }
    catch (Exception e)
    {
        Console.WriteLine($"    Config check error: {e.Message}");
    }

    Console.WriteLine("=======================");
    Console.WriteLine("Test Completed Successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("=======================");
        Console.WriteLine($"Exception: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
    }