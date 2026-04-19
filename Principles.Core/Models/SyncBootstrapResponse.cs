namespace Principles.Core.Models;

public class SyncBootstrapResponse
{
    public User? User { get; set; }
    public List<UserGoal>? Goals { get; set; }
    public List<UserHabit>? ActiveHabits { get; set; }
    public List<UserHabit>? ArchivedHabits { get; set; }
    public Reminder? HabitsReportReminder { get; set; }
}
