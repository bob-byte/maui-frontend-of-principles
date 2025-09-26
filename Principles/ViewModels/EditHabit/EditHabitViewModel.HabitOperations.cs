using CommunityToolkit.Maui.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.ViewModels;
public partial class EditHabitViewModel
{
    [RelayCommand( CanExecute = nameof( CanSave ) )]
    private async Task SaveAsync()
    {
        bool canSaveHabit = await CanSaveHabitAsync();

        if (canSaveHabit)
        {
            Habit.Name = NameOfHabit.Value;
            EditUserHabitDto dto = new()
            {
                AreasOfLife = Habit.AreasOfLife!.ToList(),//get copy, because it will be changed
                ColorName = Habit.ColorName,
                Description = Habit.Description,
                Complexity = Habit.Complexity,
                Frequency = Habit.Frequency,
                Id = Habit.Id,
                Name = Habit.Name,
                Priority = Habit.Priority,
                Goal = Habit.Goal,
                Status = Habit.Status,
                Type = Habit.Type,
                Reminders = Habit.Reminders,
                IsArchived = Habit.IsArchived,
                PrioritizedHabits = new List<UserHabitWithPriority>()
            };

            dto.AreasOfLife!.Remove( AllAreasOfLifeAsOneItem );

            List<UserHabit> copyOfHabits = new( ServiceOfHabit.StoredUserHabits! );

            foreach (UserHabit habit in copyOfHabits)
            {
                dto.PrioritizedHabits.Add( new UserHabitWithPriority { Id = habit.Id, Priority = habit.Priority } );
            }

            bool isHabitSaved = false;
            
            await UiBusyFor( async () =>
            {
                UserHabit savedHabit = Habit;
                if (copyOfHabits.Count == 1)
                {
                    await ServiceOfHabit.UpdateHabitAsync( Habit );
                    await HandleHabitSaveAsync();

                    if (dto.IsArchived)
                    {
                        ReferenceMessenger.Send( new ArchiveHabitMessage( savedHabit, doShowAllArchivedHabits: true ) );
                    }
                    else
                    {
                        ReferenceMessenger.Send( new HabitSavedMessage( savedHabit ) );
                    }

                    await Navigation.GoBackAsync();
                }
                else
                {
                    await ServiceOfHabit.UpdateHabitAsync( Habit );
                    await HandleHabitSaveAsync();
                    await Navigation.GoBackAsync();

                    if (dto.IsArchived)
                    {
                        ReferenceMessenger.Send( new ArchiveHabitMessage( savedHabit, doShowAllArchivedHabits: true ) );
                    }
                    else
                    {
                        ReferenceMessenger.Send( new HabitSavedMessage( savedHabit ) );
                    }
                }

                isHabitSaved = true;
            } );

            if (isHabitSaved)
            {
                await TipService.ShowToastAsync( LocStrings.TheHabitSuccessfullySaved );
            }
        }
    }

    public bool CanSave()
    {
        NameOfHabit.Validate();

        return NameOfHabit.IsValid && !IsBusy && !IsLoadingHabitInfo;
    }
    
    private async Task<bool> CanSaveHabitAsync()
    {
        bool result;
        
        if (IsNewHabit)
        {
            result = ServiceOfHabit.IsItRecommendedToCreateNewHabit( Habit );

            if (!result)
            {
                MultipleActionPopupViewModel viewModel =
                    ServiceProvider.GetRequiredService<MultipleActionPopupViewModel>();
                var multipleActionPopup = new MultipleActionPopup( viewModel );

                List<ActionData> availableActions =
                [
                    new(LocStrings.ArchiveHabit, () =>
                        {
                            Habit.IsArchived = true;
                            result = true;
                            return Task.CompletedTask;
                        }
                    ),

                    new(LocStrings.CreateItAnyway, () =>
                        {
                            result = true;
                            return Task.CompletedTask;
                        }
                    )
                ];

                viewModel.Configure( availableActions, LocStrings.DescriptionOfAdviceNotToWorkOnNewHabit, LocStrings.TitleOfAdviceNotToWorkOnNewHabit );

                await Shell.Current.ShowPopupAsync( multipleActionPopup );
            }
        }
        else
        {
            result = true;
        }
        
        return result; 
    }

    private async Task HandleHabitSaveAsync()
    {
        if (InactiveDaysToDelete.Count > 0)
        {
            RemoveInactiveNotifications( InactiveDaysToDelete );
        }
    }

    [RelayCommand]
    private async Task ArchiveHabitAsync()
    {
        Habit.IsArchived = true;

        await TipService.ShowToastAsync( LocStrings.TheHabitWillBeArchivedAfterSaving ).DefaultConfigureAwait();
    }

    [RelayCommand]
    private async Task UnarchiveHabitAsync()
    {
        Habit.IsArchived = false;
        
        await TipService.ShowToastAsync( LocStrings.TheHabitWillBeUnarchivedAfterSaving ).DefaultConfigureAwait();
    }
    
    [RelayCommand]
    private async Task DeleteHabitAsync( object? obj )
    {
        if (obj is UserHabit habit)
        {
            bool doDelete = await DialogService.ShowConfirmAsync(
                LocStrings.MessageInDeleteHabitConfirm,
                LocStrings.DeleteHabitQuestion
            );

            if (doDelete)
            {
                await UiBusyFor( async () =>
                {
                    await ServiceOfHabit.DeleteAsync( habit );
                    
                    await Navigation.GoToInitialViewAsync();
                    ReferenceMessenger.Send( new HabitsDeletedMessege( habit ) );
                    
                    ServiceOfHabit.StoredUserHabits!.Remove( habit );
                } ).DefaultConfigureAwait();
            }
        }
    }
}
