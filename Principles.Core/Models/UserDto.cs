using SQLite;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;
public class UserDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? MainSlogan { get; set; }
    public string? Mission { get; set; }
    public Gender Gender { get; set; }
    public bool IsSynced { get; set; }
}

