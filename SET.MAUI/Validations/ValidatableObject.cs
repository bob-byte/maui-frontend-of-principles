using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.MAUI.Validations;

//TODO: refactor it (use ObservablePropertyAttribute)
public class ValidatableObject<T> : ObservableObject, IValidity
{
    private IEnumerable<string> m_errors;
    private bool m_isValid;
    private T m_value;

    public List<IValidationRule<T>> Validations { get; } = new();

    public IEnumerable<string> Errors
    {
        get => m_errors;
        private set => SetProperty( ref m_errors, value );
    }

    public bool IsValid
    {
        get => m_isValid;
        private set => SetProperty( ref m_isValid, value );
    }

    public T Value
    {
        get => m_value;
        set => SetProperty( ref m_value, value );
    }

    public ValidatableObject()
    {
        m_isValid = true;
        m_errors = Enumerable.Empty<string>();
    }

    public bool Validate()
    {
        Errors = Validations
            ?.Where( v => !v.IsValid( Value ) )
            ?.Select( v => v.ValidationMessage )
            ?? Enumerable.Empty<string>();

        IsValid = !Errors.Any();

        return IsValid;
    }

    public void ResetValidation()
    {
        IsValid = true;
    }
}
