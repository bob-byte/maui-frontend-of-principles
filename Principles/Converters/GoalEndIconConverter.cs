using CommunityToolkit.Maui.Converters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Converters;
public class GoalEndIconConverter : BaseConverterOneWay<bool, ImageSource>
{
    public override ImageSource DefaultConvertReturnValue { get; set; } = ImageSource.FromFile( "dotshorizontal" );

    public override ImageSource ConvertFrom( bool isGoalEmpty, CultureInfo? culture )
    {
        return ImageSource.FromFile( isGoalEmpty ? "dotshorizontal" : "cross" );
    }
}
