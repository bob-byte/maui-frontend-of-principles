using System;
namespace Principles.Views.Templates;

public class MessageDataTemplateSelector : DataTemplateSelector
{
    public MessageDataTemplateSelector( HelperViewModel viewModel )
    {
        ViewModel = viewModel;
    }

    public HelperViewModel ViewModel { get; }

    public static double PageWidth { get; set; }

    protected override DataTemplate OnSelectTemplate( object item, BindableObject container )
    {
        var message = (DisplayMessage)item;
        if (message.IsUserMessage)
        {
            if (message.View == null)
            {
                message.View = new UserMessageTemplate( PageWidth );
                message.DataTemplate = new DataTemplate( () => message.View );
            }
        }
        else if (message.View == null)
        {
            message.View = new HelperMessageTemplate( message, ViewModel, PageWidth );
            message.DataTemplate = new DataTemplate( () => message.View );
        }

        return message.DataTemplate!;
    }
}
