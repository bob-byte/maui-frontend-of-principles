using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Principles.Messages;

public class MsgThatProgressOfHabitUpdated : ValueChangedMessage<ProgressOfHabit>
{
    public MsgThatProgressOfHabitUpdated( ProgressOfHabit value )
        : base( value )
    {
        
    }
}