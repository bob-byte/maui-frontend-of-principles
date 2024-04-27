using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

using SET.MAUI.ViewModels;

using System;
using System.Linq;

namespace SET.MAUI.Views
{
    public partial class LoginView : ContentPageBase
    {
        public LoginView(LoginViewModel viewModel)
        {
            BindingContext = viewModel;

            InitializeComponent();
        }
    }
}