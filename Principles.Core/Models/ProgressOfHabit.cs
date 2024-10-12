namespace Principles.Core.Models;

public partial class ProgressOfHabit : ObservableObject
{
    [ObservableProperty]
    private long m_id;
    [ObservableProperty]
    private DateOnly m_date;
    [ObservableProperty]
    private int m_value;
    [ObservableProperty]
    private UserHabit? m_habit;
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
