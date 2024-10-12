using System;
using System.Collections.Concurrent;

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

    public void Recompute( int complexity, FrequencyOfHabit frequency, ListOfProgressOfHabit progressList, DateOnly from, DateOnly to )
    {
        m_scores.Clear();
        double rollingSum = 0;
        int repeats = frequency.Repeats;
        int intervalInDays = frequency.IntervalLengthInDays;
        double frequencyValue = frequency.Value;
        int[] values = progressList.GetByInterval( from, to ).Select( p => p.Value ).ToArray();

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
            if (values[offset] == ProgressValue.YES_MANUAL)
            {
                rollingSum += 1.0;
            }

            //reset rolling sum for new interval
            if(offset + intervalInDays < values.Length
                && values[offset + intervalInDays] == ProgressValue.YES_MANUAL)
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

