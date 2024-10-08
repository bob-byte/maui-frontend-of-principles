namespace Principles.Core.Models;

//No,
//Low,
//Medium,
//High
public class Priority
{
    public int Value { get; set; }
    public string IconName { get; set; }
    public string ColorName { get; }
    public string Name { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals( object obj )
    {
        return obj is StatementPriority priority && priority.Value == Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
