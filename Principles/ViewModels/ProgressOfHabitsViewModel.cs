
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

    private readonly SemaphoreSlim m_initLocker;
    
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
        m_initLocker = new SemaphoreSlim( initialCount: 1, maxCount: 1 );

        Title = LocStrings.ProgressOfHabits;
        ProgressOfHabitService = serviceProvider.GetRequiredService<IProgressOfHabitService>();
        ReminderService = serviceProvider.GetRequiredService<IReminderService>();

        EndProgressInterval = DateOnly.FromDateTime( DateTime.Today );
        StartProgressInterval = EndProgressInterval.AddDays( -HabitConstants.NUMBER_OF_DAYS_IN_PROGRESS + 1 );
        m_isBusyForChangeCompleted = new ConcurrentDictionary<UserHabit, SemaphoreSlim>();
        m_userHabits = new ObservableCollectionEx<UserHabit>();
        ArchivedHabits = new ObservableCollectionEx<ArсhivedHabitDto>();
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

        ReferenceMessenger.Register<ChangedGoalMessage>( this, ( sender, msg ) =>
        {
            UserGoal editedGoal = msg.Value;

            foreach (UserHabit habit in UserHabits.Where( h => h.Goal!.Id == editedGoal.Id ))
            {
                habit.Goal!.Name = editedGoal.Name;
            }
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
                NotifyPropertyChanged( nameof( UserHabits ) );
            }
        } );

        ReferenceMessenger.Register<ArchiveHabitMessage>( this, async ( _, msg ) => 
        {
            if (msg?.Value != null)
            {
                UserHabit? habitToRemove = UserHabits.FirstOrDefault( h => h.Id == msg.Value.Id );
                if (habitToRemove != null)
                {
                    UserHabits.Remove( habitToRemove );
                }

                if (msg.DoShowAllArchivedHabits)
                {
                    await Task.Delay( 500 );
                    ReferenceMessenger.Send( new ShowAllArchivedHabitsMsg() );
                }
                NotifyPropertyChanged( nameof( UserHabits ) );
            }
        } );

        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            UpdateLocalizedStrings();
        } );

        ReferenceMessenger.Register<MsgThatProgressOfHabitUpdated>( this, ( sender, msg ) =>
        {
            NotifyPropertyChanged( nameof( UserHabits ) );
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

    private void HandleHabitSave(object receiver, HabitSavedMessage message)
    {
        SelectedHabit = null;
        
        UserHabit savedHabit = message.Value;
        UserHabit? foundHabit = UserHabits.FirstOrDefault( u => u.Id == savedHabit.Id );

        if (foundHabit is null)
        {
            ServiceOfHabit.InitializeHabitProgresses( savedHabit, StartProgressInterval, EndProgressInterval );

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
            foundHabit.MergeFrom( savedHabit );

            ServiceOfHabit.Recompute( foundHabit );
        }
        NotifyPropertyChanged( nameof( UserHabits ) );
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

                    NotifyPropertyChanged( nameof( UserHabits ) );
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
    private async Task ChangeValueOfProgressOfHabitAsync( ProgressOfHabit? progressOfHabit )
    {
        if (!IsProgressesInitialized || progressOfHabit is null)
        {
            return;
        }

        UserHabit habit = progressOfHabit.Habit!;

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

                    NotifyPropertyChanged( nameof( UserHabits ) );
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
                        NotifyPropertyChanged( nameof( UserHabits ) );
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
        if (habit != null)
        {
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
    private async Task EditArchivedHabitAsync( ArсhivedHabitDto archivedHabit )
    {
        UserHabit userHabit = await ServiceOfHabit.UserHabitAsync( archivedHabit.Id );

        if (userHabit.Progresses?.Count > 0)
        {
            ServiceOfHabit.InitializeHabitProgresses( userHabit, StartProgressInterval, userHabit.Progresses[^1].Date );
        }
        else
        {
            ServiceOfHabit.InitializeHabitProgresses( userHabit, StartProgressInterval, EndProgressInterval );
        }

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
        await UiBusyFor( async () =>
        {
            List<ArсhivedHabitDto> archivedHabits = await ServiceOfHabit.GetArchivedHabits();
            ArchivedHabits = new ObservableCollection<ArсhivedHabitDto>( archivedHabits );
        } );
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
            await UiBusyFor( async () =>
            {
                await ServiceOfHabit.SetHabitArchiveStatusAsync( new HabitArchiveStatus
                {
                    HabitId = habit.Id, IsArchived = true
                } );
                UserHabits.Remove( habit );
            } );
        }
    }

    [RelayCommand]
    private async Task DeleteArchivedHabitAsync( ArсhivedHabitDto archivedHabit )
    {
        var multipleActionViewModel =
            ServiceProvider.GetRequiredService<MultipleActionPopupViewModel>();
        MultipleActionPopup popup = new( multipleActionViewModel );

        List<ActionData> availableActions =
        [
            new(
                LocStrings.RemoveFromArchive,
                async () =>
                {
                    await RemoveHabitFromArchiveAsync( archivedHabit );
                }
            ),
            new(
                LocStrings.DeleteTheHabit,
                async () =>
                {
                    await DeleteArchivedHabitInServerAsync( archivedHabit );
                }
            )
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

        if (doDelete)
        {
            await UiBusyFor( async () =>
            {
                HabitDeletionResponse? response = await ServiceOfHabit.DeleteAsync( habit.Id );

                if (response is not null)
                {
                    foreach (HabitDeletionResponse.NotificationRequest notification in response.DeletedNotifications)
                    {
                        LocalNotificationCenter.Current.Cancel( notification.Id );
                    }
                }

                ArchivedHabits?.Remove( habit );
            } ).DefaultConfigureAwait();
        }
    }

    private async Task RemoveHabitFromArchiveAsync( ArсhivedHabitDto archivedHabit )
    {
        await UiBusyFor( async () =>
        {
            await ServiceOfHabit.SetHabitArchiveStatusAsync( new HabitArchiveStatus
            {
                HabitId = archivedHabit.Id, IsArchived = false
            } );
            
            ArchivedHabits!.Remove( archivedHabit );

            UserHabit habit = await ServiceOfHabit.UserHabitAsync( archivedHabit.Id );

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
    private async Task HabitDetailAsync( UserHabit? habit )
    {
        if (habit != null)
        {
            Dictionary<string, object> routeParams = new()
            {
                { "Habit", habit },
            };
            await Navigation.NavigateToAsync<HabitDetailViewModel>( routeParams );
        }
    }

    [RelayCommand]
    private async Task ArchivedHabitDetailAsync(ArсhivedHabitDto? archivedHabit)
    {
        if (archivedHabit is null)
        {
            return;
        }

        UserHabit userHabit = await ServiceOfHabit.UserHabitAsync( archivedHabit.Id);

        if (userHabit.Progresses?.Count > 0)
        {
            ServiceOfHabit.InitializeHabitProgresses( userHabit, StartProgressInterval, userHabit.Progresses[^1].Date );
        }
        else
        {
            ServiceOfHabit.InitializeHabitProgresses( userHabit, StartProgressInterval, EndProgressInterval );
        }
        
        userHabit.IsArchived = true;

        Dictionary<string, object> routeParams = new()
        {
            { "Habit", userHabit },
            { "IsArchived", true }
        };
        
        await Navigation.NavigateToAsync<HabitDetailViewModel>(routeParams);
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
        openPopup();
        await UiBusyFor(async () =>
        {
            Reminder reminder = await ReminderService.HabitsReportReminderAsync() ?? new Reminder();
            
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

            if (reminder.IsEnabled)
            {
                DateTime notifyTime = DateTime.Today.Add( reminder.Time.ToTimeSpan() );
                await ReminderService.SaveLocallyAsync(
                    reminder.UserNotificationRequestId,
                    reminder.Title,
                    reminder.Description,
                    notifyTime,
                    ReminderRepeat.Daily
                );
            }
            else
            {
                LocalNotificationCenter.Current.Cancel( ReminderReport.UserNotificationRequestId );
            }

            closePopup();
        } );
    }

    private Task GetAccessToSendNotificationsAsync()
    {
        //TODO: it should support all Android versions which our app supports
        return LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    [RelayCommand]
    private async Task HabitStreakInfoAsync()
    {
        await TipService.ShowSnackbarAsync(
            LocStrings.HabitStreakExplanation,
            duration: TimeSpan.FromSeconds( 10 )
        );
    }
}