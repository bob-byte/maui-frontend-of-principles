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
    private UserHabit m_habit;

    [ObservableProperty]
    private ObservableCollectionEx<bool> m_isDayChecked = new();

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

    [ObservableProperty]
    private bool m_canSelectMindType;
    public class EnumOption<T>
    {
        public T Value { get; set; }
        public string Display { get; set; }
    }
    public List<EnumOption<NumericalHabitType>> MinMaxOptions { get; private set; }
    private void InitMinMaxOfHabit()
    {
        MinMaxOptions = new List<EnumOption<NumericalHabitType>>
        {
            new() { Value = NumericalHabitType.AtLeast, Display = LocStrings.Min },
            new() { Value = NumericalHabitType.AtMost, Display = LocStrings.Max }
        };
    }

    public UserAreaOfLife AllAreasOfLifeAsOneItem { get; }

    public IAiRecommenderOfHabitsService AiRecommenderOfHabits { get; }
    public IGoalService GoalService { get; }
    public IReminderService ReminderService { get; }

    public List<WeekDay> InactiveDaysToDelete { get; }
    [ObservableProperty]
    private DefaultProgressValue m_selectedDefaultProgressValue;

    [ObservableProperty]
    private ObservableCollectionEx<DefaultProgressValue> m_defaultProgressValues;

     [ObservableProperty]
    private String m_typeOfHabitInfo;

    public EditHabitViewModel( IServiceProvider serviceProvider )
: base( serviceProvider )
    {
        AllAreasOfLifeAsOneItem = new UserAreaOfLife
        {
            Id = 0,
            Name = LocStrings.AllAreasOfLife
        };

        InitDefaultProgerssValues();
        InitPeriodsOfHabit();
        InitMinMaxOfHabit();

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
            InitDefaultProgerssValues();
            InitPeriodsOfHabit();
            InitMinMaxOfHabit();
            ChangeTypeOfHabitInfo(TypeOfHabit.Flexible);

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

    // private void SelectedDefaultProgressValue
    [RelayCommand]
    private void SetDefaultProgressValue(DefaultProgressValue defaultValue)
    {
        Habit.DefaultProgressValue = defaultValue.Value;
    }

    private void InitPeriodsOfHabit()
    {
        PeriodsOfHabit =
        [
            new PeriodOfHabit { Type = PeriodTypeOfHabit.Week, Name = LocStrings.Week.ToLower() },
            new PeriodOfHabit { Type = PeriodTypeOfHabit.Month, Name = LocStrings.Month.ToLower() }
        ];
    }

    private void InitDefaultProgerssValues()
    {
        DefaultProgressValues =
        [
            new DefaultProgressValue { Value = ProgressValue.UNKNOWN, Name = LocStrings.UnknownValue, Icon = "question" },
            new DefaultProgressValue { Value = ProgressValue.SKIP, Name = LocStrings.SkipValue, Icon = "fire_fifth" }
        ];
    }

    private void ApplyQuery( IDictionary<string, object> query )
    {
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

        if (query.TryGetValue( "ProgressMarkVariaty", out object? kindObj ) && kindObj is ProgressMarkVariaty kind)
        {
            Habit.ProgressMarkVariaty = kind;
        }
        if (query.TryGetValue( "TypeOfHabit", out object? typeObj ) && typeObj is TypeOfHabit type)
        {
            Habit.Type = type;
        }
        CheckHabitProgressRestriction();

        AdService.IfRequiredShowInterstitialAdAsync();
    }

    public override async Task InitializePageAsync( IDictionary<string, object> query )
    {
        IsLoadingHabitInfo = true;

        ApplyQuery( query );

        IsDayChecked =
        [
            true, // Sunday
            true, // Monday
            true, // Tuesday
            true, // Wednesday
            true, // Thursday
            true, // Friday
            true, // Saturday
        ];

        RecommendedHabits = new ObservableCollectionEx<RecommendedHabit>();
        Habit.Frequency ??= new FrequencyOfHabit();

        if (IsNewHabit)
        {
            Habit.Id = 0;
            Habit.AreasOfLife = new ObservableCollectionEx<UserAreaOfLife>();
            Habit.Complexity = 5;
            EditedReminder = new EditedUserHabitReminder();
            Habit.MinRate = 0;
            Habit.MaxRate = 10;
            Habit.TargetPerOneTime = 0;
            ResetDaysOfWeek();

            var normalTextColor = (Color)Application.Current!.Resources["LightNormalText"];
            Habit.ColorName = normalTextColor.ToArgbHex();

            if (Habit.Type == TypeOfHabit.Flexible )
            {
                SelectedDefaultProgressValue = DefaultProgressValues.First( p => p.Value == ProgressValue.SKIP );
            }
            else
            {
                SelectedDefaultProgressValue = DefaultProgressValues.First( p => p.Value == ProgressValue.UNKNOWN );
            }
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
                UserHabitReminder reminder = Habit.Reminders.First();

                EditedReminder.Title = reminder.Title;
                EditedReminder.Description = reminder.Description;

                EditedReminder.Time = DateTime.Today.Add( reminder.Time.ToTimeSpan() );
                EditedReminder.IsEnabled = reminder.IsEnabled;

                if (reminder.DaysOfWeek != null)
                {
                    for (int i = 0; i < IsDayChecked.Count; i++)
                    {
                        IsDayChecked[i] = false;
                    }

                    foreach (WeekDay day in reminder.DaysOfWeek)
                    {
                        int dayIndex = (int)day.Type;
                        if (dayIndex >= 0 && dayIndex < IsDayChecked.Count)
                        {
                            IsDayChecked[dayIndex] = true;
                        }
                    }

                    EditedReminder.DaysOfWeek = reminder.DaysOfWeek.Select( d => new WeekDay
                    {
                        Id = d.Id,
                        Type = d.Type,
                        UserNotificationRequestId = d.UserNotificationRequestId
                    } ).ToList();
                }

            }

            Habit.AreasOfLife ??= new ObservableCollectionEx<UserAreaOfLife>();

            foreach (DefaultProgressValue el in DefaultProgressValues)
            {
                if(el.Value == Habit.DefaultProgressValue)
                    {
                        SelectedDefaultProgressValue = el;
                        break;
                    }
            }
        }

        OnPropertyChanged( nameof( EditedReminder ) );
        OnPropertyChanged( nameof( IsDayChecked ) );
        InitValidations();

        switch (Habit.Frequency!.IntervalLengthInDays)
        {
            default:
                {
                    SelectedPeriodOfHabit = PeriodsOfHabit.First( p => p.Type == PeriodTypeOfHabit.Week );
                    break;
                }
            case 30:
                {
                    SelectedPeriodOfHabit = PeriodsOfHabit.First( p => p.Type == PeriodTypeOfHabit.Month );
                    break;
                }
            case 365:
                {
                    SelectedPeriodOfHabit = PeriodsOfHabit.First( p => p.Type == PeriodTypeOfHabit.Year );
                    break;
                }
        }

        FrequencyInfo = new HabitFrequencyInfo
        {
            Frequency = Habit.Frequency,
            Period = SelectedPeriodOfHabit
        };

        Habit.Goal ??= new UserGoal();
        await ReloadGoalsAsync();

        NotifyPropertyChanged( nameof( Habit ) );

        await base.InitializePageAsync( query );

        IsLoadingHabitInfo = false;
    }

    public override async Task HandleDisappearingOfPageAsync( object parameter = null )
    {
        await base.HandleDisappearingOfPageAsync( parameter );

        //delay to not show user clearing
        await Task.Delay(1000);
        ClearViewModelData();
    }

    private void NameOfHabitOnPropertyChanging( object? sender, System.ComponentModel.PropertyChangingEventArgs e )
    {
        if (e.PropertyName == nameof( ValidatableObject<string>.Value ) && Habit!.Reminders?.Any() == true)
        {
            foreach (UserHabitReminder reminder in Habit.Reminders.Where( r => r.Description == NameOfHabit.Value ))
            {
                reminder.Description = "";
            }
        }
    }

    private void NameOfHabitOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if (e.PropertyName == nameof( ValidatableObject<string>.Value ) && Habit!.Reminders?.Any() == true)
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

        for (int i = 0; i < IsDayChecked.Count; i++)
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
        IsDayChecked.Clear();
        IsDayChecked.Add( true ); // Monday
        IsDayChecked.Add( true ); // Tuesday
        IsDayChecked.Add( true ); // Wednesday
        IsDayChecked.Add( true ); // Thursday
        IsDayChecked.Add( true ); // Friday
        IsDayChecked.Add( true ); // Saturday
        IsDayChecked.Add( true ); // Sunday
    }
    private void CheckHabitProgressRestriction( )
    {
        if (Habit.IsNew() || Habit.Progresses == null)
        {
            CanSelectMindType = true;
            return;
        }
        bool hasExistingProgress = Habit.Progresses.Any( p => p.Value != -1 && p.Value != 0);
        CanSelectMindType = !hasExistingProgress;
    }

    public void ChangeTypeOfHabitInfo(TypeOfHabit type)
    {
        if (type == TypeOfHabit.Principled)
        {
            TypeOfHabitInfo = LocStrings.PrincipleInfo;
        }
        else if(type == TypeOfHabit.Flexible)
        {
            TypeOfHabitInfo = LocStrings.FlexibleInfo;
        }
    }

    private void ClearViewModelData()
    {
        EditedReminder = new EditedUserHabitReminder();
        EditedGoal = new UserGoal();
        Habit = new UserHabit();
        RecommendedHabits = new ObservableCollectionEx<RecommendedHabit>();
        
#if ANDROID
        UserGoals = new ObservableCollectionEx<UserGoal>();
        AllUserAreasOfLife = new ObservableCollectionEx<UserAreaOfLife>();
#endif
    }
}
