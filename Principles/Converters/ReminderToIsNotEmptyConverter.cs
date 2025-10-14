using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace Principles.Converters;

public class ReminderToIsNotEmptyConverter : BaseConverterOneWay<EditedUserHabitReminder, bool>
{
    public override bool DefaultConvertReturnValue { get; set; } = false;

    public override bool ConvertFrom(EditedUserHabitReminder? reminder, CultureInfo? culture)
    {
        bool result = reminder is not null && reminder.IsEnabled && reminder.DaysOfWeek?.Any() == true;
        return result;
    }
}
