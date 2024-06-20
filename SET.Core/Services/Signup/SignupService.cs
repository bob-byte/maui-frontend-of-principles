
namespace SET.Core.Services;

public class SignupService : BaseRemoteService, ISignupService
{
    private readonly IConfiguration m_configuration;

    public SignupService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
    }

    public Task SignupAsync( string name, string email, string password, Gender gender, string mainSlogan, string mission )
    {
        string firstKey = m_configuration["EncryptionSettings:FirstKey"]!;
        string secondKey = m_configuration["EncryptionSettings:SecondKey"]!;

        string encryptedPassword = PasswordChanger.EncryptNewPassword( password, firstKey, secondKey );
        int genderId = (int)gender;

        SignUpRequest request = new( name, email, encryptedPassword, genderId, mainSlogan, mission );
        string url = UrlBuilder.SignUp;

        return RequestProvider.PostAsync( url, request );
    }
}
