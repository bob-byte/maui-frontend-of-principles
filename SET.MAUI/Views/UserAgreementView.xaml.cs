namespace SET.MAUI.Views;

public partial class UserAgreementView : ContentPageBase
{
	public UserAgreementView( UserAgreementViewModel viewModel )
	{
        BindingContext = viewModel;

		InitializeComponent();
	}
}