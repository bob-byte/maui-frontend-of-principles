
using Microsoft.Maui.Handlers;

namespace Principles.Views.Templates;

public partial class HelperMessageTemplate : Grid
{
    private readonly DisplayMessage m_message;
    private readonly HelperViewModel m_viewModel;

    public HelperMessageTemplate(DisplayMessage message, double pageWidth)
    {
        m_message = message;
        m_viewModel = ServiceLocator.Current!.GetRequiredService<HelperViewModel>();

        InitializeComponent();

        if(pageWidth > 0)
        {
            G_Answer.MaximumWidthRequest = pageWidth - 16 - 10 - 35 - 10 - 35; //values are taken based on the values in the UI controls
        }
    }
}
