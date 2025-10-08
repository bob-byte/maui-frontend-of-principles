using CommunityToolkit.Maui.Converters;
using System.Collections;

namespace Principles.Converters;

public class UserHabitsToStreakConverter : BaseConverterOneWay<ObservableCollectionEx<UserHabit>, int>
{
    public override int DefaultConvertReturnValue { get; set; } = 0;
    
    public override int ConvertFrom( ObservableCollectionEx<UserHabit>? habits, CultureInfo? culture )
    {
        if (habits is null || habits.Count == 0)
        {
            return 0;
        }

        Dictionary<DateOnly, List<int>> progressesByDate = new();

        foreach (UserHabit habit in habits)
        {
            if (habit.Progresses is null)
            {
                continue;
            }

            foreach (ProgressOfHabit progress in habit.Progresses)
            {
                if (progressesByDate.TryGetValue( progress.Date, out List<int>? value ))
                {
                    value.Add( progress.Value );
                }
                else
                {
                    progressesByDate[progress.Date] = [progress.Value];
                }
            }
        }

        int streak = 0;
        List<DateOnly> sortedDates = progressesByDate.Keys.OrderBy( d => d ).ToList();

        foreach (DateOnly date in sortedDates)
        {
            if (!progressesByDate.TryGetValue( date, out List<int>? values ))
            {
                continue;
            }

            if (values.All( v => v is ProgressValue.UNKNOWN or ProgressValue.NO ))
            {
                streak = 0;
            }
            else
            {
                streak += values.Count( progressValue => progressValue == ProgressValue.YES_MANUAL );
            }
        }

        return streak;
    }
}

