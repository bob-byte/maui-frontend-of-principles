using CommunityToolkit.Maui.Views;

namespace Principles.Views;

public partial class HabitPopup : Popup
{
    private readonly ISettingsService m_settingsService;
    public bool IsShown { get; private set; }
    public HabitPopupViewModel ViewModel { get; }
    public HabitPopup( HabitPopupViewModel viewModel )
    {
        InitializeComponent();

        m_settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
        SetPopupWidth();

        ViewModel = viewModel;
        BindingContext = viewModel;

        ViewModel.RequestClose = ( result ) =>
        {
            Close( result );
        };

    }

    private void UpdatePopup_Opened( object? sender, PopupOpenedEventArgs e )
    {
        IsShown = true;
    }
    private void UpdatePopup_Closed( object? sender, PopupClosedEventArgs e )
    {
        IsShown = false;
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