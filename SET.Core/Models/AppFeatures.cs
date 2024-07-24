namespace SET.Core.Models;
public partial class AppFeatures : ObservableObject
{
    [ObservableProperty]
    private string? m_title;
    [ObservableProperty]
    private string? m_description;
}
