namespace SET.Core.Services;

public interface ILoginService
{
    Task LoginAsync( string email, string password );
}