using Microsoft.Extensions.DependencyInjection;
using OllamaInteract.Core.Services;

namespace OllamaInteract.Core;

public static class ServiceCollection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddHttpClient<IOllamaApiClient, OllamaApiClient>();
        services.AddSingleton<IConfigManager, ConfigManager>();
        services.AddSingleton<ServerManager>();
        services.AddSingleton<IDatabaseManager, DatabaseManager>();

        return services;
    }
}