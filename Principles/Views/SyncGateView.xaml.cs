using Principles.ViewModels;

namespace Principles.Views;

public partial class SyncGateView : ContentPageBase
{
    public SyncGateView( SyncGateViewModel viewModel )
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
