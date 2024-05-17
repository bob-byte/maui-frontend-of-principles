using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services;

public class MockServiceOfHabit : IServiceOfHabit
{
    private readonly List<UserHabit> m_userHabits;

    public MockServiceOfHabit()
    {
        m_userHabits = new List<UserHabit>()
        {
            new UserHabit
            {
                Id = 0,
                Name = "I wake up before 5:00 a.m.",
                ReasonToFollow = "I have more confidence, energy to achieve my goals",
                Description = ""
            }
        };
    }

    public Task DeleteAsync( long id )
    {
        throw new NotImplementedException();
    }

    public Task<List<UserHabit>> ActiveHabitsAsync( DateOnly startInterval, DateOnly endInterval )
    {
        return Task.FromResult(m_userHabits);
    }

    public Task<UserHabit> UserHabitAsync( long id )
    {
        throw new NotImplementedException();
    }


    public Task<UserHabit> PostHabitAsync( UserHabit habit )
    {
        throw new NotImplementedException();
    }

    public bool ShouldHabitBeFollowed( ProgressOfHabit progress, UserHabit habit )
    {
        throw new NotImplementedException();
    }

    public bool CanAddNewHabit( UserHabit newHabit, IEnumerable<UserHabit> allHabits )
    {
        throw new NotImplementedException();
    }

    public double RecomputedScoreAchieved( UserHabit habit, DateOnly from, DateOnly to )
    {
        throw new NotImplementedException();
    }

    public void Recompute( UserHabit habit )
    {
        throw new NotImplementedException();
    }

    public Task UpdatePrioritiesAsync( IEnumerable<UserHabitWithPriority> habitsWithPriorities )
    {
        throw new NotImplementedException();
    }

    public void ResetPriorities( IEnumerable<UserHabit> habits )
    {
        throw new NotImplementedException();
    }

    public Task<SaveHabitResponse> UpdateHabitAsync( EditUserHabitDto habit )
    {
        throw new NotImplementedException();
    }
}
