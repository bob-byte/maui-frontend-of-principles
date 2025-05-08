using Azure.AI.OpenAI;
using Azure;
using System.Text;
using Principles.Core.Constants;

namespace Principles.Core.Services;

public class AiChatService : BaseRemoteService, IAiChatService
{
    private readonly ICachingService m_cachingService;
    private readonly IServiceOfHabit m_serviceOfHabit;
    private readonly IGoalService m_goalService;
    private readonly List<ChatMessage> m_chatMessages;

    public AiChatService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_cachingService = serviceProvider.GetRequiredService<ICachingService>();
        m_serviceOfHabit = serviceProvider.GetRequiredService<IServiceOfHabit>();
        m_goalService = serviceProvider.GetRequiredService<IGoalService>();

        m_chatMessages = new List<ChatMessage>();
    }

    public async Task<StreamingResponse<StreamingChatCompletionsUpdate>> GetAnswerStreamAsync( string prompt, int choiceCount, CancellationToken cancellationToken = default )
    {
        ChatMessage systemMsg = await SystemMessage().DefaultConfigureAwait();
        if(m_chatMessages.Count == 0)
        {
            m_chatMessages.Add( systemMsg );
        }
        else
        {
            m_chatMessages[0] = systemMsg;
        }

        ChatMessage newMessage = new( ChatRole.User, prompt );
        m_chatMessages.Add( newMessage );

        ChatCompletionsOptions options = new( DEFAULT_AI_DEPLOYMENT_NAME, m_chatMessages )
        {
            ChoiceCount = choiceCount
        };

        OpenAIClient aiClient = await LazyAiClient;
        StreamingResponse<StreamingChatCompletionsUpdate> result = await aiClient.GetChatCompletionsStreamingAsync( options, cancellationToken ).DefaultConfigureAwait();
        return result;
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

    private async ValueTask<ChatMessage> SystemMessage()
    {
        StringBuilder systemMessageBuilder = new();
        systemMessageBuilder.Append( "You are a self-development assistant, but you can answer at any question. You have to support the user in their quest to become better and help them identify their habits. You should also provide information on how to better stick to them and become better every day in all areas of the user's life. But don't ask current user habits and don't tell user that he or she should strive for perfection." );

        string newLine = Environment.NewLine;
        string userGender = m_cachingService.GetStoredValue( PreferenceKeys.USER_GENDER );
        if (!string.IsNullOrWhiteSpace( userGender ))
        {
            systemMessageBuilder.Append( $"{newLine}User gender is {userGender}." );
        }

        string userName = m_cachingService.GetStoredValue( PreferenceKeys.USER_NAME );
        if (!string.IsNullOrWhiteSpace( userName ))
        {
            systemMessageBuilder.Append( $"{newLine}User name is \"{userName}\". You should use his/her name frequently." );
        }

        string userMission = m_cachingService.GetStoredValue( PreferenceKeys.USER_MISSION );
        if (!string.IsNullOrWhiteSpace( userMission ))
        {
            systemMessageBuilder.Append( $"{newLine}User mission is \"{userMission}\"." );
        }

        string userMainSlogan = m_cachingService.GetStoredValue( PreferenceKeys.USER_MAIN_SLOGAN );
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

        IEnumerable<UserGoal> userGoals = await m_goalService.UserGoalsAsync().DefaultConfigureAwait();

        if (userGoals.Any())
        {
            systemMessageBuilder.Append( " My current goals are: " );
            foreach (UserGoal userGoal in userGoals)
            {
                systemMessageBuilder.Append( $"{userGoal.Name}; " );
            }

            systemMessageBuilder.Replace( ';', '.', systemMessageBuilder.Length - 2, 1 );
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
