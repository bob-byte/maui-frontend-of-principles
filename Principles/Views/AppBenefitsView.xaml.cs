namespace Principles.Views;

public partial class AppBenefitsView : ContentPageBase
{
    public AppBenefitsView( AppBenefitsViewModel viewModel )
    {
        BindingContext = viewModel;
        ViewModel = viewModel;
        InitializeComponent();

        BtnPrev.Text = "<";
        BtnNext.Text = ">";

        UpdateButtons();
    }

    private AppBenefitsViewModel ViewModel { get; }

    private void BtnPrev_Clicked( object sender, EventArgs e )
    {
        if (CV_Benefits.Position > 0)
        {
            CV_Benefits.Position--;
        }
    }

    private async void BtnNext_Clicked( object sender, EventArgs e )
    {
        if (CV_Benefits.Position == CV_Benefits.ItemsSource.Cast<object>().Count() - 1)
        {
            await ViewModel.NavigateToNextViewAsync();
        }
        else if (CV_Benefits.Position < CV_Benefits.ItemsSource.Cast<object>().Count() - 1)
        {
            CV_Benefits.Position++;
        }
    }

    private void CV_Benefits_PositionChanged( object sender, PositionChangedEventArgs e )
    {        
        if( CV_Benefits.Position == CV_Benefits.ItemsSource.Cast<object>().Count() - 1 )
        {
            BtnNext.Text = LocStrings.Ahead;
            double targetWidth = PageWidth - 50 - 30;
            Animation animation = new (
                v => BtnNext.WidthRequest = v,
                BtnNext.WidthRequest,
                targetWidth,
                Easing.Default 
            );

            uint duration;
            #if IOS
                duration = 1000;
            #else
                duration = 500;
            #endif
            animation.Commit( BtnNext, "WidthAnimation", rate: 16, duration );
        }
        else if( BtnNext.WidthRequest != 50 )
        {
            BtnNext.Text = ">";
            Animation animation = new (
                v => BtnNext.WidthRequest = v,
                BtnNext.WidthRequest,
                50,
                Easing.Default 
            );
            animation.Commit( BtnNext, "WidthAnimation2", rate: 16, length: 500 );
        }
        
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        BtnPrev.IsVisible = CV_Benefits.Position > 0;
    }
}
