using CommunityToolkit.Mvvm.Messaging.Messages;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Messages;

/// <summary>
/// Signals that habit was updated
/// </summary>
public class HabitSavedMessage : ValueChangedMessage<UserHabit>
{
    public HabitSavedMessage(UserHabit userHabit, IEnumerable<UserHabit> prioterizedHabits)
        : base( userHabit )
    {
        PrioterizedHabits = prioterizedHabits;
    }

    public IEnumerable<UserHabit> PrioterizedHabits { get; }
}
