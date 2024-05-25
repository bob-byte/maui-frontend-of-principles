
namespace SET.MAUI.Views;

public partial class SettingsView : ContentPageBase
{
	public SettingsView( SettingsViewModel viewModel )
	{
        BindingContext = viewModel;

		InitializeComponent();
	}
}
