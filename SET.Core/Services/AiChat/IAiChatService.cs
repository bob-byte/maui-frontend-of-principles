using Azure.AI.OpenAI;

namespace SET.Core.Services;

public interface IAiChatService
{
    Task<string> GetAnswerAsync( string prompt );
    Task<StreamingResponse<StreamingChatCompletionsUpdate>> GetAnswerStreamAsync( string prompt, int choiceCount, CancellationToken cancellationToken = default );
    void AddChatAnswer( string answer );
    void ClearChat();
}