using CommunityToolkit.Maui.Converters;

namespace Principles.Converters
{
    public class HabitKindToBoolConverter : IValueConverter
    {
        public HabitKind TargetKind { get; set; }
        public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
        {
            if (value is HabitKind kind && parameter is string param)
            {
                if (Enum.TryParse( typeof( HabitKind ), param, out var enumValue ))
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
