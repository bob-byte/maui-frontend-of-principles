using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;
public partial class UserGoal : ObservableObject, ICloneable
{
    public long Id { get; set; }

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
