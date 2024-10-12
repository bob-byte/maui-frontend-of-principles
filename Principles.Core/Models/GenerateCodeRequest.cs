namespace Principles.Core.Models;
public class GenerateCodeRequest
{
    public GenerateCodeRequest( string emailWhereSendCode )
    {
        EmailWhereSendCode = emailWhereSendCode;
    }

    public string EmailWhereSendCode { get; }
}
