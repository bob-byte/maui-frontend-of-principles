using System;

namespace Principles.Core.Services;

public interface IUserRemoteApi
{
    Task<User> GetCurrentUserAsync();
    Task SaveUserNameAsync( string userName );
    Task SaveMainSloganAsync( string mainSlogan );
    Task SaveMissionAsync( string mission );
    Task DeleteAccountAsync();
}
