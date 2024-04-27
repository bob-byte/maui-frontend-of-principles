using System;
namespace SET.MAUI.Messages;

public partial class DisplayMessage : ObservableObject
{
    [ObservableProperty]
    private string m_text;

    [ObservableProperty]
    private bool m_isUserMessage;

    public object? View { get; set; }
    public DataTemplate? DataTemplate { get; set; }
}

