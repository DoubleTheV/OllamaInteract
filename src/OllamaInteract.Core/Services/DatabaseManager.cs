using OllamaInteract.Core.Models;
using Microsoft.Data.Sqlite;

namespace OllamaInteract.Core.Services;

public class DatabaseManager : IDatabaseManager
{
    private List<Conversation> _conversations = new List<Conversation>();
    private string _dbFilePath { get; }
    private readonly object _lock = new object(); // thread safety

    private readonly string _connectionString = string.Empty;

    public DatabaseManager()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appName = "OllamaInteract";
        var appDirectory = Path.Combine(appDataPath, appName);

        Directory.CreateDirectory(appDirectory);

        _dbFilePath = Path.Combine(appDirectory, "db.sqlite");
        _connectionString = $"Data Source={_dbFilePath};Version=3";

        LoadConversations();
    }

    public List<Conversation> Conversations
    {
        get
        {
            lock (_lock)
            {
                return _conversations;
            }
        }
    }

    private void LoadConversations()
    {
        lock (_lock)
        {
            try
            {
                if (File.Exists(_dbFilePath))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    _conversations = new List<Conversation>();
                    SaveConversations();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when loading conversations from database: {e.Message}");
            }
        }
    }

    private void SaveConversations()
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(_dbFilePath))
                {
                    File.Create(_dbFilePath);
                }
                using (SqliteConnection connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when saving conversations to database: {e.Message}");
            }
        }
    }

    public void UpdateConversation(uint conversationID, Action<Conversation> updateAction)
    {
        throw new NotImplementedException();
    }

}