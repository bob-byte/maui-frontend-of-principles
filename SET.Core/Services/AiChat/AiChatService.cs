using SET.Core.Services;
using Azure.AI.OpenAI;

using System;
using System.Net.Http.Headers;
using System.Net.Mime;
using Azure;

namespace SET.Core.Services;

public class AiChatService : BaseRemoteService, IAiChatService
{
    private List<ChatMessage> m_chatMessages;
    private ChatMessage m_systemMsg;

    public AiChatService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_systemMsg = new ChatMessage( role: ChatRole.System, content: "You are a self-development assistant. You have to support the user in their quest to become better and help them identify their atomic habits. You should also provide information on how to better stick to them and become better every day in all areas of the user's life. But don't ask current user atomic habits" );
        m_chatMessages = new List<ChatMessage>
        {
            m_systemMsg
        };
    }

    public Task<StreamingResponse<StreamingChatCompletionsUpdate>> GetAnswerStreamAsync( string prompt, int choiceCount, CancellationToken cancellationToken = default )
    {
        ChatMessage newMessage = new(ChatRole.User, prompt);
        m_chatMessages.Add( newMessage );

        ChatCompletionsOptions options = new( DEFAULT_AI_DEPLOYMENT_NAME, m_chatMessages );
        options.ChoiceCount = choiceCount;
        return AiClient.GetChatCompletionsStreamingAsync( options, cancellationToken );
    }

    public async Task<string> GetAnswerAsync( string prompt )
    {
        ChatMessage newMessage = new( ChatRole.User, prompt );
        m_chatMessages.Add( newMessage );

        ChatCompletionsOptions options = new( DEFAULT_AI_DEPLOYMENT_NAME, m_chatMessages );
        Response<ChatCompletions> response = await AiClient.GetChatCompletionsAsync( options ).DefaultConfigureAwait();

        string result = response != null && response.Value.Choices.Count > 0
            ? response.Value.Choices[0].Message.Content
            : string.Empty;

        return result;
    }

    public void AddChatAnswer(string answer)
    {
        ChatMessage message = new( ChatRole.Assistant, answer );
        m_chatMessages.Add( message );
    }

    public void ClearChat()
    {
        m_chatMessages.Clear();
        m_chatMessages.Add( m_systemMsg );
    }
}
