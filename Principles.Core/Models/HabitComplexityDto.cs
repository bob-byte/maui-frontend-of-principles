namespace Principles.Core.Models;

public record HabitComplexityDto
{
    public long HabitId { get; set; }
    public int Complexity { get; set; }

    public HabitComplexityDto( long habitId, int complexity )
    {
        HabitId = habitId;
        Complexity = complexity;
    }
}
