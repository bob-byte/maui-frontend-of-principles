
using DevExpress.Maui.DataGrid;

using Principles.Core.Models;
using Principles.Exceptions;
using Plugin.LocalNotification;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;

namespace Principles.ViewModels;

public partial class ProgressOfHabitsViewModel : BaseViewModel
{
    private bool m_isInitialized;

    private readonly ConcurrentDictionary<UserHabit, SemaphoreSlim> m_isBusyForChangeCompleted;

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
    private ArсhivedHabitDto? m_archivedHabitToRemove;

    private readonly SemaphoreSlim m_initLocker;
    private MultipleActionPopup? m_multipleActionPopup;
    private readonly MultipleActionPopupViewModel m_multipleActionPopupViewModel;

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
        m_initLocker = new SemaphoreSlim( initialCount: 1, maxCount: 1 );

        Title = LocStrings.ProgressOfHabits;
        ReferenceMessenger.Register<HabitSavedMessage>( this, HandleHabitSave );
        ProgressOfHabitService = serviceProvider.GetRequiredService<IProgressOfHabitService>();
        ReminderService = serviceProvider.GetRequiredService<IReminderService>();

        EndProgressInterval = DateOnly.FromDateTime( DateTime.Today );
        StartProgressInterval = EndProgressInterval.AddDays( -HabitConstants.AVERAGE_NUMBER_OF_DAYS_TO_AUTOMATE_HABIT + 1 );
        m_isBusyForChangeCompleted = new ConcurrentDictionary<UserHabit, SemaphoreSlim>();
        m_userHabits = new ObservableCollectionEx<UserHabit>();
        m_multipleActionPopupViewModel = new MultipleActionPopupViewModel( serviceProvider );

        ReferenceMessenger.Register<UserLoggedOutMessage>( this, ( sender, msg ) =>
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

        ReferenceMessenger.Register<ChangedGoalMessage>( this, ( sender, msg ) =>
        {
            UserGoal editedGoal = msg.Value;

            foreach (UserHabit habit in UserHabits.Where( h => h.Goal!.Id == editedGoal.Id ))
            {
                habit.Goal!.Name = editedGoal.Name;
            }

            UserHabits.Reload( UserHabits.ToArray() );
        } );
        ReferenceMessenger.Register<GoalIsDeletedMessage>( this, ( sender, msg ) =>
        {
            UserGoal deletedGoal = msg.Value;

            foreach (UserHabit habit in UserHabits.Where( h => h.Goal!.Id == deletedGoal.Id ))
            {
                habit.Goal = new UserGoal()
                {
                    Id = 0,
                    Name = LocStrings.NoGoalSpecified
                };
            }

            UserHabits.Reload( UserHabits.ToArray() );
        } );

        ReferenceMessenger.Register<HabitsDeletedMessege>( this, ( sender, msg ) =>
        {
            UserHabit habit = msg.Value;

            UserHabit? habitToRemove = UserHabits.FirstOrDefault( h => h.Id == habit.Id );
            if (habitToRemove != null)
            {
                UserHabits.Remove( habitToRemove );
                ServiceOfHabit.StoredUserHabits?.Remove( habitToRemove );
                SelectedHabit = null;

                m_isBusyForChangeCompleted.TryRemove( habitToRemove, out SemaphoreSlim? locker );
                locker?.Dispose();
            }
        } );

        ReferenceMessenger.Register<ArchiveHabitMessage>( this, ( sender, msg ) => 
        {
            if (msg?.Value != null)
            {
                UserHabit? habitToRemove = UserHabits.FirstOrDefault( h => h.Id == msg.Value.Id );
                if (habitToRemove != null)
                {
                    UserHabits.Remove( habitToRemove );

                    if (ArchivedHabits == null)
                    {
                        ArchivedHabits = new ObservableCollection<ArсhivedHabitDto>( );
                    }

                    ArchivedHabits.Add( new ArсhivedHabitDto
                    {
                        Id = msg.Value.Id,
                        Name = msg.Value.Name
                    } );
                }
            }
        } );

        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            UpdateLocalizedStrings();
        } );
    }

    private void UpdateLocalizedStrings()
    {
        List<UserHabit> habits = UserHabits.Where( h => h.Goal.Id == 0 ).ToList();
        foreach (UserHabit? habit in habits)
        {
            habit.Goal.Name = LocStrings.NoGoalSpecified;
        }
    }

    public IProgressOfHabitService ProgressOfHabitService { get; }
    public IReminderService ReminderService { get; }

    internal DataGridView? DataGridViewWithHabits { get; set; }

    internal bool IsProgressesInitialized { get; set; }

    public void HandleHabitSave(object receiver, HabitSavedMessage message)
    {
        SelectedHabit = null;

        UserHabit savedHabit = message.Value;
        UserHabit? foundHabit = UserHabits.FirstOrDefault( u => u.Id == savedHabit.Id );

        if (foundHabit == null)
        {
            savedHabit.Progresses ??= new ObservableCollectionEx<ProgressOfHabit>();

            for (DateOnly date = EndProgressInterval; date >= StartProgressInterval; date = date.AddDays( value: -1 ))
            {
                if (!savedHabit.Progresses.Any( p => p.Date == date ))
                {
                    ProgressOfHabit progress = new()
                    {
                        Id = 0,
                        Date = date,
                        Value = ProgressValue.UNKNOWN,
                        Habit = savedHabit
                    };
                    savedHabit.Progresses.Add( progress );
                }
            }

            savedHabit.Goal ??= new UserGoal();
            if (savedHabit.Goal.Id == 0)
            {
                savedHabit.Goal.Name = LocStrings.NoGoalSpecified;
            }

            UserHabits.Add( savedHabit );
            m_isBusyForChangeCompleted.TryAdd( savedHabit, new SemaphoreSlim( 1, 1 ) );
        }
        else
        {
            foundHabit.Name = savedHabit.Name;
            foundHabit.Complexity = savedHabit.Complexity;
            foundHabit.Frequency = savedHabit.Frequency;
            foundHabit.Priority = savedHabit.Priority;

            foundHabit.Goal = savedHabit.Goal is null || savedHabit.Goal.Id == 0
                ? new UserGoal()
                {
                    Id = 0,
                    Name = LocStrings.NoGoalSpecified
                }
                : savedHabit.Goal;

            ServiceOfHabit.Recompute( foundHabit );
        }

        ServiceOfHabit.StoredUserHabits = UserHabits.OrderBy( h => h.Goal!.Id ).ThenBy( h => h.Id ).ToList();
        UserHabits = new ObservableCollectionEx<UserHabit>( ServiceOfHabit.StoredUserHabits );
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        await base.InitializeAsync( parameter );

        if (!m_isInitialized)
        {
            await m_initLocker.WaitAsync();

            try
            {
                if (!m_isInitialized)
                {
                    await InitUserInfoAsync();
                    List<UserHabit> habits = await ServiceOfHabit.ActiveHabitsAsync( StartProgressInterval, EndProgressInterval );

                    foreach (UserHabit habit in habits)
                    {
                        m_isBusyForChangeCompleted.TryAdd( habit, new SemaphoreSlim( 1, 1 ) );

                        habit.Goal ??= new UserGoal();
                        if (habit.Goal.Id == 0)
                        {
                            habit.Goal.Name = LocStrings.NoGoalSpecified;
                        }
                    }

                    habits = habits.OrderBy( h => h.Goal!.Id ).ThenBy( h => h.Id ).ToList();
                    UserHabits.Reload( habits );

                    ServiceOfHabit.StoredUserHabits = habits;
                                        
                    IsProgressesInitialized = true;
                    m_isInitialized = true;
                }
            }
            finally
            {
                m_initLocker.Release();
            }
        }
    }

    [RelayCommand]
    private async Task ChangeValueOfProgressOfHabitAsync( ProgressOfHabit progressOfHabit )
    {
        if (!IsProgressesInitialized || progressOfHabit == null)
        {
            return;
        }

        UserHabit habit = progressOfHabit.Habit!;

        bool doTryAgain;

        SemaphoreSlim locker = m_isBusyForChangeCompleted[habit];
        await locker.WaitAsync();

        int previousValueOfProgress = progressOfHabit.Value;

        try
        {
            do
            {
                try
                {
                    progressOfHabit.Value = ProgressValue.NextToggled( previousValueOfProgress );
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
            { "Id", default( long ) },
            { "UserHabits", UserHabits },
            { "IsArchived", false }
        };

        await Navigation.NavigateToAsync<EditHabitViewModel>( routeParams );
    }

    [RelayCommand]
    private async Task EditHabitAsync( UserHabit? habit )
    {
        if (habit != null)
        {
            Dictionary<string, object> routeParams = new()
            {
                { "Id", habit.Id },
                { "UserHabits", UserHabits },
                { "IsArchived", false }
            };
            await Navigation.NavigateToAsync<EditHabitViewModel>( routeParams );
        }
    }

    [RelayCommand]
    private async Task CreateArchivedHabit()
    {
        Dictionary<string, object> routeParams = new()
            {
                { "IsArchived", true },
            };
        await Navigation.NavigateToAsync<EditHabitViewModel>( routeParams );
    }

    [RelayCommand]
    private async Task EditArchivedHabitAsync( ArсhivedHabitDto arhivedHabit )
    {
            Dictionary<string, object> routeParams = new()
            {
                { "Id", arhivedHabit.Id },
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

        if (sendToArchive)
        {
            await ServiceOfHabit.SetHabitArchiveStatus( new HabitArchiveStatus
            {
                HabitId = habit.Id,
                IsArchived = true
            } );
            UserHabits.Remove( habit );
        }
    }

    [RelayCommand]
    private async void SelectArchivedHabit( ArсhivedHabitDto arhivedHabit )
    {
        ArchivedHabitToRemove = null;
        ArchivedHabitToRemove = arhivedHabit;

        OpenMultiActionPopupAsync();
    }

    private async void OpenMultiActionPopupAsync()
    {
        m_multipleActionPopupViewModel.SetActions( new List<ActionButtons>
        {
            new ActionButtons { Text = LocStrings.RemoveFromArchive, Command = new Command(() => RemoveHabitFromArchive()) },
            new ActionButtons { Text = LocStrings.DeleteTheHabit, Command = new Command(() => DeleteArchivedHabit()) }
        }, LocStrings.DeleteArchivedHabitConfirmationText );

        m_multipleActionPopup = new MultipleActionPopup( m_multipleActionPopupViewModel );
        await Application.Current.MainPage.ShowPopupAsync( m_multipleActionPopup );
    }

    [RelayCommand]
    private async Task DeleteArchivedHabit()
    {
        bool doDelete = await DialogService.ShowConfirmAsync(
            LocStrings.MessageInDeleteHabitConfirm,
            LocStrings.DeleteHabitQuestion
        );

        if (doDelete)
        {
            await UiBusyFor( async () =>
            {
                HabitDeletionResponse? response = await ServiceOfHabit.DeleteAsync( ArchivedHabitToRemove.Id );

                if (response != null)
                {
                    foreach (HabitDeletionResponse.NotificationRequest notification in response.DeletedNotifications)
                    {
                        LocalNotificationCenter.Current.Cancel( notification.Id );
                    }
                }

                ArchivedHabits.Remove( ArchivedHabitToRemove );
            } ).DefaultConfigureAwait();
        }
    }

    [RelayCommand]
    private async Task RemoveHabitFromArchive()
    {
        bool doRemoveFromAchive = await DialogService.ShowConfirmAsync(
            LocStrings.MessageRemoveHabitFromArchive,
            LocStrings.RemoveHabitFromArchiveQuestion
        );

        if (doRemoveFromAchive)
        {
            ArchivedHabits.Remove( ArchivedHabitToRemove );
            await ServiceOfHabit.SetHabitArchiveStatus( new HabitArchiveStatus
            {
                HabitId = ArchivedHabitToRemove.Id,
                IsArchived = false
            } );
            UserHabit habit = await ServiceOfHabit.UserHabitAsync( ArchivedHabitToRemove.Id );

            habit.Goal ??= new UserGoal();
            if (habit.Goal.Id == 0)
            {
                habit.Goal.Name = LocStrings.NoGoalSpecified;
            }

            List<ProgressOfHabit> progresses = await ServiceOfHabit.GetProgressesOfHabit( habit.Id );
            habit.Progresses = new ObservableCollectionEx<ProgressOfHabit>( progresses );

            ServiceOfHabit.InitializeHabitProgresses( habit, StartProgressInterval, EndProgressInterval );
            UserHabits.Add( habit );
            UpdateLocalizedStrings();


            if (!m_isBusyForChangeCompleted.ContainsKey( habit ))
            {
                m_isBusyForChangeCompleted.TryAdd( habit, new SemaphoreSlim( 1, 1 ) );
            }
        }
    }

    [RelayCommand]
    private async Task DeleteHabitAsync(object? obj)
    {
        if(obj is UserHabit habit)
        {
            bool doDelete = await DialogService.ShowConfirmAsync(
                LocStrings.MessageInDeleteHabitConfirm,
                LocStrings.DeleteHabitQuestion
            );

            if (doDelete)
            {
                await UiBusyFor( async () =>
                {
                    HabitDeletionResponse? response = await ServiceOfHabit.DeleteAsync( habit.Id );
                    
                    if (response != null)
                    {
                        foreach (HabitDeletionResponse.NotificationRequest notification in response.DeletedNotifications)
                        {
                            LocalNotificationCenter.Current.Cancel( notification.Id );
                        }
                    }

                    UserHabits.Remove( habit );
                    ServiceOfHabit.StoredUserHabits?.Remove( habit );
                    m_isBusyForChangeCompleted.TryRemove( habit, out SemaphoreSlim? locker );
                    locker?.Dispose();

                    if (habit.Id == SelectedHabit?.Id)
                    {
                        SelectedHabit = DataGridViewWithHabits!.SelectedRowHandle >= 0 && UserHabits.Count > DataGridViewWithHabits.SelectedRowHandle
                            ? UserHabits[DataGridViewWithHabits.SelectedRowHandle]
                            : null;
                    }
                } ).DefaultConfigureAwait();
            }
        }
    }

    [RelayCommand]
    private void SelectRow( UserHabit selectedHabit )
    {
        if(SelectedHabit == null || SelectedHabit.Id != selectedHabit.Id)
        {
            SelectedHabit = selectedHabit;
        }
        else
        {
            SelectedHabit = null;
        }
    }

    [RelayCommand]
    private async Task LoadHabitReportReminderAsync(Action openPopup)
    {
        await UiBusyFor(async () =>
        {
            Reminder reminder = await ReminderService.HabitsReportReminderAsync() ?? new Reminder();
            openPopup();
            
            if (reminder.Id == 0)
            {
                reminder.UserNotificationRequestId = 0;
                reminder.Title = LocStrings.ReminderTitleText;
                reminder.Description = string.IsNullOrWhiteSpace( UserName?.Value ) ? LocStrings.ReminderDescriptionText : $"{UserName.Value}, {LocStrings.ReminderDescriptionText}";
                reminder.IsEnabled = true;
                reminder.Time = new TimeOnly( 7, 0 );

                ReminderReport = new EditedReminderReport
                {
                    UserNotificationRequestId = reminder.UserNotificationRequestId,
                    Title = reminder.Title,
                    Description = reminder.Description,
                    IsEnabled = reminder.IsEnabled,
                    Time = reminder.Time.ToTimeSpan()
                };
            }
            else
            {
                ReminderReport = new EditedReminderReport();
                ReminderReport.Id = reminder.Id;
                ReminderReport.Title = reminder.Title;
                ReminderReport.Description = reminder.Description;
                ReminderReport.IsEnabled = reminder.IsEnabled;
                ReminderReport.UserNotificationRequestId = reminder.UserNotificationRequestId;

                ReminderReport.Time = reminder.Time.ToTimeSpan();
            }

            OnPropertyChanged( nameof( ReminderReport ) );
        });
    }

    [RelayCommand]
    private async Task SaveHabitsReportReminderAsync(Action closePopup)
    {
        await GetAccessToSendNotificationsAsync();

        if (!ReminderReport.IsEnabled && ReminderReport.UserNotificationRequestId != 0)
        {
            LocalNotificationCenter.Current.Cancel( ReminderReport.UserNotificationRequestId );
            closePopup();
            return;
        }

        Reminder reminder = new()
        {
            Id = ReminderReport.Id,
            Title = ReminderReport.Title,
            Description = ReminderReport.Description,
            Time = new TimeOnly( ReminderReport.Time!.Value.Hours, ReminderReport.Time.Value.Minutes ),
            IsEnabled = ReminderReport.IsEnabled
        };

        await UiBusyFor( async () =>
        {
            SaveHabitsReportReminderResponse response = await ReminderService.SaveHabitsReportReminderAsync( reminder );
            ReminderReport.Id = response.Id;
            reminder.Id = response.Id;
        
            ReminderReport.UserNotificationRequestId = response.UserNotificationRequestId;
            reminder.UserNotificationRequestId = response.UserNotificationRequestId;

            DateTime notifyTime = DateTime.Today.Add( reminder.Time.ToTimeSpan() );
            await ReminderService.SaveAsync(
                reminder.UserNotificationRequestId,
                reminder.Title,
                reminder.Description,
                notifyTime,
                ReminderRepeat.Daily
            );

            closePopup();
        } );
    }

    private Task GetAccessToSendNotificationsAsync()
    {
        //TODO: it should support all Android versions which our app supports
        return LocalNotificationCenter.Current.RequestNotificationPermission();
    }
}