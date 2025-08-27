namespace Principles.Core.Models;

public class EditUserHabitDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public TypeOfHabit Type { get; set; }
    public ICollection<UserAreaOfLife>? AreasOfLife { get; set; }
    public string? Description { get; set; }
    public UserGoal? Goal { get; set; }
    public string? Question { get; set; }
    public StatusOfHabit Status { get; set; }
    public bool IsArchived { get; set; }
    public FrequencyOfHabit? Frequency { get; set; }
    public TimeOnly? Remind { get; set; }
    public int Priority { get; set; }
    public int Complexity { get; set; }
    public string? ColorName { get; set; }
    public ICollection<UserHabitReminder>? Reminders { get; set; }
    public List<UserHabitWithPriority>? PrioritizedHabits { get; set; }
    public HabitKind Kind { get; set; }
    public string? Unit { get; set; }
    public double? TargetPerOneTime { get; set; }
    public NumericalHabitType TargetType { get; set; }
    public double? MinRate { get; set; }
    public double? MaxRate { get; set; }
}
