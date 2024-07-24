using DevExpress.Maui.Controls;

namespace SET.MAUI.Views;

public partial class StartupView : ContentPageBase
{
	public StartupView( StartupViewModel viewModel )
	{
        BindingContext = viewModel;

		InitializeComponent();
	}
}