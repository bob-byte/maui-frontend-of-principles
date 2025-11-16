namespace Principles.Views;
public partial class AppCarouselView : ContentPageBase
{

    public AppCarouselView( AppCarouselViewModel viewModel )
    {
        BindingContext = viewModel;
        ViewModel = viewModel;
        InitializeComponent();

        CV_Features.PositionChanged += CV_Features_PositionChanged;

        this.Appearing += async ( s, e ) =>
        {
            await Task.Delay( 100 ); // невелика затримка, щоб рендер завершився
        };

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
            await ViewModel.Navigation.GoToInitialViewAsync();
        }
        else if (CV_Features.Position < CV_Features.ItemsSource.Cast<object>().Count() - 1)
        {
            CV_Features.Position++;
        }
    }

    private async void CV_Features_PositionChanged( object sender, PositionChangedEventArgs e )
    {
        if( CV_Features.Position == CV_Features.ItemsSource.Cast<object>().Count() - 1 )
        {
            BtnNext.WidthRequest = 320;
            BtnNext.Text = "Start";
        }
        else
        {
            BtnNext.WidthRequest = 50;
            BtnNext.Text = ">";
        }
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var count = CV_Features.ItemsSource.Cast<object>().Count();

        BtnPrev.IsVisible = CV_Features.Position > 0;
    }
}
