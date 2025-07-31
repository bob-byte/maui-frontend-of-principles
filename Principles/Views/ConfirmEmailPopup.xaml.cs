using CommunityToolkit.Maui.Views;

namespace Principles.Views;

public partial class ConfirmEmailPopup : Popup
{
    private readonly ISettingsService m_settingsService;

    public ConfirmEmailPopup( ConfirmEmailPopupViewModel viewModel )
    {
        BindingContext = viewModel;
        ViewModel = viewModel;

        m_settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
        InitializeComponent();
        SetPopupWidth();

        Opened += ChangePasswordPopup_Opened;
        Closed += ChangePasswordPopup_Closed;
    }

    private void ChangePasswordPopup_Closed( object? sender, PopupClosedEventArgs e )
    {
        IsShown = false;
    }

    private void ChangePasswordPopup_Opened( object? sender, PopupOpenedEventArgs e )
    {
        IsShown = true;
    }

    public bool IsShown { get; private set; }

    public ConfirmEmailPopupViewModel ViewModel { get; }

    private void SetPopupWidth()
    {
        if (m_settingsService.NormalPageWidth == 0 || m_settingsService.NormalPageWidth > 350)
        {
            G_Popup.WidthRequest = 350;
        }
        else
        {
            G_Popup.WidthRequest = m_settingsService.NormalPageWidth - 10;
        }
    }

    private async void SB_ChangePassword_Clicked( object sender, EventArgs e )
    {
        await ViewModel.ConfirmPasswordChangeAsync( () => CloseAsync() );
    }
}