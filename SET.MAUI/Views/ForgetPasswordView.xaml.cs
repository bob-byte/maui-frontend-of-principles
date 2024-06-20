using System.Net.Mail;
using System.Net;
using DevExpress.Maui.Editors;

namespace SET.MAUI.Views;

public partial class ForgetPasswordView : ContentPageBase
{
    public ForgetPasswordView( ForgetPasswordViewModel viewModel )
	{
        BindingContext = viewModel;

		InitializeComponent();
	}
}