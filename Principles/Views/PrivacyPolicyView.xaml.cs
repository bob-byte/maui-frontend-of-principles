namespace Principles.Views;

public partial class PrivacyPolicyView : ContentPageBase
{
	public PrivacyPolicyView( PrivacyPolicyViewModel viewModel )
	{
        BindingContext = viewModel;

		InitializeComponent();
	}
}