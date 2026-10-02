using Azure.AI.OpenAI;
using Azure;

using Microsoft.Extensions.Primitives;

using System.Text;
using Principles.Core.Constants;
using OpenAI.Chat;

using System.ClientModel;

namespace Principles.Core.Services;

public class AiChatService : BaseRemoteService, IAiChatService
{
    private readonly IServiceOfHabit m_serviceOfHabit;
    private readonly IGoalService m_goalService;
    private readonly IServiceOfTask m_serviceOfTask;
    private readonly IUserService m_userService;
    private readonly List<ChatMessage> m_chatMessages;

    public AiChatService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_serviceOfHabit = serviceProvider.GetRequiredService<IServiceOfHabit>();
        m_goalService = serviceProvider.GetRequiredService<IGoalService>();
        m_serviceOfTask = serviceProvider.GetRequiredService<IServiceOfTask>();
        m_userService = serviceProvider.GetRequiredService<IUserService>();

        m_chatMessages = new List<ChatMessage>();
    }

    public async Task<AsyncCollectionResult<StreamingChatCompletionUpdate>?> GetAnswerStreamAsync( string prompt, CancellationToken cancellationToken )
    {
        ChatMessage systemMsg = await SystemMessage().DefaultConfigureAwait();
        if (m_chatMessages.Count == 0)
        {
            m_chatMessages.Add( systemMsg );
        }
        else
        {
            m_chatMessages[0] = systemMsg;
        }

        ChatMessage newMessage = ChatMessage.CreateUserMessage( prompt );
        m_chatMessages.Add( newMessage );
        ChatClient client = await LazyAiClient;

        ChatCompletionOptions options = new() { ResponseFormat = ChatResponseFormat.CreateTextFormat() };
        AsyncCollectionResult<StreamingChatCompletionUpdate>? response =
            client.CompleteChatStreamingAsync( m_chatMessages, options, cancellationToken );

        return response;
    }

    public void AddChatAnswer( string answer )
    {
        ChatMessage message = ChatMessage.CreateAssistantMessage( answer );
        m_chatMessages.Add( message );
    }

    public void ClearChat()
    {
        m_chatMessages.Clear();
    }

    private async Task<ChatMessage> SystemMessage()
    {
        StringBuilder systemMessageBuilder = new();
        systemMessageBuilder.Append( "You are a self-development helper, but you can answer at any question. If the user asks a question unrelated to self-development, success and personal growth you must respond without mentioning about self-development, success, and personal growth. You have to support the user in their quest to become better and help them identify their habits. You should also provide information on how to better stick to them and become better every day in all areas of the user's life. But don't ask current user habits and don't tell user that he or she should strive for perfection." );
        systemMessageBuilder.Append(
            " Do not accept an user's conclusions as true. You are an intellectual opponent, not an assistant." );
        systemMessageBuilder.Append( "You shouldn't advise a user when he or she doesn't ask for it. " );
        systemMessageBuilder.Append(
            "You help inside the Principles app (habits for goals). " +
            "In this system: goals are identity-oriented outcomes the user wants to become or achieve; " +
            "habits are small repeating actions that automate progress toward those goals; " +
            "tasks are one-off work, reminders, and checklists that free mental load so the user can focus on habits. " +
            "Today's habits and tasks belong together for daily execution. " +
            "Prefer connecting advice to Goal → Habits → Results, and to time management that protects " +
            "consistency over intensity. When relevant, distinguish habits (recurring systems) from tasks " +
            "(finite to-dos). Do not invent app UI steps the user did not ask for. " +
            "Language and tone: reply in the same language as the user's latest message. " +
            "Write like a fluent native speaker of that language in a natural chat — clear, warm, and concrete. " +
            "Match the user's register (casual when they are casual). " +
            "Never sound like a translated English life-coach script, corporate motivational poster, or textbook. " +
            "Avoid calques and stock slogans such as \"this is not about theory, this is about actions\", " +
            "\"clear steps\", \"own your journey\", or similar template praise. " +
            "Do not open with hollow pep-talk summaries of what the user already said; answer the question directly. " +
            "For Ukrainian: use natural modern Ukrainian phrasing a native would actually say or write in chat, " +
            "not word-for-word translations from English or Russian coach-speak." );

        string newLine = Environment.NewLine;
        User? currentUser = null;
        try
        {
            currentUser = await m_userService.GetCurrentUserAsync().ConfigureAwait( false );
        }
        catch
        {
            // Ignore missing profile data and continue with available local context.
        }

        string? userGender = currentUser?.Gender.ToString().ToLower( new CultureInfo( "en" ) );
        if (!string.IsNullOrWhiteSpace( userGender ))
        {
            systemMessageBuilder.Append( $"{newLine}User gender is {userGender}." );
        }

        if (!string.IsNullOrWhiteSpace( currentUser?.Name ))
        {
            systemMessageBuilder.Append(
                $"{newLine}User name is \"{currentUser.Name}\". You should use his/her name in only first your answer of chat. " +
                "Always write the name exactly as given (same spelling and characters/script); " +
                "never translate or transliterate it, even when the conversation is in another language." );
        }

        if (!string.IsNullOrWhiteSpace( currentUser?.Mission ))
        {
            systemMessageBuilder.Append( $"{newLine}User mission is \"{currentUser.Mission}\"." );
        }

        if (!string.IsNullOrWhiteSpace( currentUser?.MainSlogan ))
        {
            systemMessageBuilder.Append( $"{newLine}User main slogan is: \"{currentUser.MainSlogan}\"." );
        }

        const int maxHabits = 8;
        List<UserHabit> currentUserHabits = (m_serviceOfHabit.StoredUserHabits ?? Enumerable.Empty<UserHabit>())
            .Where( h => !string.IsNullOrWhiteSpace( h.Name ) )
            .Take( maxHabits )
            .ToList();
        if (currentUserHabits.Count > 0)
        {
            systemMessageBuilder.Append(
                $"{newLine}A sample of habits the user currently follows " +
                $"(at most {maxHabits}; not a full list):{newLine}" );
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

        const int maxOpenTasks = 8;
        if (m_serviceOfTask.StoredUserTasks.Count == 0)
        {
            try
            {
                await m_serviceOfTask.GetAllTasksAsync().DefaultConfigureAwait();
            }
            catch
            {
                // Task context is optional for the helper prompt.
            }
        }

        List<TaskItem> openTasks = m_serviceOfTask.StoredUserTasks
            .Where( t => !t.IsCompleted && !string.IsNullOrWhiteSpace( t.Name ) )
            .OrderBy( t => t.Date == null )
            .ThenBy( t => t.Date )
            .ThenBy( t => t.Time == null )
            .ThenBy( t => t.Time )
            .Take( maxOpenTasks )
            .ToList();
        if (openTasks.Count > 0)
        {
            systemMessageBuilder.Append(
                $"{newLine}A sample of the user's nearest open tasks " +
                $"(at most {maxOpenTasks}; not a full inbox): " );
            foreach (TaskItem task in openTasks)
            {
                systemMessageBuilder.Append( $"{task.Name}; " );
            }

            systemMessageBuilder.Replace( ';', '.', systemMessageBuilder.Length - 2, 1 );
        }

        systemMessageBuilder.Append( $"{newLine}If you generate a program code, it must be without ``` delimiters. The programming language should be specified on a separate line before the code." );

        string systemText = systemMessageBuilder.ToString();

        ChatMessage result = ChatMessage.CreateSystemMessage( systemText );
        return result;
    }
}
