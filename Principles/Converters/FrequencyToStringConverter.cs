using CommunityToolkit.Maui.Converters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Converters;
public class FrequencyToStringConverter : BaseConverterOneWay<HabitFrequencyInfo, string>
{
    public override string DefaultConvertReturnValue { get; set; } = string.Empty;

    public override string ConvertFrom( HabitFrequencyInfo value, CultureInfo? culture )
    {
        if (value?.Frequency == null || value?.Period == null)
            return DefaultConvertReturnValue;

        FrequencyOfHabit frequency = value.Frequency;
        PeriodOfHabit period = value.Period;

        if (frequency.Repeats == 1)
        {
            switch (frequency.Type)
            {
                case FrequencyType.EveryDay:
                    return LocStrings.EveryDay;

                case FrequencyType.EverySeveralDays:
                    return $"{LocStrings.Every} {frequency.IntervalLengthInDays} {LocStrings.days}";

                case FrequencyType.SeveralTimesPerPeriod:
                    switch (period.Type)
                    {
                        case PeriodTypeOfHabit.Month:
                            return LocStrings.EveryMonth;

                        case PeriodTypeOfHabit.Year:
                            return LocStrings.EveryYear;

                        default:
                            return LocStrings.EveryWeek;
                    }

                default:
                    return DefaultConvertReturnValue;
            }
        }

        return $"{frequency.Repeats} {LocStrings.timesPer} {period.Name}";
    }

}
