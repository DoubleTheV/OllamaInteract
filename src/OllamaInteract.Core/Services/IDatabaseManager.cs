using OllamaInteract.Core.Models;

namespace OllamaInteract.Core.Services;

public interface IDatabaseManager
{
    List<Conversation> Conversations { get; }
    List<Conversation> LoadConversations();
    void UpdateConversation(uint conversationID, Action<Conversation> updateAction);
}