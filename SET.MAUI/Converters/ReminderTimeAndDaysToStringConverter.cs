using CommunityToolkit.Maui.Converters;

using System.Collections;
using System.Text;

namespace SET.MAUI.Converters;
public class ReminderTimeAndDaysToStringConverter : BaseConverterOneWay<EditedUserHabitReminder, string>
{
    public override string DefaultConvertReturnValue { get; set; } = string.Empty;

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
            for (int i = 0; i < daysOfWeek.Count; i++)
            {
                if (i > 0)
                    result.Append( ", " );

                result.Append( daysOfWeek[i].Type.ToString() );
            }
        }

        return result.ToString();
    }
}