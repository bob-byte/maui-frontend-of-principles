using System.Collections.Specialized;

namespace SET.MAUI.Views;

public partial class HelperView : ContentPageBase
{
	public HelperView(HelperViewModel viewModel)
	{
		BindingContext = viewModel;
		ViewModel = viewModel;

		InitializeComponent();

        ViewModel.DisplayMessages.CollectionChanged += DisplayMessages_CollectionChanged;
        DXC_DisplayMessages.ItemTemplate = new MessageDataTemplateSelector( ViewModel );
    }

    private void DisplayMessages_CollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
    {
		if(e.Action == NotifyCollectionChangedAction.Add)
		{
            DXC_DisplayMessages.ScrollTo( ViewModel.DisplayMessages.Count - 1 );

            if (ViewModel.DisplayMessages.Count > 3)
            {
                Thread.Sleep( 100 );
            }
        }
    }

    private HelperViewModel ViewModel { get; }

    async void SB_AskQuestion_Clicked( System.Object sender, System.EventArgs e )
    {
        await AskQuestionAsync();
    }

    async void TE_Prompt_Completed( System.Object sender, System.EventArgs e )
    {
        await AskQuestionAsync();
    }

    private async Task AskQuestionAsync()
	{
        if (ViewModel.AskUserQuestionCommand.CanExecute( null ))
        {
            await ViewModel.AskUserQuestionCommand.ExecuteAsync( null );
        }
    }

    void CPB_Helper_SizeChanged( System.Object sender, System.EventArgs e )
    {
        if(CPB_Helper.Width != -1)
        {
            MessageDataTemplateSelector.PageWidth = CPB_Helper.Width;
        }
    }
}