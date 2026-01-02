using CommunityToolkit.Maui.Views;

namespace Principles.Views;

public partial class MultipleActionPopup : Popup
{
    private MultipleActionPopupViewModel ViewModel { get; }

    public MultipleActionPopup( MultipleActionPopupViewModel viewModel )
    {
        KeyboardHelper.HideKeyboard();
        
        BindingContext = viewModel;
        ViewModel = viewModel;

        InitializeComponent();
        SetPopupWidth();
    }
    
    private void SetPopupWidth()
    {
        ISettingsService settingsService = ViewModel.ServiceProvider.GetRequiredService<ISettingsService>();
        if (settingsService.NormalPageWidth is 0 or > 350)
        {
            G_Popup.WidthRequest = 350;
        }
        else
        {
            G_Popup.WidthRequest = settingsService.NormalPageWidth - 10;
        }
    }
}