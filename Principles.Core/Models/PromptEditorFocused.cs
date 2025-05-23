namespace Principles.Core.Models;

public partial class PromptEditorFocused: ObservableObject
{
    [ObservableProperty]
    private object? m_viewModel;
    
    [ObservableProperty]
    private bool m_isEditorFocused;

    public PromptEditorFocused()
    {
        
    }

    public PromptEditorFocused( object viewModel, bool isEditorFocused )
    {
        ViewModel = viewModel;
        IsEditorFocused = isEditorFocused;
    }
}