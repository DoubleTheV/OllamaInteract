using System.Threading.Tasks;
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

    private async Task InitializeAsync()
    {
        
    }
}
