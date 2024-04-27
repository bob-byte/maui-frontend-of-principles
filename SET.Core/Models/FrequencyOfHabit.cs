using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Models;

public partial class FrequencyOfHabit : ObservableObject
{
    public const double MAX_VALUE = 1.0;
    [ObservableProperty]
    private Guid m_id = Guid.NewGuid();
    [ObservableProperty]
    private FrequencyType m_type = FrequencyType.EveryDay;

    public double Value =>
        (double)Repeats / IntervalLengthInDays;
    [ObservableProperty]
    private int m_repeats = 1;
    [ObservableProperty]
    private int m_intervalLengthInDays = 1;
    public IntervalType IntervalType
    {
        get
        {
            var result = (IntervalType)m_intervalLengthInDays;
            if (result != IntervalType.Day && result != IntervalType.Week && result != IntervalType.Month && result != IntervalType.Year)
            {
                result = IntervalType.Other;
            }
            return result;
        }
    }
}

public enum IntervalType
{
    Day = 1,
    Week = 7,
    Month = 30,
    Year = 365,
    //for example, every 2 days
    Other = 0
}

public enum FrequencyType
{
    EveryDay,
    /// <summary>
    /// For example, every 3 days
    /// </summary>
    EverySeveralDays,
    SeveralTimesPerPeriod
}
