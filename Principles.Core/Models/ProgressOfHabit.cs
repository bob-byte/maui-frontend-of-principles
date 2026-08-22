namespace Principles.Core.Models;

[Table( "ProgressOfHabit" )]
public partial class ProgressOfHabit : ObservableObject, IEntity
{
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }
    public DateTime LastModified { get; set; }

    [ObservableProperty]
    private long m_id;

    [Ignore]
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
    [property: Ignore]
    private UserHabit? m_habit;

    public long UserHabitLocalId { get; set; }

    [ObservableProperty]
    private string m_notes;

    public bool IsCompleted()
    {
        return Value == ProgressValue.YES_MANUAL;
    }

    public override string ToString()
    {
        return Date.ToString();
    }
}
