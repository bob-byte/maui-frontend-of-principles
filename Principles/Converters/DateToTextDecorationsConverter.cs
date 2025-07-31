using System.Globalization;
using CommunityToolkit.Maui.Converters;

using DevExpress.Maui.Editors;

namespace Principles.Converters;

public class DateToTextDecorationsConverter : BaseConverterOneWay<DateTime, TextDecorations>
{
    public override TextDecorations DefaultConvertReturnValue { get; set; } = TextDecorations.None;

    public override TextDecorations ConvertFrom(DateTime value, CultureInfo culture)
    {
        return value.Date == DateTime.Today ? TextDecorations.Underline : TextDecorations.None;
    }
}
