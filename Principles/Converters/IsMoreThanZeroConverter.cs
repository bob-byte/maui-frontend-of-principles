using CommunityToolkit.Maui.Converters;

using System.Collections;

namespace Principles.Converters;

public class IsMoreThanZeroConverter : BaseConverterOneWay<int, bool>
{
    public override bool DefaultConvertReturnValue { get; set; } = false;

    public override bool ConvertFrom( int value, CultureInfo? culture )
    {
        return value > 0;
    }
}
