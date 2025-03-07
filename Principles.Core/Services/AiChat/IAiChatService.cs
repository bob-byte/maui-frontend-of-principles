using Azure.AI.OpenAI;

using OpenAI.Chat;
namespace Principles.Core.Services;

public interface IAiChatService
{
    Task<ChatCompletion> GetAnswerStreamAsync( string prompt );
    void AddChatAnswer( string answer );
    void ClearChat();
}