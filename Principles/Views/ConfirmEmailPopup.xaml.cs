using CommunityToolkit.Maui.Views;

namespace Principles.Views;

public partial class ConfirmEmailPopup : Popup
{
    public ConfirmEmailPopup( ConfirmEmailPopupViewModel viewModel )
    {
        BindingContext = viewModel;
        ViewModel = viewModel;

        InitializeComponent();

        SetPopupWidth();
    }

    public ConfirmEmailPopupViewModel ViewModel { get; }

    private void SetPopupWidth()
    {
        ISettingsService settingsService = ServiceLocator.Current.GetRequiredService<ISettingsService>();
        if (settingsService.NormalPageWidth == 0 || settingsService.NormalPageWidth > 350)
        {
            G_Popup.WidthRequest = 350;
        }
        else
        {
            G_Popup.WidthRequest = settingsService.NormalPageWidth - 10;
        }
    }
}