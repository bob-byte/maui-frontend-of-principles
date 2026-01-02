namespace Principles.Views;

public class ContentPageBase : ContentPage
{
    private readonly ISettingsService m_settingsService;
    private readonly IDeviceOrientation m_deviceOrientationService;
    public ContentPageBase()
    {
        m_settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
        m_deviceOrientationService = DependencyService.Get<IDeviceOrientation>();

        NavigationPage.SetBackButtonTitle( this, value: string.Empty );
        SizeChanged += OnSizeChanged;
    }

    protected double PageWidth { get; set; }

    protected double PageHeight { get; set; }

    private void OnSizeChanged( object? sender, EventArgs e )
    {
        PageWidth = Width;
        PageHeight = Height;

        if (m_settingsService.NormalPageWidth == 0)
        {
            DeviceOrientationType orientation = m_deviceOrientationService.GetOrientation();
            if (orientation == DeviceOrientationType.Portrait)
            {
                m_settingsService.NormalPageWidth = PageWidth;
            }
        }

        if (m_settingsService.NormalPageHeight == 0)
        {
            DeviceOrientationType orientation = m_deviceOrientationService.GetOrientation();
            if (orientation == DeviceOrientationType.Portrait)
            {
                m_settingsService.NormalPageHeight = PageHeight;
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is IViewModelBase viewModel && !viewModel.DialogService.IsPopupOpen)
        {
            await viewModel.HandlePageAppearingAsync();
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        
        if (BindingContext is IViewModelBase viewModel && !viewModel.DialogService.IsPopupOpen)
        {
            await viewModel.HandleDisappearingOfPageAsync();
        }
    }
}