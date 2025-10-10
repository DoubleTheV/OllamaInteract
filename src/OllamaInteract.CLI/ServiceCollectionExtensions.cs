using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using OllamaInteract.CLI.Services;

namespace OllamaInteract.CLI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCliServices(this IServiceCollection services)
    {
        services.AddTransient<CLIApplication>();

        return services;
    }
}