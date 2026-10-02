using System;

namespace Principles.ViewModels;
public partial class EditHabitViewModel
{
    [RelayCommand( CanExecute = nameof( CanSaveGoal ) )]
    private async Task SaveGoalAsync( System.Action afterAction )
    {
        bool isNewGoal = EditedGoal.Id == 0 && EditedGoal.LocalId == 0;

        await UiBusyFor( async () =>
        {
            string? oldGoalName = null;
            string? oldGoalNotes = null;
            UserGoal? foundGoalInCollection = UserGoals!.FirstOrDefault( g => g.Equals( EditedGoal ) || g.LocalId == EditedGoal.LocalId );
            if (foundGoalInCollection != null)
            {
                oldGoalName = foundGoalInCollection.Name;
                oldGoalNotes = foundGoalInCollection.Notes;
            }

            // Keep Notes from SQLite when the editor only binds Name (avoids wiping Flutter/synced notes).
            if (EditedGoal.LocalId != 0)
            {
                UserGoal? persisted = await GoalService.GetGoalByLocalIdAsync( EditedGoal.LocalId );
                if (persisted is not null &&
                    string.IsNullOrWhiteSpace( EditedGoal.Notes ) &&
                    !string.IsNullOrWhiteSpace( persisted.Notes ))
                {
                    EditedGoal.Notes = persisted.Notes;
                }
            }

            await GoalService.SaveGoalAsync( EditedGoal );

            if (isNewGoal)
            {
                UserGoals!.Add( EditedGoal );
            }
            else if (foundGoalInCollection != null)
            {
                bool nameChanged = oldGoalName != EditedGoal.Name;
                bool notesChanged = !string.Equals( oldGoalNotes, EditedGoal.Notes, StringComparison.Ordinal );

                foundGoalInCollection.Name = EditedGoal.Name;
                foundGoalInCollection.Notes = EditedGoal.Notes;
                foundGoalInCollection.LastModified = EditedGoal.LastModified;
                foundGoalInCollection.Id = EditedGoal.Id;

                if (nameChanged || notesChanged)
                {
                    ReferenceMessenger.Send( new ChangedGoalMessage( EditedGoal ) );
                }

                if (Habit.Goal is not null && (Habit.Goal.Equals( EditedGoal ) || Habit.Goal.LocalId == EditedGoal.LocalId))
                {
                    Habit.Goal.Name = EditedGoal.Name;
                    Habit.Goal.Notes = EditedGoal.Notes;
                    Habit.Goal.LastModified = EditedGoal.LastModified;
                    Habit.Goal.Id = EditedGoal.Id;
                }

                if (nameChanged && !string.IsNullOrWhiteSpace( oldGoalName ))
                {
                    await ReminderService.RenamePendingNotificationTitlesAsync( oldGoalName, EditedGoal.Name! );

                    if (Habit.Reminders is not null)
                    {
                        foreach (UserHabitReminder reminder in Habit.Reminders.Where( r => r.Title == oldGoalName ))
                        {
                            reminder.Title = EditedGoal.Name!;
                        }
                    }
                }
            }

            afterAction();
        } );
    }

    private bool CanSaveGoal()
    {
        return !string.IsNullOrWhiteSpace( EditedGoal?.Name );
    }

    [RelayCommand]
    private async Task DeleteGoalAsync( UserGoal goal )
    {
        bool isConfirmedDelete = await DialogService.ShowAlertWithTwoBtnsAsync(
            LocStrings.MessageInDeleteGoalConfirm,
            LocStrings.DeleteGoalQuestion,
            LocStrings.OK,
            LocStrings.Cancel
        );

        if (isConfirmedDelete)
        {
            await UiBusyFor( async () =>
            {
                await GoalService.DeleteGoalAsync( goal );

                UserGoals!.Remove( goal );
                ReferenceMessenger.Send( new GoalIsDeletedMessage( goal ) );

                if (Habit.Goal is not null && (Habit.Goal.Equals( goal ) || Habit.Goal.LocalId == goal.LocalId) &&
                    ClearHabitGoalCommand.CanExecute( null ))
                {
                    ClearHabitGoalCommand.Execute( null );
                }
            } );
        }
    }

    [RelayCommand]
    private void ClearHabitGoal()
    {
        Habit.Goal = null;
    }

    [RelayCommand]
    public async Task OnGoalNameTapped( UserGoal goal )
    {
        if (goal is null)
        {
            return;
        }

        EditedGoal = await CopyGoalForEditorAsync( goal );
        Habit.Goal = goal;
    }

    [RelayCommand]
    private async Task OpenUpdateGoalPopup( UserGoal goal )
    {
        EditedGoal = await CopyGoalForEditorAsync( goal );
    }

    private async Task<UserGoal> CopyGoalForEditorAsync( UserGoal goal )
    {
        UserGoal? fresh = goal.LocalId != 0
            ? await GoalService.GetGoalByLocalIdAsync( goal.LocalId )
            : null;

        UserGoal source = fresh ?? goal;
        return new UserGoal
        {
            LocalId = source.LocalId,
            Id = source.Id,
            Name = source.Name,
            Notes = source.Notes,
            LastModified = source.LastModified
        };
    }

    [RelayCommand]
    private async Task ReloadGoalsAsync()
    {
        UserGoals = new ObservableCollectionEx<UserGoal>();
        IEnumerable<UserGoal> goals = await GoalService.UserGoalsAsync( forceReload: true );

        //sometimes goals are not displayed without reloading
        UserGoals.Reload( goals );

        GoalService.StoredGoals = UserGoals;
    }
}
