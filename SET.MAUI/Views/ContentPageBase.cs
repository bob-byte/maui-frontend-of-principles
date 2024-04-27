namespace SET.MAUI.Views;

public class ContentPageBase : ContentPage
{
    public ContentPageBase()
    {
        NavigationPage.SetBackButtonTitle( this, value: string.Empty );
        SizeChanged += OnSizeChanged;
    }

    protected double PageWidth { get; set; }

    protected double PageHeight { get; set; }

    private void OnSizeChanged( object? sender, EventArgs e )
    {
        PageWidth = Width;
        PageHeight = Height;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is IViewModelBase viewModel)
        {
            await viewModel.InitializeAsyncCommand.ExecuteAsync( parameter: null );
        }
    }
}