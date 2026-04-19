using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.ViewModels;
public partial class EditHabitViewModel
{
    [RelayCommand( CanExecute = nameof( CanSaveGoal ) )]
    private async Task SaveGoalAsync( Action afterAction )
    {
        bool isNewGoal = EditedGoal.Id == 0 && EditedGoal.LocalId == 0;

        await UiBusyFor( async () =>
        {
            string? oldGoalName = null;
            UserGoal? foundGoalInCollection = UserGoals!.FirstOrDefault( g => g.Equals( EditedGoal ) || g.LocalId == EditedGoal.LocalId );
            if (foundGoalInCollection != null)
            {
                oldGoalName = foundGoalInCollection.Name;
            }

            await GoalService.SaveGoalAsync( EditedGoal );

            if (isNewGoal)
            {
                UserGoals!.Add( EditedGoal );
            }
            else
            {
                if (foundGoalInCollection != null && oldGoalName != EditedGoal.Name)
                {
                    foundGoalInCollection.Name = EditedGoal.Name;
                    ReferenceMessenger.Send( new ChangedGoalMessage( EditedGoal ) );

                    if (Habit.Goal is not null && (Habit.Goal.Equals( EditedGoal ) || Habit.Goal.LocalId == EditedGoal.LocalId))
                    {
                        Habit.Goal.Name = EditedGoal.Name;
                    }

                    IList<NotificationRequest> notifications = await ReminderService.GetPendingLocallyAsync();
                    if (!string.IsNullOrWhiteSpace( oldGoalName ))
                    {
                        if (Habit.Reminders is not null)
                        {
                            foreach (UserHabitReminder reminder in Habit.Reminders.Where( r => r.Title == oldGoalName ))
                            {
                                reminder.Title = EditedGoal.Name!;
                            }
                        }

                        foreach (NotificationRequest notification in notifications.Where( n => n.Title == oldGoalName ))
                        {
                            notification.Title = EditedGoal.Name!;
                            await ReminderService.SaveLocallyAsync( notification );
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
    public void OnGoalNameTapped( UserGoal goal )
    {
        if (goal != null)
        {
            EditedGoal = new UserGoal
            {
                LocalId = goal.LocalId,
                Id = goal.Id,
                Name = goal.Name
            };
            Habit.Goal = goal;
        }
    }

    [RelayCommand]
    private void OpenUpdateGoalPopup( UserGoal goal )
    {
        EditedGoal = new UserGoal
        {
            LocalId = goal.LocalId,
            Id = goal.Id,
            Name = goal.Name
        };
    }

    [RelayCommand]
    private async Task ReloadGoalsAsync()
    {
        UserGoals = new ObservableCollectionEx<UserGoal>();
        IEnumerable<UserGoal> goals = await GoalService.UserGoalsAsync();

        //sometimes goals are not displayed without reloading
        UserGoals.Reload( goals );

        GoalService.StoredGoals = UserGoals;
    }
}
