using CommunityToolkit.Maui.Converters;

namespace Principles.Converters;

public class ReminderTimeAndDaysToStringConverter : BaseConverterOneWay<EditedUserHabitReminder, string>
{
    public override string DefaultConvertReturnValue { get; set; } = string.Empty;
    
    public override string ConvertFrom( EditedUserHabitReminder reminder, CultureInfo? culture )
    {
        if (reminder == null || reminder.DaysOfWeek == null || reminder.DaysOfWeek.Count == 0 || !reminder.IsEnabled)
        {
            return DefaultConvertReturnValue;
        }

        string timeStr = reminder.Time.ToString( "H:mm", culture );

        var sortedDays = reminder.DaysOfWeek
            .Select( d => new { Original = (int)d.Type, Order = d.Type == 0 ? 7 : (int)d.Type } )
            .OrderBy( x => x.Order )
            .ToList();

        if (sortedDays.Count == 7)
        {
            return $"{timeStr} {LocStrings.EveryDay}";
        }

        string[] daysOfWeekNames =
        [
            LocStrings.Sunday, LocStrings.Monday, LocStrings.Tuesday, LocStrings.Wednesday,
            LocStrings.Thursday, LocStrings.Friday, LocStrings.Saturday
        ];

        List<string> periodStrings = new List<string>();
        int numDay = 0;
        while (numDay < sortedDays.Count)
        {
            int startIndex = numDay;
            int endIndex = numDay;

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

            numDay = endIndex + 1;
        }

        return $"{timeStr} {string.Join( ", ", periodStrings )}";
    }
}
