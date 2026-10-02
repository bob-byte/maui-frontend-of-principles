using CommunityToolkit.Mvvm.Messaging;

using Principles.Core.Messages;

namespace Principles.Core.Services;

public class SyncStateNotifier : ISyncStateNotifier
{
    public void NotifyUserChanged( User user )
    {
        UserInfoChangedMessage message = new( new UserInfo
        {
            Name = user.Name ?? string.Empty,
            MainSlogan = user.MainSlogan,
            Mission = user.Mission,
            Email = user.Email,
            Gender = user.Gender
        } );

        WeakReferenceMessenger.Default.Send( message );
    }
}
