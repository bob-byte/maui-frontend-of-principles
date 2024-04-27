namespace SET.MAUI.Views;

public partial class BaseTemplateView : ContentPageBase
{
	public BaseTemplateView( BaseViewModel viewModel)
	{
        BindingContext = viewModel;

		InitializeComponent();
	}
}