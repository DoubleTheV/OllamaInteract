using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaInteract.Core;
using OllamaInteract.GUI.ViewModels;

namespace OllamaInteract.GUI;

public static class AppBootstrapper
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    public static void ConfigureServices()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddCoreServices();

                services.AddTransient<MainWindowViewModel>();
            })
        .Build();

        ServiceProvider = host.Services;
    }
}