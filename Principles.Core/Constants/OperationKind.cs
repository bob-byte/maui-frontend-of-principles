
namespace Principles.Core.Constants;

public record OperationKind(string Name)
{
    public static readonly OperationKind Save = new( "Save" );
    public static readonly OperationKind Delete = new( "Delete" );

    public static OperationKind Custom( string name )
    {
        return new( name );
    }

    public static bool operator ==( OperationKind left, string right ) => left.Name == right;
    public static bool operator !=( OperationKind left, string right ) => left.Name != right;

    public static bool operator ==( string left, OperationKind right ) => right == left;
    public static bool operator !=( string left, OperationKind right ) => right != left;
}
