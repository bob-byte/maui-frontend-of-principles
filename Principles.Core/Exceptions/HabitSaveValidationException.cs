namespace Principles.Exceptions;

public sealed class HabitSaveValidationException : Exception
{
    public HabitSaveValidationException( IEnumerable<string> errors )
        : base( string.Join( Environment.NewLine, NormalizeErrors( errors ) ) )
    {
        Errors = NormalizeErrors( errors );
    }

    public IReadOnlyList<string> Errors { get; }

    private static IReadOnlyList<string> NormalizeErrors( IEnumerable<string> errors )
    {
        List<string> normalizedErrors = errors
            .Where( error => !string.IsNullOrWhiteSpace( error ) )
            .Select( error => error.Trim() )
            .Distinct( StringComparer.Ordinal )
            .ToList();

        return normalizedErrors.Count > 0
            ? normalizedErrors
            : ["Habit save validation failed."];
    }
}
