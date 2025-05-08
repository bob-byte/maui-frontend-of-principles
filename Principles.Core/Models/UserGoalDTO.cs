using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Principles.Core.Models;
public partial class UserGoal : ObservableObject
{
    public long Id { get; set; }

    [ObservableProperty]
    private string? m_name;
    
    [ObservableProperty]
    private bool m_isSynced;
    
    public UserGoal()
    {
        m_isSynced = false;
    }
}
