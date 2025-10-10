using OllamaInteract.Core.Services;

namespace OllamaInteract.CLI.Services;

public class CLIApplication
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigManager _configManager;
    private readonly IOllamaApiClient _ollamaClient;
    private readonly ServerManager _serverManager;

    public CLIApplication(
        IServiceProvider serviceProvider,
        IConfigManager configManager,
        IOllamaApiClient ollamaClient,
        ServerManager serverManager
    )
    {
        _serviceProvider = serviceProvider;
        _configManager = configManager;
        _ollamaClient = ollamaClient;
        _serverManager = serverManager;
    }

    public async Task RunAsync(string[] args)
    {
        Console.WriteLine("CLI Application started successfully.");
    }
}