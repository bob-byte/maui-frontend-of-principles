using CommunityToolkit.Maui.Converters;

using System.Collections;
using System.Text;

namespace Principles.Converters;

public class ReminderTimeAndDaysToStringConverter : BaseConverterOneWay<EditedUserHabitReminder, string>
{
    public override string DefaultConvertReturnValue { get; set; } = string.Empty;

    private static readonly string[] DaysOfWeekNames =
    {
        LocStrings.Sunday, LocStrings.Monday, LocStrings.Tuesday, LocStrings.Wednesday, LocStrings.Thursday, LocStrings.Friday, LocStrings.Saturday
    };

    public override string ConvertFrom( EditedUserHabitReminder reminder, CultureInfo? culture )
    {
        if (reminder == null)
            return DefaultConvertReturnValue;

        DateTime time = reminder.Time;
        IList<WeekDay> daysOfWeek = reminder.DaysOfWeek;

        if (daysOfWeek == null || daysOfWeek.Count == 0)
        {
            return DefaultConvertReturnValue;
        }

        bool allDaysSelected = daysOfWeek.Count == 7;

        StringBuilder result = new StringBuilder();

        result.Append( $"{time.ToString( "H:mm", culture )} " );

        if (allDaysSelected)
        {
            result.Append( LocStrings.EveryDay );
        }
        else
        {
            IOrderedEnumerable<WeekDay> sortedDays = daysOfWeek.OrderBy( day => day.Type == 0 ? 7 : (int)day.Type );

            foreach (WeekDay day in sortedDays)
            {
                if (result.Length > time.ToString( "H:mm", culture ).Length + 1)
                    result.Append( ", " );

                int dayIndex = (int)day.Type;
                if (dayIndex >= 0 && dayIndex < DaysOfWeekNames.Length)
                {
                    result.Append( DaysOfWeekNames[dayIndex] );
                }
            }
        }

        return result.ToString();
    }
}