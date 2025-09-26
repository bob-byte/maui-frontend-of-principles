using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

public record User : IOfflineEntity
{
    public long Id { get; set; }
    public long LocalId { get; set; }
    public string Name { get; set; }
    public string MainSlogan { get; set; }
    public string Mission { get; set; }
    public string Email { get; set; }
    public Gender Gender { get; set; }
    public DateTime LastModified { get; set; }
}
