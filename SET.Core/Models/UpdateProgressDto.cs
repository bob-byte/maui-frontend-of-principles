namespace SET.Core.Models;

public class UpdateProgressDto
{
    public long Id { get; set; }
    public bool IsCompleted { get; set; }
    public int FollowedHabitCount { get; set; }
    public DateOnly Date { get; set; }
    public double? Value { get; set; }
    public long HabitId { get; set; }
    public double? PreviousPercentageAchieved { get; set; }
    public double PercentageAchieved { get; set; }
}
