namespace SET.Core;

public interface IUrlBuilder
{
    string BaseUrl { get; }
    string BaseApiUrl { get; }
    string HabitsInProgress { get; }
    string Habits { get; }
    string HabitsPriorities { get; }
    string Login { get; }
    string Profile { get; }
    string UserName { get; }
    string UserMainSlogan { get; }
    string UserMission { get; }
    string SignUp { get; }
    string ProgressOfHabit { get; }
    string AreasOfLife { get; }
    string Account { get; }

    string Combine( params string[] uri );
}