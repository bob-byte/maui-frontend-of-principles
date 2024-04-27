using CommunityToolkit.Maui.Converters;

namespace SET.MAUI.Converters;

public class HasCountConverter : BaseConverterOneWay<int, bool>
{
    public override bool DefaultConvertReturnValue { get; set; } = false;

    public override bool ConvertFrom( int value, CultureInfo culture )
    {
        return value > 0;
    }
}
