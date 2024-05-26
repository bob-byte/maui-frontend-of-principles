using Microsoft.Maui;
using Microsoft.Maui.Controls;

using Application = Microsoft.Maui.Controls.Application;

namespace SET.MAUI;

public partial class App : Application
{
    public App( INavigationService navigationService )
    {
        InitializeComponent();

        UserAppTheme = AppTheme.Light;
        MainPage = new AppShell( navigationService );
    }
}
