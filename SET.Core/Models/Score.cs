using System;
namespace SET.Core.Models
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
            double coefOfComplexity = complexity switch
            {
                1 => 5.6,
                2 => 3.33,
                3 => 2.3,
                4 => 1.7,
                5 => 1.525,
                6 => 1.4,
                7 => 1.0,
                8 => 0.77,
                9 => 0.524,
                10 => 0.392,
                _ => throw new ArgumentException( message: $"Complexity {complexity} of habit is not supported",
                paramName: nameof( complexity ) )
            };

            double exponentialSmoothingFactor = 0.5;
            double scalingFactor = 13.0;
            double decayFactor = Math.Pow( exponentialSmoothingFactor, Math.Sqrt( frequency ) * coefOfComplexity / scalingFactor );

            double score = previousScore * decayFactor;
            score += checkmarkValue * (1 - decayFactor);
            return score;
        }
    }
}

