
namespace SET.MAUI.Validations;

public class IsNotNullOrWhiteSpaceRule : IValidationRule<string>
{
    public IsNotNullOrWhiteSpaceRule()
    {
        ValidationMessage = LocStrings.RequiredErrorText;
    }

    public IsNotNullOrWhiteSpaceRule( string errMsg )
    {
        ValidationMessage = errMsg;
    }

    public string ValidationMessage { get; set; }

    public bool IsValid( string value )
    {
        return value is string str && !string.IsNullOrWhiteSpace( str );
    }
}
