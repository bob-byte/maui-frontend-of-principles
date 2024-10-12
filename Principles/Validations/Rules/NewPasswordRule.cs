namespace Principles.Validations;

public class NewPasswordRule : IValidationRule<string>
{
    public NewPasswordRule()
    {
        ValidationMessage = LocStrings.NewPasswordIsIncorrectError;
    }

    public NewPasswordRule(string validationMessage)
    {
        ValidationMessage = validationMessage;
    }

    public string ValidationMessage { get; set; }

    public bool IsValid( string value )
    {
        bool isValid = value is string str && RegExps.NewPassword.IsMatch( str );
        return isValid;
    }
}
