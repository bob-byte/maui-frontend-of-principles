namespace SET.Core.Services;

public interface ILoginService
{
    Task<LoginResponse> LoginAsync( string email, string password );
}