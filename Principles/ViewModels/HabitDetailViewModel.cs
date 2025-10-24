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

    private readonly SemaphoreSlim m_lockerOfHabitProgressUpdate;

    public HabitDetailViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ProgressOfHabitService = serviceProvider.GetRequiredService<IProgressOfHabitService>();

        EditedReminder = new EditedUserHabitReminder();
        m_lockerOfHabitProgressUpdate = new SemaphoreSlim( initialCount: 1, maxCount: 1 );
        
        FrequencyInfo = new();
        ReferenceMessenger.Register<HabitSavedMessage>( this, (_, msg) =>
        {
            if (Habit.Id == msg.Value.Id)
            {
                if (msg.Value.IsArchived)
                {
                    Habit.MergeFrom( msg.Value );
                }

                NotifyPropertyChanged( nameof( Habit ) );
                SetEditedRemider();
                SetFrequency();
            }
        } );

        InitPeriodsOfHabit();
    }

    public ISeries[] Series { get; set; }
    public IProgressOfHabitService ProgressOfHabitService { get; }
    public HabitFrequencyInfo FrequencyInfo { get; set; }

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

        if (query.TryGetValue( "ShowAd", out object? showAdObj ) && showAdObj is bool showAd)
        {
            if (showAd)
            {
                AdService.IfRequiredShowInterstitialAdAsync();
            }
        }
        else
        {
            AdService.IfRequiredShowInterstitialAdAsync();
        }

        base.ApplyQueryAttributes( query );
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        await base.InitializeAsync( parameter );

        SetEditedRemider();

        SetFrequency();

        SetCharts();
    }
    
    private void SetFrequency()
    {
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

        FrequencyInfo.Frequency = Habit.Frequency;
        FrequencyInfo.Period = SelectedPeriodOfHabit;

        NotifyPropertyChanged( nameof( FrequencyInfo ) );
    }

    private void SetCharts()
    {
        CalculateStreaks();
        LoadProgressChartData();
        DrawHabitExecutionChart();
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

        DayOfWeek[] orderedDays =
        [
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        ];

        Dictionary<DayOfWeek, string> dayNames = new()
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
                IsVisible = true,
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
                TextSize = 9,
                Labels = dayNames,
            }
        };

        HabitExecutionYAxes = new Axis[]
        {
            new Axis
            {
                Name = LocStrings.NumberOfExecution,
                NameTextSize = 14,
                MinLimit = 0,
                MinStep = 1,
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

        AdService.IfRequiredShowInterstitialAdAsync().GetAwaiter();

        var date = DateOnly.FromDateTime( selectedDateTime.Value );

        ProgressOfHabit? progressOfHabit = Habit!.Progresses!.FirstOrDefault( h => h.Date == date );
        if (selectedDateTime.Value.Date > DateTime.Today.Date)
        {
            await DialogService.ShowErrorAsync( LocStrings.YouCannotCompleteHabitInTheFuture );
        }
        else
        {
            int previousValueOfProgress;

            if (progressOfHabit is null)
            {
                previousValueOfProgress = ProgressValue.NO;

                progressOfHabit = new ProgressOfHabit { Date = date, Habit = Habit, Value = ProgressValue.YES_MANUAL };
                Habit.Progresses!.Add( progressOfHabit );
                Habit.Progresses =
                    new ObservableCollectionEx<ProgressOfHabit>( Habit.Progresses.OrderByDescending( p => p.Date ) );
            }
            else
            {
                previousValueOfProgress = progressOfHabit.Value;
                progressOfHabit.Value = ProgressValue.NextToggled( progressOfHabit.Value );

                progressOfHabit.Habit = Habit;
            }

            await m_lockerOfHabitProgressUpdate.WaitAsync();

            try
            {
                ServiceOfHabit.Recompute( Habit );

                ReferenceMessenger.Send( new MsgThatProgressOfHabitUpdated( progressOfHabit ) );

                SetCharts();
            }
            finally
            {
                m_lockerOfHabitProgressUpdate.Release();
            }

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

                        await m_lockerOfHabitProgressUpdate.WaitAsync();

                        try
                        {
                            ServiceOfHabit.Recompute( Habit );

                            ReferenceMessenger.Send( new MsgThatProgressOfHabitUpdated( progressOfHabit ) );

                            SetCharts();
                        }
                        finally
                        {
                            m_lockerOfHabitProgressUpdate.Release();
                        }

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
        
        if (Habit.Progresses!.Count(p => p.Value == ProgressValue.YES_MANUAL) < 2)
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
                    Labeler = v =>
                    {
                        var dateTime = DateTime.FromOADate( v );
                        string result = $"{dateTime.Day} {TranslateMonth( dateTime.Month )}";
                        return result;
                    },
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

        List<ProgressOfHabit> progresses = Habit.ComputedProgresses.GetKnown().ToList();
        ProgressOfHabit? firstExecuted = progresses.LastOrDefault(p => p.Value == ProgressValue.YES_MANUAL); 
        
        //TODO: scores are wider than progresses. We should make them shorter
        DateTime minDate = firstExecuted is null 
            ? DateTime.Today.Subtract( TimeSpan.FromDays( 1 ) ) 
            : firstExecuted.Date.ToDateTime( TimeOnly.MinValue );

        DateTime maxDate = progresses[0].Date.ToDateTime( TimeOnly.MinValue );

        TimeSpan totalRange = maxDate - minDate;

        List<double> customSeparators = new();

        if (totalRange > TimeSpan.FromDays( 16 ))
        {
            // Кількість бажаних дат + 1 (4 дат = 4 + 1)
            int segmentCount = 4;
            TimeSpan segment = TimeSpan.FromTicks( totalRange.Ticks / segmentCount );
            
            for (int i = 0; i < segmentCount; i++)
            {
                DateTime labelDate = minDate.AddTicks( segment.Ticks * i );
                customSeparators.Add( labelDate.ToOADate() );
            }
        }
        else
        {
            customSeparators.Add( minDate.ToOADate() );
        }
        
        customSeparators.Add( maxDate.ToOADate() );
        
        XAxes = new[]
        {
            new Axis
            {
                IsVisible = true,
                Labeler = v =>
                {
                    var dateTime = DateTime.FromOADate( v );
                    string result = $"{dateTime.Day} {TranslateMonth( dateTime.Month )}";
                    return result;
                },
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
        
        List<ProgressOfHabit> completedDates = storedHabit.ComputedProgresses.GetKnown()
            .Where( p => p.Value is ProgressValue.YES_MANUAL or ProgressValue.YES_AUTO)
            .OrderBy( d => d.Date )
            .ToList();

        CompletedDays = completedDates.Count( p => p.Value is ProgressValue.YES_MANUAL );
        
        Streaks.Clear();

        List<StreakData> streakList = new();
        int currentStreak = 0;
        int longestStreak = 0;
        DateOnly? prevDate = null;
        DateOnly? streakStartDate = null;

        foreach (ProgressOfHabit progress in completedDates)
        {
            if (prevDate == null || progress.Date == prevDate.Value.AddDays( 1 ))
            {
                if (currentStreak == 0)
                {
                    streakStartDate = progress.Date;
                }

                if (progress.Value == ProgressValue.YES_MANUAL)
                {
                    currentStreak++;
                }
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

                streakStartDate = progress.Date;
                currentStreak = 1;
            }

            longestStreak = Math.Max( longestStreak, currentStreak );
            prevDate = progress.Date;
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
            .Take( 5 )
            .ToList();

        while (topStreaks.Count < 5)
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

    private string TranslateMonth( int month )
    {
        string result = month switch
        {
            1 => LocStrings.JanuaryShort,
            2 => LocStrings.FebruaryShort,
            3 => LocStrings.MarchShort,
            4 => LocStrings.AprilShort,
            5 => LocStrings.MayShort,
            6 => LocStrings.JuneShort,
            7 => LocStrings.JulyShort,
            8 => LocStrings.AugustShort,
            9 => LocStrings.SeptemberShort,
            10 => LocStrings.OctoberShort,
            11 => LocStrings.NovemberShort,
            _ => LocStrings.DecemberShort,
        };
        
        return result;
    }

    private void LoadStreaksChartData()
    {
        List<int> values = Streaks.Select( s => s.StreakDays ).ToList();
        List<string> labels = Streaks
            .Select( s => s.StreakDays > 0 && s.StartDate != DateOnly.MinValue
                ? $"{s.StartDate.Day} {TranslateMonth( s.StartDate.Month )} - {s.EndDate.Day} {TranslateMonth( s.EndDate.Month )} {s.EndDate.ToString("yy")}"
                : string.Empty )
            .ToList();

        List<double> separators = Enumerable.Range( 0, labels.Count ).Select( i => (double)i ).ToList();

        StreakSeries =
        [
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
        ];

        StreakXAxes =
        [
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
                LabelsAlignment = Align.Start,
                CustomSeparators = separators
            }
        ];

        StreakYAxes =
        [
            new Axis
            {
                Name = LocStrings.NumberOfExecution,
                NameTextSize = 14,
                MinLimit = 0,
                TextSize = 12
            }
        ];

        if (values.All( v => v == 0 ))
        {
            StreakYAxes[0].MaxLimit = 30;
        }
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

    [RelayCommand]
    private async Task EditHabitAsync()
    {
        Dictionary<string, object> routeParams = new()
        {
            { "Habit", Habit },
            { "IsInHabitDetails", true },
            { "WillBeAdShownAfterBack", true }
        };
        
        await Navigation.NavigateToAsync<EditHabitViewModel>( routeParams );
    }

    [RelayCommand]
    private async Task DeleteHabitAsync()
    {
        if (Habit.IsArchived)
        {
            List<ActionData> availableActions =
            [
                new(
                    LocStrings.RemoveFromArchive,
                    RemoveHabitFromArchiveAsync
                ),
                new(
                    LocStrings.DeleteTheHabit,
                    DeleteHabitFromServerAsync
                )
            ];
            MultipleActionPopup popup = new( availableActions, LocStrings.DeleteArchivedHabitConfirmationText );
            
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
                    ServiceOfHabit.CancelAllRemindersOfHabit( Habit );
                    SetEditedRemider();
                    ReferenceMessenger.Send( new ArchiveHabitMessage( Habit, doShowAllArchivedHabits: false ) );
                    TipService.ShowToastAsync( LocManager["TheHabitIsArchived"]! ).GetAwaiter();
                }
                else
                {
                    await ServiceOfHabit.RestoreRemindersOfHabit( Habit );
                    ReferenceMessenger.Send( new HabitSavedMessage( Habit ) );
                    TipService.ShowToastAsync( LocManager["TheHabitIsUnarchived"]! ).GetAwaiter();
                }
            }).DefaultConfigureAwait();
        }
    }

    public override async Task OnDisappearingAsync( object? parameter = null )
    {
        await base.OnDisappearingAsync(parameter);

        Streaks = new ObservableCollection<StreakData>();
        Series = [];
        XAxes = [];
        YAxes = [];
        StreakSeries = [];
        StreakXAxes = [];
        StreakYAxes = [];
        HabitExecutionSeries = [];
        HabitExecutionXAxes = [];
        HabitExecutionYAxes = [];
    }

    [RelayCommand]
    private async Task ShowStreakTipAsync()
    {
        await TipService.ShowSnackbarAsync(
            LocStrings.TopFiveStreaksExplanation,
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
}