using CommunityToolkit.Maui.Converters;

namespace Principles.Converters
{
    public class ProgressMarkVariatyToBoolConverter : IValueConverter
    {
        public ProgressMarkVariaty TargetKind { get; set; }
        public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
        {
            if (value is ProgressMarkVariaty kind && parameter is string param)
            {
                if (Enum.TryParse( typeof( ProgressMarkVariaty ), param, out var enumValue ))
                {
                    return kind.Equals( enumValue );
                }
            }
            return false;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
            => throw new NotImplementedException();
    }
}
