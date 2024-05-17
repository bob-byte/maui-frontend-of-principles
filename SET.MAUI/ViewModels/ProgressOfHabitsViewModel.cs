
using DevExpress.Maui.DataGrid;

using SET.Core.Models;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace SET.MAUI.ViewModels;

public partial class ProgressOfHabitsViewModel : BaseViewModel
{
    private bool m_isInitialized;
    private readonly ConcurrentDictionary<UserHabit, SemaphoreSlim> m_isBusyForChangeCompleted;

    private UserHabit? m_selectedHabit;
    public UserHabit? SelectedHabit
    {
        get => m_selectedHabit;
        set
        {
            OnPropertyChanging();
            m_selectedHabit = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private DateOnly m_startProgressInterval;

    [ObservableProperty]
    private DateOnly m_endProgressInterval;

    [ObservableProperty]
    private ObservableCollectionEx<UserHabit> m_userHabits;

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
        Title = LocStrings.ProgressOfHabits;
        ReferenceMessenger.Register<HabitSavedMessage>( this, HandleHabitSave );
        ProgressOfHabitService = progressOfHabitService;

        EndProgressInterval = DateOnly.FromDateTime( DateTime.Today );
        StartProgressInterval = EndProgressInterval.AddDays( -HabitConstants.AVERAGE_NUMBER_OF_DAYS_TO_AUTOMATE_HABIT + 1 );
        m_isBusyForChangeCompleted = new ConcurrentDictionary<UserHabit, SemaphoreSlim>();
        UserHabits = new ObservableCollectionEx<UserHabit>();
    }

    public IProgressOfHabitService ProgressOfHabitService { get; }

    public void HandleHabitSave(object receiver, HabitSavedMessage message)
    {
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

            UserHabits.Add( savedHabit );
            m_isBusyForChangeCompleted.TryAdd( savedHabit, new SemaphoreSlim( 1, 1 ) );
        }
        else
        {
            //TODO: try to change frequency of existing habit and check whether icons of progresses is changed
            //ObservableCollectionEx<ProgressOfHabit> progresses = foundHabit.Progresses;
            foundHabit.Name = savedHabit.Name;
            foundHabit.Complexity = savedHabit.Complexity;
            //foundHabit.Progresses = progresses;
            foundHabit.Frequency = savedHabit.Frequency;
            foundHabit.Priority = savedHabit.Priority;

            ServiceOfHabit.Recompute( foundHabit );
        }

        foreach (UserHabit habit in message.PrioterizedHabits)
        {
            foundHabit = UserHabits.First( h => h.Id == habit.Id );
            foundHabit.Priority = habit.Priority;
        }

        UserHabits = new ObservableCollectionEx<UserHabit>( UserHabits.OrderBy( h => h.Priority ) );

        SelectedHabit = null;
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        await base.InitializeAsync( parameter );

        if (!m_isInitialized)
        {
            await InitUserInfoAsync();
            List<UserHabit> habits = await ServiceOfHabit.ActiveHabitsAsync( StartProgressInterval, EndProgressInterval );
            UserHabits.Reload( habits );

            foreach (UserHabit habit in habits)
            {
                m_isBusyForChangeCompleted.TryAdd( habit, new SemaphoreSlim( 1, 1 ) );
            }

            m_isInitialized = true;
        }
    }

    [RelayCommand]
    private async Task ChangeValueOfProgressOfHabitAsync( ProgressOfHabit progressOfHabit )
    {
        if (progressOfHabit == null)
        {
            return;
        }

        UserHabit habit = progressOfHabit.Habit!;

        try
        {
            SemaphoreSlim locker = m_isBusyForChangeCompleted[habit];

            await locker.WaitAsync();
            int previousValueOfProgress = progressOfHabit.Value;

            try
            {
                progressOfHabit.Value = ProgressValue.NextToggled( progressOfHabit.Value );
                await ProgressOfHabitService.UpdateAsync( progressOfHabit );
            }
            catch
            {
                progressOfHabit.Value = previousValueOfProgress;
                throw;
            }
            finally
            {
                locker.Release();
            }
        }
        catch ( Exception ex )
        {
            await DialogService.ShowErrorAsync( ex.Message );
            LoggingService.LogCriticalError( ex );
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

    [RelayCommand( CanExecute = nameof( CanResetPriorities ) )]
    private async Task ResetPrioritiesAsync()
    {
        UserHabit[] habits = UserHabits.ToArray();
        ServiceOfHabit.ResetPriorities( habits );

        var habitsWithPriorities = new UserHabitWithPriority[UserHabits.Count];
        for (int numHabit = 0; numHabit < habits.Length; numHabit++)
        {
            habitsWithPriorities[numHabit] = new UserHabitWithPriority
            {
                Id = habits[numHabit].Id,
                Priority = habits[numHabit].Priority
            };
        }

        try
        {
            await ServiceOfHabit.UpdatePrioritiesAsync( habitsWithPriorities );
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync( ex.ToString() );
        }
    }

    private bool CanResetPriorities()
    {
        return UserHabits.Count >= 2;
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
                try
                {
                    await ServiceOfHabit.DeleteAsync( habit.Id );

                    UserHabits.Remove( habit );
                    m_isBusyForChangeCompleted.TryRemove( habit, out SemaphoreSlim? locker );
                    locker?.Dispose();

                    if (habit.Id == SelectedHabit?.Id)
                    {
                        SelectedHabit = null;
                    }
                }
                catch(Exception ex)
                {
                    if (SettingsService.IsDebug)
                    {
                        await DialogService.ShowErrorAsync( ex.ToString() );
                    }
                    else
                    {
                        await DialogService.ShowErrorAsync( ex.Message );
                    }
                }
            }
        }
    }
}