namespace Principles.Messages;

/// <summary>
/// Signals that habit was completed
/// </summary>

internal class CompletedHabitMessage
{
    public int PreviousValueOfProgress { get; }
    public ProgressOfHabit ProgressOfHabit { get; }
    public CompletedHabitMessage(int previousValueOfProgress, ProgressOfHabit progressOfHabit )
    {
        PreviousValueOfProgress = previousValueOfProgress;
        ProgressOfHabit = progressOfHabit;
    }
}
