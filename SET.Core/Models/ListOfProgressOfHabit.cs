using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;

namespace SET.Core.Models
{
    public class ListOfProgressOfHabit : INotifyCollectionChanged
    {
        private readonly Dictionary<DateOnly, ProgressOfHabit> m_progresses;
        private readonly TimeOnly m_zeroTime;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public ListOfProgressOfHabit(UserHabit habit)
        {
            m_progresses = new Dictionary<DateOnly, ProgressOfHabit>();
            Habit = habit;
            m_zeroTime = TimeOnly.FromTimeSpan( TimeSpan.Zero );
        }

        public UserHabit Habit { get; }

        public DateOnly? StartInterval { get; set; }
        public DateOnly? EndInterval { get; set; }

        public ProgressOfHabit Get( DateOnly date )
        {
            m_progresses.TryGetValue( date, out ProgressOfHabit? result );
            if (result is null)
            {
                result = new ProgressOfHabit
                {
                    Date = date,
                    Habit = Habit,
                    Value = ProgressValue.UNKNOWN
                };
            }

            return result;
        }

        public void Set( ProgressOfHabit progress )
        {
            m_progresses[progress.Date] = progress;
            //CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.))
        }

        public List<ProgressOfHabit> GetByInterval( DateOnly from, DateOnly to )
        {
            List<ProgressOfHabit> result = new();
            if(from <= to)
            {
                DateOnly current = to;
                while(current >= from)
                {
                    result.Add( Get( current ) );
                    current = current.AddDays( -1 );
                }
            }

            return result;
        }

        public IEnumerable<ProgressOfHabit> GetKnown()
        {
            return m_progresses.Values.OrderByDescending( p => p.Date );
        }

        public void RecomputeFrom(IEnumerable<ProgressOfHabit> originalKnown, FrequencyOfHabit frequency, bool isNumerical )
        {
            Clear();

            if (isNumerical)
            {
                foreach (ProgressOfHabit it in originalKnown)
                {
                    Set( it );
                }
            }
            else
            {
                List<Interval> intervals = BuildIntervals( frequency, originalKnown );
                SnapIntervalsTogether( intervals );
                List<ProgressOfHabit> computed = BuildProgressesFromInterval( originalKnown, intervals );

                foreach (ProgressOfHabit progress in computed.Where( p => p.Value != ProgressValue.UNKNOWN || !string.IsNullOrWhiteSpace( p.Notes ) ))
                {
                    Set( progress );
                }
            }
        }

        public void Clear()
        {
            m_progresses.Clear();
        }

        private List<Interval> BuildIntervals(FrequencyOfHabit frequency, IEnumerable<ProgressOfHabit> progresses)
        {
            List<ProgressOfHabit> filtered = progresses.Where( p => p.Value == ProgressValue.YES_MANUAL ).ToList();
            int repeats = frequency.Repeats;
            int intervalInDays = frequency.IntervalLengthInDays;

            List<Interval> result = new();
            for (int i = repeats - 1; i < filtered.Count; i++)
            {
                DateOnly begin = filtered[i].Date;
                DateOnly center = filtered[i - repeats + 1].Date;
                int size = intervalInDays;

                if(frequency.IntervalType == IntervalType.Month)
                {
                    size = begin.Day == DateTime.DaysInMonth( begin.Year, begin.Month )
                        ? DateTime.DaysInMonth( begin.Year, begin.Month + 1 )
                        : DateTime.DaysInMonth( begin.Year, begin.Month );
                }
                else if (frequency.IntervalType == IntervalType.Year)
                {
                    DateTime beginDate = begin.ToDateTime( m_zeroTime ).ToLocalTime();
                    int year = begin.Day == 31 && begin.Month == 12
                        ? beginDate.Year + 1
                        : beginDate.Year;

                    size = DateTime.IsLeapYear( year )
                        ? 366
                        : 365;
                }

                if(begin.DaysUntil(center) < size)
                {
                    DateOnly end = begin.AddDays( size - 1 );
                    result.Add( new Interval
                    {
                        Begin = begin,
                        Center = center,
                        End = end
                    } );
                }
            }

            return result;
        }

        private void SnapIntervalsTogether(List<Interval> intervals)
        {
            for (int numInterval = 1; numInterval < intervals.Count; numInterval++)
            {
                Interval current = intervals[numInterval];
                //we go from end to start
                Interval next = intervals[numInterval - 1];
                int gapOfNextToCurrent = next.Begin.DaysUntil( current.End );
                
                if(gapOfNextToCurrent >= 0)
                {
                    int gapOfCenterToEnd = current.Center.DaysUntil( current.End );
                    int shift = Math.Min( gapOfCenterToEnd, gapOfNextToCurrent + 1 );
                    intervals[numInterval] = new Interval
                    {
                        Begin = current.Begin.AddDays( -shift ),
                        Center = current.Center,
                        End = current.End.AddDays( -shift )
                    };
                }
            }
        }

        private List<ProgressOfHabit> BuildProgressesFromInterval(IEnumerable<ProgressOfHabit> original, List<Interval> intervalList )
        {
            List<ProgressOfHabit> result = new();
            if(!original.Any())
            {
                return result;
            }

            DateOnly from;
            DateOnly to;
            if (StartInterval == null || EndInterval == null)
            {
                from = original.First().Date;
                to = original.First().Date;

                foreach (ProgressOfHabit progress in original)
                {
                    if (progress.Date < from)
                    {
                        from = progress.Date;
                    }

                    if (progress.Date > to)
                    {
                        to = progress.Date;
                    }
                }

                foreach (Interval interval in intervalList)
                {
                    if (interval.Begin < from)
                    {
                        from = interval.Begin;
                    }

                    if (interval.End > to)
                    {
                        to = interval.End;
                    }
                }
            }
            else
            {
                from = (DateOnly)StartInterval;
                to = (DateOnly)EndInterval;
            }

            //create unknown progresses
            DateOnly current = to;
            while (current >= from)
            {
                result.Add( new ProgressOfHabit
                {
                    Date = current,
                    Value = ProgressValue.UNKNOWN
                } );
                current = current.AddDays( value: -1 );
            }

            //Create YES_AUTO progresses of habit
            foreach (Interval interval in intervalList)
            {
                current = interval.End;
                while(current >= interval.Begin)
                {
                    int offset = current.DaysUntil( to );
                    result[offset] = new ProgressOfHabit
                    {
                        Date = current,
                        Value = ProgressValue.YES_AUTO
                    };
                    current = current.AddDays( -1 );
                }
            }

            //copy original
            foreach (ProgressOfHabit progress in original)
            {
                int offset = progress.Date.DaysUntil( to );
                if (result[offset].Value == ProgressValue.UNKNOWN
                    || progress.Value == ProgressValue.SKIP
                    || progress.Value == ProgressValue.YES_MANUAL)
                {
                    result[offset] = progress;
                }
            }

            return result;
        }
    }
}

