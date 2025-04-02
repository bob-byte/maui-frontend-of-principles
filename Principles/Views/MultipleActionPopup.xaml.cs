using CommunityToolkit.Maui.Views;

namespace Principles.Views;

public partial class MultipleActionPopup : Popup
{
    public MultipleActionPopup( MultipleActionPopupViewModel viewModel )
    {
        BindingContext = viewModel;
        ViewModel = viewModel;
        m_settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();

        InitializeComponent();
        SetPopupWidth();
    }

    public MultipleActionPopupViewModel ViewModel { get; }

    private ISettingsService m_settingsService;

    private void SB_Clicked( object sender, EventArgs e )
    {
        Close();
    }

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
}