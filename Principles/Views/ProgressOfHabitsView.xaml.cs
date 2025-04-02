using CommunityToolkit.Maui.Behaviors;

using DevExpress.Maui.Controls;
using DevExpress.Maui.Core;
using DevExpress.Maui.DataGrid;
using Plugin.LocalNotification;
using Principles.Controls;
using System.Windows.Input;

using Application = Microsoft.Maui.Controls.Application;
using SwipeItem = DevExpress.Maui.DataGrid.SwipeItem;

namespace Principles.Views;

public partial class ProgressOfHabitsView : ContentPageBase
{
    private static readonly SemaphoreSlim s_lockerOfAddingNewDayColumn = new ( initialCount: 1, maxCount: 1 );

    private Timer? m_newDayEventTimer;

#if IOS
    private TimeZoneChangeObserver? m_timeZoneChangeObserver;
#elif ANDROID
    private TimeZoneChangedReceiver? m_timeZoneChangeReceiver;
#endif

    public ProgressOfHabitsView(ProgressOfHabitsViewModel viewModel)
	{
        BindingContext = viewModel;
        ViewModel = viewModel;

        InitializeComponent();
        LoadLocalizationData();

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
            LoadLocalizationData();
        } );

    }

    private void LoadLocalizationData()
    {
        L_MyHabits.Text = LocStrings.MyHabits;
        L_LoadingContent.Text = LocStrings.LoadingContent;
        L_HabitCollectionIsEmptyDescription.Text = LocStrings.HabitCollectionIsEmptyDescription;
        L_Reminder.Text = LocStrings.Reminder;
        ME_ReminderTitle.LabelText = LocStrings.ReminderTitle;
        ME_ReminderDescription.LabelText = LocStrings.ReminderDescription;
        TIE_Time.LabelText = LocStrings.Time;
        SB_ReminderReport_Cancel.Text = LocStrings.Cancel;
        SB_Save.Text = LocStrings.Save;
    }

    private void UpdateLocalizedStrings()
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
    }

    private ProgressOfHabitsViewModel ViewModel { get; }

    private void ViewModel_PropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if (e.PropertyName == nameof( ViewModel.IsInitialized ) && ViewModel.IsInitialized && m_newDayEventTimer is null)
        {
            TimeSpan timeUntilMidnight = TimeUntilMidnight();
            TimeSpan period = TimeSpan.FromHours( 24 );
            m_newDayEventTimer = new Timer( TryAddNewDayColumn, state: null, dueTime: timeUntilMidnight, period );

#if IOS
            m_timeZoneChangeObserver = new TimeZoneChangeObserver();
            m_timeZoneChangeObserver.StartObservingTimeZoneChanges( ( notification ) =>
            {
                TimeSpan timeUntilMidnight = TimeUntilMidnight();
                m_newDayEventTimer!.Change( timeUntilMidnight, period );
            } );
#elif ANDROID
            m_timeZoneChangeReceiver = new TimeZoneChangedReceiver( ( context, intent ) =>
            {
                TimeSpan timeUntilMidnight = TimeUntilMidnight();
                m_newDayEventTimer!.Change( timeUntilMidnight, period );
            } );
#endif
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

            bool isAlreadyAddedCol = DGV_Habits.Columns[1].Caption.Equals( colCaption, StringComparison.CurrentCultureIgnoreCase )  ||
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
                                Id = 0, Date = today, Habit = habit, Value = ProgressValue.UNKNOWN
                            } );

                        ViewModel.ServiceOfHabit.Recompute( habit );
                    }
                    
#if IOS
                    DGV_Habits.Columns.Clear();

                    AddGroupingColumn();
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

    private void AddGroupingColumn()
    {
        TextColumn goalColumn = new()
        {
            FieldName = "Goal.Name",
            IsGrouped = true,
            SortMode = DataSortMode.Custom,
            GroupInterval = DataGroupInterval.DisplayText,
            GroupCaptionTemplate = new DataTemplate( () =>
            {
                Label label = new()
                {
                    FontSize = 15,
                    TextColor = Colors.Black
                };
                label.SetBinding( Label.TextProperty, new Binding( path: "GroupValueText" ) );
                return label;
            } )
        };

        DGV_Habits.Columns.Add( goalColumn );
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
                    Spacing = 4
                };

                stack.BindTapGesture( "EditHabitCommand", commandSource: ViewModel, parameterPath: "Item", numberOfTapsRequired: 1 );

                CircularProgressBar progressBar = new();
                IValueConverter progressConverter = new ProgressOfHabitToInt32Converter( ViewModel.ProgressOfHabitService );
                progressBar.Bind( CircularProgressBar.ProgressProperty, path: "Item.PercentageAchieved", converter: progressConverter );

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

                return stack;
            } )
        };

        DGV_Habits.Columns.Add( habitNameCol );
    }

#if ANDROID31_0_OR_GREATER || IOS16_0_OR_GREATER
    private void SwipeItemInitialize() 
    {
        SwipeItem swipeForDeletion = new()
        {
            BackgroundColor = Application.Current!.Resources["RedColor"] as Color,
            Caption = (string)LocalizationResourceManager.Instance["Delete"]
        };
        swipeForDeletion.SetBinding( SwipeItem.CommandProperty, new Binding( nameof( ProgressOfHabitsViewModel.DeleteHabitCommand ) ) );
        DGV_Habits.StartSwipeItems.Add( swipeForDeletion );

        SwipeItem editOnSwipe = new()
        {
            BackgroundColor = Application.Current!.Resources["Primary"] as Color,
            Caption = LocStrings.Edit
        };
        editOnSwipe.SetBinding( SwipeItem.CommandProperty, new Binding( nameof( ProgressOfHabitsViewModel.EditHabitCommand ) ) );
        DGV_Habits.StartSwipeItems.Add( editOnSwipe );

        SwipeItem archiveOnSwipe = new()
        {
            BackgroundColor = Application.Current!.Resources["NormalHeaderText"] as Color,
            Caption = LocStrings.AddToArchive
        }; 
        archiveOnSwipe.SetBinding( SwipeItem.CommandProperty, new Binding( nameof( ProgressOfHabitsViewModel.ArchiveHabitCommand ) ) );
        DGV_Habits.StartSwipeItems.Add( archiveOnSwipe );
    }
#endif

#if ANDROID31_0_OR_GREATER || IOS16_0_OR_GREATER
    private void SwipeItem_Invoked(object sender, EventArgs e )
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

    private void DGV_Habits_SortByGoalName( object sender, CustomSortEventArgs e )
    {
        if (e.Column.FieldName == "Goal.Name")
        {
            string goalNameToCompare = e.Value1 as string;

            e.Result = goalNameToCompare == LocStrings.NoGoalSpecified ? 1 : 0;
        }
    }

    private async void DXI_Reminder_Tapped( object sender, TappedEventArgs e )
    {
        if (LocalNotificationCenter.Current.IsSupported)
        {
            Action openPopup = () => DXP_Reminder.IsOpen = true;
            if (ViewModel.LoadHabitReportReminderCommand.CanExecute( openPopup ))
            {
                await ViewModel.LoadHabitReportReminderCommand.ExecuteAsync( openPopup ).DefaultConfigureAwait();
            }
        }
        else
        {
            DXP_Reminder.IsOpen = false;
            
            Snackbar.Make(
                LocStrings.DeviceDoesNotSupportNotifications,
                visualOptions: SnackbarHelper.DefaultOptions()
            ).Show().GetAwaiter();
        }
    }

    private async void SB_Save_Clicked( object sender, EventArgs e )
    {
        Action closePopup = () => DXP_Reminder.IsOpen = false;
        
        if (ViewModel.SaveHabitsReportReminderCommand.CanExecute( closePopup ))
        {
            await ViewModel.SaveHabitsReportReminderCommand.ExecuteAsync( closePopup ).DefaultConfigureAwait();
        }
    }

    private void SB_ReminderReport_Cancel_Clicked( object sender, EventArgs e )
    {
        DXP_Reminder.IsOpen = false;
    }

    private void DXI_Archive_Tapped( object sender, TappedEventArgs e )
    {
        ArchiveBottomSheet.State = BottomSheetState.HalfExpanded;


        double heightOfReminderBottomSheet = 500;

        if (ViewModel.SettingsService.NormalPageHeight == 0)
        {
            ArchiveBottomSheet.HalfExpandedRatio = 0.7;
        }
        else
        {
            //bottom_sheet_height = full_height * HalfExpandedRatio
            //HalfExpandedRatio = bottom_sheet_height / full_height
            ArchiveBottomSheet.HalfExpandedRatio = heightOfReminderBottomSheet / ViewModel.SettingsService.NormalPageHeight;
        }

        double rowSpacing = G_ArchivedHabits.RowSpacing * (G_ArchivedHabits.RowDefinitions.Count - 1);
        double additionalSpacing = 15;
        double height = heightOfReminderBottomSheet - rowSpacing - L_ArhiveCenralHeader.HeightRequest - L_ArhiveCenralHeader.HeightRequest - additionalSpacing;

        SKL_Archive.HeightRequest = height;
        SKL_Archive.WidthRequest = PageWidth - (G_ArchivedHabits.Padding.Left + G_ArchivedHabits.Padding.Right);
        DXCV_Archive.HeightRequest = height;
    }

    private void ME_ArchivedHabitEndIconClicked( object sender, EventArgs e )
    {
        ArchiveBottomSheet.State = BottomSheetState.Hidden;
    }

    private void DXI_Info_Tapped( object sender, TappedEventArgs e )
    {
        DXP_Tip.IsOpen = true;
    }

    private void B_Ok_Clicked( object sender, EventArgs e )
    {
        DXP_Tip.IsOpen = false;
    }

    private void DXI_Plus_Tapped( object sender, TappedEventArgs e )
    {
        ArchiveBottomSheet.State = BottomSheetState.Hidden;
    }
}