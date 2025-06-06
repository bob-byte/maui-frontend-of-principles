using CommunityToolkit.Maui.Views;

using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;

using SkiaSharp;

using System;
using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;

namespace Principles.ViewModels;

public partial class HabitDetailViewModel : BaseViewModel
{
    [ObservableProperty]
    private UserHabit m_habit;
    [ObservableProperty]
    private string m_frequencyRepresentation;
    [ObservableProperty]
    private PeriodOfHabit m_selectedPeriodOfHabit;
    [ObservableProperty]
    private ObservableCollectionEx<PeriodOfHabit> m_periodsOfHabit;
    [ObservableProperty]
    private EditedUserHabitReminder? m_editedReminder;
    [ObservableProperty]
    private ObservableCollectionEx<bool> m_isDayChecked = new();
    [ObservableProperty]
    private int m_completedDays;
    [ObservableProperty]
    private int m_longestStreak;
    [ObservableProperty]
    private ObservableCollection<StreakData> m_streaks = new();
    [ObservableProperty]
    private Chart m_streakChart;
    [ObservableProperty]
    private Chart m_stabilityChart;
    [ObservableProperty]
    private int m_chartWidth;
    [ObservableProperty]
    private ScoreList m_scoreListofHabit;
    [ObservableProperty]
    private ObservableCollection<DateTime> m_selectedDates = new();
    [ObservableProperty]
    private Axis[] m_xAxes;
    [ObservableProperty]
    private Axis[] m_yAxes;
    [ObservableProperty]
    private ISeries[] m_streakSeries;
    [ObservableProperty]
    private Axis[] m_streakXAxes;
    [ObservableProperty]
    private Axis[] m_streakYAxes;
    [ObservableProperty]
    private ISeries[] m_habitExecutionSeries;
    [ObservableProperty]
    private Axis[] m_habitExecutionXAxes;
    [ObservableProperty]
    private Axis[] m_habitExecutionYAxes;
    [ObservableProperty]
    private string? m_goalTitle;

    private SemaphoreSlim m_lockerOfHabitProgressUpdate;

    [ObservableProperty]
    private ObservableCollection<UserHabit> m_habitsWithSameGoal = new();

    public HabitDetailViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ProgressOfHabitService = serviceProvider.GetRequiredService<IProgressOfHabitService>();
        ServiceOfHabit = serviceProvider.GetRequiredService<IServiceOfHabit>();

        EditedReminder = new EditedUserHabitReminder();
        m_lockerOfHabitProgressUpdate = new SemaphoreSlim( initialCount: 1, maxCount: 1 );

        InitPeriodsOfHabit();
    }

    public ISeries[] Series { get; set; }
    public IProgressOfHabitService ProgressOfHabitService { get; }
    public IServiceOfHabit ServiceOfHabit { get; }

    public override void ApplyQueryAttributes( IDictionary<string, object> query )
    {
        if (query.TryGetValue( "Habit", out object? habitObj ) && habitObj is UserHabit habit)
        {
            Habit = habit;
        }
        else
        {
            throw new ArgumentException( "Habit is not supplied to HabitDetailViewModel" );
        }

        base.ApplyQueryAttributes( query );
    }

    public override Task InitializeAsync( object? parameter = null )
    {
        SetEditedRemider();

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

        UpdateFrequencyRepresentation( Habit.Frequency, SelectedPeriodOfHabit );
        SetCharts();
        GoalOfHabit();

        return base.InitializeAsync( parameter ); ;
    }

    private void SetCharts()
    {
        CalculateStreaks();
        LoadProgressChartData();
        DrawHabitExecutionChart();
    }

    public void GoalOfHabit()
    {
        if (Habit?.Goal == null)
        {
            GoalTitle = LocStrings.NoGoalSpecified;
            HabitsWithSameGoal.Clear();
            return;
        }

        GoalTitle = Habit.Goal.Name;
        List<UserHabit> relatedHabits = ServiceOfHabit.StoredUserHabits
            .Where( h => h.Goal?.Name == Habit.Goal.Name )
            .ToList();

        HabitsWithSameGoal.Clear();
        if (relatedHabits.Any())
        {
            foreach (UserHabit habit in relatedHabits)
            {
                HabitsWithSameGoal.Add( habit );
            }
        }
    }

    public List<HabitDayStat> CalculateDaysOfWeekData()
    {
        if (Habit?.Progresses == null || Habit.Progresses.Count == 0)
        {
            return new List<HabitDayStat>();
        }

        List<DateOnly> completedDates = Habit.Progresses
            .Where(p => p.Value == ProgressValue.YES_MANUAL || p.Value == ProgressValue.YES_AUTO)
            .Select(p => p.Date)
            .ToList();

        Dictionary<DayOfWeek, int> dayCounts = Enumerable.Range(0, 7).ToDictionary(i => (DayOfWeek)i, _ => 0);

        foreach (DateOnly date in completedDates)
        {
            dayCounts[date.DayOfWeek]++;
        }

        DayOfWeek[] orderedDays = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        Dictionary<DayOfWeek, string> dayNames = new Dictionary<DayOfWeek, string>
        {
            [DayOfWeek.Monday] = LocStrings.MondayShort,
            [DayOfWeek.Tuesday] = LocStrings.TuesdayShort,
            [DayOfWeek.Wednesday] = LocStrings.WednesdayShort,
            [DayOfWeek.Thursday] = LocStrings.ThursdayShort,
            [DayOfWeek.Friday] = LocStrings.FridayShort,
            [DayOfWeek.Saturday] = LocStrings.SaturdayShort,
            [DayOfWeek.Sunday] = LocStrings.SundayShort
        };

        return orderedDays.Select(d => new HabitDayStat
        {
            DayName = dayNames[d],
            ExecutionCount = dayCounts[d]
        }).ToList();
    }

    public void DrawHabitExecutionChart()
    {
        List<HabitDayStat> daysOfWeekData = CalculateDaysOfWeekData();
        List<int> dayCounts = daysOfWeekData.Select( d => d.ExecutionCount ).ToList();
        string[] dayNames = daysOfWeekData.Select( d => d.DayName ).ToArray();

        HabitExecutionSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Values = dayCounts,
                Stroke = null,
                Fill = new SolidColorPaint(new SKColor(33, 150, 243)),
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsSize = 14,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                IsHoverable = false,
                IsVisible = true
            }
        };

        HabitExecutionXAxes = new Axis[]
        {
            new Axis
            {
                IsVisible = true,
                MinLimit = -0.5,
                MaxLimit = 6.5,
                LabelsRotation = 0,
                TextSize = 12,
                Labels = dayNames
            }
        };

        HabitExecutionYAxes = new Axis[]
        {
            new Axis
            {
                Name = LocStrings.DaysCount,
                NameTextSize = 14,
                MinLimit = 0,
                TextSize = 12
            }
        };
    }

    [RelayCommand]
    private async Task DateTappedAsync( DateTime? selectedDateTime )
    {
        if (selectedDateTime == null)
        {
            return;
        }

        var date = DateOnly.FromDateTime( selectedDateTime.Value );

        ProgressOfHabit? progressOfHabit = Habit!.Progresses!.FirstOrDefault( h => h.Date == date );
        if (selectedDateTime.Value.Date > DateTime.Today.Date)
        {
            await DialogService.ShowErrorAsync( LocStrings.YouCannotCompleteHabitInTheFuture );
        }
        else if (Habit.Progresses![^1].Date > date)
        {
            await DialogService.ShowErrorAsync( LocStrings.HabitWasntCreatedBeforeThisDay );
        }
        else if (progressOfHabit is null)
        {
            LoggingService.LogError( "Progress of habit should exist but it was not created" );
            await DialogService.ShowErrorAsync( LocStrings.SomethingWentWrong );
        }
        else
        {
            int previousValueOfProgress = progressOfHabit.Value;
            progressOfHabit.Value = ProgressValue.NextToggled( progressOfHabit.Value );
            progressOfHabit.Habit = Habit;
            ServiceOfHabit.Recompute( Habit );

            ReferenceMessenger.Send( new MsgThatProgressOfHabitUpdated( progressOfHabit ) );

            bool doTryAgain;

            do
            {
                try
                {
                    await ProgressOfHabitService.UpdateAsync( progressOfHabit );
                    doTryAgain = false;
                }
                catch (Exception ex)
                {
                    doTryAgain = await DoRetryOperationOnErrorAsync( ex );
                    if (!doTryAgain)
                    {
                        progressOfHabit.Value = previousValueOfProgress;
                        ServiceOfHabit.Recompute( Habit );
                        
                        ReferenceMessenger.Send( new MsgThatProgressOfHabitUpdated( progressOfHabit ) );
                    }
                }
            } while (doTryAgain);
        }
    }

    private void InitPeriodsOfHabit()
    {
        PeriodsOfHabit =
        [
            new PeriodOfHabit { Type = PeriodTypeOfHabit.Week, Name = LocStrings.Week.ToLower() },
            new PeriodOfHabit { Type = PeriodTypeOfHabit.Month, Name = LocStrings.Month.ToLower() }
        ];
    }

    private void LoadProgressChartData()
    {

        ScoreList? scoreList = Habit.ScoreList;

        if (scoreList is null || !scoreList.GetAll().Any())
        {
            Series = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = new List<ObservablePoint>(),
                    GeometrySize = 10,
                    Stroke = new SolidColorPaint(SKColors.DeepSkyBlue, 2),
                    GeometryFill = new SolidColorPaint(SKColors.DeepSkyBlue),
                    Fill = null,
                    LineSmoothness = 0.5,
                }
            };

            XAxes = new[]
            {
                new Axis
                {
                    IsVisible = true,
                    Labeler = v => DateTime.FromOADate(v).ToString("dd.MM"),
                    LabelsRotation = 55,
                    TextSize = 10,
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 100,
                    MinStep = 20,
                    ForceStepToMin = true,
                    Labeler = v => $"{v:F0}%"
                }
            };

            OnPropertyChanged( nameof( Series ) );
            OnPropertyChanged( nameof( XAxes ) );
            OnPropertyChanged( nameof( YAxes ) );
            return;
        }

        List<Score> ordered = scoreList.GetAll()
            .OrderBy( s => s.Date )
            .ToList();

        if (ordered.Count < 2)
        {
            return;
        }

        List<(ObservablePoint point, string label)> events = [];
        double prevDiff = ordered[0].Value - ordered[1].Value;

        if (prevDiff != 0)
        {
            DateTime date = ordered[1].Date.ToDateTime( TimeOnly.MinValue );
            events.Add( (new ObservablePoint( date.ToOADate(), ordered[1].Value * 100 ), date.ToString( "dd.MM" )) );
        }

        for (int i = 2; i < ordered.Count; i++)
        {
            double diff = ordered[i].Value - ordered[i - 1].Value;
            //if (diff != 0 && Math.Sign( diff ) != Math.Sign( prevDiff ))
            if (diff != 0)
            {
                DateTime date = ordered[i].Date.ToDateTime( TimeOnly.MinValue );
                events.Add( (new ObservablePoint( date.ToOADate(), ordered[i].Value * 100 ), date.ToString( "dd.MM" )) );
                prevDiff = diff;
            }
        }

        LineSeries<ObservablePoint> mainSeries = new LineSeries<ObservablePoint>
        {
            Values = events.Select( e => e.point ).ToList(),
            GeometrySize = 1,
            Stroke = new SolidColorPaint( SKColors.DeepSkyBlue, 2 ),
            GeometryFill = new SolidColorPaint( SKColors.DeepSkyBlue ),
            Fill = null,
            LineSmoothness = 0.5,
            DataLabelsPaint = null,
            DataLabelsFormatter = null,
            GeometryStroke = new SolidColorPaint( SKColors.DeepSkyBlue )
        };

        Series = new ISeries[] { mainSeries };

        List<double?> xLabelsPositions = events.Select( e => (double?)e.point.X ).ToList();

        DateTime minDate = ordered[0].Date.ToDateTime( TimeOnly.MinValue );
        DateTime maxDate = ordered[ordered.Count - 1].Date.ToDateTime( TimeOnly.MinValue );

        // Кількість бажаних дат + 1 (6 дат = 6 + 1)
        int segmentCount = 7;

        TimeSpan totalRange = maxDate - minDate;
        TimeSpan segment = TimeSpan.FromTicks( totalRange.Ticks / segmentCount );

        List<double> customSeparators = new();

        for (int i = 0; i <= segmentCount; i++)
        {
            DateTime labelDate = minDate.AddTicks( segment.Ticks * i );
            customSeparators.Add( labelDate.ToOADate() );
        }
        XAxes = new[]
        {
            new Axis
            {
                IsVisible = true,
                Labeler = v => DateTime.FromOADate(v).ToString("dd.MM"),
                LabelsRotation = 45,
                TextSize = 10,
                CustomSeparators = customSeparators
            }
        };

        YAxes = new[]
        {
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 100,
                MinStep = 20,
                ForceStepToMin = true,
                Labeler = v => $"{v:F0}%"
            }
        };

        OnPropertyChanged( nameof( Series ) );
        OnPropertyChanged( nameof( XAxes ) );
        OnPropertyChanged( nameof( YAxes ) );
    }

    public void CalculateStreaks()
    {
        UserHabit? storedHabit = Habit;

        if (storedHabit?.ComputedProgresses == null || storedHabit.ComputedProgresses.GetKnown().Count() == 0)
        {
            CompletedDays = 0;
            LongestStreak = 0;
            Streaks.Clear();
            return;
        }
        
        List<DateOnly> completedDates = storedHabit.ComputedProgresses.GetKnown()
            .Where( p => p.Value == ProgressValue.YES_MANUAL)
            .Select( p => p.Date )
            .OrderBy( d => d )
            .ToList();

        CompletedDays = completedDates.Count;
        Streaks.Clear();

        List<StreakData> streakList = new();
        int currentStreak = 0;
        int longestStreak = 0;
        DateOnly? prevDate = null;
        DateOnly? streakStartDate = null;

        foreach (DateOnly date in completedDates)
        {
            if (prevDate == null || date == prevDate.Value.AddDays( 1 ))
            {
                if (currentStreak == 0)
                {
                    streakStartDate = date;
                }

                currentStreak++;
            }
            else
            {
                if (currentStreak >= 3 && streakStartDate.HasValue && prevDate.HasValue)
                {
                    streakList.Add( new StreakData
                    {
                        StartDate = streakStartDate.Value,
                        EndDate = prevDate.Value,
                        StreakDays = currentStreak
                    } );
                }

                streakStartDate = date;
                currentStreak = 1;
            }

            longestStreak = Math.Max( longestStreak, currentStreak );
            prevDate = date;
        }

        if (currentStreak >= 3 && streakStartDate.HasValue && prevDate.HasValue)
        {
            streakList.Add( new StreakData
            {
                StartDate = streakStartDate.Value,
                EndDate = prevDate.Value,
                StreakDays = currentStreak
            } );
        }

        List<StreakData> topStreaks = streakList
            .OrderByDescending( s => s.StreakDays )
            .ThenByDescending( s => s.StartDate )
            .Take( 7 )
            .ToList();

        while (topStreaks.Count < 7)
        {
            topStreaks.Add( new StreakData
            {
                StartDate = DateOnly.FromDateTime( DateTime.Now ),
                EndDate = DateOnly.FromDateTime( DateTime.Now ),
                StreakDays = 0
            } );
        }

        Streaks.Clear();
        foreach (StreakData streak in topStreaks)
        {
            if (streak.StreakDays == 0)
            {
                streak.StartDate = DateOnly.MinValue;
                streak.EndDate = DateOnly.MinValue;
            }

            Streaks.Add( streak );
        }

        LongestStreak = longestStreak;
        LoadStreaksChartData();
    }

    private void LoadStreaksChartData()
    {
        List<int> values = Streaks.Select( s => s.StreakDays ).ToList();
        List<string> labels = Streaks
            .Select( s => s.StreakDays > 0 && s.StartDate != DateOnly.MinValue
                ? $"{s.StartDate:dd.MM.yy}–{s.EndDate:dd.MM.yy}"
                : string.Empty )
            .ToList();

        List<double> separators = Enumerable.Range( 0, labels.Count ).Select( i => (double)i ).ToList();

        StreakSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Values = values,
                Stroke = null,
                Fill = new SolidColorPaint(new SKColor(33, 150, 243)),
                DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
                DataLabelsSize = 14,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                IsHoverable = false,
                IsVisible = true
            }
        };

        StreakXAxes = new Axis[]
        {
            new Axis
            {
                IsVisible = true,
                MinLimit = -0.5,
                MaxLimit = values.Count - 0.5,
                LabelsRotation = 35,
                TextSize = 10,
                Labeler = v =>
                {
                    int idx = (int)Math.Round(v);
                    return idx >= 0 && idx < labels.Count
                        ? labels[idx]
                        : string.Empty;
                },
                CustomSeparators = separators
            }
        };

        StreakYAxes = new Axis[]
        {
            new Axis
            {
                Name = LocStrings.DaysCount,
                NameTextSize = 14,
                MinLimit = 0,
                TextSize = 12
            }
        };
    }


    private void SetEditedRemider()
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

        OnPropertyChanged( nameof( EditedReminder ) );
    }

    public void UpdateFrequencyRepresentation( FrequencyOfHabit? frequency, PeriodOfHabit periodOfHabit )
    {
        if (frequency != null)
        {
            if (frequency.Repeats == 1)
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

    [RelayCommand]
    private async Task EditHabitAsync()
    {
        Dictionary<string, object> routeParams = new()
        {
            { "Id", Habit.Id },
        };
        await Navigation.NavigateToAsync<EditHabitViewModel>( routeParams );
    }

    [RelayCommand]
    private async Task DeleteHabitAsync()
    {
        if (Habit.IsArchived)
        {
            var popupViewModel = ServiceProvider.GetRequiredService<MultipleActionPopupViewModel>();
            List<ActionData> availableActions =
            [
                new(
                    LocStrings.RemoveFromArchive,
                    async () =>
                    {
                        await RemoveHabitFromArchiveAsync();
                    }
                ),
                new(
                    LocStrings.DeleteTheHabit,
                    async () =>
                    {
                        await DeleteHabitFromServerAsync();
                    }
                )
            ];
            popupViewModel.SetActions( availableActions, LocStrings.DeleteArchivedHabitConfirmationText );

            MultipleActionPopup popup = new( popupViewModel );
            await Shell.Current.ShowPopupAsync( popup );
        }
        else
        {
            bool doDelete = await DialogService.ShowConfirmAsync(
                LocStrings.MessageInDeleteHabitConfirm,
                LocStrings.DeleteHabitQuestion
            );

            if (doDelete)
            {
                await DeleteHabitFromServerAsync();
            }
        }
    }

    private async Task DeleteHabitFromServerAsync()
    {
        await UiBusyFor( async () =>
        {
            HabitDeletionResponse? response = await ServiceOfHabit.DeleteAsync( Habit.Id );

            if (response != null)
            {
                foreach (HabitDeletionResponse.NotificationRequest notification in response.DeletedNotifications)
                {
                    LocalNotificationCenter.Current.Cancel( notification.Id );
                }
            }

            await Navigation.GoBackAsync();
            ReferenceMessenger.Send( new HabitsDeletedMessege( Habit ) );

        } ).DefaultConfigureAwait();
    }

    private async Task RemoveHabitFromArchiveAsync()
    {
        await UiBusyFor( async () =>
        {
            await ServiceOfHabit.SetHabitArchiveStatusAsync( new HabitArchiveStatus
            {
                HabitId = Habit.Id, 
                IsArchived = false
            } );
            Habit.IsArchived = false;
            
            ReferenceMessenger.Send( new HabitSavedMessage( Habit ) );
        } );
    }

    [RelayCommand]
    private async Task ArchiveHabit()
    {
        bool newValueOfIsArchived = !Habit.IsArchived;
        
        string confirmMsg = newValueOfIsArchived ? LocStrings.MessageAddHabitToArchive : LocStrings.MessageRemoveHabitFromArchive;
        string confirmTitle = newValueOfIsArchived ? LocStrings.AddHabitToArchiveQuestion : LocStrings.UnarchiveHabitQuestion;
        
        bool isConfirmed = await DialogService.ShowConfirmAsync(confirmMsg, confirmTitle);
        
        if (isConfirmed)
        {
            await UiBusyFor(async () =>
            {
                await ServiceOfHabit.SetHabitArchiveStatusAsync( new HabitArchiveStatus
                {
                    HabitId = Habit.Id, 
                    IsArchived = newValueOfIsArchived
                } );
                
                Habit.IsArchived = newValueOfIsArchived;
                
                if (Habit.IsArchived)
                {
                    ReferenceMessenger.Send( new ArchiveHabitMessage( Habit ) );
                }
                else
                {
                    ReferenceMessenger.Send( new HabitSavedMessage( Habit ) );
                }
            }).DefaultConfigureAwait();
        }
    }

    [RelayCommand]
    private async Task ShowStreakTipAsync()
    {
        await TipService.ShowSnackbarAsync(
            LocStrings.TopSevenStreaksExplanation,
            duration: TimeSpan.FromSeconds( 10 )
        );
    }

    [RelayCommand]
    private async Task StabilityInfoAsync()
    {
        await TipService.ShowSnackbarAsync(
            LocStrings.StabilityExplanation,
            duration: TimeSpan.FromSeconds( 10 ) 
        );
    }

    [RelayCommand]
    private async Task HabitByDayWeeksAsync()
    {
        await TipService.ShowSnackbarAsync(
            LocStrings.HabitByDayweeksExplanation,
            duration: TimeSpan.FromSeconds( 10 ) 
        );
    }

    [RelayCommand]
    private async Task CalendarInfoAsync()
    {
        await TipService.ShowSnackbarAsync(
            LocStrings.CalendarInfoExplanation,
            duration: TimeSpan.FromSeconds( 10 ) 
        );
    }

    [RelayCommand]
    private void RefreshHabitCharts()
    {
        SetCharts();
    }
}