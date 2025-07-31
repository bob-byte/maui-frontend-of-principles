using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace Principles.Converters;

public class ComplexityToHelpTextConverter : BaseConverterOneWay<int, string>
{
    public override string DefaultConvertReturnValue { get; set; } = "";

    public override string ConvertFrom(int complexity, CultureInfo? culture)
    {
        EditHabitViewModel viewModel = ServiceLocator.Current!.GetRequiredService<EditHabitViewModel>();
        UserHabit? habit = viewModel.Habit;
        
        string result;
        if (habit?.IsNew() == true)
        {
            try
            {
                int daysCount = HabitComplexity.ToDaysCount( habit.Complexity );
                result =
                    $"{LocStrings.HabitAutomationExplanation} {daysCount} {LocStrings.days}.";
            }
            catch
            {
                result = string.Empty;
            }
        }
        else if (habit is null || habit.Frequency is null || habit.Progresses is null)
        {
            result = string.Empty;
        }
        else
        {
            try
            {
                IServiceLocator serviceLocator = ServiceLocator.Current!;
                IServiceOfHabit serviceOfHabit = serviceLocator.GetRequiredService<IServiceOfHabit>();
                
                int daysCount = serviceOfHabit.GetDaysUntilFullAutomation( habit );
                if (daysCount == 0)
                {
                    result = LocStrings.TheHabitIsAlreadyAutomated;
                }
                else
                {
                    result = daysCount < 30
                        ? $"{LocStrings.Just} {daysCount} {LocStrings.daysToGoUntilHabitAutomatic}."
                        : $"{LocStrings.ThereAreStill} {daysCount} {LocStrings.daysToGoUntilHabitAutomatic}.";
                }
            }
            catch (Exception ex)
            {
                IServiceLocator serviceLocator = ServiceLocator.Current!;
                ISettingsService settingsService = serviceLocator.GetRequiredService<ISettingsService>();
                if (settingsService.IsDebug)
                {
                    IDialogService dialogService = serviceLocator.GetRequiredService<IDialogService>();
                    dialogService.ShowErrorAsync( ex.ToString() );
                }
                else
                {
                    ILoggingService loggingService = serviceLocator.GetRequiredService<ILoggingService>();
                    loggingService.LogCriticalError( ex );
                }

                result = string.Empty;
            }
        }

        return result;
    }
}
