namespace Principles.Views;

public partial class ChangePasswordView : ContentPageBase
{
	public ChangePasswordView( ChangePasswordViewModel viewModel )
	{
        BindingContext = viewModel;

		InitializeComponent();
	}

    private void SB_ChangePassword_Clicked( object sender, EventArgs e )
    {
        PE_NewPasswordEntry.Unfocus();
    }
}