using OllamaInteract.Core.Models;
using OllamaInteract.Core.Services;

Console.WriteLine("OllamaInteract test CLI");
Console.WriteLine("=======================");

var httpClient = new HttpClient();
var ollamaClient = new OllamaApiClient(httpClient);

try
{
    Console.WriteLine($"Server healthy? - {await ollamaClient.IsServerHealthyAsync()}");
    Console.WriteLine("Available Models:");
    var models = await ollamaClient.GetAvailableModelsAsync();
    foreach(var m in models)
    {
        Console.WriteLine($"Name: {m.Name}; ParameterS: {m.ParameterS};");
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