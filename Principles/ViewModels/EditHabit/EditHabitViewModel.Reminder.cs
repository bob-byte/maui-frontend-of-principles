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
    private async Task AddNotificationToDeviceAsync( UserHabitReminder reminder, WeekDay weekDay )
    {
        if (reminder.IsEnabled)
        {
            DateTime currentDate = DateTime.Now;
            TimeSpan currentTime = currentDate.TimeOfDay;

            int reminderDayIndex = (int)weekDay.Type;

            int currentDayIndex = (int)currentDate.DayOfWeek;

            int daysUntilNextReminder = (reminderDayIndex - currentDayIndex + 7) % 7;

            if (daysUntilNextReminder == 0 && reminder.Time.ToTimeSpan() < currentTime)
            {
                daysUntilNextReminder = 7;
            }

            DateTime notifyDateTime = currentDate.Date
                .AddDays( daysUntilNextReminder )
                .Add( reminder.Time.ToTimeSpan() );

            await ReminderService.SaveLocallyAsync(
                weekDay.UserNotificationRequestId,
                reminder.Title,
                reminder.Description,
                notifyDateTime,
                ReminderRepeat.Weekly
            );
        }
        else if (!IsNewHabit && weekDay.UserNotificationRequestId != 0)
        {
            //TODO: test how it works for new habit
            LocalNotificationCenter.Current.Cancel( weekDay.UserNotificationRequestId );
        }
    }
}
