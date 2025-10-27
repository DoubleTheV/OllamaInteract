using System;
using System.Collections.Generic;
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
    private readonly IDatabaseManager _dbManager;
    private readonly ServerManager _serverManager;

    public MainWindowViewModel(IOllamaApiClient ollamaClient, IConfigManager configManager, IDatabaseManager dbManager,ServerManager serverManager)
    {
        _ollamaClient = ollamaClient;
        _configService = configManager;
        _dbManager = dbManager;
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
    private ObservableCollection<AvailableModel> _availableModels = new ObservableCollection<AvailableModel>();
    public ObservableCollection<AvailableModel> AvailableModels
    {
        get => _availableModels;
        set
        {
            if (_availableModels != value)
            {
                _availableModels = value;
                OnPropertyChanged(nameof(AvailableModels));
            }
        }
    }

    private AvailableModel _selectedModel = new AvailableModel() {Name="None"};
    public AvailableModel SelectedModel
    {
        get => _selectedModel;
        set
        {
            _selectedModel = value;
            OnPropertyChanged(nameof(SelectedModel));
        }
    }

    private ObservableCollection<Conversation> _conversations = new ObservableCollection<Conversation>();
    public ObservableCollection<Conversation> Conversations
    {
        get => _conversations;
        set
        {
            _conversations = value;
            OnPropertyChanged(nameof(Conversations));
        }
    }
    private Conversation _selectedConversation = new Conversation(0);
    public Conversation SelectedConversation
    {
        get => _selectedConversation;
        set
        {
            _selectedConversation = value;
            ChatHistory = new ObservableCollection<ChatMessage>(SelectedConversation.Messages);
            OnPropertyChanged(nameof(ChatHistory));
            OnPropertyChanged(nameof(SelectedConversation));
        }
    }


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

            await GetAvailableModelsAsync();

            if (AvailableModels.Count > 0)
            {
                SelectedModel = AvailableModels.First();
                StatusMessage = "Loaded available models";
            }

            GetConversations();
            if (Conversations.Count > 0)
            {
                SelectedConversation = Conversations.First();
                StatusMessage = "Loaded conversations";
            }
            else
            {
                StatusMessage = "There were no saved conversations";    
            }

            await Task.Delay(1000);

            StatusMessage = "Successfully initialized";
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

    private async Task GetAvailableModelsAsync()
    {
        StatusMessage = "Requesting available models";

        try
        {
            var models = await _ollamaClient.GetAvailableModelsAsync();

            AvailableModels = new ObservableCollection<AvailableModel>(models);
        }
        catch (Exception e)
        {
            StatusMessage = $"Error when getting available models: {e.Message}";
        }
    }

    private void GetConversations()
    {
        StatusMessage = "Getting conversations";

        try
        {
            var convos = _dbManager.Conversations;

            Conversations = new ObservableCollection<Conversation>(convos);
        }
        catch (Exception e)
        {
            StatusMessage = $"Error when getting conversations: {e.Message}";
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
            var request = new ChatRequest();
            request.Content = userMessage;
            request.Model = SelectedModel.Name;
            request.Messages = ChatHistory.ToArray();

            var response = _ollamaClient.SendChatAsync(request);

            ChatHistory.Add(new ChatMessage(request));

            await response;
            if (response.IsCompletedSuccessfully)
            {
                ChatHistory.Add(response.Result);
            }

            _dbManager.UpdateConversation(SelectedConversation.ID, convo =>
            {
                convo.Messages = new List<ChatMessage>(ChatHistory);
            });
        }
        catch (Exception e)
        {
            StatusMessage = $"Error occured when sending / recieving a message: {e.Message}";
        }
    }

    [RelayCommand]
    public void AddConversation()
    {
        try
        {
            var newID = (uint)Conversations.Count;
            Conversations.Add(new Conversation(newID));
            _selectedConversation = Conversations.Last();
            _dbManager.UpdateConversation(newID, convo => new Conversation(newID));
        }
        catch (Exception e)
        {
            StatusMessage = $"Error occured when creating a new conversation: {e.Message}";
        }
    }
}
