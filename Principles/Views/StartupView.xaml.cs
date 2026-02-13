
using DevExpress.Maui.Core;

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