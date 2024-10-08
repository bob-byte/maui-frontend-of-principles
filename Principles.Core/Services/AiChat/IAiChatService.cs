using Azure.AI.OpenAI;

namespace Principles.Core.Services;

public interface IAiChatService
{
    Task<StreamingResponse<StreamingChatCompletionsUpdate>> GetAnswerStreamAsync( string prompt, int choiceCount, CancellationToken cancellationToken = default );
    void AddChatAnswer( string answer );
    void ClearChat();
}