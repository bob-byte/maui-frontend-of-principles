using CommunityToolkit.Maui.Views;

namespace SET.MAUI.Views;

public partial class UpdatePopup : Popup
{
    private readonly ISettingsService m_settingsService;

    public UpdatePopup( UpdatePopupViewModel viewModel )
    {
        BindingContext = viewModel;
        m_settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
        InitializeComponent();
        SetPopupWidth();
    }

    private void SetPopupWidth()
    {
        if( m_settingsService.NormalPageWidth == 0 || m_settingsService.NormalPageWidth > 350) 
        {
            G_Popup.WidthRequest = 350;

            SB_AppUpdate.WidthRequest = 170;
        }
        else
        {
            G_Popup.WidthRequest = m_settingsService.NormalPageWidth - 10;

            SB_AppUpdate.WidthRequest = 150;
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
}