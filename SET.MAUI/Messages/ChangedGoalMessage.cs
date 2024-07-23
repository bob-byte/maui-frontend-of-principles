using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SET.MAUI.Messages;

/// <summary>
/// Signals that a goal was edited
/// </summary>
public class ChangedGoalMessage : ValueChangedMessage<UserGoal>
{
    public ChangedGoalMessage( UserGoal goal )
        : base( goal )
    {
        //do nothing
    }
}
