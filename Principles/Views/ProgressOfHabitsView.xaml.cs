
using CommunityToolkit.Maui.Extensions;

using DevExpress.Maui.Controls;
using DevExpress.Maui.Core;
using DevExpress.Maui.DataGrid;
using Principles.Controls;

using System.Windows.Input;

using Application = Microsoft.Maui.Controls.Application;

namespace Principles.Views;

public partial class ProgressOfHabitsView : ContentPageBase
{
    private static readonly SemaphoreSlim s_lockerOfAddingNewDayColumn = new( initialCount: 1, maxCount: 1 );

    private Timer? m_newDayEventTimer;

    public ICommand SortByGoalCommand { get; }
    public ICommand SortByHabitTypeCommand { get; }

#if IOS
    private TimeZoneChangeObserver? m_timeZoneChangeObserver;
#elif ANDROID
    private TimeZoneChangedReceiver? m_timeZoneChangeReceiver;
#endif

    public ProgressOfHabitsView( ProgressOfHabitsViewModel viewModel )
    {
        BindingContext = viewModel;
        ViewModel = viewModel;

        InitializeComponent();

        if(ViewModel.IsInitialized)
        {
            InitDayChangeObserver();
        }

        SortByGoalCommand = new Command( SortByGoal );
        SortByHabitTypeCommand = new Command( SortByHabitType );

        if (ViewModel.SettingsService.IsAdsEnabled)
        {
            IAdService adService = ServiceLocator.Current!.GetRequiredService<IAdService>();
            if (adService.IsConsentConfigured)
            {
                AppendBannerAd();
            }
            else
            {
                adService.ConsentIsConfigured += ( _, _ ) => AppendBannerAd();
            }
        }

#if ANDROID31_0_OR_GREATER || IOS16_0_OR_GREATER
        SwipeItemInitialize();
#endif

        ViewModel.DataGridViewWithHabits = DGV_Habits;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        AddFirstCol();
        AddColumns();

        ViewModel.ReferenceMessenger.Register<TryAddNewDayInHabitListMessage>( this, ( sender, msg ) => TryAddNewDayColumn( null ) );
        ViewModel.ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            UpdateLocalizedStrings();
        } );

        ViewModel.ReferenceMessenger.Register<ShowAllArchivedHabitsMsg>( this, async ( _, _ ) =>
        {
            await ShowArchivedHabitsAsync();
        } );

        
    }

    // public IAsyncRelayCommand OpenArchiveCommand {get; set;}

    private void AppendBannerAd()
    {
        BoxView bannerPlaceholder = new()
        {
            Margin = new Thickness( 0, 0, 0, 10 ),
            HeightRequest = 50,
            WidthRequest = 320,
            Color = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start
        };

        G_Main.Children.Add( bannerPlaceholder );
        G_Main.SetRow( bannerPlaceholder, 2 );
    }

    private void UpdateLocalizedStrings()
    {
        RebuildViewOfActiveHabits();
    }

    private void RebuildViewOfActiveHabits()
    {
        DGV_Habits.Columns.Clear();
        AddGroupingColumn();
        AddFirstCol();
        AddColumns();
#if ANDROID31_0_OR_GREATER || IOS16_0_OR_GREATER
        DGV_Habits.StartSwipeItems.Clear();
        SwipeItemInitialize();
#endif
    }

    ~ProgressOfHabitsView()
    {
#if IOS
        m_timeZoneChangeObserver?.StopObservingTimeZoneChanges();
#elif ANDROID
        m_timeZoneChangeReceiver?.Dispose();
#endif

        m_newDayEventTimer?.Dispose();
    }

    private ProgressOfHabitsViewModel ViewModel { get; }

    private void ViewModel_PropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if (e.PropertyName == nameof( ViewModel.IsInitialized ) && ViewModel.IsInitialized)
        {
            InitDayChangeObserver();
        }
    }

    private async void TryAddNewDayColumn( object? state )
    {
        await s_lockerOfAddingNewDayColumn.WaitAsync();

        try
        {
            DateTime todayAsDatetime = DateTime.Today;
            DateOnly today = DateOnly.FromDateTime( todayAsDatetime );

            string dayOfWeek =
                ViewModel.LocManager[$"{today.DayOfWeek}Short"]!.ToUpperInvariant();
            int dayOfMonth = today.Day;
            string colCaption = $"{dayOfWeek}{Environment.NewLine}{dayOfMonth}";

            bool isAlreadyAddedCol = DGV_Habits.Columns[1].Caption.Equals( colCaption, StringComparison.CurrentCultureIgnoreCase ) ||
                                     DGV_Habits.Columns[2].Caption.Equals( colCaption, StringComparison.CurrentCultureIgnoreCase ) ||
                                     DGV_Habits.Columns[3].Caption.Equals( colCaption, StringComparison.CurrentCultureIgnoreCase ) ||
                                     (ViewModel.UserHabits?.Count > 0 && ViewModel.UserHabits[0].Progresses!.Any(p => p.Date == today) );

            if (!isAlreadyAddedCol && ViewModel.UserHabits is not null)
            {
                ViewModel.IsProgressesInitialized = false;

                await MainThread.InvokeOnMainThreadAsync( () =>
                {
                    ViewModel.EndProgressInterval = today;

                    foreach (UserHabit habit in ViewModel.UserHabits)
                    {
                        habit.Progresses!.Insert( index: 0,
                            new ProgressOfHabit
                            {
                                Id = 0,
                                Date = today,
                                Habit = habit,
                                Value = habit.DefaultProgressValue
                            } );

                        ViewModel.ServiceOfHabit.Recompute( habit );
                    }

#if IOS
                    DGV_Habits.Columns.Clear();

                    AddFirstCol();
                    AddColumns();
#else
                    TemplateColumn templateColumn = new()
                    {
                        Caption = colCaption,
                        HeaderFontSize = 9,
                        Width = new GridLength( 55 ),
                        HeaderCaptionLineBreakMode = LineBreakMode.WordWrap,
                        VerticalContentAlignment = TextAlignment.Center,
                        HorizontalContentAlignment = TextAlignment.Center,
                        HorizontalHeaderAlignment = TextAlignment.Center,
                        DisplayTemplate = new HabitWithProgressTemplateSelector( ViewModel, today )
                    };

                    DGV_Habits.Columns.Insert( index: 1, templateColumn );
#endif
                } );

                ViewModel.IsProgressesInitialized = true;
            }
        }
        finally
        {
            s_lockerOfAddingNewDayColumn.Release();
        }
    }

    private TimeSpan TimeUntilMidnight()
    {
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc( DateTime.UtcNow, TimeZoneInfo.Local );
        DateTime midnight = now.Date.AddDays( 1 );
        TimeSpan result = midnight - now;

        return result;
    }

#if ANDROID31_0_OR_GREATER || IOS16_0_OR_GREATER
    void SwipeItem_Tap( System.Object sender, DevExpress.Maui.DataGrid.SwipeItemTapEventArgs e )
    {
        ICommand command = ViewModel.DeleteHabitCommand;
        if (command.CanExecute( e.Item ))
        {
            command.Execute( e.Item );
        }
    }
#endif

    private void AddColumns()
    {
        DateTime endTime = ViewModel.StartProgressInterval.ToDateTime( TimeOnly.FromTimeSpan( TimeSpan.Zero ) );
        var oneDay = TimeSpan.FromDays( 1 );
        int columnIndex = 0;

        for (DateTime day = ViewModel.EndProgressInterval.ToDateTime( TimeOnly.FromTimeSpan( TimeSpan.Zero ) );
             endTime <= day;
             day = day.Subtract( oneDay ), columnIndex++)
        {
            string dayOfWeek =
                ViewModel.LocManager[$"{day.DayOfWeek}Short"]!.ToUpperInvariant();
            int dayOfMonth = day.Day;
            string colCaption = $"{dayOfWeek}{Environment.NewLine}{dayOfMonth}";

            TemplateColumn templateColumn = new()
            {
                Caption = colCaption,
                HeaderFontSize = 9,
                Width = new GridLength( 55 ),
                HeaderCaptionLineBreakMode = LineBreakMode.WordWrap,
                VerticalContentAlignment = TextAlignment.Center,
                HorizontalContentAlignment = TextAlignment.Center,
                HorizontalHeaderAlignment = TextAlignment.Center,
                DisplayTemplate = new HabitWithProgressTemplateSelector( ViewModel, DateOnly.FromDateTime( day ) )
            };

            DGV_Habits.Columns.Add( templateColumn );
        }
    }

    private void AddFirstCol()
    {
        TemplateColumn habitNameCol = new()
        {
            FixedStyle = FixedStyle.Start,
            Caption = " ",
            MinWidth = 200,

            DisplayTemplate = new DataTemplate( loadTemplate: () =>
            {
                Color primaryColor = (Color)Application.Current!.Resources["Primary"];
                Color normalTextColor = (Color)Application.Current.Resources["LightNormalText"];

                HorizontalStackLayout stack = new()
                {
                    VerticalOptions = LayoutOptions.Center,
                    Background = Colors.White,
                    Spacing = 4
                };


                CircularProgressBar progressBar = new();
                IValueConverter progressConverter = new ProgressOfHabitToInt32Converter();
                progressBar.Bind( CircularProgressBar.ProgressProperty, path: "Item.PercentageAchieved", converter: progressConverter );

                progressBar.Background = Colors.White;
                progressBar.ProgressColor = primaryColor;
                progressBar.TextColor = primaryColor;
                progressBar.ProgressLeftColor = (Application.Current!.Resources["GrayColor"] as Color)!;
                progressBar.Size = 30;
                progressBar.Thickness = 2.5f;
                progressBar.Margin = new Thickness( 3 );

                Label habitName = new()
                {
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    Background = Colors.White,
                    MaxLines = 2,
                    HeightRequest = 45,
#if IOS17_0_OR_GREATER
                    FontSize = 14
#elif IOS16_0_OR_GREATER
                    FontSize = 13
#elif IOS
                    FontSize = 12
#elif ANDROID
                    FontSize = 13
#endif
                };
                habitName.Bind( Label.TextProperty, path: "Item.Name", mode: BindingMode.TwoWay );

                habitName.TextColor = normalTextColor;
                habitName.WidthRequest = 145;

                stack.Add( progressBar );
                stack.Add( habitName );

                stack.BindTapGesture("HabitDetailCommand", ViewModel, parameterPath: "Item", numberOfTapsRequired: 1);

                return stack;
            } )
        };

        DGV_Habits.Columns.Add( habitNameCol );
    }

#if ANDROID31_0_OR_GREATER || IOS16_0_OR_GREATER
    private void SwipeItemInitialize()
    {
        LocalizationResourceManager.Initialize( ViewModel.SettingsService );

        GridSwipeItem editOnSwipe = new()
        {
            BackgroundColor = Application.Current!.Resources["Primary"] as Color,
            Image = ImageSource.FromFile( "edit_solid" )
        };
        editOnSwipe.SetBinding( GridSwipeItem.CommandProperty, new Binding( nameof( ProgressOfHabitsViewModel.EditHabitCommand ) ) );
        DGV_Habits.StartSwipeItems.Add( editOnSwipe );

        GridSwipeItem archiveOnSwipe = new()
        {
            BackgroundColor = Application.Current!.Resources["LightPrimary"] as Color,
            Image = ImageSource.FromFile( "archive_habit" )
        };
        archiveOnSwipe.SetBinding( GridSwipeItem.CommandProperty, new Binding( nameof( ProgressOfHabitsViewModel.ArchiveHabitCommand ) ) );
        DGV_Habits.StartSwipeItems.Add( archiveOnSwipe );

        GridSwipeItem swipeForDeletion = new()
        {
            BackgroundColor = Application.Current!.Resources["RedColor"] as Color,
            Image = ImageSource.FromFile( "delete_solid" )
        };
        swipeForDeletion.SetBinding( GridSwipeItem.CommandProperty, new Binding( nameof( ProgressOfHabitsViewModel.DeleteHabitCommand ) ) );
        DGV_Habits.StartSwipeItems.Add( swipeForDeletion );
    }
#endif

#if ANDROID31_0_OR_GREATER || IOS16_0_OR_GREATER
    private void SwipeItem_Invoked( object sender, EventArgs e )
    {
        ICommand command = ViewModel.DeleteHabitCommand;
        var swipeItem = sender as SwipeItemView;
        var cellData = swipeItem.BindingContext as CellData;

        if (command.CanExecute( cellData.Item ))
        {
            command.Execute( cellData.Item );
        }
    }
#endif

    private void DGV_Habits_CustomSort( object sender, CustomSortEventArgs e )
    {
        if (e.Column.FieldName == "Goal")
        {
            var goal1 = e.Value1 as UserGoal;
            var goal2 = e.Value2 as UserGoal;

            if (goal1 == null && goal2 == null) { e.Result = 0; }
            else if (goal1 == null) { e.Result = -1; }
            else if (goal2 == null) { e.Result = 1; }
            else
            {
                bool isFirstNoGoal = goal1.Name == LocStrings.NoGoalSpecified;
                bool isSecondNoGoal = goal2.Name == LocStrings.NoGoalSpecified;

                if (isFirstNoGoal && !isSecondNoGoal)
                {
                    e.Result = -1;
                }
                else if (!isFirstNoGoal && isSecondNoGoal)
                {
                    e.Result = 1;
                }
                else
                {
                    e.Result = goal1.Id.CompareTo( goal2.Id );
                }
            }
        }
        else if (e.Column.FieldName == "Type")
        {
            if (e.Value1 == null && e.Value2 == null) { e.Result = 0; }
            else if (e.Value1 == null) { e.Result = -1; }
            else if (e.Value2 == null) { e.Result = 1; }
            else
            {
                var kind1 = (TypeOfHabit)e.Value1;
                var kind2 = (TypeOfHabit)e.Value2;
                e.Result = kind1.CompareTo( kind2 );
            }
        }
        else
        {

        }
    }

    private async void DXI_Reminder_Tapped( object sender, TappedEventArgs e )
    {
        if (ViewModel.ReminderService.IsLocalNotificationSupported())
        {
            System.Action openPopup = () => DXP_Reminder.IsOpen = true;
            if (ViewModel.LoadHabitReportReminderCommand.CanExecute( openPopup ))
            {
                await ViewModel.LoadHabitReportReminderCommand.ExecuteAsync( openPopup );
            }
        }
        else
        {
            DXP_Reminder.IsOpen = false;

            await Snackbar.Make(
                LocStrings.DeviceDoesNotSupportNotifications,
                visualOptions: SnackbarHelper.DefaultOptions()
            ).Show();
        }
    }

    private async void SB_Save_Clicked( object sender, EventArgs e )
    {
        Action closePopup = () => DXP_Reminder.IsOpen = false;

        if (ViewModel.SaveHabitsReportReminderCommand.CanExecute( closePopup ))
        {
            await ViewModel.SaveHabitsReportReminderCommand.ExecuteAsync( closePopup );
        }
    }

    private void SB_ReminderReport_Cancel_Clicked( object sender, EventArgs e )
    {
        DXP_Reminder.IsOpen = false;
    }

    private async void DXI_Archive_Tapped( object sender, EventArgs e )
    {
        await ShowArchivedHabitsAsync();
    }

    private async void DXI_Habits_Tapped( object sender, TappedEventArgs e )
    {
        await ChooseProgressMarkVariatyAsync();
    }

    private void InitDayChangeObserver()
    {
        // intentionally left minimal for compile stability
    }

    private void SortByGoal()
    {
        // intentionally left minimal for compile stability
    }

    private void SortByHabitType()
    {
        // intentionally left minimal for compile stability
    }

    private void AddGroupingColumn()
    {
        // intentionally left minimal for compile stability
    }

    private Task ChooseProgressMarkVariatyAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ShowArchivedHabitsAsync()
    {
        if (ViewModel.GetArchivedHabitsCommand.CanExecute( null ))
        {
            double heightOfBottomSheet;

            if (ViewModel.SettingsService.NormalPageHeight == 0 ||
                DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Landscape)
            {
                heightOfBottomSheet = 300;
                ArchiveBottomSheet.HalfExpandedRatio = heightOfBottomSheet / CPB_Page.Height;
            }
            else
            {
                heightOfBottomSheet = 500;

                //bottom_sheet_height = full_height * HalfExpandedRatio
                //HalfExpandedRatio = bottom_sheet_height / full_height
                ArchiveBottomSheet.HalfExpandedRatio = heightOfBottomSheet / ViewModel.SettingsService.NormalPageHeight;
            }

            double rowSpacing = DXSL_ArchivedHabits.ItemSpacing * 2;
            double additionalSpacing = 40;
            double height = heightOfBottomSheet - rowSpacing - L_ArhiveCenralHeader.HeightRequest -
                            L_ArhiveCenralHeader.HeightRequest - additionalSpacing;

            SKL_Archive.HeightRequest = height;
            SKL_Archive.WidthRequest =
                PageWidth - (DXSL_ArchivedHabits.Padding.Left + DXSL_ArchivedHabits.Padding.Right);
            DXCV_Archive.HeightRequest = height;

            ArchiveBottomSheet.State = BottomSheetState.HalfExpanded;

            await ViewModel.GetArchivedHabitsCommand.ExecuteAsync( null );
        }

    }

    private void ME_ArchivedHabitEndIconClicked( object sender, EventArgs e )
    {
        ArchiveBottomSheet.State = BottomSheetState.Hidden;
    }

    private void DXI_Info_Tapped( object sender, TappedEventArgs e )
    {
        DXP_Tip.WidthRequest = CPB_Page.Width - 20;
        DXP_Tip.MinimumWidthRequest = CPB_Page.Width - 20;
        DXP_Tip.IsOpen = true;
    }

    private void B_Ok_Clicked( object sender, EventArgs e )
    {
        DXP_Tip.IsOpen = false;
    }

    private void DXI_Plus_Tapped( object sender, TappedEventArgs e )
    {
        if (ViewModel.CreateArchivedHabitCommand.CanExecute( null ))
        {
            ArchiveBottomSheet.State = BottomSheetState.Hidden;
            ViewModel.CreateArchivedHabitCommand.Execute( null );
        }
    }

    private void ArchivedHabitButton_Clicked( object sender, EventArgs e )
    {
        if (sender is Button button && button.CommandParameter is ArсhivedHabitDto archivedHabit)
        {
            ArchiveBottomSheet.State = BottomSheetState.Hidden;

            if (BindingContext is ProgressOfHabitsViewModel vm &&
            vm.ArchivedHabitDetailCommand.CanExecute( archivedHabit ))
            {
                vm.ArchivedHabitDetailCommand.Execute( archivedHabit );
            }
        }
    }
}
