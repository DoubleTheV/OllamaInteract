using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaInteract.Core.Models;
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
    [ObservableProperty]
    private string _userInput = string.Empty;

    public ObservableCollection<ChatMessage> ChatHistory { get; set; } = new ObservableCollection<ChatMessage>();

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

    [RelayCommand]
    public async Task SendMessageAsync()
    {
        if (string.IsNullOrEmpty(UserInput))
        {
            return;
        }

        var userMessage = UserInput;
        UserInput = string.Empty;

        try
        {
            var models = await _ollamaClient.GetAvailableModelsAsync();

            var request = new ChatRequest();
            request.Message = userMessage;
            request.Model = models.First().Name;

            ChatHistory.Add(request);

            var response = await _ollamaClient.SendChatAsync(request);

            if(response.Success)
            {
                ChatHistory.Add(response);
            }
        }
        catch (Exception e)
        {
            StatusMessage = $"Error occured when sending / recieving a message: {e.Message}";
        }
    }
}
