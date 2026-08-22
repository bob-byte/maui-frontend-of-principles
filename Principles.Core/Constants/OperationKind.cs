
namespace Principles.Core.Constants;

public sealed class OperationKind
{
    public OperationKind( string name )
    {
        Name = name;
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

    public static bool operator ==( OperationKind left, string right ) => left.Name == right;
    public static bool operator !=( OperationKind left, string right ) => left.Name != right;

    public static bool operator ==( string left, OperationKind right ) => right == left;
    public static bool operator !=( string left, OperationKind right ) => right != left;
}
