namespace Principles.Core.Services;

public interface IServiceOfHabit
{
    ObservableCollectionEx<UserHabit>? StoredUserHabits { get; set; }

    Task<List<UserHabit>> ActiveHabitsAsync( DateOnly startInterval, DateOnly endInterval );
    Task<UserHabit> UserHabitAsync( long id );
    Task<SaveHabitResponse> UpdateHabitAsync( EditUserHabitDto habit );
    Task SetHabitArchiveStatusAsync( HabitArchiveStatus habitArchiveStatus );
    Task<List<ArсhivedHabitDto>> GetArchivedHabits();
    void InitializeHabitProgresses( UserHabit habit, DateOnly startInterval, DateOnly endInterval );
    Task UpdatePrioritiesAsync( IEnumerable<UserHabitWithPriority> habitsWithPriorities );
    bool IsItRecommendedToCreateNewHabit( UserHabit newHabit );
    Task<HabitDeletionResponse?> DeleteAsync( long id );

    void Recompute( UserHabit habit );
    double RecomputedScoreAchieved( UserHabit habit, DateOnly from, DateOnly to );

    bool ShouldHabitBeFollowed( ProgressOfHabit progress, UserHabit habit );

    int GetDaysUntilFullAutomation( UserHabit habit );
}