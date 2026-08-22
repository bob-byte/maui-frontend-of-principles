
namespace Principles.Core.Constants;

public sealed record OperationKind
{
    private const string RecordToStringPrefix = "OperationKind { Name = ";
    private const string RecordToStringSuffix = " }";

    public OperationKind( string name )
    {
        Name = NormalizeName( name );
    }

    public string Name { get; init; }

    public static readonly OperationKind Save = new( "Save" );
    public static readonly OperationKind Delete = new( "Delete" );

    public static OperationKind Custom( string name )
    {
        return new( name );
    }

    public override string ToString()
    {
        return Name;
    }

    public static bool operator ==( OperationKind left, string right ) => left.Name == NormalizeName( right );
    public static bool operator !=( OperationKind left, string right ) => left.Name != NormalizeName( right );

    public static bool operator ==( string left, OperationKind right ) => right == left;
    public static bool operator !=( string left, OperationKind right ) => right != left;

    private static string NormalizeName( string name )
    {
        if (name.StartsWith( RecordToStringPrefix, StringComparison.Ordinal ) &&
            name.EndsWith( RecordToStringSuffix, StringComparison.Ordinal ))
        {
            return name[RecordToStringPrefix.Length..^RecordToStringSuffix.Length];
        }

        return name;
    }
}
