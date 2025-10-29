using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using OllamaInteract.GUI.ViewModels;
using OllamaInteract.GUI.Views;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using OllamaInteract.Core.Services;

namespace OllamaInteract.GUI;

public partial class App : Application
{
    private IServiceProvider? _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        AppBootstrapper.ConfigureServices();
        if (AppBootstrapper.ServiceProvider == null)
        {
            throw new NoNullAllowedException("Service provider is null");
        }
        _services = AppBootstrapper.ServiceProvider;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = _services?.GetRequiredService<MainWindowViewModel>();

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
            };

            desktop.Exit += (sender, e) =>
            {
                var serverManager = _services?.GetService<ServerManager>();
                serverManager?.Dispose();
                var dbManager = _services?.GetService<DatabaseManager>();
                dbManager?.SaveConversations();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}