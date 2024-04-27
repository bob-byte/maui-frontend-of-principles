namespace SET.Core.Services;

public interface IServiceOfHabit
{
    Task<List<UserHabit>> ActiveHabitsAsync( DateOnly startInterval, DateOnly endInterval );
    Task<UserHabit> UserHabitAsync( Guid id );
    Task UpdateHabitAsync( EditUserHabitDto habit );
    Task<UserHabit> PostHabitAsync( UserHabit habit );
    Task UpdatePrioritiesAsync( IEnumerable<UserHabitWithPriority> habitsWithPriorities );
    void ResetPriorities( IEnumerable<UserHabit> habits );
    bool CanAddNewHabit( UserHabit newHabit, IEnumerable<UserHabit> allHabits );
    Task DeleteAsync( Guid id );

    void Recompute( UserHabit habit );
    double RecomputedScoreAchieved( UserHabit habit, DateOnly from, DateOnly to );

    bool ShouldHabitBeFollowed( ProgressOfHabit progress, UserHabit habit );
}