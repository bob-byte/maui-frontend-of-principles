namespace Principles.Core.Models;
[Table( "UserGoal" )]
public partial class UserGoal : ObservableObject, ICloneable, IEntity
{
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }
    public long Id { get; set; }
    public DateTime LastModified { get; set; }

    [ObservableProperty]
    private string? m_name;

    public override bool Equals( object? obj )
    {
        return obj is UserGoal goal &&
               ((Id != 0 && goal.Id == Id) || (Id == 0 && LocalId != 0 && goal.LocalId == LocalId));
    }

    public override int GetHashCode()
    {
        return Id != 0 ? Id.GetHashCode() : LocalId.GetHashCode();
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
