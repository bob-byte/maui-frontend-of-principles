using CommunityToolkit.Maui.Converters;

namespace Principles.Converters;

public class ProgressOfHabitToInt32Converter : BaseConverterOneWay<double, int>
{
    public override int DefaultConvertReturnValue { get; set; } = 0;

    public override int ConvertFrom( double value, CultureInfo culture )
    {
        int result = Score.Round( value );
        return result;
    }
}
