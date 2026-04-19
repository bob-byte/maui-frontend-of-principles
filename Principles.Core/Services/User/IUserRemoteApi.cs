using System;

namespace Principles.Core.Services;

public interface IUserRemoteApi
{
    Task<User> GetCurrentUserAsync();
    Task SaveUserNameAsync( string userName, DateTime lastModified = default );
    Task SaveMainSloganAsync( string mainSlogan, DateTime lastModified = default );
    Task SaveMissionAsync( string mission, DateTime lastModified = default );
    Task DeleteAccountAsync();
}
