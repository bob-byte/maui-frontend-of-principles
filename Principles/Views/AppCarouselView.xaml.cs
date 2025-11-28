namespace Principles.Views;

public partial class AppCarouselView : ContentPageBase
{

    public AppCarouselView( AppCarouselViewModel viewModel )
    {
        BindingContext = viewModel;
        ViewModel = viewModel;
        InitializeComponent();

        BtnPrev.Text = "<";
        BtnNext.Text = ">";

        UpdateButtons();
    }

    private AppCarouselViewModel ViewModel { get; }

    private void BtnPrev_Clicked( object sender, EventArgs e )
    {

        if (CV_Features.Position > 0)
        {
            CV_Features.Position--;
        }
    }

    private async void BtnNext_Clicked( object sender, EventArgs e )
    {

        if (CV_Features.Position == CV_Features.ItemsSource.Cast<object>().Count() - 1)
        {
            ISettingsService settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
            bool isLoggedIn = !string.IsNullOrWhiteSpace( settingsService.AuthAccessToken );
            if (isLoggedIn)
            {
                await ViewModel.Navigation.GoToInitialViewAsync();
            }
            else
            {
                await ViewModel.Navigation.NavigateToAsync<StartupViewModel>();
            }
        }
        else if (CV_Features.Position < CV_Features.ItemsSource.Cast<object>().Count() - 1)
        {
            CV_Features.Position++;
        }
    }

    private void CV_Features_PositionChanged( object sender, PositionChangedEventArgs e )
    {        
        if( CV_Features.Position == CV_Features.ItemsSource.Cast<object>().Count() - 1 )
        {
            BtnNext.Text = LocStrings.Ahead;
            Animation animation = new (
                v => BtnNext.WidthRequest = v,
                BtnNext.WidthRequest,
                320,
                Easing.Default 
            );
            animation.Commit( BtnNext, "WidthAnimation", 16, 500 );
        }
        else
        {
            BtnNext.Text = ">";
            Animation animation = new (
                v => BtnNext.WidthRequest = v,
                BtnNext.WidthRequest,
                50,
                Easing.Default 
            );
            animation.Commit( BtnNext, "WidthAnimation2", 16, 500 );
        }
        
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        BtnPrev.IsVisible = CV_Features.Position > 0;
    }
}
