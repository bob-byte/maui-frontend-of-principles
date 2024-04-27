using CommunityToolkit.Mvvm.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SET.Core.Extensions;

public static class ICommandExtensions
{
    public static void AttemptNotifyCanExecuteChanged<TCommand>( this TCommand command )
        where TCommand : ICommand
    {
        if (command is IRelayCommand relayCommand)
        {
            relayCommand?.NotifyCanExecuteChanged();
        }
    }
}
