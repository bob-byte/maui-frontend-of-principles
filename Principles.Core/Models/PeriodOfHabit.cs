using System;
namespace Principles.Core.Models;

public partial class PeriodOfHabit : ObservableObject
{
    [ObservableProperty]
    private PeriodTypeOfHabit m_type;
    [ObservableProperty]
    private string? m_name;
}

