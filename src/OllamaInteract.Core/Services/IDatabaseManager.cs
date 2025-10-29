using OllamaInteract.Core.Models;

namespace OllamaInteract.Core.Services;

public interface IDatabaseManager
{
    List<Conversation> Conversations { get; }
    void UpdateConversation(uint conversationID, Action<Conversation> updateAction);
    void DeleteConversation(uint conversationID);
    void SaveConversations();
}