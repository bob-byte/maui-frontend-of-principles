using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;
public class HabitArchiveStatus
{
    public long HabitId { get; set; }
    public bool IsArchived { get; set; }
    public DateTime LastModified { get; set; }
}
