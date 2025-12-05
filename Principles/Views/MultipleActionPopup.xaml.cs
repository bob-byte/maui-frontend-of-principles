using CommunityToolkit.Maui.Views;

namespace Principles.Views;

public partial class MultipleActionPopup : Popup
{
    private MultipleActionPopupViewModel ViewModel { get; }

    public MultipleActionPopup( List<ActionData> actions, string description, string? title = null )
    {
        KeyboardHelper.HideKeyboard();

        MultipleActionPopupViewModel viewModel =
            ServiceLocator.Current!.GetRequiredService<MultipleActionPopupViewModel>();
        viewModel.Configure( actions, description, title );
        
        BindingContext = viewModel;
        ViewModel = viewModel;

        InitializeComponent();
        SetPopupWidth();
        
        viewModel.ReferenceMessenger.Register<CloseMultipleActionPopupMsg>( this, ( _, _ ) =>
        {
            CloseAsync();
        } );
    }

    public override Task CloseAsync( CancellationToken token = default )
    {
        ViewModel.ReferenceMessenger.Unregister<CloseMultipleActionPopupMsg>( this );
        return base.CloseAsync( token );
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

    private void SB_Cancel_OnClicked( object? sender, EventArgs e )
    {
        CloseAsync();
    }
}