using CommunityToolkit.Maui.Views;

namespace Principles.Views;

public partial class HabitProgressPopup : Popup
{
    public HabitProgressPopupViewModel ViewModel { get; }
    
    public HabitProgressPopup( HabitProgressPopupViewModel viewModel )
    {        
        InitializeComponent();

        SetPopupWidth();

        ViewModel = viewModel;
        BindingContext = viewModel;
    }

    private void SetPopupWidth()
    {
        ISettingsService settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
        G_Popup.WidthRequest = settingsService.NormalPageWidth == 0 || settingsService.NormalPageWidth > 350 ? 
            350 : 
            settingsService.NormalPageWidth - 10;
    }
}