using CommunityToolkit.Mvvm.Messaging.Messages;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Messages;

/// <summary>
/// Signals that goal was deleted
/// </summary>
public class GoalIsDeletedMessage : ValueChangedMessage<UserGoal>
{
    public GoalIsDeletedMessage( UserGoal goal )
        : base( goal )
    {
        //do nothing
    }
}
