
using DevExpress.Maui.DataGrid;

using SET.Core.Models;
using SET.MAUI.Exceptions;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace SET.MAUI.ViewModels;

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
    private ObservableCollectionEx<UserHabit> m_userHabits;

    private readonly SemaphoreSlim m_initLocker;

    ~ProgressOfHabitsViewModel()
    {
        foreach (SemaphoreSlim locker in m_isBusyForChangeCompleted.Values)
        {
            locker.Dispose();
        }
    }

    public ProgressOfHabitsViewModel( IServiceProvider serviceProvider, IProgressOfHabitService progressOfHabitService )
        : base( serviceProvider )
    {
        m_initLocker = new SemaphoreSlim( initialCount: 1, maxCount: 1 );

        Title = LocStrings.ProgressOfHabits;
        ReferenceMessenger.Register<HabitSavedMessage>( this, HandleHabitSave );
        ProgressOfHabitService = progressOfHabitService;

        EndProgressInterval = DateOnly.FromDateTime( DateTime.Today );
        StartProgressInterval = EndProgressInterval.AddDays( -HabitConstants.AVERAGE_NUMBER_OF_DAYS_TO_AUTOMATE_HABIT + 1 );
        m_isBusyForChangeCompleted = new ConcurrentDictionary<UserHabit, SemaphoreSlim>();
        m_userHabits = new ObservableCollectionEx<UserHabit>();

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
    }
    
    public IProgressOfHabitService ProgressOfHabitService { get; }

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

        foreach (UserHabit habit in message.PrioterizedHabits)
        {
            foundHabit = UserHabits.First( h => h.Id == habit.Id );
            foundHabit.Priority = habit.Priority;
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
            { "UserHabits", UserHabits }
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
                { "UserHabits", UserHabits }
            };
            await Navigation.NavigateToAsync<EditHabitViewModel>( routeParams );
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
                    await ServiceOfHabit.DeleteAsync( habit.Id );

                    UserHabits.Remove( habit );
                    ServiceOfHabit.StoredUserHabits?.Remove( habit );
                    m_isBusyForChangeCompleted.TryRemove( habit, out SemaphoreSlim? locker );
                    locker?.Dispose();

                    if (habit.Id == SelectedHabit?.Id)
                    {
                        SelectedHabit = DataGridViewWithHabits!.SelectedRowHandle >= 0
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
}