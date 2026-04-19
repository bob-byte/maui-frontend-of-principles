namespace Principles.Core.Models;

[Table( "UserAreaOfLife" )]
public partial class UserAreaOfLife : IEntity
{
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }
    public long Id { get; set; }
    public string? Name { get; set; }
    public DateTime LastModified { get; set; }

    public override string ToString()
    {
        return Name ?? string.Empty;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals( object? obj )
    {
        var areaOfLife = obj as UserAreaOfLife;
        return areaOfLife is not null && areaOfLife.Id == Id;
    }
}
