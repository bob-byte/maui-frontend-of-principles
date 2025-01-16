using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

public partial class UserAreaOfLife
{
    public long Id { get; set; }
    public string? Name { get; set; }

    public override string ToString()
    {
        return Name;
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
