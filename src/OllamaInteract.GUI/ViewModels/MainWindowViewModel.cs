using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OllamaInteract.Core.Services;

namespace OllamaInteract.GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IOllamaApiClient _ollamaClient;
    private readonly IConfigManager _configService;
    private readonly ServerManager _serverManager;

    public MainWindowViewModel(IOllamaApiClient ollamaClient, IConfigManager configManager, ServerManager serverManager)
    {
        _ollamaClient = ollamaClient;
        _configService = configManager;
        _serverManager = serverManager;

        _ = InitializeAsync();
    }

    [ObservableProperty]
    private bool _isConnected = false;
    [ObservableProperty]
    private string _statusMessage = string.Empty;


    private async Task InitializeAsync()
    {
        StatusMessage = "Initializing";

        try
        {
            await ConnectToServerAsync();

            if (IsConnected)
            {
                StatusMessage = "Successfully connected to Ollama";
            }
        }
        catch (Exception e)
        {
            StatusMessage = $"Initialization failed: {e.Message}";
        }
    }
    
    private async Task ConnectToServerAsync()
    {
        StatusMessage = "Connecting to server";

        try
        {
            IsConnected = await _serverManager.EnsureServerRunningAsync();

            if (IsConnected)
            {
                StatusMessage = "Connected to Python bridge-server";
            }
            else
            {
                StatusMessage = "Failed to connect to Python bridge-server";
            }
        }
        catch (Exception e)
        {
            StatusMessage = $"Error when connecting to python bridge server: {e.Message}";
            IsConnected = false;
        }


    }
}
