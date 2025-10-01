using OllamaInteract.Core.Models;

namespace OllamaInteract.Core.Services;
public interface IConfigManager
{
    AppConfig Config { get; }
    void LoadConfig();
    void SaveConfig();
    void UpdateConfig(Action<AppConfig> updateAction);
    void ResetToDefault();
}