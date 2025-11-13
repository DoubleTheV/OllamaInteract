using OllamaInteract.Core.Models;
using OllamaInteract.Core.Services;

Console.WriteLine("OllamaInteract test CLI");
Console.WriteLine("=======================");

var httpClient = new HttpClient();
var ollamaClient = new OllamaApiClient(httpClient);

var configManager = new ConfigManager();
var dbManager = new DatabaseManager();

var serverManager = new ServerManager(configManager, ollamaClient);

try
{
    configManager.ResetToDefault();
    Console.WriteLine($"Server running? - {await serverManager.EnsureServerRunningAsync()}");
    Console.WriteLine($"Server healthy? - {await ollamaClient.IsServerHealthyAsync()}");
    Console.WriteLine("Available Models:");
    var models = await ollamaClient.GetAvailableModelsAsync();
    foreach (var m in models)
    {
        Console.WriteLine($"    Name: {m.Name}; ParameterS: {m.ParameterS};");
    }
    var request = new ChatRequest();
    var answer = new ChatResponse();    
    if (models.Count >= 1)
    {
        Console.WriteLine("Communication test:");
        request.Content = "Hi! I'm testing communication! Please answer shortly!";
        request.Model = models[0].Name;

        answer = await ollamaClient.SendChatAsync(request);
        Console.WriteLine($"    TestMessage: {request.Content} \n   Answer: {answer.Content} \n    Time elapsed: {answer.ResponseTime / 1000}s");
    }
    Console.WriteLine("Config check");
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

    var searchedModels = await ollamaClient.SearchModelsAsync("minimax-m2", new CancellationToken());
    foreach (var m in searchedModels)
    {
        Console.WriteLine(m.Name);
        foreach (var ParameterS in m.ParameterS)
        {
            Console.WriteLine($"    {ParameterS}");
        }
    }

    var pulled = await ollamaClient.PullOllamaModelAsync("qwen3:0.6b");
    Console.WriteLine($"Pulling qwen3:0.6b result: {pulled}");


    serverManager.Dispose();

    Console.WriteLine("=======================");
    Console.WriteLine("Test Completed Successfully!");
}
catch (Exception ex)
{
    Console.WriteLine("=======================");
    Console.WriteLine($"Exception: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
}