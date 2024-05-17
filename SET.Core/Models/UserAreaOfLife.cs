using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Models;

public partial class UserAreaOfLife : ObservableObject
{
    [ObservableProperty]
    private long m_id;
    [ObservableProperty]
    private string? m_name;
    [ObservableProperty]
    private int m_priority;
    //public ImageSource Icon { get; set; }
    [ObservableProperty]
    private string? m_description;
    [ObservableProperty]
    private string? m_colorName;
    [ObservableProperty]
    private double? m_rate;
    [ObservableProperty]
    private long m_userId;

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals( object? obj )
    {
        return obj is UserAreaOfLife area && area.m_id == m_id;
    }

    public override int GetHashCode()
    {
        return m_id.GetHashCode();
    }
}
