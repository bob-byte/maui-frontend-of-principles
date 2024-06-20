
namespace SET.MAUI.Validations;

public interface IValidationRule<T>
{
    string ValidationMessage { get; }
    bool IsValid( T value );
}
