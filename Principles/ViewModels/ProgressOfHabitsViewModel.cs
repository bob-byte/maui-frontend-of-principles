using CommunityToolkit.Maui.Views;
using DevExpress.Maui.DataGrid;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Principles.ViewModels;

public partial class ProgressOfHabitsViewModel : BaseViewModel
{
    private bool m_isInitialized;
    private readonly ConcurrentDictionary<UserHabit, SemaphoreSlim> m_isBusyForChangeCompleted;
    private readonly SemaphoreSlim m_initLocker;

    [ObservableProperty]
    private UserHabit? m_selectedHabit;

    [ObservableProperty]
    private DateOnly m_startProgressInterval;

    [ObservableProperty]
    private DateOnly m_endProgressInterval;

    [ObservableProperty]
    private EditedReminderReport m_reminderReport;

    [ObservableProperty]
    private ObservableCollectionEx<UserHabit> m_userHabits;

    [ObservableProperty]
    private ObservableCollection<ArсhivedHabitDto> m_archivedHabits;

    [ObservableProperty]
    private bool m_doShowPlusButton;

    ~ProgressOfHabitsViewModel()
    {
        foreach (SemaphoreSlim locker in m_isBusyForChangeCompleted.Values)
        {
            locker.Dispose();
        }
    }

    public ProgressOfHabitsViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        DoShowPlusButton = true;
        m_initLocker = new SemaphoreSlim( 1, 1 );

        Title = LocStrings.ProgressOfHabits;
        ProgressOfHabitService = serviceProvider.GetRequiredService<IProgressOfHabitService>();
        ReminderService = serviceProvider.GetRequiredService<IReminderService>();

        EndProgressInterval = DateOnly.FromDateTime( DateTime.Today );
        StartProgressInterval = EndProgressInterval.AddDays( -HabitConstants.NUMBER_OF_DAYS_IN_PROGRESS + 1 );
        m_isBusyForChangeCompleted = new ConcurrentDictionary<UserHabit, SemaphoreSlim>();
        m_userHabits = new ObservableCollectionEx<UserHabit>();
        m_archivedHabits = new ObservableCollection<ArсhivedHabitDto>();
        m_reminderReport = new EditedReminderReport();
        ServiceOfHabit.StoredUserHabits = UserHabits;

        ReferenceMessenger.Register<HabitSavedMessage>( this, HandleHabitSave );
        ReferenceMessenger.Register<UserLoggedOutMessage>( this, ( _, msg ) =>
        {
            m_isInitialized = false;
            IsProgressesInitialized = false;

            DefaultHandleLogout( msg );
            UserHabits.Clear();

            foreach (SemaphoreSlim locker in m_isBusyForChangeCompleted.Values)
            {
                locker.Dispose();
            }

            m_isBusyForChangeCompleted.Clear();
            ServiceOfHabit.StoredUserHabits?.Clear();
            SelectedHabit = null;
        } );

        ReferenceMessenger.Register<ChangedGoalMessage>( this, ( _, msg ) =>
        {
            UserGoal editedGoal = msg.Value;

            foreach (UserHabit habit in UserHabits.Where( h => h.Goal is not null && h.Goal.Equals( editedGoal ) ))
            {
                habit.Goal!.Name = editedGoal.Name;
            }
        } );

        ReferenceMessenger.Register<GoalIsDeletedMessage>( this, ( _, msg ) =>
        {
            UserGoal deletedGoal = msg.Value;

            foreach (UserHabit habit in UserHabits.Where( h => h.Goal is not null && h.Goal.Equals( deletedGoal ) ))
            {
                habit.Goal = new UserGoal
                {
                    Id = 0,
                    Name = LocStrings.NoGoalSpecified
                };
            }
        } );

        ReferenceMessenger.Register<HabitsDeletedMessege>( this, ( _, msg ) =>
        {
            UserHabit habit = msg.Value;

            UserHabit? habitToRemove = UserHabits.FirstOrDefault( h => h.Equals( habit ) || h.LocalId == habit.LocalId );
            if (habitToRemove is not null)
            {
                UserHabits.Remove( habitToRemove );
                ServiceOfHabit.StoredUserHabits?.Remove( habitToRemove );
                SelectedHabit = null;

                m_isBusyForChangeCompleted.TryRemove( habitToRemove, out SemaphoreSlim? locker );
                locker?.Dispose();
            }
        } );

        ReferenceMessenger.Register<ArchiveHabitMessage>( this, async ( _, msg ) =>
        {
            if (msg?.Value is null)
            {
                return;
            }

            UserHabit? habitToRemove = UserHabits.FirstOrDefault( h => h.Equals( msg.Value ) || h.LocalId == msg.Value.LocalId );
            if (habitToRemove is not null)
            {
                UserHabits.Remove( habitToRemove );
            }

            if (msg.DoShowAllArchivedHabits)
            {
                await Task.Delay( 500 );
                ReferenceMessenger.Send( new ShowAllArchivedHabitsMsg() );
            }
        } );

        ReferenceMessenger.Register<NewCultureMessage>( this, ( _, _ ) => UpdateLocalizedStrings() );
    }

    public IProgressOfHabitService ProgressOfHabitService { get; }
    public IReminderService ReminderService { get; }

    internal DataGridView? DataGridViewWithHabits { get; set; }
    internal bool IsProgressesInitialized { get; set; }

    private void UpdateLocalizedStrings()
    {
        foreach (UserHabit habit in UserHabits.Where( h => h.Goal?.Id == 0 ))
        {
            habit.Goal!.Name = LocStrings.NoGoalSpecified;
        }
    }

    private void HandleHabitSave( object receiver, HabitSavedMessage message )
    {
        try
        {
            SelectedHabit = null;

            UserHabit savedHabit = message.Value;
            ServiceOfHabit.InitializeHabitProgresses( savedHabit, StartProgressInterval, EndProgressInterval );
            UserHabit? foundHabit = UserHabits.FirstOrDefault( u =>
                (savedHabit.Id != 0 && u.Id == savedHabit.Id) ||
                (savedHabit.LocalId != 0 && u.LocalId == savedHabit.LocalId) ||
                u.Equals( savedHabit ) );

            if (foundHabit is null)
            {
                EnsureDisplayGoal( savedHabit );

                UserHabits.Add( savedHabit );
                m_isBusyForChangeCompleted.TryAdd( savedHabit, new SemaphoreSlim( 1, 1 ) );
            }
            else
            {
                foundHabit.MergeFrom( savedHabit );
                ServiceOfHabit.InitializeHabitProgresses( foundHabit, StartProgressInterval, EndProgressInterval );
                EnsureDisplayGoal( foundHabit );
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError( ex, "Failed to apply saved habit to progress list." );
            _ = DialogService.ShowErrorAsync(
                SettingsService.IsDebug ? ex.ToString() : LocStrings.SomethingWentWrong
            );
        }
    }

    private static void EnsureDisplayGoal( UserHabit habit )
    {
        habit.Goal ??= new UserGoal();
        if (habit.Goal.Id == 0)
        {
            habit.Goal.Name = LocStrings.NoGoalSpecified;
        }
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        await base.InitializeAsync( parameter );

        if (m_isInitialized)
        {
            return;
        }

        await m_initLocker.WaitAsync();
        try
        {
            if (m_isInitialized)
            {
                return;
            }

            await InitUserInfoAsync();

            List<UserHabit> habits = await ServiceOfHabit.ActiveHabitsAsync();
            foreach (UserHabit habit in habits)
            {
                m_isBusyForChangeCompleted.TryAdd( habit, new SemaphoreSlim( 1, 1 ) );

                habit.Goal ??= new UserGoal();
                if (habit.Goal.Id == 0)
                {
                    habit.Goal.Name = LocStrings.NoGoalSpecified;
                }
            }

            UserHabits.Reload( habits.OrderBy( h => h.Goal?.Name ).ThenBy( h => h.Priority ).ThenBy( h => h.Name ) );
            IsProgressesInitialized = true;
            m_isInitialized = true;
        }
        finally
        {
            m_initLocker.Release();
        }
    }

    [RelayCommand]
    private async Task ChangeValueOfProgressOfHabitAsync( ProgressOfHabit? progressOfHabit )
    {
        if (!IsProgressesInitialized || progressOfHabit?.Habit is null)
        {
            return;
        }

        UserHabit habit = progressOfHabit.Habit;
        SemaphoreSlim locker = m_isBusyForChangeCompleted[habit];
        await locker.WaitAsync();

        int previousValueOfProgress = progressOfHabit.Value;

        try
        {
            bool doTryAgain;
            do
            {
                try
                {
                    progressOfHabit.Value = ProgressValue.NextToggled( previousValueOfProgress );
                    ServiceOfHabit.Recompute( habit );
                    await ProgressOfHabitService.UpdateAsync( progressOfHabit );
                    doTryAgain = false;
                }
                catch (Exception ex)
                {
                    doTryAgain = await DoRetryOperationOnErrorAsync( ex );
                    if (!doTryAgain)
                    {
                        progressOfHabit.Value = previousValueOfProgress;
                        ServiceOfHabit.Recompute( habit );
                    }
                }
            }
            while (doTryAgain);
        }
        finally
        {
            locker.Release();
        }
    }

    [RelayCommand]
    private async Task AddHabitAsync()
    {
        Dictionary<string, object> routeParams = new()
        {
            { "IsArchived", false }
        };

        DoShowPlusButton = false;
        try
        {
            await Navigation.NavigateToAsync<EditHabitViewModel>( routeParams );
        }
        finally
        {
            DoShowPlusButton = true;
        }
    }

    [RelayCommand]
    private async Task EditHabitAsync( UserHabit? habit )
    {
        if (habit is null)
        {
            return;
        }

        Dictionary<string, object> routeParams = new()
        {
            { "Habit", habit },
            { "IsArchived", false }
        };

        DoShowPlusButton = false;
        try
        {
            await Navigation.NavigateToAsync<EditHabitViewModel>( routeParams );
        }
        finally
        {
            DoShowPlusButton = true;
        }
    }

    [RelayCommand]
    private Task CreateArchivedHabit()
    {
        Dictionary<string, object> routeParams = new()
        {
            { "IsArchived", true }
        };

        return Navigation.NavigateToAsync<EditHabitViewModel>( routeParams );
    }

    [RelayCommand]
    private async Task EditArchivedHabitAsync( ArсhivedHabitDto archivedHabit )
    {
        UserHabit userHabit = await ServiceOfHabit.UserHabitAsync( archivedHabit.LocalId );
        ServiceOfHabit.InitializeHabitProgresses(
            userHabit,
            StartProgressInterval,
            userHabit.Progresses?.Count > 0 ? userHabit.Progresses[^1].Date : EndProgressInterval
        );

        userHabit.IsArchived = true;
        Dictionary<string, object> routeParams = new()
        {
            { "Habit", userHabit },
            { "IsArchived", true }
        };

        await Navigation.NavigateToAsync<EditHabitViewModel>( routeParams );
    }

    [RelayCommand]
    private async Task GetArchivedHabits()
    {
        List<ArсhivedHabitDto> archivedHabits = await ServiceOfHabit.GetArchivedHabits();
        ArchivedHabits = new ObservableCollection<ArсhivedHabitDto>( archivedHabits );
    }

    [RelayCommand]
    private async Task ArchiveHabitAsync( UserHabit habit )
    {
        bool sendToArchive = await DialogService.ShowConfirmAsync(
            LocStrings.MessageAddHabitToArchive,
            LocStrings.AddHabitToArchiveQuestion
        );

        if (!sendToArchive)
        {
            return;
        }

        await UiBusyFor( async () =>
        {
            habit.IsArchived = true;
            await ServiceOfHabit.SetHabitArchiveStatusAsync( habit );
            UserHabits.Remove( habit );
        } );
    }

    [RelayCommand]
    private async Task DeleteArchivedHabitAsync( ArсhivedHabitDto archivedHabit )
    {
        MultipleActionPopupViewModel multipleActionViewModel = ServiceProvider.GetRequiredService<MultipleActionPopupViewModel>();
        MultipleActionPopup popup = new( multipleActionViewModel );

        List<ActionData> availableActions =
        [
            new( LocStrings.RemoveFromArchive, () => RemoveHabitFromArchiveAsync( archivedHabit ) ),
            new( LocStrings.DeleteTheHabit, () => DeleteArchivedHabitInServerAsync( archivedHabit ) )
        ];

        multipleActionViewModel.Configure( availableActions, LocStrings.DeleteArchivedHabitConfirmationText );
        await Shell.Current.ShowPopupAsync( popup );
    }

    private async Task DeleteArchivedHabitInServerAsync( ArсhivedHabitDto habit )
    {
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
            UserHabit localHabit = await ServiceOfHabit.UserHabitAsync( habit.LocalId );
            await ServiceOfHabit.DeleteAsync( localHabit );
            ArchivedHabits.Remove( habit );
        } ).DefaultConfigureAwait();
    }

    private async Task RemoveHabitFromArchiveAsync( ArсhivedHabitDto archivedHabit )
    {
        await UiBusyFor( async () =>
        {
            UserHabit habit = await ServiceOfHabit.UserHabitAsync( archivedHabit.LocalId );
            habit.IsArchived = false;
            await ServiceOfHabit.SetHabitArchiveStatusAsync( habit );

            ArchivedHabits.Remove( archivedHabit );

            habit.Goal ??= new UserGoal();
            if (habit.Goal.Id == 0)
            {
                habit.Goal.Name = LocStrings.NoGoalSpecified;
            }

            ServiceOfHabit.InitializeHabitProgresses( habit, StartProgressInterval, EndProgressInterval );
            UserHabits.Add( habit );

            if (!m_isBusyForChangeCompleted.ContainsKey( habit ))
            {
                m_isBusyForChangeCompleted.TryAdd( habit, new SemaphoreSlim( 1, 1 ) );
            }
        } );
    }

    [RelayCommand]
    private Task HabitDetailAsync( UserHabit? habit )
    {
        if (habit is null)
        {
            return Task.CompletedTask;
        }

        Dictionary<string, object> routeParams = new()
        {
            { "Habit", habit }
        };

        return Navigation.NavigateToAsync<HabitDetailViewModel>( routeParams );
    }

    [RelayCommand]
    private async Task ArchivedHabitDetailAsync( ArсhivedHabitDto? archivedHabit )
    {
        if (archivedHabit is null)
        {
            return;
        }

        UserHabit userHabit = await ServiceOfHabit.UserHabitAsync( archivedHabit.LocalId );
        ServiceOfHabit.InitializeHabitProgresses(
            userHabit,
            StartProgressInterval,
            userHabit.Progresses?.Count > 0 ? userHabit.Progresses[^1].Date : EndProgressInterval
        );
        userHabit.IsArchived = true;

        Dictionary<string, object> routeParams = new()
        {
            { "Habit", userHabit },
            { "IsArchived", true }
        };

        await Navigation.NavigateToAsync<HabitDetailViewModel>( routeParams );
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

            UserHabits.Remove( habit );
            ServiceOfHabit.StoredUserHabits?.Remove( habit );
            m_isBusyForChangeCompleted.TryRemove( habit, out SemaphoreSlim? locker );
            locker?.Dispose();

            if (SelectedHabit is not null && SelectedHabit.Equals( habit ))
            {
                SelectedHabit = DataGridViewWithHabits!.SelectedRowHandle >= 0 &&
                               UserHabits.Count > DataGridViewWithHabits.SelectedRowHandle
                    ? UserHabits[DataGridViewWithHabits.SelectedRowHandle]
                    : null;
            }
        } ).DefaultConfigureAwait();
    }

    [RelayCommand]
    private void SelectRow( UserHabit selectedHabit )
    {
        if (SelectedHabit is null || !SelectedHabit.Equals( selectedHabit ))
        {
            SelectedHabit = selectedHabit;
        }
        else
        {
            SelectedHabit = null;
        }
    }

    [RelayCommand]
    private async Task LoadHabitReportReminderAsync( Action openPopup )
    {
        openPopup();

        Reminder reminder = await ReminderService.HabitsReportReminderAsync() ?? new Reminder();
        bool isDefaultReminder = reminder.Id == 0 && string.IsNullOrWhiteSpace( reminder.Title ) && string.IsNullOrWhiteSpace( reminder.Description );

        if (isDefaultReminder)
        {
            reminder.UserNotificationRequestId = 1;
            reminder.Title = LocStrings.ReminderTitleText;
            reminder.Description = string.IsNullOrWhiteSpace( UserName?.Value )
                ? LocStrings.ReminderDescriptionText
                : $"{UserName.Value}, {LocStrings.ReminderDescriptionText}";
            reminder.IsEnabled = true;
            reminder.Time = new TimeOnly( 7, 0 );
        }

        ReminderReport = new EditedReminderReport
        {
            Id = reminder.Id,
            UserNotificationRequestId = reminder.UserNotificationRequestId,
            Title = reminder.Title,
            Description = reminder.Description,
            IsEnabled = reminder.IsEnabled,
            Time = reminder.Time.ToTimeSpan()
        };

        OnPropertyChanged( nameof( ReminderReport ) );
    }

    [RelayCommand]
    private async Task SaveHabitsReportReminderAsync( Action closePopup )
    {
        if (ReminderReport.IsEnabled)
        {
            bool canSendNotifications = await GetAccessToSendNotificationsAsync();
            if (!canSendNotifications)
            {
                await DialogService.ShowErrorAsync( LocStrings.NotificationsPermissionRequired );
                return;
            }
        }

        Reminder reminder = new()
        {
            Id = ReminderReport.Id,
            UserNotificationRequestId = ReminderReport.UserNotificationRequestId,
            Title = ReminderReport.Title,
            Description = ReminderReport.Description,
            Time = new TimeOnly( ReminderReport.Time!.Value.Hours, ReminderReport.Time.Value.Minutes ),
            IsEnabled = ReminderReport.IsEnabled
        };

        SaveHabitsReportReminderResponse response = await ReminderService.SaveHabitsReportReminderAsync( reminder );
        ReminderReport.Id = response.Id;
        ReminderReport.UserNotificationRequestId = response.UserNotificationRequestId;
        closePopup();
    }

    private Task<bool> GetAccessToSendNotificationsAsync()
    {
        return ReminderService.RequestAccessToSendNotificationsAsync();
    }
}
