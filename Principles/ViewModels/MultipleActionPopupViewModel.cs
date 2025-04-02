using System.Collections.ObjectModel;

namespace Principles.ViewModels;

public partial class MultipleActionPopupViewModel : BaseViewModel
{
    public ObservableCollection<ActionButtons> Buttons { get; set; } = new();
    [ObservableProperty]
    private string? m_textOfLabel;
    public MultipleActionPopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }


    public void SetActions( List<ActionButtons> actions, string label )
    {
        TextOfLabel = label;
        Buttons.Clear();
        foreach (ActionButtons action in actions)
        {
            Buttons.Add( action );
        }
    }
}