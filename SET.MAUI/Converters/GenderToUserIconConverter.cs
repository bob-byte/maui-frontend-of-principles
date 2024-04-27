using CommunityToolkit.Maui.Converters;

using System;

namespace SET.MAUI.Converters;

public class GenderToUserIconConverter : BaseConverterOneWay<Gender, ImageSource>
{
    public override ImageSource DefaultConvertReturnValue { get; set; } = ImageSource.FromFile( file: "user" );

    public override ImageSource ConvertFrom( Gender value, CultureInfo? culture )
    {
        ImageSource result = value == Gender.Woman
            ? ImageSource.FromFile( "woman_user" )
            : ImageSource.FromFile( "user" );
        return result;
    }
}

