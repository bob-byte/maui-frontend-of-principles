using CommunityToolkit.Maui.Converters;

using System.Collections;
using System.Text;

namespace Principles.Converters;

public class ReminderTimeAndDaysToStringConverter : BaseConverterOneWay<EditedUserHabitReminder, string>
{
    public override string DefaultConvertReturnValue { get; set; } = string.Empty;

    private static readonly string[] DaysOfWeekNames =
    {
        LocStrings.Sunday, LocStrings.Monday, LocStrings.Tuesday, LocStrings.Wednesday,
        LocStrings.Thursday, LocStrings.Friday, LocStrings.Saturday
    };

    public override string ConvertFrom( EditedUserHabitReminder reminder, CultureInfo? culture )
    {
        if (reminder == null || reminder.DaysOfWeek == null || reminder.DaysOfWeek.Count == 0)
            return DefaultConvertReturnValue;

        string timeStr = reminder.Time.ToString( "H:mm", culture );

        var sortedDays = reminder.DaysOfWeek
            .Select( d => new { Original = (int)d.Type, Order = d.Type == 0 ? 7 : (int)d.Type } )
            .OrderBy( x => x.Order )
            .ToList();

        if (sortedDays.Count == 7)
            return $"{timeStr} {LocStrings.EveryDay}";

        List<string> periodStrings = new List<string>();
        int i = 0;
        while (i < sortedDays.Count)
        {
            int startIndex = i;
            int endIndex = i;

            while (endIndex + 1 < sortedDays.Count &&
                   sortedDays[endIndex + 1].Order == sortedDays[endIndex].Order + 1)
            {
                endIndex++;
            }

            if (startIndex == endIndex)
            {
                periodStrings.Add( DaysOfWeekNames[sortedDays[startIndex].Original] );
            }
            else
            {
                periodStrings.Add( $"{DaysOfWeekNames[sortedDays[startIndex].Original]}-{DaysOfWeekNames[sortedDays[endIndex].Original]}" );
            }

            i = endIndex + 1;
        }

        return $"{timeStr} {string.Join( ", ", periodStrings )}";
    }
}
