namespace Principles.Core.Services;

public interface IProgressOfHabitService
{
    Task UpdateAsync( ProgressOfHabit progressOfHabit );
    int ConvertScoreToPercentage( double score );
}