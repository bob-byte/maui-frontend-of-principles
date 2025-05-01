using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;
public class StreakData
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int StreakDays { get; set; }
}
