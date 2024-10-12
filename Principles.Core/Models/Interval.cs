using System;
namespace Principles.Core.Models
{
    public class Interval
    {
        public DateOnly Begin { get; set; }
        public DateOnly Center { get; set; }
        public DateOnly End { get; set; }
        public int Length =>
            Begin.DaysUntil( End ) + 1;
    }
}

