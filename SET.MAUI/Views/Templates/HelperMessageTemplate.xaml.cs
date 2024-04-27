
using Microsoft.Maui.Handlers;

namespace SET.MAUI.Views.Templates;

public partial class HelperMessageTemplate : Grid
{
    private readonly DisplayMessage m_message;
    private readonly HelperViewModel m_viewModel;

    public HelperMessageTemplate(DisplayMessage message, HelperViewModel viewModel, double pageWidth)
    {
        m_message = message;
        m_viewModel = viewModel;

        InitializeComponent();

        if(pageWidth != -1)
        {
            G_Answer.MaximumWidthRequest = pageWidth - 16 - 10 - 35 - 10 - 35; //values are taken based on the values in the UI controls
        }
    }

    void ME_Answer_TextChanged( object sender, EventArgs e )
    {
        bool isThisMessageLast = m_viewModel.DisplayMessages.LastOrDefault() == m_message;

        if (isThisMessageLast && !string.IsNullOrWhiteSpace(m_message.Text) && ME_Answer.Height != -1 && G_HelperMessageTemplate.Height < ME_Answer.Height + 10)
        {
            G_HelperMessageTemplate.HeightRequest = ME_Answer.Height + 10;
        }
        else if (string.IsNullOrWhiteSpace( m_message.Text ) || m_message.Text?.Length <= 5)
        {
            G_HelperMessageTemplate.HeightRequest = 44;
        }
    }
}
