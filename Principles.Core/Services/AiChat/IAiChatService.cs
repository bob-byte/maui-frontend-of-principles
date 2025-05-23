using OpenAI.Chat;

using System.ClientModel;

namespace Principles.Core.Services;

public interface IAiChatService
{
    Task<AsyncCollectionResult<StreamingChatCompletionUpdate>?> GetAnswerStreamAsync( string prompt, CancellationToken cancellationToken );
    void AddChatAnswer( string answer );
    void ClearChat();
}