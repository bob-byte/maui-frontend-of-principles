using CommunityToolkit.Maui.Converters;

using System;
namespace SET.MAUI.Converters;

public class ProgressOfHabitValueConverter : BaseConverterOneWay<int, bool>
{
    public override bool DefaultConvertReturnValue { get; set; } = false;

    public override bool ConvertFrom( int value, CultureInfo culture )
    {
        bool result = value == ProgressValue.YES_MANUAL;
        return result;
    }
}
