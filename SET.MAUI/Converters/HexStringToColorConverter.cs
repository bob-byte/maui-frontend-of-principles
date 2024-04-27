using CommunityToolkit.Maui.Converters;

using System;
namespace SET.MAUI.Converters;

public class HexStringToColorConverter : BaseConverterOneWay<string, Color>
{
    //TODO: use default color of app
    public override Color DefaultConvertReturnValue { get; set; } = Colors.White;

    public override Color ConvertFrom( string value, CultureInfo culture )
    {
        return Color.FromArgb( value );
    }
}

