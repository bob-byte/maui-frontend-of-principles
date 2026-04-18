using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Principles.Core.Models;

public class ScoreList
{
    private readonly Dictionary<DateOnly, Score> m_scores;

    public ScoreList()
    {
        m_scores = new Dictionary<DateOnly, Score>();
    }

    public Score Get(DateOnly date )
    {
        m_scores.TryGetValue( date, out Score? value );
        if(value == null)
        {
            value = new Score( date, 0.0 );
        }

        return value;
    }

    public IEnumerable<Score> GetAll()
    {
        return m_scores.Values;
    }

    public void Recompute( int complexity, FrequencyOfHabit frequency, ListOfProgressOfHabit progressList, DateOnly from, DateOnly to, bool isNumerical, double? targetValue = null, NumericalHabitType numericalHabitType = NumericalHabitType.AtLeast )
    {
        m_scores.Clear();
        double rollingSum = 0;
        int repeats = frequency.Repeats;
        int intervalInDays = frequency.IntervalLengthInDays;
        double frequencyValue = frequency.Value;
        int[] values = progressList.GetByInterval( from, to ).Select( p => p.Value ).ToArray();
        bool isAtMost = numericalHabitType == NumericalHabitType.AtMost;

        // For non-daily boolean habits, we double the repeats and the intervalInDays to smooth
        // out irregular repetition schedules (for example, weekly habits performed on different
        // days of the week)
        if (frequencyValue < FrequencyOfHabit.MAX_VALUE)
        {
            repeats *= 2;
            intervalInDays *= 2;
        }

        double previousValue = 0.0;
        for (int numValue = 0; numValue < values.Length; numValue++)
        {
            int offset = values.Length - numValue - 1;
            if (isNumerical)
            {
                rollingSum += Math.Max( 0, values[offset] );
                if (offset + intervalInDays < values.Length)
                {
                    rollingSum -= Math.Max( 0, values[offset + intervalInDays] );
                }

                double normalizedRollingSum = rollingSum / 1000;
                if (values[offset] != ProgressValue.SKIP)
                {

                    double percentageCompleted;
                    if (isAtMost)
                    {
                        if (targetValue > 0)
                        {
                            percentageCompleted = 1 - ((normalizedRollingSum - targetValue.Value) / targetValue.Value);
                            percentageCompleted = Math.Clamp( percentageCompleted, 0.0, 1.0 );
                        }
                        else
                        {
                            percentageCompleted = normalizedRollingSum > 0 ? 0.0 : 1.0;
                        }
                    }
                    else
                    {
                        if (targetValue > 0)
                            percentageCompleted = Math.Min( 1.0, normalizedRollingSum / targetValue.Value );
                        else
                            percentageCompleted = 1.0;
                    }

                    previousValue = Score.Compute( frequencyValue, previousValue, percentageCompleted, complexity );
                }
            }
            else
            {
                if (values[offset] == ProgressValue.YES_MANUAL)
                {
                    rollingSum += 1.0;
                }

                if (offset + intervalInDays < values.Length && values[offset + intervalInDays] == ProgressValue.YES_MANUAL)
                {
                    rollingSum -= 1.0;
                }

                if (values[offset] != ProgressValue.SKIP)
                {
                    double percentageAchieved = Math.Min( 1.0, rollingSum / repeats );
                    previousValue = Score.Compute( frequencyValue, previousValue, percentageAchieved, complexity );
                }

                DateOnly date = from.AddDays( numValue );
                m_scores[date] = new Score( date, previousValue );
            }
        }
    }
}

