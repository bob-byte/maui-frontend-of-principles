namespace Principles.Core.Models;
public partial class UserGoal : ObservableObject, ICloneable, IEntity
{
    public long LocalId { get; set; }
    public long Id { get; set; }
    public DateTime LastModified { get; set; }

    [ObservableProperty]
    private string? m_name;

    public override bool Equals( object? obj )
    {
        return obj is UserGoal goal && goal.Id == Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
