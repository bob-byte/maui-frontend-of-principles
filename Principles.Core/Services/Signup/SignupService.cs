
namespace Principles.Core.Services;

public class SignupService : BaseRemoteService, ISignupService
{
    public SignupService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        
    }

    public Task SignupAsync( string name, string email, string password, Gender gender, string mainSlogan, string mission )
    {
        string firstKey = "yX7g53NL7X)xjV7#6DP+ipK5n)@9)_r!";
        string secondKey = "M%m5Vy9R(_k74t^M";

        string encryptedPassword = PasswordChanger.EncryptNewPassword( password, firstKey, secondKey );
        int genderId = (int)gender;

        SignUpRequest request = new( name, email, encryptedPassword, genderId, mainSlogan, mission );
        string url = UrlBuilder.SignUp;

        return RequestProvider.PostAsync( url, request );
    }
}
