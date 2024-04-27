using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Models;

public record UserStatement
{
    public Guid Id { get; set; }
    public string Author { get; set; }
    public string ShortDescription { get; set; }
    public List<AreaOfLife> AreasOfLife { get; set; }
    public AreaOfLife? FirstAreaOfLife =>
        AreasOfLife.FirstOrDefault();
    public string IconName { get; set; }
    public StatementPriority Priority { get; set; }
}
