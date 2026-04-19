using DevExpress.Maui.Core.Internal;

using Plugin.LocalNotification;

using System.Collections.ObjectModel;
using System.Collections.Specialized;

using PropertyChangingEventArgs = Microsoft.Maui.Controls.PropertyChangingEventArgs;
using PropertyChangingEventHandler = System.ComponentModel.PropertyChangingEventHandler;

namespace Principles.ViewModels;

public partial class EditHabitViewModel : BaseViewModel
{
    [ObservableProperty]
    private UserHabit m_habit = new();

    [ObservableProperty]
    private ObservableCollectionEx<bool> m_isDayChecked = CreateDefaultDayChecks();

    [ObservableProperty]
    private ValidatableObject<string> m_nameOfHabit;

    [ObservableProperty]
    private bool m_isNewHabit;

    [ObservableProperty]
    private UserGoal m_editedGoal;

    [ObservableProperty]
    private PeriodOfHabit m_selectedPeriodOfHabit;

    [ObservableProperty]
    private string m_frequencyRepresentation;

    [ObservableProperty]
    private EditedUserHabitReminder? m_editedReminder;

    [ObservableProperty]
    private ObservableCollectionEx<RecommendedHabit> m_recommendedHabits;

    [ObservableProperty]
    private ObservableCollectionEx<UserGoal>? m_userGoals;

    [ObservableProperty]
    private bool m_isLoadingHabitInfo;

    [ObservableProperty]
    private bool m_isRecommendedHabitsLoading;

    private bool m_isInHabitDetails;

    [ObservableProperty]
    private ObservableCollectionEx<PeriodOfHabit> m_periodsOfHabit;

    [ObservableProperty]
    private ObservableCollectionEx<UserAreaOfLife> m_allUserAreasOfLife;

    [ObservableProperty]
    private HabitFrequencyInfo m_frequencyInfo;

    public UserAreaOfLife AllAreasOfLifeAsOneItem { get; }

    public IAiRecommenderOfHabitsService AiRecommenderOfHabits { get; }
    public IGoalService GoalService { get; }
    public IReminderService ReminderService { get; }

    public List<WeekDay> InactiveDaysToDelete { get; }

    public EditHabitViewModel( IServiceProvider serviceProvider )
        : base(serviceProvider)
    {
        AllAreasOfLifeAsOneItem = new UserAreaOfLife
        {
            Id = 0,
            Name = LocStrings.AllAreasOfLife
        };

        InitPeriodsOfHabit();

        AiRecommenderOfHabits = serviceProvider.GetRequiredService<IAiRecommenderOfHabitsService>();

        GoalService = serviceProvider.GetRequiredService<IGoalService>();
        ReminderService = serviceProvider.GetRequiredService<IReminderService>();

        AllUserAreasOfLife = new ObservableCollectionEx<UserAreaOfLife>();

        ReferenceMessenger.Register<UserLoggedOutMessage>( this, ( sender, msg ) =>
        {
            DefaultHandleLogout( msg );

            AllUserAreasOfLife?.Clear();
            ServiceOfHabit.StoredUserHabits?.Clear();
            UserGoals?.Clear();
        } );

        ReferenceMessenger.Register<NewCultureMessage>( this, async ( sender, msg ) =>
        {
            InitPeriodsOfHabit();

            AiRecommenderOfHabits.RecreateSystemMessage();

            AllAreasOfLifeAsOneItem.Name = LocStrings.AllAreasOfLife;
            try
            {
                await ReloadAllAreasOfLifeAsync();
            }
            catch
            {
                //do nothing (maybe no internet connection)
            }
        } );

        InactiveDaysToDelete = new List<WeekDay>();
    }

    private void InitPeriodsOfHabit()
    {
        PeriodsOfHabit =
        [
            new PeriodOfHabit { Type = PeriodTypeOfHabit.Week, Name = LocStrings.Week.ToLower() },
            new PeriodOfHabit { Type = PeriodTypeOfHabit.Month, Name = LocStrings.Month.ToLower() },
            new PeriodOfHabit { Type = PeriodTypeOfHabit.Year, Name = LocStrings.Year.ToLower() }
        ];
    }

    private static ObservableCollectionEx<bool> CreateDefaultDayChecks()
    {
        return
        [
            true, // Sunday
            true, // Monday
            true, // Tuesday
            true, // Wednesday
            true, // Thursday
            true, // Friday
            true, // Saturday
        ];
    }

    private PeriodOfHabit GetPeriodForFrequency( FrequencyOfHabit frequency )
    {
        PeriodTypeOfHabit periodType = frequency.IntervalLengthInDays switch
        {
            30 => PeriodTypeOfHabit.Month,
            365 => PeriodTypeOfHabit.Year,
            _ => PeriodTypeOfHabit.Week
        };

        return PeriodsOfHabit.FirstOrDefault( p => p.Type == periodType )
            ?? PeriodsOfHabit.FirstOrDefault( p => p.Type == PeriodTypeOfHabit.Week )
            ?? new PeriodOfHabit { Type = PeriodTypeOfHabit.Week, Name = LocStrings.Week.ToLower() };
    }

    //It is call on navigate to this EditHabitView
    public override void ApplyQueryAttributes( IDictionary<string, object> query )
    {
        IsLoadingHabitInfo = false;

        base.ApplyQueryAttributes( query );

        Habit = new UserHabit();
        InitValidations();

        if (query.TryGetValue( "Habit", out object? value ) && value is UserHabit habit)
        {
            Habit.MergeFrom( habit );
            ServiceOfHabit.Recompute( Habit );
            if (Habit.Goal is not null && Habit.Goal.Id == 0)
            {
                Habit.Goal = null;
            }

            IsNewHabit = false;
        }
        else
        {
            IsNewHabit = true;
        }

        if (query.TryGetValue( "IsArchived", out object? isArchivedValue ) && isArchivedValue is bool archived)
        {
            Habit.IsArchived = archived;
        }

        if (query.TryGetValue( "IsInHabitDetails", out object? objIsInHabitDetails ) &&
            objIsInHabitDetails is bool isInHabitDetails)
        {
            m_isInHabitDetails = isInHabitDetails;
        }
        else
        {
            m_isInHabitDetails = false;
        }
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        await InitUserInfoAsync();

        IsDayChecked = CreateDefaultDayChecks();

        RecommendedHabits = new ObservableCollectionEx<RecommendedHabit>();
        Habit.Frequency ??= new FrequencyOfHabit();

        if (IsNewHabit)
        {
            Habit.Id = 0;
            Habit.Type = TypeOfHabit.IntegrallyWise;
            Habit.AreasOfLife = new ObservableCollectionEx<UserAreaOfLife>();
            Habit.Complexity = 5;
            EditedReminder = new EditedUserHabitReminder();
            ResetDaysOfWeek();

            var normalTextColor = (Color)Application.Current!.Resources["LightNormalText"];
            Habit.ColorName = normalTextColor.ToArgbHex();
        }
        else
        {
            if (Habit.Complexity < HabitConstants.MIN_HABIT_COMPLEXITY || HabitConstants.MAX_HABIT_COMPLEXITY < Habit.Complexity)
            {
                Habit.Complexity = HabitConstants.DEFAULT_HABIT_COMPLEXITY;
            }

            EditedReminder = new EditedUserHabitReminder();

            if (Habit.Reminders != null && Habit.Reminders.Count > 0)
            {
                UserHabitReminder? reminder = Habit.Reminders.FirstOrDefault();
                if (reminder is not null)
                {
                    EditedReminder.Title = reminder.Title ?? string.Empty;
                    EditedReminder.Description = reminder.Description ?? string.Empty;

                    EditedReminder.Time = DateTime.Today.Add( reminder.Time.ToTimeSpan() );
                    EditedReminder.IsEnabled = reminder.IsEnabled;

                    if (reminder.DaysOfWeek != null)
                    {
                        for (int i = 0; i < IsDayChecked.Count; i++)
                        {
                            IsDayChecked[i] = false;
                        }

                        foreach (WeekDay day in reminder.DaysOfWeek.Where( d => d is not null ))
                        {
                            int dayIndex = (int)day.Type;
                            if (dayIndex >= 0 && dayIndex < IsDayChecked.Count)
                            {
                                IsDayChecked[dayIndex] = true;
                            }
                        }

                        EditedReminder.DaysOfWeek = reminder.DaysOfWeek
                            .Where( d => d is not null && (int)d.Type is >= 0 and <= 6 )
                            .Select( d => new WeekDay
                            {
                                Id = d.Id,
                                Type = d.Type,
                                UserNotificationRequestId = d.UserNotificationRequestId
                            } )
                            .ToList();
                    }
                }
            }

            Habit.AreasOfLife ??= new ObservableCollectionEx<UserAreaOfLife>();
            List<UserAreaOfLife> habitAreas = new( Habit.AreasOfLife.Count );

            foreach (UserAreaOfLife area in Habit.AreasOfLife!)
            {
                //localize names
                string? locName = LocManager[area.Name!];
                if (!string.IsNullOrWhiteSpace( locName ))
                {
                    area.Name = locName;
                }

                habitAreas.Add( area );
            }

            //otherwise, the areas are not localised for some reason
            Habit.AreasOfLife.Reload( habitAreas );

            if (Habit.AreasOfLife.Count == 0)
            {
                Habit.AreasOfLife.Add( AllAreasOfLifeAsOneItem );
            }
        }

        OnPropertyChanged( nameof( EditedReminder ) );
        OnPropertyChanged( nameof( IsDayChecked ) );

        InitValidations();

        SelectedPeriodOfHabit = GetPeriodForFrequency( Habit.Frequency! );

        FrequencyInfo = new HabitFrequencyInfo
        {
            Frequency = Habit.Frequency,
            Period = SelectedPeriodOfHabit
        };

        Habit.AreasOfLife.CollectionChanged += AreasOfLife_CollectionChanged;

        if (AllUserAreasOfLife.Count == 0)
        {
            await ReloadAllAreasOfLifeAsync();
        }
        else
        {
            AllUserAreasOfLife.Reload( AllUserAreasOfLife.ToList() );
        }

        Habit.Goal ??= new UserGoal();
        await ReloadGoalsAsync();

        NotifyPropertyChanged( nameof(Habit) );

        await base.InitializeAsync( parameter );

        IsLoadingHabitInfo = false;
    }

    public override Task OnDisappearingAsync( object? parameter = null )
    {
        EditedReminder = new EditedUserHabitReminder();
        EditedGoal = new UserGoal();
        Habit = new UserHabit();
        RecommendedHabits = new ObservableCollectionEx<RecommendedHabit>();

#if ANDROID
        UserGoals = new ObservableCollectionEx<UserGoal>();
        AllUserAreasOfLife = new ObservableCollectionEx<UserAreaOfLife>();
#endif

        return Task.CompletedTask;
    }

    private void NameOfHabitOnPropertyChanging( object? sender, System.ComponentModel.PropertyChangingEventArgs e )
    {
        if (e.PropertyName == nameof(ValidatableObject<string>.Value) && Habit!.Reminders?.Any() == true)
        {
            foreach (UserHabitReminder reminder in Habit.Reminders.Where( r => r.Description == NameOfHabit.Value ))
            {
                reminder.Description = "";
            }
        }
    }

    private void NameOfHabitOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if (e.PropertyName == nameof(ValidatableObject<string>.Value) && Habit!.Reminders?.Any() == true)
        {
            foreach (UserHabitReminder reminder in Habit.Reminders.Where( r => r.Description == "" ))
            {
                reminder.Description = NameOfHabit.Value;
            }
        }
    }

    public IList<int> GetSelectedDaysIndexes()
    {
        var selectedDaysIndexes = new List<int>();

        int dayCount = Math.Min( IsDayChecked.Count, 7 );
        for (int i = 0; i < dayCount; i++)
        {
            if (IsDayChecked[i])
            {
                selectedDaysIndexes.Add( i );
            }
        }

        return selectedDaysIndexes;
    }

    public void ResetDaysOfWeek()
    {
        IsDayChecked = CreateDefaultDayChecks();
    }
}