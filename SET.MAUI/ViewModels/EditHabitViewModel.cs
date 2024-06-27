using System.Collections.Specialized;

namespace SET.MAUI.ViewModels;

public partial class EditHabitViewModel : BaseViewModel, IQueryAttributable
{
    [ObservableProperty]
    private UserHabit m_habit;

    [ObservableProperty]
    private ValidatableObject<string> m_nameOfHabit;

    [ObservableProperty]
    private ValidatableObject<string> m_reasonToFollow;

    [ObservableProperty]
    private bool m_isNewHabit;

    [ObservableProperty]
    private PeriodOfHabit m_selectedPeriodOfHabit;

    [ObservableProperty]
    private string m_frequencyRepresentation;

    [ObservableProperty]
    private ObservableCollectionEx<RecommendedHabit> m_recommendedHabits;

    [ObservableProperty]
    private ObservableCollectionEx<UserHabit> m_userHabits;

    [ObservableProperty]
    private string m_complexityHelpText;

    [ObservableProperty]
    private bool m_isLoadingHabitInfo;

    public EditHabitViewModel( IServiceProvider serviceProvider, IAiRecommenderOfHabitsService aiRecommenderOfHabits)
        : base(serviceProvider)
    {
        AllAreasOfLifeAsOneItem = new UserAreaOfLife
        {
            Id = 0,
            Name = LocStrings.AllAreasOfLife
        };

        PeriodsOfHabit = new List<PeriodOfHabit>
        {
            new PeriodOfHabit
            {
                Type = PeriodTypeOfHabit.Week,
                Name = LocStrings.ResourceManager.GetString("Week")!.ToLower()
            },
            new PeriodOfHabit
            {
                Type = PeriodTypeOfHabit.Month,
                Name = LocStrings.ResourceManager.GetString("Month")!.ToLower()
            }
        };
        AiRecommenderOfHabits = aiRecommenderOfHabits;
        AllUserAreasOfLife = new ObservableCollectionEx<UserAreaOfLife>();

        UserHabits = new ObservableCollectionEx<UserHabit>();
        ReferenceMessenger.Register<UserLoggedOutMessage>( this, ( sender, msg ) =>
        {
            DefaultHandleLogout( msg );

            AllUserAreasOfLife.Clear();
            UserHabits.Clear();
        } );
    }

    public UserAreaOfLife AllAreasOfLifeAsOneItem { get; }

    public List<PeriodOfHabit> PeriodsOfHabit { get; }

    public IAiRecommenderOfHabitsService AiRecommenderOfHabits { get; }

    public ObservableCollectionEx<UserAreaOfLife> AllUserAreasOfLife { get; set; }

    [RelayCommand(CanExecute = nameof(CanReorderHabits))]
    private void ReorderUserHabits()
    {
        UserHabit? habitWithSamePriority = UserHabits.FirstOrDefault( h => h.Priority == Habit.Priority && h.Id != Habit.Id );
        if (habitWithSamePriority != null)
        {
            List<UserHabit> copyOfHabits = UserHabits.Where( h => h.Id != Habit.Id ).ToList();//except Habit
            UserHabits.Clear();

            for (int priority = 1, numHabit = 0; priority <= copyOfHabits.Count + 1; priority++)
            {
                if (Habit.Priority == priority)
                {
                    UserHabits.Add( Habit );
                }
                else
                {
                    UserHabit userHabit = copyOfHabits[numHabit];
                    userHabit.Priority = priority;
                    UserHabits.Add( userHabit );

                    numHabit++;
                }
            }
        }
    }

    private bool CanReorderHabits()
    {
        return IsInitialized && UserHabits.Count > 1;
    }

    [RelayCommand(CanExecute = nameof( CanSave ) )]
    private async Task SaveAsync()
    {
        List<UserHabit> copyOfHabits = new( UserHabits );
        if(copyOfHabits.Any(h => h.Id == Habit.Id ))
        {
            copyOfHabits.Remove( Habit );
        }

        bool canSaveHabit;
        if (IsNewHabit)
        {
            canSaveHabit = ServiceOfHabit.CanAddNewHabit( Habit, copyOfHabits );

            if (!canSaveHabit)
            {
                canSaveHabit = await DialogService.ShowAlertWithTwoBtnsAsync(
                    msg: LocStrings.DescriptionOfCannotAddNewHabit,
                    title: LocStrings.TitleOfCannotAddNewHabit,
                    accept: LocStrings.AddItAnyway,
                    cancel: LocStrings.Cancel
                );
            }
        }
        else
        {
            canSaveHabit = true;
        }

        if (canSaveHabit)
        {
            Habit.Name = NameOfHabit.Value;
            Habit.ReasonToFollow = ReasonToFollow.Value;

            EditUserHabitDto dto = new()
            {
                AreasOfLife = Habit.AreasOfLife,
                ColorName = Habit.ColorName,
                Description = Habit.Description,
                Complexity = Habit.Complexity,
                Frequency = Habit.Frequency,
                Id = Habit.Id,
                Name = Habit.Name,
                Priority = Habit.Priority,
                Question = Habit.Question,
                ReasonToFollow = Habit.ReasonToFollow,
                Status = Habit.Status,
                Type = Habit.Type,
                PrioritizedHabits = new List<UserHabitWithPriority>()
            };

            dto.AreasOfLife!.Remove( AllAreasOfLifeAsOneItem );

            copyOfHabits.Add( Habit );
            foreach (UserHabit habit in copyOfHabits)
            {
                dto.PrioritizedHabits.Add( new UserHabitWithPriority { Id = habit.Id, Priority = habit.Priority } );
            }

            if (UserHabits.Count == 1)
            {
                await UiBusyFor( async () =>
                {
                    SaveHabitResponse response = await ServiceOfHabit.UpdateHabitAsync( dto );
                    Habit.Id = response.Id;
                    Habit.Frequency!.Id = response.FrequencyId;
                    ReferenceMessenger.Send( new HabitSavedMessage( Habit, copyOfHabits ) );
                    await Navigation.GoBackAsync();
                } );
            }
            else
            {
                await UiBusyFor( async () =>
                {
                    SaveHabitResponse response = await ServiceOfHabit.UpdateHabitAsync( dto );
                    Habit.Id = response.Id;
                    Habit.Frequency!.Id = response.FrequencyId;
                    await Navigation.GoBackAsync();
                    ReferenceMessenger.Send( new HabitSavedMessage( Habit, copyOfHabits ) );
                } );
            }
        }
    }

    public bool CanSave()
    {
        NameOfHabit.Validate();
        ReasonToFollow.Validate();

        return NameOfHabit.IsValid && ReasonToFollow.IsValid;
    }

    [RelayCommand]
    private Task BackAsync()
    {
        return Navigation.GoBackAsync();
    }

    //It is call on navigate to this EditHabitView
    public override void ApplyQueryAttributes( IDictionary<string, object> query )
    {
        IsLoadingHabitInfo = true;

        base.ApplyQueryAttributes( query );

        Habit = new UserHabit();
        if (query.TryGetValue( "Id", out object? value ) && (long)value != 0)
        {
            Habit.Id = (long)value;
            ComplexityHelpText = "";

            IsNewHabit = false;
        }
        else
        {
            IsNewHabit = true;
            ComplexityHelpText = LocStrings.ComplexityHelpText;
        }

        query.TryGetValue( "UserHabits", out object? userHabitsObj );
        if(userHabitsObj is IEnumerable<UserHabit> userHabits)
        {
            UserHabits = new ObservableCollectionEx<UserHabit>();
            foreach(UserHabit habit in userHabits)
            {
                UserHabits.Add( new UserHabit()
                {
                    Name = habit.Name,
                    Priority = habit.Priority,
                    Id = habit.Id,
                    Complexity = habit.Complexity,
                    ColorName = habit.ColorName,
                    AreasOfLife = habit.AreasOfLife,
                    PercentageAchieved = habit.PercentageAchieved,
                    Frequency = habit.Frequency,
                    Status = habit.Status
                } );
            }
        }

        query.TryGetValue( "CanHasSubhabits", out object? canHasSubhabitsObj );
        if(canHasSubhabitsObj is bool canHasSubhabits)
        {
            Habit.CanHasSubhabits = canHasSubhabits;
        }
        else
        {
            Habit.CanHasSubhabits = true;
        }
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        RecommendedHabits = new ObservableCollectionEx<RecommendedHabit>();
        Habit.Reminder ??= new Reminder();
        Habit.Frequency ??= new FrequencyOfHabit();

        try
        {
            if (Habit.Id != 0)
            {
                Habit = await ServiceOfHabit.UserHabitAsync( Habit.Id );
                if (Habit.Complexity < HabitConstants.MIN_HABIT_COMPLEXITY || HabitConstants.MIN_HABIT_COMPLEXITY < Habit.Complexity)
                {
                    Habit.Complexity = 5;
                }

                Habit.AreasOfLife ??= new ObservableCollectionEx<UserAreaOfLife>();
                foreach (UserAreaOfLife area in Habit.AreasOfLife!)
                {
                    //localize names
                    string? locName = LocStrings.ResourceManager.GetString( area.Name! );
                    if (!string.IsNullOrWhiteSpace( locName ))
                    {
                        area.Name = locName;
                    }
                }

                if (Habit.AreasOfLife.Count == 0)
                {
                    Habit.AreasOfLife.Add( AllAreasOfLifeAsOneItem );
                }

                UserHabit? foundHabit = UserHabits.FirstOrDefault( h => h.Id == Habit.Id );
                if (foundHabit != null)
                {
                    int index = UserHabits.IndexOf( foundHabit );
                    UserHabits[index] = Habit;
                }
            }
            else
            {
                Habit.Id = 0;
                UserHabits.Add( Habit );
                Habit.AreasOfLife = new ObservableCollectionEx<UserAreaOfLife>();
                Habit.Complexity = 5;
                Habit.Priority = UserHabits.Count;

                var normalTextColor = (Color)Application.Current!.Resources["LightNormalText"];
                Habit.ColorName = normalTextColor.ToArgbHex();
            }

            if (AllUserAreasOfLife.Count == 0)
            {
                List<UserAreaOfLife> areasOfLife = await AreaOfLifeService.UserAreasOfLife();
                foreach (UserAreaOfLife area in areasOfLife)
                {
                    //localize names
                    string? locName = LocStrings.ResourceManager.GetString( area.Name! );
                    if (!string.IsNullOrWhiteSpace( locName ))
                    {
                        area.Name = locName;
                    }
                }
                areasOfLife.Insert( index: 0, AllAreasOfLifeAsOneItem );

                AllUserAreasOfLife.Reload( areasOfLife );
            }

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

            UpdateFrequencyRepresentation();

            Habit.AreasOfLife.CollectionChanged += AreasOfLife_CollectionChanged;

            await base.InitializeAsync( parameter );
        }
        finally
        {
            IsLoadingHabitInfo = false;
        }
    }

    private async void AreasOfLife_CollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
    {
        if(e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            List<UserAreaOfLife> areasOfLife = Habit.AreasOfLife!.ToList();

            bool isAddedAllAreasOfLifeAsOneItem = e.NewItems.Contains( AllAreasOfLifeAsOneItem );
            if (isAddedAllAreasOfLifeAsOneItem)
            {
                for (int index = areasOfLife.Count - 1; index >= 0; index--)
                {
                    if (areasOfLife[index] != AllAreasOfLifeAsOneItem)
                    {
                        areasOfLife.RemoveAt( index );
                    }
                }
            }
            else
            {
                areasOfLife.Remove( AllAreasOfLifeAsOneItem );
            }

            if (areasOfLife.Count != Habit.AreasOfLife!.Count)
            {
                //to change collection after collection change event
                await Task.Delay( millisecondsDelay: 20 );
                Habit.AreasOfLife!.Reload( areasOfLife );
            }
        }
    }

    [RelayCommand]
    private async Task ReloadRecommendedHabitsAsync( Action afterAction )
    {
        await UiBusyFor( async () =>
        {
            if (RecommendedHabits.Any())
            {
                RecommendedHabits.Clear();
            }

            List<RecommendedHabit> recommendedHabits = await AiRecommenderOfHabits.RecommendedHabitsAsync(
                UserHabits,
                Habit.AreasOfLife!,
                Gender,
                Mission,
                MainSlogan
            );
            RecommendedHabits.Reload( recommendedHabits );
        } );

        afterAction();
    }

    [RelayCommand]
    private void SelectedRecommendedHabit( RecommendedHabit recommendedHabit )
    {
        NameOfHabit.Value = recommendedHabit.Name;
    }

    [RelayCommand(CanExecute = nameof(CanResetPriorities))]
    private void ResetPriorities()
    {
        ServiceOfHabit.ResetPriorities( UserHabits );
    }

    private bool CanResetPriorities()
    {
        return UserHabits.Count >= 2;
    }

    private void InitValidations()
    {
        string isRequired = LocStrings.isRequired;

        if (NameOfHabit == null)
        {
            NameOfHabit = new ValidatableObject<string>();

            IValidationRule<string> rule = new IsNotNullOrWhiteSpaceRule( $"{LocStrings.TabData}. {LocStrings.FieldName} {isRequired}." );
            NameOfHabit.Validations.Add( rule );
        }
        NameOfHabit.Value = Habit.Name;

        if (ReasonToFollow == null)
        {
            ReasonToFollow = new ValidatableObject<string>();

            IValidationRule<string> rule = new IsNotNullOrWhiteSpaceRule( $"{LocStrings.TabHowToKeep}. {LocStrings.FieldReasonToFollow} {isRequired}" );
            ReasonToFollow.Validations.Add( rule );
        }
        ReasonToFollow.Value = Habit.ReasonToFollow;
    }

    public void UpdateFrequencyRepresentation(FrequencyOfHabit? frequency, PeriodOfHabit periodOfHabit)
    {
        if(frequency != null)
        {
            if(frequency.Repeats == 1)
            {
                switch (frequency.Type)
                {
                    case FrequencyType.EveryDay:
                        {
                            FrequencyRepresentation = LocStrings.EveryDay;
                            break;
                        }

                    case FrequencyType.EverySeveralDays:
                        {
                            FrequencyRepresentation = $"{LocStrings.Every} {frequency.IntervalLengthInDays} {LocStrings.days}";
                            break;
                        }

                    case FrequencyType.SeveralTimesPerPeriod:
                        {
                            FrequencyRepresentation = periodOfHabit.Type switch
                            {
                                PeriodTypeOfHabit.Month => LocStrings.EveryMonth,
                                PeriodTypeOfHabit.Year => LocStrings.EveryYear,
                                _ => LocStrings.EveryWeek
                            };
                            break;
                        }
                }
            }
            else
            {
                FrequencyRepresentation = $"{frequency.Repeats} {LocStrings.timesPer} {periodOfHabit?.Name}";
            }
        }
    }

    public void UpdateFrequencyRepresentation()
    {
        UpdateFrequencyRepresentation( Habit?.Frequency, SelectedPeriodOfHabit );
    }

    [RelayCommand]
    private Task TapWithoutExceptionsTypeAsync(VisualElement visualElement)
    {
        Habit.Type = TypeOfHabit.WithoutExceptions;
        return visualElement.DisplaySnackbar(
            LocStrings.WithoutExceptionsHabitTypeShortDescription,
            duration: TimeSpan.FromMinutes(1),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    [RelayCommand]
    private Task ShowSnackbarForReasonToFollow( VisualElement visualElement )
    {
        return visualElement.DisplaySnackbar(
            LocStrings.ReasonToFollowExplanation,
            duration: TimeSpan.FromMinutes( 1 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    [RelayCommand]
    private Task ShowSnackbarForQuestion( VisualElement visualElement )
    {
        return visualElement.DisplaySnackbar(
            LocStrings.QuestionExplanation,
            duration: TimeSpan.FromMinutes( 1 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    [RelayCommand]
    private Task TapIntegrallyWiseTypeAsync( VisualElement visualElement )
    {
        Habit.Type = TypeOfHabit.IntegrallyWise;
        return visualElement.DisplaySnackbar(
            LocStrings.IntegrallyWiseHabitTypeShortDescription,
            duration: TimeSpan.FromMinutes(1),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    [RelayCommand]
    private Task TapComplexityInfoAsync( VisualElement visualElement )
    {
        return visualElement.DisplaySnackbar(
            ComplexityHelpText,
            duration: TimeSpan.FromSeconds( 6 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }
}