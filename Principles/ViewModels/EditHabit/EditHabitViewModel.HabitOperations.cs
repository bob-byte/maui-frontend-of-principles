using CommunityToolkit.Maui.Views;

namespace Principles.ViewModels;

public partial class EditHabitViewModel
{
    [RelayCommand( CanExecute = nameof( CanSave ) )]
    private async Task SaveAsync()
    {
        bool canSaveHabit = await CanSaveHabitAsync();
        if (!canSaveHabit)
        {
            return;
        }

        Habit.Name = NameOfHabit.Value;
        Habit.AreasOfLife ??= new ObservableCollectionEx<UserAreaOfLife>();
        Habit.AreasOfLife.Remove( AllAreasOfLifeAsOneItem );

        await UiBusyFor( async () =>
        {
            await ServiceOfHabit.UpdateHabitAsync( Habit );
            UserHabit savedHabit = await ServiceOfHabit.UserHabitAsync( Habit.LocalId );
            HandleHabitSaveAsync();

            if (savedHabit.IsArchived)
            {
                ReferenceMessenger.Send( new ArchiveHabitMessage( savedHabit, doShowAllArchivedHabits: true ) );
            }
            else
            {
                ReferenceMessenger.Send( new HabitSavedMessage( savedHabit ) );
            }

            await Navigation.GoBackAsync();
        } );

        await TipService.ShowToastAsync( LocStrings.TheHabitSuccessfullySaved );
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
                List<ActionData> availableActions =
                [
                    new( LocStrings.ArchiveHabit, () =>
                        {
                            Habit.IsArchived = true;
                            result = true;
                            return Task.CompletedTask;
                        } ),
                    new( LocStrings.CreateItAnyway, () =>
                        {
                            result = true;
                            return Task.CompletedTask;
                        } )
                ];

                Dictionary<string, object> popupParameters = new()
                {
                    ["AvailableActions"] = availableActions,
                    ["Description"] = LocStrings.DescriptionOfAdviceNotToWorkOnNewHabit,
                    ["Title"] = LocStrings.TitleOfAdviceNotToWorkOnNewHabit
                };

                await DialogService.ShowPopupAsync<MultipleActionPopupViewModel>( popupParameters );
            }
        }
        else
        {
            result = true;
        }

        return result;
    }

    private void HandleHabitSaveAsync()
    {
        if (InactiveDaysToDelete.Count > 0)
        {
            RemoveInactiveNotifications( InactiveDaysToDelete );
            InactiveDaysToDelete.Clear();
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
        if (obj is not UserHabit habit)
        {
            return;
        }

        bool doDelete = await DialogService.ShowConfirmAsync(
            LocStrings.MessageInDeleteHabitConfirm,
            LocStrings.DeleteHabitQuestion
        );

        if (!doDelete)
        {
            return;
        }

        await UiBusyFor( async () =>
        {
            await ServiceOfHabit.DeleteAsync( habit );

            await Navigation.GoToInitialViewAsync();
            ReferenceMessenger.Send( new HabitsDeletedMessege( habit ) );
            ServiceOfHabit.StoredUserHabits?.Remove( habit );
        } ).DefaultConfigureAwait();
    }
}
