using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.ViewModels;
public partial class EditHabitViewModel
{
    [RelayCommand]
    private async Task BackAsync()
    {
        Dictionary<string, object> routeParams = [];
        routeParams["ShowAd"] = false;

        await Navigation.GoBackAsync(routeParams);
    }

    [RelayCommand]
    private Task TapWithoutExceptionsTypeAsync( VisualElement visualElement )
    {
        Habit.Type = TypeOfHabit.Principled;
        return visualElement.DisplaySnackbar(
            LocStrings.WithoutExceptionsHabitTypeShortDescription,
            duration: TimeSpan.FromMinutes( 1 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    [RelayCommand]
    private Task TapHabitNameInfoAsync( VisualElement visualElement )
    {
        return visualElement.DisplaySnackbar(
            LocStrings.HabitNameRecommendation,
            duration: TimeSpan.FromSeconds( 6 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    [RelayCommand]
    private Task ShowSnackbarForQuestion( VisualElement visualElement )
    {
        return visualElement.DisplaySnackbar(
            LocStrings.QuestionExplanation,
            duration: TimeSpan.FromMinutes( 1 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    [RelayCommand]
    private Task TapIntegrallyWiseTypeAsync( VisualElement visualElement )
    {
        Habit.Type = TypeOfHabit.Flexible;
        return visualElement.DisplaySnackbar(
            LocStrings.IntegrallyWiseHabitTypeShortDescription,
            duration: TimeSpan.FromMinutes( 1 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    [RelayCommand]
    private Task TapComplexityInfoAsync( VisualElement visualElement )
    {
        return visualElement.DisplaySnackbar(
            LocStrings.ComplexityInfoText,
            duration: TimeSpan.FromSeconds( 6 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }
    //Todo: Change Description to MindHabitTypeShortDescription
    [RelayCommand]
    private Task TapMindTypeAsync( VisualElement visualElement )
    {
        Habit.Type = TypeOfHabit.Mind;
        return visualElement.DisplaySnackbar(
            LocStrings.WithoutExceptionsHabitTypeShortDescription,
            duration: TimeSpan.FromMinutes( 1 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }
}
