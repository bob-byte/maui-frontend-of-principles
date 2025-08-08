using CommunityToolkit.Maui.Converters;
using System.Collections;


namespace Principles.Converters;
public class UserHabitsToStreakConverter : BaseConverterOneWay<ObservableCollectionEx<UserHabit>, int>
{
    public override int DefaultConvertReturnValue { get; set; } = 0;
    public override int ConvertFrom( ObservableCollectionEx<UserHabit> habits, CultureInfo? culture )
    {
        if (habits == null || habits.Count == 0)
            return 0;

        Dictionary<DateOnly, int> progressesByDate = new Dictionary<DateOnly, int>();

        foreach (UserHabit habit in habits)
        {
            if (habit.Progresses == null)
                continue;

            foreach (ProgressOfHabit progress in habit.Progresses)
            {   
                if (!progressesByDate.ContainsKey( progress.Date ))
                {
                    progressesByDate[progress.Date] = progress.Value;
                }
                else
                {
                    if (progress.Value == ProgressValue.YES_MANUAL)
                        progressesByDate[progress.Date] = ProgressValue.YES_MANUAL;
                }
            }
        }

        int streak = 0;
        DateOnly currentDate = DateOnly.FromDateTime( DateTime.Today );
        List<DateOnly> sortedDates = progressesByDate.Keys.OrderBy( d => d ).ToList();

        foreach (DateOnly date in sortedDates)
        {
            if (!progressesByDate.TryGetValue( date, out int value ))
                continue;

            if (value == ProgressValue.YES_MANUAL)
            {
                streak++;
            }
            else if (value == ProgressValue.NO || value == ProgressValue.UNKNOWN)
            {
                streak = 0;
            }
            else
            {
                continue;
            }
        }
        return streak;
    }

}

