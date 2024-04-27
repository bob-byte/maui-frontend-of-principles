using CommunityToolkit.Maui.Converters;

namespace SET.MAUI.Converters;

public class ProgressOfHabitToInt32Converter : BaseConverterOneWay<double, int>
{
    private readonly IProgressOfHabitService m_service;

    //TODO: don't use IProgressOfHabitService. Method IProgressOfHabitService.ConvertScoreToPercentage should be implemented here
    public ProgressOfHabitToInt32Converter(IProgressOfHabitService service)
    {
        m_service = service;
    }

    public override int DefaultConvertReturnValue { get; set; } = 0;

    public override int ConvertFrom( double value, CultureInfo culture )
    {
        int result = m_service.ConvertScoreToPercentage( value );
        return result;
    }
}
