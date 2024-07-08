using Azure.AI.OpenAI;
using Azure;
using System.Text;
using SET.Core.Constants;

namespace SET.Core.Services;

public class AiChatService : BaseRemoteService, IAiChatService
{
    private readonly ICachingService m_cachingService;
    private readonly IServiceOfHabit m_serviceOfHabit;
    private readonly List<ChatMessage> m_chatMessages;

    public AiChatService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_cachingService = serviceProvider.GetRequiredService<ICachingService>();
        m_serviceOfHabit = serviceProvider.GetRequiredService<IServiceOfHabit>();
        m_chatMessages = new List<ChatMessage>();
    }

    public Task<StreamingResponse<StreamingChatCompletionsUpdate>> GetAnswerStreamAsync( string prompt, int choiceCount, CancellationToken cancellationToken = default )
    {
        if(m_chatMessages.Count == 0)
        {
            m_chatMessages.Add( SystemMessage() );
        }
        else
        {
            m_chatMessages[0] = SystemMessage();
        }

        ChatMessage newMessage = new( ChatRole.User, prompt );
        m_chatMessages.Add( newMessage );

        ChatCompletionsOptions options = new( DEFAULT_AI_DEPLOYMENT_NAME, m_chatMessages );
        options.ChoiceCount = choiceCount;
        return AiClient.GetChatCompletionsStreamingAsync( options, cancellationToken );
    }

    public void AddChatAnswer( string answer )
    {
        ChatMessage message = new( ChatRole.Assistant, answer );
        m_chatMessages.Add( message );
    }

    public void ClearChat()
    {
        m_chatMessages.Clear();
    }

    private ChatMessage SystemMessage()
    {
        StringBuilder systemMessageBuilder = new();
        systemMessageBuilder.Append( "You are a self-development assistant, but you can answer at any question. You have to support the user in their quest to become better and help them identify their habits. You should also provide information on how to better stick to them and become better every day in all areas of the user's life. But don't ask current user habits." );

        string newLine = Environment.NewLine;
        string userGender = m_cachingService.StoredValue( CacheKeys.USER_GENDER );
        if (!string.IsNullOrWhiteSpace( userGender ))
        {
            systemMessageBuilder.Append( $"{newLine}User gender is {userGender}." );
        }

        string userName = m_cachingService.StoredValue( CacheKeys.USER_NAME );
        if (!string.IsNullOrWhiteSpace( userName ))
        {
            systemMessageBuilder.Append( $"{newLine}User name is \"{userName}\"." );
        }

        string userMission = m_cachingService.StoredValue( CacheKeys.USER_MISSION );
        if (!string.IsNullOrWhiteSpace( userMission ))
        {
            systemMessageBuilder.Append( $"{newLine}User mission is \"{userMission}\"." );
        }

        string userMainSlogan = m_cachingService.StoredValue( CacheKeys.USER_MAIN_SLOGAN );
        if (!string.IsNullOrWhiteSpace( userMainSlogan ))
        {
            systemMessageBuilder.Append( $"{newLine}User main slogan is: \"{userMainSlogan}\"." );
        }

        IEnumerable<UserHabit>? currentUserHabits = m_serviceOfHabit.StoredUserHabits;
        if (currentUserHabits?.Any() == true)
        {
            systemMessageBuilder.Append( $"{newLine}Now the user adheres to the following habits:{newLine}" );
            foreach (UserHabit habit in currentUserHabits)
            {
                systemMessageBuilder.Append( $"{habit.Name}; " );
            }

            systemMessageBuilder.Replace( oldValue: "; ", newValue: ".", startIndex: systemMessageBuilder.Length - 2, count: 2 );
        }

        systemMessageBuilder.Append( $"{newLine}If you generate a program code, it must be without ``` delimiters. The programming language should be specified on a separate line before the code." );

        string systemText = systemMessageBuilder.ToString();

        ChatMessage result = new(
            role: ChatRole.System,
            content: systemText
        );
        return result;
    }
}
