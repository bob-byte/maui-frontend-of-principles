using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;
public partial class UserGoal : ObservableObject
{
    public long Id { get; set; }

    [ObservableProperty]
    private string? m_name;
}
