using CommunityToolkit.Mvvm.Messaging.Messages;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Messages;
public class ArchiveHabitMessage : ValueChangedMessage<UserHabit>
{
    public bool DoShowAllArchivedHabits { get; }

    public ArchiveHabitMessage( UserHabit userHabit, bool doShowAllArchivedHabits )
        : base( userHabit )
    {
        DoShowAllArchivedHabits = doShowAllArchivedHabits;
    }
}
