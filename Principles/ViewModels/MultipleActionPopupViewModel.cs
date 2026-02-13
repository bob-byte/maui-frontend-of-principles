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

    private void ApplyQuery( IDictionary<string, object> query )
    {
        if (query.TryGetValue( "AvailableActions", out object? availableActions ) && availableActions is List<ActionData> actions)
        {
            string? description = query.TryGetValue( "Description", out object? descObj ) && descObj is string descStr
                ? descStr
                : null;
            string? title = query.TryGetValue( "Title", out object? titleObj ) && titleObj is string titleStr
                ? titleStr
                : null;

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
                        await DialogService.ClosePopupAsync();
                    }
                );

                Buttons.Add( action );
            }
        }
    }

    public override async Task InitializePopupAsync( IDictionary<string, object> query )
    {
        ApplyQuery( query );
        await base.InitializePopupAsync( query );
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await DialogService.ClosePopupAsync();
    }
}