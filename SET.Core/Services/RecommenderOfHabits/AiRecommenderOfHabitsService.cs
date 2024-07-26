using Azure.AI.OpenAI;
using Azure;
using System.Text;

namespace SET.Core.Services;

public class AiRecommenderOfHabitsService : BaseRemoteService, IAiRecommenderOfHabitsService
{
    private readonly ChatMessage m_setupMessage;
    private readonly IGoalService m_goalService;

    public AiRecommenderOfHabitsService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_setupMessage = new ChatMessage(
            role: ChatRole.System,
            content: "You are self development assistant. You are in the app that focuses on helping users to create, keep and track their atomic habits. You should recommend new atomic habits for user."
        );

        m_goalService = serviceProvider.GetRequiredService<IGoalService>();
    }

    //it creates user chat message that contains:
    // - user mission
    // - name of current habits
    // - name of areas of life of new habit
    // - format of response (language, JSON array with fields name, reasonToFollow, notes)
    //then parses and returns a response
    public async Task<List<RecommendedHabit>> RecommendedHabitsAsync(
        IEnumerable<UserHabit> currentHabits,
        IEnumerable<UserAreaOfLife> areasOfLifeOfNewHabit,
        Gender userGender,
        string? userMission,
        string? userMainSlogan,
        string? goal
    ) {

#if DEBUG
        var timeComputer = Stopwatch.StartNew();
#endif

        StringBuilder messageContentBuilder = new();
        messageContentBuilder.Append( $"Please recommend me a list of 4 next atomic habits that I can select. " );
        if (!string.IsNullOrWhiteSpace( userMission ))
        {
            messageContentBuilder.Append( $"They shouldn't conflict with my mission: \"{userMission}\". " );
        }

        if (!string.IsNullOrWhiteSpace( userMainSlogan ))
        {
            messageContentBuilder.Append( $"They also shouldn't conflict with my main slogan of life: \"{userMainSlogan}\". " );
        }

        messageContentBuilder.Append( $"I am a {userGender.ToString().ToLower()}. " );

        if (currentHabits.Any())
        {
            messageContentBuilder.Append( $"Now I adhere to the following habits:{Environment.NewLine}" );
            foreach (UserHabit habit in currentHabits)
            {
                messageContentBuilder.Append( $"{habit.Name}; " );
            }

            messageContentBuilder.Replace( oldChar: ';', newChar: '.', startIndex: messageContentBuilder.Length - 2, count: 1 );

            messageContentBuilder.Append( " The atomic habits you recommend should be related to specified habits. " );
        }

        if (areasOfLifeOfNewHabit.Any())
        {
            messageContentBuilder.Append( $"The atomic habits you recommend will be applied to all the following areas of my life: {Environment.NewLine}" );
            foreach (UserAreaOfLife areaOfLife in areasOfLifeOfNewHabit)
            {
                messageContentBuilder.Append( $"{areaOfLife.Name}; " );
            }

            messageContentBuilder.Replace( ';', '.', messageContentBuilder.Length - 2, 1 );

            messageContentBuilder.Append( " So you should recommend me habits which are related to them. " );
        }

        if (string.IsNullOrWhiteSpace( goal ))
        {
            IEnumerable<UserGoal> userGoals = await m_goalService.UserGoalsAsync().DefaultConfigureAwait();

            messageContentBuilder.Append( "My current goals are: " );
            foreach (UserGoal userGoal in userGoals)
            {
                messageContentBuilder.Append( $"{userGoal.Name}; " );
            }

            messageContentBuilder.Replace( ';', '.', messageContentBuilder.Length - 2, 1 );
        }
        else
        {
            messageContentBuilder.Append( $"NOTE: The atomic habits you recommend will be applied to achieve my goal: \"{goal}\"." );
        }

        messageContentBuilder.Append( $"{Environment.NewLine}Your answer should be in JSON format and contain an array of habits. Each array object should consist of the following fields: {nameof( RecommendedHabit.Name )}, {nameof( RecommendedHabit.ReasonToFollow )}. The {nameof( RecommendedHabit.Name )} field indicates habit name and time when execute habit (for example, \"when I wake up\") or location (\"when I am in a gym\"). It should NOT contain frequency of a habit (for example, \"every day\"). Example of the {nameof( RecommendedHabit.Name )} field is \"Meditate at least 5 minutes when I wake up\". The {nameof( RecommendedHabit.ReasonToFollow )} field indicates why I should follow it and must be very briefly, but accurately explained. You must send only a JSON array in your response and nothing else. Your response must be in the {CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName} language. JSON object which contains array of habits must be named \"Habits\" in english. " );

        string userMsg = messageContentBuilder.ToString();
        if (SettingsService.IsDebug)
        {
            LoggingService.LogInfo( userMsg );
        }

        ChatMessage newChatMessage = new( ChatRole.User, userMsg );
        List<ChatMessage> chatMessages = new()
        {
            m_setupMessage,
            newChatMessage,
        };

        ChatCompletionsOptions chatResponseOptions = new( DEFAULT_AI_DEPLOYMENT_NAME, chatMessages );
        Response<ChatCompletions> response = await AiClient.GetChatCompletionsAsync( chatResponseOptions ).DefaultConfigureAwait();
        if (response != null && response.Value.Choices.Count > 0)
        {
            string responseContent = response.Value.Choices[0].Message.Content;
            responseContent = responseContent.Replace( "```", string.Empty );
            responseContent = responseContent.Replace( "json\n", string.Empty );

            List<RecommendedHabit> result;

            try
            {
                RecomendedHabitsResponse? jsonResponse = JsonSerializer.Deserialize<RecomendedHabitsResponse>( responseContent );
                result = jsonResponse!.Habits;
            }
            catch
            {
                result = JsonSerializer.Deserialize<List<RecommendedHabit>>( responseContent )!;
            }

#if DEBUG
            timeComputer.Stop();
            LoggingService.LogInfo( $"Time to find recommended habits: {timeComputer.ElapsedMilliseconds} milliseconds" );
#endif

            return result;
        }
        else
        {
            throw new InvalidOperationException( "SomethingWentWrong" );
        }
    }
}