
namespace Principles.Validations;

public interface IValidationRule<T>
{
    string ValidationMessage { get; }
    bool IsValid( T value );
}
