using DevExpress.Maui.Controls;

namespace Principles.Views;

public partial class StartupView : ContentPageBase
{
	public StartupView( StartupViewModel viewModel )
	{
        BindingContext = viewModel;

		InitializeComponent();
	}
}