using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.ViewModels;
public partial class EditHabitViewModel
{
    [RelayCommand]
    private async Task ReloadRecommendedHabitsAsync( Action afterAction )
    {
        IsRecommendedHabitsLoading = true;

        bool doTryAgain;

        do
        {
            try
            {
                if (RecommendedHabits.Any())
                {
                    RecommendedHabits.Clear();
                }

                IEnumerable<UserHabit> userHabits = UserHabits.Where( u => u.Id > 0 );
                List<RecommendedHabit> recommendedHabits = await AiRecommenderOfHabits.RecommendedHabitsAsync(
                    userHabits,
                    Habit.AreasOfLife!,
                    Gender,
                    Mission,
                    MainSlogan,
                    Habit.Goal!.Name
                );
                RecommendedHabits.Reload( recommendedHabits );

                doTryAgain = false;

                afterAction();
            }
            catch (Exception ex)
            {
                doTryAgain = await DoRetryOperationOnErrorAsync( ex );
            }
        }
        while (doTryAgain);

        IsRecommendedHabitsLoading = false;
    }

    [RelayCommand]
    private void SelectedRecommendedHabit( RecommendedHabit recommendedHabit )
    {
        NameOfHabit.Value = recommendedHabit.Name;

        if (string.IsNullOrWhiteSpace( Habit.Description ))
        {
            Habit.Description = recommendedHabit.ReasonToFollow;
        }
        else
        {
            Habit.Description += $"{Environment.NewLine}{Environment.NewLine}{recommendedHabit.ReasonToFollow}";
        }
    }
}
