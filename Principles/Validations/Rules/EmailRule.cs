
namespace Principles.Validations;

public class EmailRule : IValidationRule<string>
{
    public string ValidationMessage { get; set; }

    public bool IsValid( string value )
    {
        bool isValid = value is string str && RegExps.Email.IsMatch( str );
        return isValid;
    }
}
