
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Helpers;

public static class SnackbarHelper
{
    public static SnackbarOptions DefaultOptions()
    {
        Color buttonTextColor;
        Color bgColor;
        Color textColor;
        if (Application.Current.UserAppTheme == AppTheme.Dark)
        {
            buttonTextColor = (Color)Application.Current.Resources["LightNormalText"];
            bgColor = (Color)Application.Current.Resources["DarkPrimary"];
            textColor = (Color)Application.Current.Resources["DarkNormalText"];
        }
        else
        {
            buttonTextColor = (Color)Application.Current.Resources["DarkNormalText"];
            bgColor = (Color)Application.Current.Resources["LightPrimary"];
            textColor = (Color)Application.Current.Resources["LightNormalText"];
        }

        SnackbarOptions result = new()
        {
            ActionButtonTextColor = buttonTextColor,
            BackgroundColor = bgColor,
            TextColor = textColor,
            CornerRadius = new CornerRadius( 6 )
        };
        return result;
    }
}
