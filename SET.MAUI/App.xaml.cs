using Microsoft.Maui;
using Microsoft.Maui.Controls;

using Application = Microsoft.Maui.Controls.Application;

namespace SET.MAUI;

public partial class App : Application
{
    public App(INavigationService navigationService)
    {
        InitializeComponent();

        UserAppTheme = AppTheme.Light;
        MainPage = new AppShell(navigationService);
        // Use the NavigateToAsync<ViewModelName> method
        // to display the corresponding view.
        // Code lines below show how to navigate to a specific page.
        // Comment out all but one of these lines
        // to open the corresponding page.
        //var navigationService = DependencyService.Get<INavigationService>();
        //navigationService.NavigateToAsync<LoginViewModel>(true);
    }
}
