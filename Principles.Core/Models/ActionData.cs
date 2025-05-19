using System.Windows.Input;

namespace Principles.Core.Models;

public class ActionData
{
    public string Text { get; }
    public Func<Task> AsyncFunc { get; }
    public ICommand? Command { get; set; }
    
    public ActionData( string text, Func<Task> command )
    {
        Text = text;
        AsyncFunc = command;
    }
}
