namespace SET.Core.Models;

public partial class AppFeature : ObservableObject
{
    [ObservableProperty]
    private string? m_title;

    [ObservableProperty]
    private string? m_description;
}
