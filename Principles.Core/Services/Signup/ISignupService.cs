namespace Principles.Core.Services;

public interface ISignupService
{
    Task SignupAsync( string name, string email, string password, Gender gender, string mainSlogan, string mission );
}