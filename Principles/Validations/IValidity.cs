namespace Principles.Validations;

public interface IValidity
{
    bool IsValid { get; }
    void SetIsValid();
}