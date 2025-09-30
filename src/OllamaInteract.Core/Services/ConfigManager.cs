using System.Text.Json;
using OllamaInteract.Core.Models;

namespace OllamaInteract.Core.Services;

public class ConfigManager : IConfigManager
{
    private AppConfig _config = new AppConfig();
    private string ConfigFilePath { get; }
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _lock = new object(); // thread safety

    public ConfigManager()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appName = "OllamaInteract";
        var appDirectory = Path.Combine(appDataPath, appName);

        Directory.CreateDirectory(appDirectory);

        ConfigFilePath = Path.Combine(appDirectory, "config.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        LoadConfig();
    }

    public AppConfig Config
    {
        get
        {
            lock(_lock)
            {
                return _config;
            }
        }
    }

    public void LoadConfig()
    {
        lock (_lock)
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    _config = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions) ?? new AppConfig();
                }
                else
                {
                    _config = new AppConfig();
                    SaveConfig();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Loading config error: {e}");
                _config = new AppConfig();
            }
        }
    }

    public void SaveConfig()
    {
        lock (_lock)
        {
            try
            {
                var json = JsonSerializer.Serialize(Config);
                File.WriteAllText(ConfigFilePath, json);                
            }
            catch (Exception e)
            {
                Console.WriteLine($"Saving config error: {e}");
            }
        }
    }

    public void UpdateConfig(Action<AppConfig> updateAction)
    {
        lock (_lock)
        {
            updateAction(Config);
        }
    }
    
    public void ResetToDefault()
    {
        lock(_lock)
        {
            _config = new AppConfig();
            SaveConfig();
        }
    }
}