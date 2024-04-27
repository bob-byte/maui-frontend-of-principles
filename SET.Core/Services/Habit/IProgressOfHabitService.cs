namespace SET.Core.Services;

public interface IProgressOfHabitService
{
    Task UpdateAsync( ProgressOfHabit progressOfHabit );

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
    /// <param name="timesCountToAchieve">
    /// Total number of days for completing the habit. It provides flexibility to
    /// adapt the function to different habit complexity.
    /// </param>
    /// <returns>
    /// Score typically ranges between 0.0 and 1.0, where 0.0 indicates no progress
    /// or failure to maintain the habit, and 1.0 represents perfect and continuous adherence
    /// to the habit. A score close to 1.0 indicates that the user is doing well in maintaining the habit.
    /// </returns>
    double ComputeScore( double frequency, double previousScore, double checkmarkValue, int timesCountToAchieve );
    int ConvertScoreToPercentage( double score );
}