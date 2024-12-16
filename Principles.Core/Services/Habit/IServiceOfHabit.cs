namespace Principles.Core.Services;

public interface IServiceOfHabit
{
    List<UserHabit>? StoredUserHabits { get; set; }

    Task<List<UserHabit>> ActiveHabitsAsync( DateOnly startInterval, DateOnly endInterval );
    Task<UserHabit> UserHabitAsync( long id );
    Task<SaveHabitResponse> UpdateHabitAsync( EditUserHabitDto habit );
    Task UpdatePrioritiesAsync( IEnumerable<UserHabitWithPriority> habitsWithPriorities );
    void ResetPriorities( IEnumerable<UserHabit> habits );
    bool CanAddNewHabit( UserHabit newHabit, IEnumerable<UserHabit> allHabits );
    Task<HabitDeletionResponse?> DeleteAsync( long id );

    void Recompute( UserHabit habit );
    double RecomputedScoreAchieved( UserHabit habit, DateOnly from, DateOnly to );

    bool ShouldHabitBeFollowed( ProgressOfHabit progress, UserHabit habit );
}