using OllamaInteract.Core.Models;
using Microsoft.Data.Sqlite;
using OllamaInteract.Core.DataAccess;
using System.Data;
using System.Runtime.InteropServices;

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
        _connectionString = $"Data Source={_dbFilePath}";

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
                InitializeDatabase();
                var newConversations = new List<Conversation>();
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    using (var conversationCommand = new SqliteCommand(SqlQueries.GetConversations, connection))
                    {
                        using (var conversationReader = conversationCommand.ExecuteReader())
                            while (conversationReader.Read())
                            {
                                Conversation conv = new Conversation((uint)conversationReader.GetInt32(0))
                                {
                                    Name = conversationReader.GetString(1)
                                };
                                newConversations.Add(conv);
                            }
                    }
                    foreach (var conversation in newConversations)
                    {
                        using (var historyCommand = new SqliteCommand(SqlQueries.GetConversationHistory, connection))
                        {
                            historyCommand.Parameters.AddWithValue("@ConversationID", conversation.ID);
                            using (var messageReader = historyCommand.ExecuteReader())
                                while (messageReader.Read())
                                {
                                    var message = new ChatMessage(
                                        messageReader.GetString(0),
                                        messageReader.GetString(1),
                                        messageReader.GetString(2)
                                    );
                                    conversation.Messages.Add(message);
                                    Console.WriteLine(message.Role);
                                }

                        }
                    }
                }
                _conversations.Clear();
                _conversations = newConversations;
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
                InitializeDatabase();
                using (SqliteConnection connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    foreach (var conversation in _conversations)
                    {
                        using (SqliteCommand saveCommand = new SqliteCommand(SqlQueries.SaveConversation, connection))
                        {
                            saveCommand.Parameters.AddWithValue("@ID", conversation.ID);
                            saveCommand.Parameters.AddWithValue("@Name", conversation.Name);

                            saveCommand.ExecuteNonQuery();
                        }

                        foreach (var (message, index) in conversation.Messages.Select((message, index) => (message, index)))
                        {
                            using (SqliteCommand saveCommand = new SqliteCommand(SqlQueries.SaveConversationMessage, connection))
                            {
                                saveCommand.Parameters.AddWithValue("@ID", index);
                                saveCommand.Parameters.AddWithValue("@ConversationID", conversation.ID);
                                saveCommand.Parameters.AddWithValue("@Role", message.Role);
                                saveCommand.Parameters.AddWithValue("@Content", message.Content);
                                saveCommand.Parameters.AddWithValue("@Timestamp", message.TimeStamp);

                                saveCommand.ExecuteNonQuery();
                            }
                        }
                    }
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
        lock (_lock)
        {
            try
            {
                var existingConversation = _conversations.FirstOrDefault(c => c.ID == conversationID);
                if (existingConversation != null)
                {
                    Console.WriteLine($"Updating existing conversation in database with ID {conversationID}");
                    updateAction(existingConversation);
                }
                else
                {
                    Console.WriteLine($"Creating new conversation in database with ID {conversationID}");
                    var newConversation = new Conversation(conversationID);
                    updateAction(newConversation);
                    _conversations.Add(newConversation);
                }
                SaveConversations();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when updating a conversation: {e.Message}");
            }
        }
    }

    public void DeleteConversation(uint conversationID)
    {
        lock (_lock)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using var command = connection.CreateCommand();
                            command.Transaction = transaction;
                            command.Parameters.AddWithValue("@ConversationID", (int)conversationID);

                            foreach (var step in SqlQueries.DeleteConversationTransaction)
                            {
                                command.CommandText = step;
                                Console.WriteLine($"{command.CommandText}");
                                Console.WriteLine($"{command.Parameters[0].ParameterName}:");
                                Console.WriteLine($"{command.Parameters[0].Value}");

                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Core: Error within delete conversation transaction: {e.Message}");
                        }
                    }
                }
                LoadConversations();
                Console.WriteLine($"After transaction: {Conversations.Count}; {conversationID}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Core: Error when deleting conversation: {e.Message}");
            }
        }
    }

    private void InitializeDatabase()
    {
        if (!File.Exists(_dbFilePath))
        {
            Console.WriteLine("Initializing Database");
            File.Create(_dbFilePath);
        }
        using (SqliteConnection connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (SqliteCommand initializeCommand = new SqliteCommand(SqlQueries.InitializeDatabase, connection))
            {
                initializeCommand.ExecuteNonQuery();
            }
        }
    }

}