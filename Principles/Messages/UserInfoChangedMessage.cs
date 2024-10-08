using CommunityToolkit.Mvvm.Messaging.Messages;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Messages;

/// <summary>
/// Signals that habit count was added
/// </summary>
public class UserInfoChangedMessage : ValueChangedMessage<UserInfo>
{
    public UserInfoChangedMessage( UserInfo userInfo )
        : base( userInfo )
    {
        //do nothing
    }
}
