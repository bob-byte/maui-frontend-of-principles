using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Models;
public partial class HabitFrequencyInfo : ObservableObject
{
    [ObservableProperty]
    private FrequencyOfHabit? m_frequency;
    [ObservableProperty]
    private PeriodOfHabit? m_period;

    public void NotifyPropertyChanged(string propertyName )
    {
        OnPropertyChanged( propertyName );
    }
}
