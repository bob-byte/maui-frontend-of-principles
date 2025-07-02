using CommunityToolkit.Maui.Views;

namespace Principles.Views;

public partial class UpdatePopup : Popup
{
    private readonly ISettingsService m_settingsService;

    public UpdatePopup( UpdatePopupViewModel viewModel )
    {
        KeyboardHelper.HideKeyboard();
        
        BindingContext = viewModel;
        ViewModel = viewModel;

        m_settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
        InitializeComponent();
        SetPopupWidth();

        Opened += UpdatePopup_Opened;
        Closed += UpdatePopup_Closed;
    }

    private void UpdatePopup_Closed( object? sender, PopupClosedEventArgs e )
    {
        IsShown = false;
    }

    private void UpdatePopup_Opened( object? sender, PopupOpenedEventArgs e )
    {
        IsShown = true;
    }

    public bool IsShown { get; private set; }

    public UpdatePopupViewModel ViewModel { get; }

    private void SetPopupWidth()
    {
        if( m_settingsService.NormalPageWidth == 0 || m_settingsService.NormalPageWidth > 350) 
        {
            G_Popup.WidthRequest = 350;
        }
        else
        {
            G_Popup.WidthRequest = m_settingsService.NormalPageWidth - 10;
        }
    }
    private void SB_AppUpdate_Clicked( object sender, EventArgs e )
    {
        Close();
    }

    private void SB_CancelAppUpdate_Clicked( object sender, EventArgs e )
    {
        Close();
    }

    void TGR_ToggleDontShowAgain_Tapped( object sender, TappedEventArgs e )
    {
        ViewModel.DontShowAgain = !ViewModel.DontShowAgain;
    }
}