namespace OllamaInteract.Core.DataAccess;

public static class SqlQueries
{
    public static readonly string InitializeDatabase = @"
        CREATE TABLE IF NOT EXISTS Conversations (
            ID INTEGER PRIMARY KEY,
            Name VARCHAR(30) NOT NULL,
            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
            LastEdited DATETIME DEFAULT CURRENT_TIMESTAMP 
        );

        CREATE TABLE IF NOT EXISTS ConversationMessages (
            ID INTEGER UNSIGNED NOT NULL,
            ConversationID UNSIGNED INTEGER NOT NULL,
            Role VARCHAR(9) NOT NULL CHECK (Role IN ('user', 'assistant', 'system')),
            Content TEXT NOT NULL,
            Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
            PRIMARY KEY (ID, ConversationID),
            FOREIGN KEY (ConversationID) REFERENCES Conversations(ID) 
            ON DELETE CASCADE 
            ON UPDATE CASCADE
        );

        CREATE TABLE IF NOT EXISTS SubModels (
            ID INTEGER PRIMARY KEY,
            ParentModel VARCHAR(30) NOT NULL,
            Name VARCHAR(30) NOT NULL,
            SystemPrompt TEXT NOT NULL
        )
    ";

    public static readonly string SaveConversation = @"
        INSERT OR REPLACE INTO Conversations (ID, Name)
        VALUES (@ID, @Name);
    ";

    public static readonly string SaveConversationMessage = @"
        INSERT OR REPLACE INTO ConversationMessages (ID, ConversationID, Role, Content, Timestamp)
        VALUES (@ID, @ConversationID, @Role, @Content, @Timestamp);
    ";

    public static readonly string GetConversations = @"
        SELECT ID, Name FROM Conversations
        ORDER BY ID ASC;
    ";

    public static readonly string GetConversationHistory = @"
        SELECT Content, Role, Timestamp FROM ConversationMessages
        WHERE ConversationID = @ConversationID;
    ";

    public static readonly string[] DeleteConversationTransaction =
    {
        @"DELETE FROM Conversations WHERE ID = @ConversationID;",
        @"UPDATE Conversations SET ID = ID - 1 WHERE ID > @ConversationID;"
    };
}