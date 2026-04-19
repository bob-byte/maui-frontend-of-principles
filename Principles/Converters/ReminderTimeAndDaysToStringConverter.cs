using CommunityToolkit.Maui.Converters;

using System.Collections;
using System.Text;

namespace Principles.Converters;

public class ReminderTimeAndDaysToStringConverter : BaseConverterOneWay<EditedUserHabitReminder, string>
{
    public override string DefaultConvertReturnValue { get; set; } = string.Empty;

    public override string ConvertFrom( EditedUserHabitReminder reminder, CultureInfo? culture )
    {
        if (reminder == null || reminder.DaysOfWeek == null || reminder.DaysOfWeek.Count == 0)
            return DefaultConvertReturnValue;

        string timeStr = reminder.Time.ToString( "H:mm", culture );

        var sortedDays = reminder.DaysOfWeek
            .Where( d => d is not null )
            .Select( d => (int)d.Type )
            .Where( day => day is >= 0 and <= 6 )
            .Distinct()
            .Select( day => new { Original = day, Order = day == 0 ? 7 : day } )
            .OrderBy( x => x.Order )
            .ToList();

        if (sortedDays.Count == 0)
        {
            return DefaultConvertReturnValue;
        }

        if (sortedDays.Count == 7)
            return $"{timeStr} {LocStrings.EveryDay}";

        string[] daysOfWeekNames =
        [
            LocStrings.Sunday, LocStrings.Monday, LocStrings.Tuesday, LocStrings.Wednesday,
            LocStrings.Thursday, LocStrings.Friday, LocStrings.Saturday
        ];

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
                periodStrings.Add( daysOfWeekNames[sortedDays[startIndex].Original] );
            }
            else
            {
                periodStrings.Add( $"{daysOfWeekNames[sortedDays[startIndex].Original]}-{daysOfWeekNames[sortedDays[endIndex].Original]}" );
            }

            i = endIndex + 1;
        }

        return $"{timeStr} {string.Join( ", ", periodStrings )}";
    }
}
