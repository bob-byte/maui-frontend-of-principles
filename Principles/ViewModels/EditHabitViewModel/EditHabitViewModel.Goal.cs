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
        bool isNewGoal = EditedGoal.Id == 0;

        await UiBusyFor( async () =>
        {
            DtoWithId response = await GoalService.SaveGoalAsync( EditedGoal );
            EditedGoal.Id = response.Id;

            if (isNewGoal)
            {
                UserGoals!.Add( EditedGoal );
            }
            else
            {
                // Store the old goal name before local saving changes
                string? oldGoalName = null;
                UserGoal? foundGoalInCollection = UserGoals!.FirstOrDefault( g => g.Id == EditedGoal.Id );
                if (foundGoalInCollection != null)
                {
                    oldGoalName = foundGoalInCollection.Name;

                    if (oldGoalName != EditedGoal.Name)
                    {
                        foundGoalInCollection.Name = EditedGoal.Name;
                        ReferenceMessenger.Send( new ChangedGoalMessage( EditedGoal ) );

                        if (Habit.Goal?.Id == EditedGoal.Id)
                        {
                            Habit.Goal.Name = EditedGoal.Name;
                        }

                        // Get all notifications and update the relevant ones
                        IList<NotificationRequest> notifications =
                            await LocalNotificationCenter.Current.GetPendingNotificationList();
                        if (!string.IsNullOrWhiteSpace( oldGoalName ))
                        {
                            if (Habit.Reminders is not null)
                            {
                                foreach (UserHabitReminder reminder in
                                             Habit.Reminders.Where( r => r.Title == oldGoalName ))
                                {
                                    reminder.Title = EditedGoal.Name!;
                                }
                            }

                            foreach (NotificationRequest? notification in notifications.Where( n =>
                                         n.Title == oldGoalName ))
                            {
                                notification.Title = EditedGoal.Name!;
                                await ReminderService.SaveAsync( notification );
                            }
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

                if (Habit.Goal?.Id > 0 && Habit.Goal.Id == goal.Id && ClearHabitGoalCommand.CanExecute( null ))
                {
                    ClearHabitGoalCommand.Execute( null );
                }
            } );
        }
    }

    [RelayCommand]
    private void ClearHabitGoal()
    {
        if (Habit.Goal is not null)
        {
            Habit.Goal.Id = 0;
            Habit.Goal.Name = null;
        }
    }

    [RelayCommand]
    public void OnGoalNameTapped( UserGoal goal )
    {
        if (goal != null)
        {
            EditedGoal = new UserGoal
            {
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
