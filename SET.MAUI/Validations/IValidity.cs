namespace SET.MAUI.Validations;

public interface IValidity
{
    bool IsValid { get; }
    void ResetValidation();
}