namespace Principles.Core.Models;

public partial class ProgressOfHabit : ObservableObject, IEntity
{
    public long LocalId { get; set; }
    public DateTime LastModified { get; set; }

    [ObservableProperty]
    private long m_id;

    public DateOnly Date
    {
        get => DateOnly.FromDayNumber(DateAsInt);
        set
        {
            DateAsInt = value.DayNumber;
            OnPropertyChanged();
        }
    }

    public int DateAsInt { get; set; }

    [ObservableProperty]
    private int m_value;

    [ObservableProperty]
    private UserHabit? m_habit;

    public long HabitLocalId { get; set; }
    
    [ObservableProperty]
    private string? m_notes;

    public bool IsCompleted()
    {
        return m_value == ProgressValue.YES_MANUAL;
    }

    public override string ToString()
    {
        return Date.ToString();
    }
}
