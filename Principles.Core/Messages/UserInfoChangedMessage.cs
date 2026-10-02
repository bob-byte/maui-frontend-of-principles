using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Principles.Core.Messages;

/// <summary>
/// Signals that profile fields shown in the shell chrome changed.
/// </summary>
public class UserInfoChangedMessage : ValueChangedMessage<UserInfo>
{
    public UserInfoChangedMessage( UserInfo userInfo )
        : base( userInfo )
    {
    }
}
