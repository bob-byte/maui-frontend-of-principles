namespace SET.MAUI.Views;

public partial class SettingsView : ContentPageBase
{
	public SettingsView( SettingsViewModel viewModel )
	{
        BindingContext = viewModel;

		InitializeComponent();
	}

    void SwipeItem_Tap( System.Object sender, DevExpress.Maui.DataGrid.SwipeItemTapEventArgs e )
    {
    }

    void SB_Appearance_Clicked( System.Object sender, System.EventArgs e )
    {
        Application.Current.UserAppTheme = AppTheme.Dark;
    }
}