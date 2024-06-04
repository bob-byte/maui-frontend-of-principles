namespace SET.MAUI.Views;

public partial class GoalsView : ContentPageBase
{
    public GoalsView( GoalsViewModel viewModel)
	{
        BindingContext = viewModel;

		InitializeComponent();
	}
}