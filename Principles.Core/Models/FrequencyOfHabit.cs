using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;

[Table( "FrequencyOfHabit" )]
public partial class FrequencyOfHabit : ObservableObject, ICloneable, IEntity
{
    public const double MAX_VALUE = 1.0;

    [ObservableProperty]
    private FrequencyType m_type = FrequencyType.EveryDay;

    [ObservableProperty]
    private int m_repeats = 1;

    [ObservableProperty]
    private int m_intervalLengthInDays = 1;

    public long Id { get; set; }

    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }

    public DateTime LastModified { get; set; }

    [Ignore]
    public double Value =>
        (double)Repeats / IntervalLengthInDays;

    [Ignore]
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

    public object Clone()
    {
        return MemberwiseClone();
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
