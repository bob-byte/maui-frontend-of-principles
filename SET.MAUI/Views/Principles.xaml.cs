namespace SET.MAUI.Views;

public partial class PrinciplesView : ContentPageBase
{
	public PrinciplesView( PrinciplesViewModel viewModel )
	{
        BindingContext = viewModel;

		InitializeComponent();
	}
}