using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.ViewModels;
public partial class EditHabitViewModel
{
    public async Task<bool> AddReminderAsync()
    {
        EditedReminder ??= new EditedUserHabitReminder();
        if (EditedReminder.IsEnabled)
        {
            bool canSendNotifications = await ReminderService.RequestAccessToSendNotificationsAsync();
            if (!canSendNotifications)
            {
                await DialogService.ShowErrorAsync( LocStrings.NotificationsPermissionRequired );
                return false;
            }
        }

        List<WeekDay> selectedDays = EditedReminder.DaysOfWeek?
            .Where( day => day is not null && (int)day.Type is >= 0 and <= 6 )
            .Select( day => day.Clone() as WeekDay )
            .Where( day => day is not null )
            .Cast<WeekDay>()
            .ToList()
            ?? [];

        if (EditedReminder.IsEnabled && selectedDays.Count == 0)
        {
            await DialogService.ShowErrorAsync( $"{LocStrings.Reminder}: {LocStrings.days} {LocStrings.isRequired}." );
            return false;
        }

        string title = EditedReminder.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace( title ))
        {
            string goalName = Habit.Goal?.Name?.Trim() ?? string.Empty;
            title = !string.IsNullOrWhiteSpace( goalName )
                ? goalName
                : LocStrings.Reminder;
        }

        string description = EditedReminder.Description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace( description ))
        {
            description = NameOfHabit?.Value?.Trim() ?? title;
        }

        UserHabitReminder reminder = new()
        {
            Title = title,
            Description = description,
            Time = TimeOnly.FromDateTime( EditedReminder.Time ),
            IsEnabled = EditedReminder.IsEnabled,
            DaysOfWeek = selectedDays
        };

        Habit.Reminders ??= new ObservableCollectionEx<UserHabitReminder>();

        if (Habit.Reminders.Count == 1)
        {
            UserHabitReminder storedReminder = Habit.Reminders[0];
            reminder.Id = storedReminder.Id;
            reminder.LocalId = storedReminder.LocalId;
            reminder.UserHabitLocalId = storedReminder.UserHabitLocalId;
            Habit.Reminders[0] = reminder;
        }
        else
        {
            Habit.Reminders.Add( reminder );
        }

        EditedReminder.Title = title;
        EditedReminder.Description = description;
        EditedReminder.DaysOfWeek = selectedDays;
        OnPropertyChanged( nameof( EditedReminder ) );
        return true;
    }

    private void RemoveInactiveNotifications( List<WeekDay> inactiveDays )
    {
        foreach (WeekDay day in inactiveDays)
        {
            _ = ReminderService.CancelLocallyAsync( day.UserNotificationRequestId );
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

            string title = string.IsNullOrWhiteSpace( reminder.Title )
                ? LocStrings.Reminder
                : reminder.Title;

            await ReminderService.SaveLocallyAsync(
                weekDay.UserNotificationRequestId,
                title,
                reminder.Description ?? string.Empty,
                notifyDateTime,
                ReminderRepeat.Weekly
            );
        }
        else if (!IsNewHabit && weekDay.UserNotificationRequestId != 0)
        {
            //TODO: test how it works for new habit
            await ReminderService.CancelLocallyAsync( weekDay.UserNotificationRequestId );
        }
    }
}
