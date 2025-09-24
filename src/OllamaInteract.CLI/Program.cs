using OllamaInteract.Core.Models;
using OllamaInteract.Core.Services;

Console.WriteLine("OllamaInteract test CLI");
Console.WriteLine("=======================");

var httpClient = new HttpClient();
var ollamaClient = new OllamaApiClient(httpClient);

try
{
    Console.WriteLine($"Server healthy? - {await ollamaClient.IsServerHealthyAsync()}");
    Console.WriteLine("=======================");
    Console.WriteLine("Test Completed Successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
}