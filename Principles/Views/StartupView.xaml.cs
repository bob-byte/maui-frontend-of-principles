using DevExpress.Maui.Controls;

namespace Principles.Views;

public partial class StartupView : ContentPageBase
{
    private readonly ILockDeviceOrientation m_deviceOrientationService;
    
	public StartupView( StartupViewModel viewModel )
	{
        BindingContext = viewModel;
        ViewModel = viewModel;

        m_deviceOrientationService = DependencyService.Get<ILockDeviceOrientation>();

		InitializeComponent();
	}

    public StartupViewModel ViewModel { get; }

    private void VisualElement_OnSizeChanged( object? sender, EventArgs e )
    {
        VisualElement? visualElement = sender as VisualElement;
        if (visualElement is not null)
        {
            visualElement.WidthRequest = ViewModel.SettingsService.NormalPageWidth - 30;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        m_deviceOrientationService.LockOrientation( DeviceOrientation.Portrait );
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        m_deviceOrientationService.UnlockOrientation();
    }
}