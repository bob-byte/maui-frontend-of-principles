using Azure;
using Azure.AI.OpenAI;

namespace SET.Core.Services;

public interface IAiRecommenderOfHabitsService
{
    Task<List<RecommendedHabit>> RecommendedHabitsAsync( IEnumerable<UserHabit> currentHabits, IEnumerable<UserAreaOfLife> areasOfLifeOfNewHabit, Gender userGender, string? userMission, string? userMainSlogan );
    Task<Response<ChatCompletions>> RecommendedHabitsStreamAsync( int choiceCount, IEnumerable<UserHabit> currentHabits, IEnumerable<UserAreaOfLife> areasOfLifeOfNewHabit, Gender userGender, string? userMission );
}