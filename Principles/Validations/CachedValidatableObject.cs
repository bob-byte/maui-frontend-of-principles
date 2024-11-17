using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Validations;

//TODO: refactor it (use ObservablePropertyAttribute)
public class CachedValidatableObject : ObservableObject, IValidity
{
    private IEnumerable<string> m_errors;
    private bool m_isValid;
    private string? m_value;
    private readonly string m_cacheKey;
    private readonly ICachingService m_cachingService;

    public List<IValidationRule<string?>> Validations { get; } = new();

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

    public string? Value
    {
        get => m_value;
        set
        {
            if(!EqualityComparer<string>.Default.Equals(m_value, value))
            {
                string? cachedValue = value?.ToString();
                if (string.IsNullOrEmpty( cachedValue ))
                {
                    m_cachingService.Remove( m_cacheKey );
                }
                else
                {
                    m_cachingService.SetForever( m_cacheKey, cachedValue );
                }

                SetProperty( ref m_value, value );
            }
        }
    }

    public CachedValidatableObject(string cacheKey, ICachingService cachingService)
    {
        m_isValid = true;
        m_errors = Enumerable.Empty<string>();
        m_cacheKey = cacheKey;
        m_cachingService = cachingService;
        m_value = m_cachingService.StoredValue( m_cacheKey );
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
    
    public void SetIsValid()
    {
        IsValid = true;
        Errors = Enumerable.Empty<string>();
    }
}
