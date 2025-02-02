using DevExpress.Maui.Core.Internal;

using Plugin.LocalNotification;

using System.Collections.ObjectModel;
using System.Collections.Specialized;

using PropertyChangingEventArgs = Microsoft.Maui.Controls.PropertyChangingEventArgs;
using PropertyChangingEventHandler = System.ComponentModel.PropertyChangingEventHandler;

namespace Principles.ViewModels;

public partial class EditHabitViewModel : BaseViewModel, IQueryAttributable
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
    private ObservableCollectionEx<UserHabit> m_userHabits;

    [ObservableProperty]
    private ObservableCollectionEx<UserGoal>? m_userGoals;

    [ObservableProperty]
    private bool m_isLoadingHabitInfo;

    [ObservableProperty]
    private bool m_isRecommendedHabitsLoading;

    public EditHabitViewModel( IServiceProvider serviceProvider )
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

        AiRecommenderOfHabits = serviceProvider.GetRequiredService<IAiRecommenderOfHabitsService>();
        GoalService = serviceProvider.GetRequiredService<IGoalService>();
        ReminderService = serviceProvider.GetRequiredService<IReminderService>();

        AllUserAreasOfLife = new ObservableCollectionEx<UserAreaOfLife>();

        UserHabits = new ObservableCollectionEx<UserHabit>();
        ReferenceMessenger.Register<UserLoggedOutMessage>( this, ( sender, msg ) =>
        {
            DefaultHandleLogout( msg );

            AllUserAreasOfLife?.Clear();
            UserHabits?.Clear();
            UserGoals?.Clear();
        } );

        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            AllAreasOfLifeAsOneItem.Name = LocStrings.AllAreasOfLife;
            AllUserAreasOfLife.Reload( Habit.AreasOfLife!.ToList() );
        } );

        InactiveDaysToDelete = new List<WeekDay>();
    }

    public UserAreaOfLife AllAreasOfLifeAsOneItem { get; }

    public List<PeriodOfHabit> PeriodsOfHabit { get; }

    public IAiRecommenderOfHabitsService AiRecommenderOfHabits { get; }
    public IGoalService GoalService { get; }
    public IReminderService ReminderService { get; }

    public ObservableCollectionEx<UserAreaOfLife> AllUserAreasOfLife { get; set; }
    
    public List<WeekDay> InactiveDaysToDelete { get; }
    
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
            EditUserHabitDto dto = new()
            {
                AreasOfLife = Habit.AreasOfLife!.ToList(),//get copy, because it will be changed
                ColorName = Habit.ColorName,
                Description = Habit.Description,
                Complexity = Habit.Complexity,
                Frequency = Habit.Frequency,
                Id = Habit.Id,
                Name = Habit.Name,
                Priority = Habit.Priority,
                Question = Habit.Question,
                Goal = Habit.Goal,
                Status = Habit.Status,
                Type = Habit.Type,
                Reminders = Habit.Reminders,
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
                    await HandleHabitSaveAsync( response );
                    ReferenceMessenger.Send( new HabitSavedMessage( Habit, copyOfHabits ) );
                    await Navigation.GoBackAsync();
                } );
            }
            else
            {
                await UiBusyFor( async () =>
                {
                    SaveHabitResponse response = await ServiceOfHabit.UpdateHabitAsync( dto );
                    await HandleHabitSaveAsync( response );
                    await Navigation.GoBackAsync();
                    ReferenceMessenger.Send( new HabitSavedMessage( Habit, copyOfHabits ) );
                } );
            }
        }
    }

    public bool CanSave()
    {
        NameOfHabit.Validate();

        return NameOfHabit.IsValid && !IsBusy && !IsLoadingHabitInfo;
    }

    private async Task HandleHabitSaveAsync( SaveHabitResponse response )
    {
        Habit.Id = response.Id;
        Habit.Frequency!.Id = response.FrequencyId;

        if (response.ReminderIds is not null && Habit.Reminders?.Any() == true)
        {
            await RequestAccessToSendNotificationsAsync();

            for (int numReminder = 0; numReminder < response.ReminderIds.Count; numReminder++)
            {
                SaveHabitResponse.Reminder dtoOfReminder = response.ReminderIds[numReminder];
                UserHabitReminder habitReminder = Habit.Reminders[numReminder];
                habitReminder.Id = dtoOfReminder.Id;

                if (InactiveDaysToDelete.Count > 0)
                {
                    RemoveInactiveNotifications( InactiveDaysToDelete );
                }

                for (int numWeekDay = 0; numWeekDay < dtoOfReminder.DaysOfWeek?.Count; numWeekDay++)
                {
                    SaveHabitResponse.WeekDay dtoOfWeekDay = dtoOfReminder.DaysOfWeek[numWeekDay];
                    WeekDay? weekDay = habitReminder.DaysOfWeek.First( d => d.Type == dtoOfWeekDay.Type );
                    weekDay.Id = dtoOfWeekDay.Id;
                    weekDay.UserNotificationRequestId = dtoOfWeekDay.NotificationRequestId;

                    await AddNotificationToDeviceAsync( habitReminder, weekDay );
                }
            }
        }
    }

    private void RemoveInactiveNotifications( List<WeekDay> inactiveDays )
    {
        foreach (WeekDay day in inactiveDays)
        {
            LocalNotificationCenter.Current.Cancel( day.UserNotificationRequestId );
        }
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
        InitValidations();
        
        if (query.TryGetValue( "Id", out object? value ) && (long)value != 0)
        {
            Habit.Id = (long)value;

            IsNewHabit = false;
        }
        else
        {
            IsNewHabit = true;
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
            Habit.Type = TypeOfHabit.IntegrallyWise;
            Habit.AreasOfLife = new ObservableCollectionEx<UserAreaOfLife>();
            Habit.Complexity = 5;
            EditedReminder = new EditedUserHabitReminder();
            ResetDaysOfWeek();

            if (!UserHabits.Contains( Habit ))
            {
                UserHabits.Insert( index: 0, Habit );
            }

            int priority = 1;
            foreach (UserHabit habit in UserHabits)
            {
                habit.Priority = priority;
                priority++;
            }

            var normalTextColor = (Color)Application.Current!.Resources["LightNormalText"];
            Habit.ColorName = normalTextColor.ToArgbHex();
        }
        else
        {
            Habit = await ServiceOfHabit.UserHabitAsync( Habit.Id );
            if (Habit.Complexity < HabitConstants.MIN_HABIT_COMPLEXITY || HabitConstants.MAX_HABIT_COMPLEXITY < Habit.Complexity)
            {
                Habit.Complexity = 5;
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
            List<UserAreaOfLife> habitAreas = new( Habit.AreasOfLife.Count );

            foreach (UserAreaOfLife area in Habit.AreasOfLife!)
            {
                //localize names
                string? locName = LocStrings.ResourceManager.GetString( area.Name! );
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

            UserHabit? foundHabit = UserHabits.FirstOrDefault( h => h.Id == Habit.Id );
            if (foundHabit != null)
            {
                int index = UserHabits.IndexOf( foundHabit );
                UserHabits[index] = Habit;
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

        UpdateFrequencyRepresentation();

        Habit.AreasOfLife.CollectionChanged += AreasOfLife_CollectionChanged;

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
        else
        {
            AllUserAreasOfLife.Reload( AllUserAreasOfLife.ToList() );
        }

        Habit.Goal ??= new UserGoal();
        await ReloadGoalsAsync();

        await base.InitializeAsync( parameter );

        IsLoadingHabitInfo = false;
    }

    private async void AreasOfLife_CollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
    {
        if(!IsLoadingHabitInfo && e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
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
                await Task.Delay( millisecondsDelay: 5 );
                Habit.AreasOfLife!.Reload( areasOfLife );
            }
        }
    }

    [RelayCommand]
    private async Task ReloadRecommendedHabitsAsync( Action afterAction )
    {
        IsRecommendedHabitsLoading = true;

        bool doTryAgain;

        do
        {
            try
            {
                if (RecommendedHabits.Any())
                {
                    RecommendedHabits.Clear();
                }

                IEnumerable<UserHabit> userHabits = UserHabits.Where( u => u.Id > 0 );
                List<RecommendedHabit> recommendedHabits = await AiRecommenderOfHabits.RecommendedHabitsAsync(
                    userHabits,
                    Habit.AreasOfLife!,
                    Gender,
                    Mission,
                    MainSlogan,
                    Habit.Goal!.Name
                );
                RecommendedHabits.Reload( recommendedHabits );

                doTryAgain = false;

                afterAction();
            }
            catch (Exception ex)
            {
                doTryAgain = await DoRetryOperationOnErrorAsync( ex );
            }
        }
        while (doTryAgain);

        IsRecommendedHabitsLoading = false;
    }

    [RelayCommand]
    private void SelectedRecommendedHabit( RecommendedHabit recommendedHabit )
    {
        NameOfHabit.Value = recommendedHabit.Name;

        if (string.IsNullOrWhiteSpace( Habit.Description ))
        {
            Habit.Description = recommendedHabit.ReasonToFollow;
        }
        else
        {
            Habit.Description += $"{Environment.NewLine}{Environment.NewLine}{recommendedHabit.ReasonToFollow}";
        }
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

        if (NameOfHabit is null)
        {
            NameOfHabit = new ValidatableObject<string>();

            NameOfHabit.PropertyChanging += NameOfHabitOnPropertyChanging;
            NameOfHabit.PropertyChanged += NameOfHabitOnPropertyChanged;
            
            IValidationRule<string> rule = new IsNotNullOrWhiteSpaceRule( $"{LocStrings.TabData}. {LocStrings.FieldName} {isRequired}." );
            NameOfHabit.Validations.Add( rule );
        }
        else
        { 
            NameOfHabit.SetIsValid();
        }

        NameOfHabit.Value = Habit!.Name ?? string.Empty;
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

    [RelayCommand(CanExecute = nameof(CanSaveGoal))]
    private async Task SaveGoalAsync(Action afterAction)
    {
        bool isNewGoal = EditedGoal.Id == 0;

        await UiBusyFor( async () =>
        {
            DtoWithId response = await GoalService.SaveGoalAsync( EditedGoal );
            EditedGoal.Id = response.Id;

            if (isNewGoal)
            {
                UserGoals!.Add( EditedGoal );
            }
            else
            {
                // Store the old goal name before local saving changes
                string? oldGoalName = null;
                UserGoal? foundGoalInCollection = UserGoals!.FirstOrDefault( g => g.Id == EditedGoal.Id );
                if (foundGoalInCollection != null)
                {
                    oldGoalName = foundGoalInCollection.Name;

                    if (oldGoalName != EditedGoal.Name)
                    {
                        foundGoalInCollection.Name = EditedGoal.Name;
                        ReferenceMessenger.Send( new ChangedGoalMessage( EditedGoal ) );

                        if (Habit.Goal?.Id == EditedGoal.Id)
                        {
                            Habit.Goal.Name = EditedGoal.Name;
                        }

                        // Get all notifications and update the relevant ones
                        IList<NotificationRequest> notifications =
                            await LocalNotificationCenter.Current.GetPendingNotificationList();
                        if (!string.IsNullOrWhiteSpace( oldGoalName ))
                        {
                            if (Habit.Reminders is not null)
                            {
                                foreach (UserHabitReminder reminder in
                                             Habit.Reminders.Where( r => r.Title == oldGoalName ))
                                {
                                    reminder.Title = EditedGoal.Name!;
                                }
                            }

                            foreach (NotificationRequest? notification in notifications.Where( n =>
                                         n.Title == oldGoalName ))
                            {
                                notification.Title = EditedGoal.Name!;
                                await ReminderService.SaveAsync( notification );
                            }
                        }
                    }
                }
            }

            afterAction();
        } );
    }

    private bool CanSaveGoal()
    {
        return !string.IsNullOrWhiteSpace( EditedGoal.Name );
    }

    [RelayCommand]
    private async Task DeleteGoalAsync( UserGoal goal )
    {
        bool isConfirmedDelete = await DialogService.ShowAlertWithTwoBtnsAsync(
            LocStrings.MessageInDeleteGoalConfirm,
            LocStrings.DeleteGoalQuestion,
            LocStrings.OK,
            LocStrings.Cancel
        );

        if (isConfirmedDelete)
        {
            await UiBusyFor( async () =>
            {
                await GoalService.DeleteGoalAsync( goal );

                UserGoals!.Remove( goal );
                ReferenceMessenger.Send( new GoalIsDeletedMessage( goal ) );

                if (Habit.Goal?.Id > 0 && Habit.Goal.Id == goal.Id && ClearHabitGoalCommand.CanExecute( null ))
                {
                    ClearHabitGoalCommand.Execute( null );
                }
            } );
        }
    }

    [RelayCommand]
    private async Task DeleteHabitAsync( object? obj )
    {
        if (obj is UserHabit habit)
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
                    
                    await Navigation.GoBackAsync();
                    ReferenceMessenger.Send( new HabitsDeletedMessege( habit ) );

                } ).DefaultConfigureAwait();
            }
        }
    }

    [RelayCommand]
    private void ClearHabitGoal()
    {
        if (Habit.Goal is not null)
        {
            Habit.Goal.Id = 0;
            Habit.Goal.Name = null;
        }
    }

    [RelayCommand]
    public void OnGoalNameTapped( UserGoal goal )
    {
        if (goal != null)
        {
            EditedGoal = new UserGoal
            {
                Id = goal.Id,
                Name = goal.Name
            };
            Habit.Goal = goal;
        }
    }

    [RelayCommand]
    private void OpenUpdateGoalPopup( UserGoal goal )
    {
        EditedGoal = new UserGoal
        {
            Id = goal.Id,
            Name = goal.Name
        };
    }

    [RelayCommand]
    private async Task ReloadGoalsAsync()
    {
        UserGoals = new ObservableCollectionEx<UserGoal>();
        IEnumerable<UserGoal> goals = await GoalService.UserGoalsAsync();

        //sometimes goals are not displayed without reloading
        UserGoals.Reload( goals );

        GoalService.StoredGoals = UserGoals;
    }

    [RelayCommand]
    private Task TapHabitNameInfoAsync( VisualElement visualElement )
    {
        return visualElement.DisplaySnackbar(
            LocStrings.HabitNameRecommendation,
            duration: TimeSpan.FromSeconds( 6 ),
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
            LocStrings.ComplexityHelpText,
            duration: TimeSpan.FromSeconds( 6 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    [RelayCommand]
    private void ValidateHabitName()
    {
        NameOfHabit?.Validate();
    }

    public async Task AddReminderAsync()
    {
        await RequestAccessToSendNotificationsAsync();

        UserHabitReminder reminder = new()
        {
            Title = EditedReminder.Title.Clone() as string,
            Description = EditedReminder.Description.Clone() as string,
            Time = TimeOnly.FromDateTime( EditedReminder.Time ),
            IsEnabled = EditedReminder.IsEnabled,
            DaysOfWeek = new List<WeekDay>(EditedReminder.DaysOfWeek)
        };
        
        Habit.Reminders ??= new ObservableCollectionEx<UserHabitReminder>();

        if (Habit.Reminders.Count == 1)
        {
            reminder.Id = Habit.Reminders[0].Id;
            Habit.Reminders[0] = reminder;
        }
        else
        {
            Habit.Reminders.Add( reminder );
        }
        
        OnPropertyChanged( nameof( EditedReminder ) );
    }

    public Task RequestAccessToSendNotificationsAsync()
    {
        return LocalNotificationCenter.Current.RequestNotificationPermission();
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
            
            await ReminderService.SaveAsync( 
                weekDay.UserNotificationRequestId, 
                reminder.Title, 
                reminder.Description, 
                notifyDateTime, 
                ReminderRepeat.Weekly 
            );
        }
        else if (!IsNewHabit && weekDay.UserNotificationRequestId != 0)
        {
            //TODO: test how it works for new habit
            LocalNotificationCenter.Current.Cancel( weekDay.UserNotificationRequestId );
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

    public void NotifyPropertyChanged( string propertyName )
    {
        OnPropertyChanged( propertyName );
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
}
