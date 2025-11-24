
namespace Principles.Core.Services;

public interface IUserService
{
    Task DeleteAccountAsync();
    Task<User> GetCurrentUserAsync();
    Task SaveMainSloganAsync( string mainSlogan );
    Task SaveMissionAsync( string mission );
    Task SaveUserNameAsync( string userName );
}
