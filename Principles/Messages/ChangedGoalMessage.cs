using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Principles.Messages;

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
