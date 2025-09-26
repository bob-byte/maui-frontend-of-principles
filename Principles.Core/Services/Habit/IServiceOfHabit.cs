namespace Principles.Core.Services;

public interface IServiceOfHabit
{
    ObservableCollectionEx<UserHabit>? StoredUserHabits { get; set; }

    Task<List<UserHabit>> ActiveHabitsAsync( DateOnly startInterval, DateOnly endInterval );
    Task<UserHabit> UserHabitAsync( long localId );
    Task UpdateHabitAsync( UserHabit habit );
    Task SetHabitArchiveStatusAsync( UserHabit habit );
    Task<List<ArсhivedHabit>> GetArchivedHabits();
    void InitializeHabitProgresses( UserHabit habit, DateOnly startInterval, DateOnly endInterval );
    bool IsItRecommendedToCreateNewHabit( UserHabit newHabit );
    Task DeleteAsync( UserHabit habit );

    void Recompute( UserHabit habit );
    double RecomputedScoreAchieved( UserHabit habit, DateOnly from, DateOnly to );

    bool ShouldHabitBeFollowed( ProgressOfHabit progress, UserHabit habit );

    int GetDaysUntilFullAutomation( UserHabit habit );
}