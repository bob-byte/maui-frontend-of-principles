using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

using Principles.ViewModels;

using System;
using System.Linq;

namespace Principles.Views
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