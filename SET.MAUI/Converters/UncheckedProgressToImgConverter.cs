using CommunityToolkit.Maui.Converters;

using System;
namespace SET.MAUI.Converters;

public class UncheckedProgressToImgConverter : BaseConverterOneWay<ProgressOfHabit, ImageSource>
{
    public override ImageSource DefaultConvertReturnValue { get; set; } = ImageSource.FromFile( file: "cross" );

    public override ImageSource ConvertFrom( ProgressOfHabit value, CultureInfo? culture )
    {
        ProgressOfHabit computed = value.Habit!.ComputedProgresses.Get( value.Date );
        bool shouldHabitBeFollowed = computed.Value != ProgressValue.YES_AUTO;

        ImageSource result = shouldHabitBeFollowed
            ? ImageSource.FromFile( "cross" )
            : ImageSource.FromFile( "fire_second" );
        return result;
    }
}
