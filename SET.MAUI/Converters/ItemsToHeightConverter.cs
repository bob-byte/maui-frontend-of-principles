using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace SET.MAUI.Converters;

public class ItemsToHeightConverter : BaseConverterOneWay<int, int>
{
    private const int ITEM_HEIGHT = 156;

    public override int DefaultConvertReturnValue { get; set; } = ITEM_HEIGHT;

    public override int ConvertFrom(int value, CultureInfo culture)
    {
        return value * ITEM_HEIGHT;
    }
}
