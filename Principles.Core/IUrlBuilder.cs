namespace Principles.Core;

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
    string Password { get; }
    string CodeGeneration { get; }
    string ProgressOfHabit { get; }
    string AreasOfLife { get; }
    string Account { get; }
    string Goal { get; }
    string Logs { get; }
    string GoogleAuth { get; }
    string VersionCheck { get; }
    string HabitsReportReminder { get; }
    string AllReminders { get; }
    string AppleAuth { get; }
    string ApiKey { get; }
    string HabitArchiveStatus { get; }
    string Archive {  get; }
    string Progresses { get; }
    string HabitComplexity { get; }

    string Combine( params string[] uri );
}