namespace Principles.Core.Services.Managers;

public interface IUserManagerService
{
    public Task SaveUserFromDtoAsync( UserDto userDto );
}