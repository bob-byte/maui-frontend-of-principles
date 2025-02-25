using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

public record UserInfo
{
    public string Name { get; set; }
    public string MainSlogan { get; set; }
    public string Mission { get; set; }
    public string Email { get; set; }
    public Gender Gender { get; set; }
}
