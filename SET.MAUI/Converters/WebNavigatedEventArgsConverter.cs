using CommunityToolkit.Maui.Converters;

namespace SET.MAUI.Converters;

public class WebNavigatedEventArgsConverter : BaseConverterOneWay<WebNavigatedEventArgs, object>
{
	public override object DefaultConvertReturnValue { get; set; }

    public override object ConvertFrom( WebNavigatedEventArgs value, CultureInfo culture = null )
    {
        return value switch
        {
            null => null,
            _ => value
        };
    }
}
