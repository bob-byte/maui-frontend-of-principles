using CommunityToolkit.Maui.Converters;

using System.Collections;

namespace Principles.Converters;

public class IsZeroConverter : BaseConverterOneWay<int, bool>
{
    public override bool DefaultConvertReturnValue { get; set; } = true;

    public override bool ConvertFrom( int value, CultureInfo? culture )
    {
        return value == 0;
    }
}
