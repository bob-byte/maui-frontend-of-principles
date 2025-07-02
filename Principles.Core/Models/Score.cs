using System;
namespace Principles.Core.Models
{
    public class Score
    {
        public Score( DateOnly date, double value )
        {
            Date = date;
            Value = value;
        }

        public DateOnly Date { get; set; }
        public double Value { get; set; }

        /// <summary>
        /// Given the frequency of the habit, the previous score, and the value of
        /// the current checkmark, computes the current score for the habit.
        /// </summary>
        /// <param name="frequency">
        /// Number of repetitions of the habit divided by the length of the interval. For example, if a habit should be repeated 3 times in 8 days, frequency would be 3.0 / 8.0 = 0.375.
        /// </param>
        /// <param name="previousScore">
        /// habit's progress score before the current evaluation. This score reflects
        /// how well the user has been doing with the habit before the current assessment.
        /// </param>
        /// <param name="checkmarkValue">
        /// This parameter represents the value of the current checkmark or the
        /// user's performance in the current evaluation. It's an indicator of
        /// how well the user did on the current day or during the current evaluation of the habit.
        /// </param>
        /// <param name="complexity">
        /// Total number of days for completing the habit. It provides flexibility to
        /// adapt the function to different habit complexity.
        /// </param>
        /// <returns>
        /// Score typically ranges between 0.0 and 1.0, where 0.0 indicates no progress
        /// or failure to maintain the habit, and 1.0 represents perfect and continuous adherence
        /// to the habit. A score close to 1.0 indicates that the user is doing well in maintaining the habit.
        /// </returns>
        public static double Compute( double frequency, double previousScore, double checkmarkValue, int complexity )
        {
            double coefOfComplexity = HabitComplexity.ToCoefficient( complexity );

            double exponentialSmoothingFactor = 0.5;
            double scalingFactor = 13.0;
            double decayFactor = Math.Pow( exponentialSmoothingFactor,
                Math.Sqrt( frequency ) * coefOfComplexity / scalingFactor );

            double score = previousScore * decayFactor;
            score += checkmarkValue * (1 - decayFactor);
            return score;
        }

        public static double Get( int complexity, FrequencyOfHabit frequency, List<int> values )
        {
            double rollingSum = 0;
            int repeats = frequency.Repeats;
            int intervalInDays = frequency.IntervalLengthInDays;
            double frequencyValue = frequency.Value;

            // For non-daily boolean habits, we double the repeats and the intervalInDays to smooth
            // out irregular repetition schedules (for example, weekly habits performed on different
            // days of the week)
            if (frequencyValue < FrequencyOfHabit.MAX_VALUE)
            {
                repeats *= 2;
                intervalInDays *= 2;
            }

            double value = 0.0;
            for (int numValue = 0; numValue < values.Count; numValue++)
            {
                int offset = values.Count - numValue - 1;
                if (values[offset] == ProgressValue.YES_MANUAL)
                {
                    rollingSum += 1.0;
                }

                //reset rolling sum for new interval
                if (offset + intervalInDays < values.Count
                    && values[offset + intervalInDays] == ProgressValue.YES_MANUAL)
                {
                    rollingSum -= 1.0;
                }

                if (values[offset] != ProgressValue.SKIP)
                {
                    double percentageAchieved = Math.Min( 1.0, rollingSum / repeats );
                    value = Compute( frequencyValue, value, percentageAchieved, complexity );
                }
            }

            return value;
        }
        
        

        public static int Round( double score )
        {
            int result = (int)Math.Round( score * 100.0, MidpointRounding.ToEven );
            return result;
        }
    }
}

