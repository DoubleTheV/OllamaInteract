using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaInteract.Core.Models;
using OllamaInteract.Core.Services;

namespace OllamaInteract.GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // Services

    private readonly IOllamaApiClient _ollamaClient;
    private readonly IConfigManager _configService;
    private readonly IDatabaseManager _dbManager;
    private readonly ServerManager _serverManager;

    // Debugging and accessibility

    [ObservableProperty]
    private bool _isConnected = false;
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    // Chatting
    
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

    private ObservableCollection<AvailableModel> _searchedModels = new ObservableCollection<AvailableModel>();
    public ObservableCollection<AvailableModel> SearchedModels
    {
        get => _searchedModels;
        set
        {
            if (_searchedModels != value)
            {
                _searchedModels = value;
                OnPropertyChanged(nameof(SearchedModels));
            }
        }
    }

    // Selected Objects

    private AvailableModel _selectedModel = new AvailableModel() { Name = "None" };
    public AvailableModel SelectedModel
    {
        get => _selectedModel;
        set
        {
            _selectedModel = value;
            OnPropertyChanged(nameof(SelectedModel));
            SelectedParameter = SelectedModel.ParameterS.First();
            OnPropertyChanged(nameof(SelectedParameter));
        }
    }
    [ObservableProperty]
    public string _selectedParameter = string.Empty;

    private ObservableCollection<Conversation> _conversations = new ObservableCollection<Conversation>();
    public ObservableCollection<Conversation> Conversations
    {
        get => _conversations;
        set
        {
            _conversations = value;
            OnPropertyChanged(nameof(Conversations));
            OnPropertyChanged(nameof(SelectedConversation));
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

    // Model management

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery != value)
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                _ = SearchForModels();
            }
        }
    }

    public enum ModelWindowMode : byte
    {
        Search = 0,
        Edit = 1
    }

    [ObservableProperty]
    public ModelWindowMode currentModelWindowMode = ModelWindowMode.Search;
    [RelayCommand]
    public void SetModelWindowModeSearch() => CurrentModelWindowMode = ModelWindowMode.Search;
    public void SetModelWindowModeEdit() => CurrentModelWindowMode = ModelWindowMode.Edit;


    [ObservableProperty]
    private bool _menuVisible = false;
    [RelayCommand]
    public void MenuButtonPressed() => MenuVisible ^= true;

    [ObservableProperty]
    private bool _modelManagementVisible = false;
    [RelayCommand]
    public void ModelManagementButtonPressed() => ModelManagementVisible ^= true;

    // Cancellation Tokens

    private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();

    // Parameter RelayCommands

    public ICommand SwitchConversationCommand { get; }
    public ICommand DeleteConversationCommand { get; }


    public MainWindowViewModel(IOllamaApiClient ollamaClient, IConfigManager configManager, IDatabaseManager dbManager, ServerManager serverManager)
    {
        _ollamaClient = ollamaClient;
        _configService = configManager;
        _dbManager = dbManager;
        _serverManager = serverManager;

        SwitchConversationCommand = new RelayCommand<object>(ChangeSelectedConversation);
        DeleteConversationCommand = new RelayCommand<object>(RemoveConversation);

        _ = InitializeAsync();
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

            await SearchForModels();

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
            var startConvoID = SelectedConversation.ID;

            var request = new ChatRequest();
            request.Content = userMessage;
            request.Model = $"{SelectedModel.Name}:{SelectedParameter}";
            request.Messages = ChatHistory.ToArray();

            Conversations[(int)startConvoID].Messages.Add(request);
            if (SelectedConversation.ID == startConvoID)
            {
                ChatHistory.Add(request);
            }

            var response = _ollamaClient.SendChatAsync(request);

            await response;
            if (response.IsCompletedSuccessfully)
            {
                Conversations[(int)startConvoID].Messages.Add(response.Result);
                if (SelectedConversation.ID == startConvoID)
                {
                    ChatHistory.Add(response.Result);
                }
            }

            _dbManager.UpdateConversation(startConvoID, convo =>
            {
                convo.Messages = new List<ChatMessage>(Conversations[(int)startConvoID].Messages);
            });
        }
        catch (Exception e)
        {
            StatusMessage = $"Error occured when sending / recieving a message: {e.Message}";
        }
    }
    
    public async Task SearchForModels()
    {
        try
        {
            var query = SearchQuery;

            _searchCancellationTokenSource.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _searchCancellationTokenSource.Token;

            await Task.Delay(300);
            SearchedModels = new ObservableCollection<AvailableModel>(await _ollamaClient.SearchModelsAsync(query, cancellationToken));
        }
        catch (Exception e)
        {
            StatusMessage = $"Error occured when searching for models: {e.Message}";
        }
    }

    [RelayCommand]
    public void AddConversation()
    {
        try
        {
            var newID = (uint)Conversations.Count;
            Conversations.Add(new Conversation(newID));
            SelectedConversation = Conversations.Last();
            _dbManager.UpdateConversation(newID, convo => new Conversation(newID));
        }
        catch (Exception e)
        {
            StatusMessage = $"Error occured when creating a new conversation: {e.Message}";
        }
    }

    private void ChangeSelectedConversation(object? param)
    {
        try
        {
            if (param != null && uint.TryParse(param.ToString(), out uint id))
            {
                SelectedConversation = Conversations[(int)id];
            }
        }
        catch (Exception e)
        {
            StatusMessage = $"Error when switching conversation: {e.Message}";
        }
    }
    private void RemoveConversation(object? param)
    {
        try
        {
            if (param != null && uint.TryParse(param.ToString(), out uint id))
            {
                _dbManager.DeleteConversation(id);
                Conversations.Clear();
                Conversations = new ObservableCollection<Conversation>(_dbManager.Conversations);
                if(SelectedConversation.ID == id)
                {
                    ChatHistory.Clear();
                }
            }
        }
        catch (Exception e)
        {
            StatusMessage = $"Error when deleting conversation: {e.Message}";
        }
    }

    public bool InputHandler(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _ = SendMessageAsync();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool UpdateConversationName()
    {
        var ID = SelectedConversation.ID;
        var name = SelectedConversation.Name;
        try
        {
            var convo = Conversations.FirstOrDefault(c => c.ID == ID);
            if (convo != null)
            {
                _dbManager.UpdateConversation(ID, convo =>
                {
                    convo.Name = name;
                });
            }
            else
            {
                throw new Exception("No conversation with ID exists");
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error when updaing name for conversation: {e.Message}");
            return false;
        }
    }

    public async Task PullModel(string model)
    {
        try
        {
            var result = await _ollamaClient.PullOllamaModelAsync(model);

            switch (result)
            {
                case true:
                    StatusMessage = $"Pulled model: {result}";
                    break;
                case false:
                    StatusMessage = $"Pulling model failed: {result}";
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error when pulling model: {e.Message}");
        }
    }
}
