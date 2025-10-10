using Microsoft.Extensions.DependencyInjection;
using OllamaInteract.Core;
using OllamaInteract.CLI.Services;

namespace OllamaInteract.CLI;


public class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddCliServices();

        using var serviceProvider = services.BuildServiceProvider();
        var app = serviceProvider.GetRequiredService<CLIApplication>();
        await app.RunAsync(args);
    }
}