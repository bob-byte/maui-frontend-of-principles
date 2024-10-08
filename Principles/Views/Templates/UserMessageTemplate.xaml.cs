namespace Principles.Views.Templates;

public partial class UserMessageTemplate : Grid
{
    public UserMessageTemplate(double pageWidth)
    {
        InitializeComponent();

        if(pageWidth != -1)
        {
            G_Question.MaximumWidthRequest = pageWidth - 16 - 10 - 35 - 10 - 35; //values are taken based on the values in the UI controls
        }
    }
}
