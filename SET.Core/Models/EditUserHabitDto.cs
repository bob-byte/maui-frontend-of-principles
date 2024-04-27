namespace SET.Core.Models;

public class EditUserHabitDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public TypeOfHabit Type { get; set; }
    public ICollection<UserAreaOfLife>? AreasOfLife { get; set; }
    public string? Description { get; set; }
    public string? ReasonToFollow { get; set; }
    public string? Question { get; set; }
    public StatusOfHabit Status { get; set; }
    public FrequencyOfHabit? Frequency { get; set; }
    public TimeOnly? Remind { get; set; }
    public int Priority { get; set; }

    public int Complexity { get; set; }
    public string? ColorName { get; set; }

    public List<UserHabitWithPriority>? PrioritizedHabits { get; set; }
}
