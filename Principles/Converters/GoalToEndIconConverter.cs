using CommunityToolkit.Maui.Converters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Converters;
public class GoalToEndIconConverter : BaseConverterOneWay<UserGoal, ImageSource>
{
    public override ImageSource DefaultConvertReturnValue { get; set; } = ImageSource.FromFile( "dotshorizontal" );

    public override ImageSource ConvertFrom( UserGoal goal, CultureInfo? culture )
    {
        return ImageSource.FromFile( string.IsNullOrEmpty( goal?.Name ) ? "dotshorizontal" : "cross" );
    }
}
