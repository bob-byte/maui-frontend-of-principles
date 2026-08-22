using Plugin.LocalNotification.AndroidOption;

namespace Principles.Services;

public class ReminderService : BaseRemoteService, IReminderService
{
    private const int HabitsReportNotificationRequestId = 1;

    private readonly IDialogService m_dialogService;
    private readonly IDatabase m_database;
    private readonly INetworkService m_networkService;
    private readonly ISyncQueueService m_syncQueueService;
    private readonly IReminderRemoteApi m_remoteApi;

    public ReminderService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_dialogService = serviceProvider.GetRequiredService<IDialogService>();
        m_database = serviceProvider.GetRequiredService<IDatabase>();
        m_networkService = serviceProvider.GetRequiredService<INetworkService>();
        m_syncQueueService = serviceProvider.GetRequiredService<ISyncQueueService>();
        m_remoteApi = serviceProvider.GetRequiredService<IReminderRemoteApi>();
    }

    public bool IsLocalNotificationSupported()
    {
        return IsNotificationSupported();
    }

    public async Task<Reminder> HabitsReportReminderAsync()
    {
        Reminder? reminder = await GetStoredHabitsReportReminderAsync().ConfigureAwait( false );
        if (reminder is not null)
        {
            return reminder;
        }

        if (!m_networkService.IsConnected)
        {
            return new Reminder
            {
                UserNotificationRequestId = HabitsReportNotificationRequestId
            };
        }

        reminder = await m_remoteApi.GetHabitsReportReminderAsync().ConfigureAwait( false );
        if (reminder.Id != 0 || !string.IsNullOrWhiteSpace( reminder.Title ) || !string.IsNullOrWhiteSpace( reminder.Description ))
        {
            await SaveReminderLocallyAsync( reminder ).ConfigureAwait( false );
        }

        return reminder ?? new Reminder { UserNotificationRequestId = HabitsReportNotificationRequestId };
    }

    public async Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder )
    {
        ArgumentNullException.ThrowIfNull( reminder );

        if (reminder.UserNotificationRequestId == 0)
        {
            reminder.UserNotificationRequestId = HabitsReportNotificationRequestId;
        }

        reminder.LastModified = DateTime.UtcNow;
        await SaveReminderLocallyAsync( reminder ).ConfigureAwait( false );
        await ApplyHabitReportReminderLocallyAsync( reminder ).ConfigureAwait( false );

        if (m_networkService.IsConnected)
        {
            try
            {
                SaveHabitsReportReminderResponse response =
                    await m_remoteApi.SaveHabitsReportReminderAsync( reminder ).ConfigureAwait( false );

                reminder.Id = response.Id;
                reminder.UserNotificationRequestId = response.UserNotificationRequestId;
                await SaveReminderLocallyAsync( reminder ).ConfigureAwait( false );
                return response;
            }
            catch
            {
                // Keep local reminder and enqueue sync below.
            }
        }

        await m_syncQueueService.AddToQueueAsync( reminder, OperationKind.Save ).ConfigureAwait( false );
        return new SaveHabitsReportReminderResponse
        {
            Id = reminder.Id,
            UserNotificationRequestId = reminder.UserNotificationRequestId
        };
    }

    public async Task<bool> RequestAccessToSendNotificationsAsync()
    {
        return await ExecuteNotificationOperationAsync(
            async () =>
            {
                if (!IsNotificationSupported())
                {
                    return false;
                }

                return await LocalNotificationCenter.Current.RequestNotificationPermission( CreateBasicNotificationPermission( askPermission: true ) );
            },
            "Failed to request notification permission.",
            fallback: false
        );
    }

    public async Task TryToRecoverAllUserRemindersAsync()
    {
        if (!IsNotificationSupported())
        {
            await m_dialogService.ShowAlertAsync(
                LocStrings.DeviceDoesNotSupportNotifications,
                LocStrings.Error,
                LocStrings.OK
            ).DefaultConfigureAwait();
            return;
        }

        AllRemindersResponse remindersResponse = await LoadAllRemindersAsync();

        if (remindersResponse.GeneralReminders?.Any( r => r.IsEnabled ) == true ||
            remindersResponse.UserHabitReminders?.Any( r => r.IsEnabled ) == true)
        {
            bool areNotificationsEnabled = await AreNotificationsEnabledAsync();
            if (!areNotificationsEnabled)
            {
                await m_dialogService.ShowAlertAsync(
                    LocStrings.AfterLoginWhenUserAccountHaveReminders,
                    LocStrings.RestoreReminders,
                    LocStrings.OK
                );

                areNotificationsEnabled = await RequestAccessToSendNotificationsAsync().ConfigureAwait( false );
                if (!areNotificationsEnabled)
                {
                    return;
                }
            }

            if (remindersResponse.GeneralReminders?.Any() == true)
            {
                foreach (Reminder reminder in remindersResponse.GeneralReminders.Where( r => r.IsEnabled ))
                {
                    await SaveLocallyAsync(
                        reminder.UserNotificationRequestId,
                        reminder.Title,
                        reminder.Description,
                        DateTime.Today.Add( reminder.Time.ToTimeSpan() ),
                        ReminderRepeat.Daily
                    );
                }
            }

            if (remindersResponse.UserHabitReminders?.Any() == true)
            {
                foreach (UserHabitReminder userHabitReminder in remindersResponse.UserHabitReminders.Where( r => r.IsEnabled ))
                {
                    foreach (WeekDay weekDay in userHabitReminder.DaysOfWeek)
                    {
                        DateTime currentDate = DateTime.Now;
                        TimeSpan currentTime = currentDate.TimeOfDay;

                        int reminderDayIndex = (int)weekDay.Type;
                        int currentDayIndex = (int)currentDate.DayOfWeek;

                        int daysUntilNextReminder = (reminderDayIndex - currentDayIndex + 7) % 7;
                        if (daysUntilNextReminder == 0 && userHabitReminder.Time.ToTimeSpan() < currentTime)
                        {
                            daysUntilNextReminder = 7;
                        }

                        DateTime notifyDateTime = currentDate.Date
                            .AddDays( daysUntilNextReminder )
                            .Add( userHabitReminder.Time.ToTimeSpan() );

                        await SaveLocallyAsync(
                            weekDay.UserNotificationRequestId,
                            userHabitReminder.Title,
                            userHabitReminder.Description,
                            notifyDateTime,
                            ReminderRepeat.Weekly
                        );
                    }
                }
            }
        }
    }

    public Task CancelLocallyAsync( int id )
    {
        if (id <= 0)
        {
            return Task.CompletedTask;
        }

        return ExecuteNotificationOperationAsync(
            () =>
            {
                if (IsNotificationSupported())
                {
                    LocalNotificationCenter.Current.Cancel( id );
                }

                return Task.CompletedTask;
            },
            $"Failed to cancel local notification {id}."
        );
    }

    public Task CancelAllLocallyAsync()
    {
        return ExecuteNotificationOperationAsync(
            () =>
            {
                if (IsNotificationSupported())
                {
                    LocalNotificationCenter.Current.CancelAll();
                }

                return Task.CompletedTask;
            },
            "Failed to cancel all local notifications."
        );
    }

    public Task ClearDeliveredLocallyAsync()
    {
        return ExecuteNotificationOperationAsync(
            () =>
            {
                if (IsNotificationSupported())
                {
                    LocalNotificationCenter.Current.ClearAll();
                }

                return Task.CompletedTask;
            },
            "Failed to clear delivered local notifications."
        );
    }

    public Task<IList<NotificationRequest>> GetPendingLocallyAsync()
    {
        return ExecuteNotificationOperationAsync<IList<NotificationRequest>>(
            async () =>
            {
                if (!IsNotificationSupported())
                {
                    return new List<NotificationRequest>();
                }

                return await LocalNotificationCenter.Current.GetPendingNotificationList();
            },
            "Failed to get pending local notifications.",
            fallback: new List<NotificationRequest>()
        );
    }

    public async Task SaveLocallyAsync( int id, string title, string description, DateTime notifyTime, ReminderRepeat repeatType )
    {
        if (id <= 0)
        {
            LoggingService.LogError( $"Cannot save local notification with id {id}." );
            return;
        }

        NotificationRepeat notificationRepeat = repeatType switch
        {
            ReminderRepeat.Weekly => NotificationRepeat.Weekly,
            _ => NotificationRepeat.Daily
        };

        NotificationRequest notification = new()
        {
            NotificationId = id,
            Title = NormalizeNotificationTitle( title ),
            Description = description ?? string.Empty,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime,
                RepeatType = notificationRepeat,
                Android = new AndroidScheduleOptions
                {
                    AlarmType = AndroidAlarmType.RtcWakeup
                }
            },
            Android = new AndroidOptions
            {
                IconLargeName = new AndroidIcon( "logolargesize" ),
                IconSmallName = new AndroidIcon( "logolargesize" ),
            },
        };

        await SaveLocallyAsync( notification );
    }

    public async Task SaveLocallyAsync( NotificationRequest notification )
    {
        ArgumentNullException.ThrowIfNull( notification );

        notification.Title = NormalizeNotificationTitle( notification.Title );
        notification.Description ??= string.Empty;

        if (notification.NotificationId <= 0)
        {
            LoggingService.LogError( $"Cannot save local notification with id {notification.NotificationId}." );
            return;
        }

        bool areNotificationsEnabled = await AreNotificationsEnabledAsync().ConfigureAwait( false );
        if (!areNotificationsEnabled)
        {
            LoggingService.LogError( $"Cannot save local notification {notification.NotificationId} because notifications are disabled." );
            return;
        }

        await ExecuteNotificationOperationAsync(
            async () =>
            {
                if (!IsNotificationSupported())
                {
                    return;
                }

                LocalNotificationCenter.Current.Cancel( notification.NotificationId );
                await LocalNotificationCenter.Current.Show( notification );
            },
            $"Failed to save local notification {notification.NotificationId}."
        );
    }

    private Task<AllRemindersResponse> LoadAllRemindersAsync()
    {
        return RequestProvider.GetAsync<AllRemindersResponse>( UrlBuilder.AllReminders, SettingsService.AuthAccessToken );
    }

    private async Task<Reminder?> GetStoredHabitsReportReminderAsync()
    {
        return (await m_database.GetAllAsync<Reminder>().ConfigureAwait( false )).FirstOrDefault();
    }

    private async Task SaveReminderLocallyAsync( Reminder reminder )
    {
        Reminder? storedReminder = await GetStoredHabitsReportReminderAsync().ConfigureAwait( false );
        if (storedReminder is not null && reminder.LocalId == 0)
        {
            reminder.LocalId = storedReminder.LocalId;
        }

        await m_database.SaveAsync( reminder ).ConfigureAwait( false );
    }

    private async Task ApplyHabitReportReminderLocallyAsync( Reminder reminder )
    {
        if (reminder.IsEnabled)
        {
            await SaveLocallyAsync(
                reminder.UserNotificationRequestId,
                reminder.Title,
                reminder.Description,
                DateTime.Today.Add( reminder.Time.ToTimeSpan() ),
                ReminderRepeat.Daily
            ).ConfigureAwait( false );
        }
        else if (reminder.UserNotificationRequestId != 0)
        {
            await CancelLocallyAsync( reminder.UserNotificationRequestId ).ConfigureAwait( false );
        }
    }

    private async Task<bool> AreNotificationsEnabledAsync()
    {
        return await ExecuteNotificationOperationAsync(
            async () =>
            {
                return IsNotificationSupported() &&
                    await LocalNotificationCenter.Current.AreNotificationsEnabled( CreateBasicNotificationPermission( askPermission: false ) );
            },
            "Failed to check notification permission.",
            fallback: false
        );
    }

    private async Task ExecuteNotificationOperationAsync( Func<Task> operation, string errorMessage )
    {
        try
        {
            if (Microsoft.Maui.ApplicationModel.MainThread.IsMainThread)
            {
                await operation();
            }
            else
            {
                await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync( operation );
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError( ex, errorMessage );
        }
    }

    private async Task<TResult> ExecuteNotificationOperationAsync<TResult>(
        Func<Task<TResult>> operation,
        string errorMessage,
        TResult fallback
    )
    {
        try
        {
            if (Microsoft.Maui.ApplicationModel.MainThread.IsMainThread)
            {
                return await operation();
            }

            return await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync( operation );
        }
        catch (Exception ex)
        {
            LoggingService.LogError( ex, errorMessage );
            return fallback;
        }
    }

    private bool IsNotificationSupported()
    {
        try
        {
            return LocalNotificationCenter.Current.IsSupported;
        }
        catch (Exception ex)
        {
            LoggingService.LogError( ex, "Failed to check local notification support." );
            return false;
        }
    }

    private static NotificationPermission CreateBasicNotificationPermission( bool askPermission )
    {
        return new NotificationPermission
        {
            AskPermission = askPermission,
            Android = new AndroidNotificationPermission
            {
                RequestPermissionToScheduleExactAlarm = false
            }
        };
    }

    private static string NormalizeNotificationTitle( string? title )
    {
        return string.IsNullOrWhiteSpace( title )
            ? LocStrings.Reminder
            : title.Trim();
    }

    public void Cancel( int id )
    {
        LocalNotificationCenter.Current.Cancel( id );
    }
    
    public async Task AddNotificationToDeviceAsync( bool isNewHabit, UserHabitReminder reminder, WeekDay weekDay )
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

            await SaveLocallyAsync(
                weekDay.UserNotificationRequestId,
                reminder.Title,
                reminder.Description,
                notifyDateTime,
                ReminderRepeat.Weekly
            );
        }
        else if (!isNewHabit && weekDay.UserNotificationRequestId != 0)
        {
            LocalNotificationCenter.Current.Cancel( weekDay.UserNotificationRequestId );
        }
    }
}