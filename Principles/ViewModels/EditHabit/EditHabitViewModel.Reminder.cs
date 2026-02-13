using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.ViewModels;
public partial class EditHabitViewModel
{
    public async Task AddReminderAsync()
    {
        await ReminderService.RequestAccessToSendNotificationsAsync();

        UserHabitReminder reminder = new()
        {
            Title = EditedReminder.Title.Clone() as string,
            Description = EditedReminder.Description.Clone() as string,
            Time = TimeOnly.FromDateTime( EditedReminder.Time ),
            IsEnabled = EditedReminder.IsEnabled,
            DaysOfWeek = new List<WeekDay>( EditedReminder.DaysOfWeek )
        };

        Habit.Reminders ??= new ObservableCollectionEx<UserHabitReminder>();

        if (Habit.Reminders.Count == 1)
        {
            reminder.Id = Habit.Reminders[0].Id;
            Habit.Reminders[0] = reminder;
        }
        else
        {
            Habit.Reminders.Add( reminder );
        }

        OnPropertyChanged( nameof( EditedReminder ) );
    }

    private void RemoveInactiveNotifications( List<WeekDay> inactiveDays )
    {
        foreach (WeekDay day in inactiveDays)
        {
            LocalNotificationCenter.Current.Cancel( day.UserNotificationRequestId );
        }
    }
    
}
