using CommunityToolkit.Maui.Converters;

using DevExpress.Maui.Core;

using System;
namespace SET.MAUI.Converters;

public class ProgressToImgConverter : BaseConverterOneWay<ProgressOfHabit, View>
{
    private static readonly ImageSource s_fireIcon = ImageSource.FromFile( "check_third" );
    private static readonly ImageSource s_crossIcon = ImageSource.FromFile( "cross" );

    private static readonly Color s_grayColor = (Application.Current!.Resources["GrayColor"] as Color)!;
    private static readonly Color s_primaryColor = (Application.Current!.Resources["Primary"] as Color)!;

    private static readonly Thickness s_margin = new( 0, 0, 0, 0 );

    public override View DefaultConvertReturnValue { get; set; } = new DXImage()
    {
        Source = s_crossIcon,
        Margin = s_margin
    };

    public override View ConvertFrom( ProgressOfHabit value, CultureInfo? culture )
    {
        ProgressOfHabit computed = value.Habit!.ComputedProgresses.Get( value.Date );

        DXImage image = new()
        {
            Margin = s_margin,
            WidthRequest = 29,
            HeightRequest = 29
        };
        switch (computed.Value)
        {
            case ProgressValue.YES_MANUAL:
                {
                    image.Source = s_fireIcon;
                    image.TintColor = s_primaryColor;

                    break;
                }

            case ProgressValue.YES_AUTO:
                {
                    image.Source = s_fireIcon;
                    image.TintColor = s_grayColor;

                    break;
                }

            default:
                {
                    image.Source = s_crossIcon;
                    image.TintColor = s_grayColor;
                    image.WidthRequest = 33;
                    image.HeightRequest = 33;

                    break;
                }
        }

        return image;
    }
}
