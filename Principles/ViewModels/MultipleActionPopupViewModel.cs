using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Principles.ViewModels;

public partial class MultipleActionPopupViewModel : BaseViewModel
{
    [ObservableProperty]
    private string? m_textOfLabel;
    
    public ObservableCollection<ActionData> Buttons { get; set; } = new();
    
    public MultipleActionPopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }

    public void Configure( List<ActionData> actions, string description, string? title = null )
    {
        Title = string.IsNullOrWhiteSpace( title ) ? 
            LocStrings.ChooseAction : 
            title;
        
        TextOfLabel = description;
        Buttons.Clear();

        foreach (ActionData action in actions)
        {
            action.Command = new AsyncRelayCommand(
                async () =>
                {
                    await action.AsyncFunc();
                    ReferenceMessenger.Send( new CloseMultipleActionPopupMsg() );
                }
            );

            Buttons.Add( action );
        }
    }
}