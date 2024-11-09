using CommunityToolkit.Maui.Behaviors;
using DevExpress.Maui.DataGrid;
using Plugin.LocalNotification;

using SET.MAUI.Controls;
using System.Windows.Input;

using Application = Microsoft.Maui.Controls.Application;

namespace SET.MAUI.Views;

public partial class ProgressOfHabitsView : ContentPageBase
{
    private Timer? m_newDayEventTimer;
    private bool m_isReminderOpenForTheFirstTime;

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
        m_isReminderOpenForTheFirstTime = Preferences.Get( "IsReminderOpenForTheFirstTime", true );

        ViewModel.DataGridViewWithHabits = DGV_Habits;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        AddFirstCol();
        AddColumns();
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
            m_newDayEventTimer = new Timer( OnNewDay, state: null, dueTime: timeUntilMidnight, period );

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

    private async void OnNewDay( object? state )
    {
        DateTime todayAsDatetime = DateTime.Today;
        DateOnly today = DateOnly.FromDateTime( todayAsDatetime );

        string dayOfWeek = LocStrings.ResourceManager.GetString( name: $"{today.DayOfWeek}Short" )!.ToUpperInvariant();
        int dayOfMonth = today.Day;
        string colCaption = $"{dayOfWeek}{Environment.NewLine}{dayOfMonth}";

        bool isAlreadyAddedCol = DGV_Habits.Columns[1].Caption == colCaption || DGV_Habits.Columns[2].Caption == colCaption;
        if (!isAlreadyAddedCol)
        {
            ViewModel.IsProgressesInitialized = false;

            await MainThread.InvokeOnMainThreadAsync( () =>
            {
                foreach (UserHabit habit in ViewModel.UserHabits)
                {
                    habit.Progresses!.Insert( index: 0, new ProgressOfHabit
                    {
                        Id = 0,
                        Date = today,
                        Habit = habit,
                        Value = ProgressValue.UNKNOWN
                    } );

                    ViewModel.ServiceOfHabit.Recompute( habit );
                }

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
            } ).DefaultConfigureAwait();

            ViewModel.EndProgressInterval = today;
            ViewModel.IsProgressesInitialized = true;
        }
    }

    private TimeSpan TimeUntilMidnight()
    {
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc( DateTime.UtcNow, TimeZoneInfo.Local );
        DateTime midnight = now.Date.AddDays( 1 );
        TimeSpan result = midnight - now;

        return result;
    }

    void SwipeItem_Tap( System.Object sender, DevExpress.Maui.DataGrid.SwipeItemTapEventArgs e )
    {
        ICommand command = ViewModel.DeleteHabitCommand;
        if (command.CanExecute( e.Item ))
        {
            command.Execute( e.Item );
        }
    }

    private void AddColumns()
    {
        DateTime endTime = ViewModel.StartProgressInterval.ToDateTime( TimeOnly.FromTimeSpan( TimeSpan.Zero ) );
        var oneDay = TimeSpan.FromDays( 1 );
        int columnIndex = 0;

        for (DateTime day = ViewModel.EndProgressInterval.ToDateTime( TimeOnly.FromTimeSpan( TimeSpan.Zero ) );
             endTime <= day;
             day = day.Subtract( oneDay ), columnIndex++)
        {
            string dayOfWeek = LocStrings.ResourceManager.GetString( name: $"{day.DayOfWeek}Short" )!.ToUpperInvariant();
            int dayOfMonth = day.Day;
            string colCaption = $"{dayOfWeek}{Environment.NewLine}{dayOfMonth}";

            TemplateColumn templateColumn = new()
            {
                Caption = colCaption,
                HeaderFontSize = 9,
                Width = new GridLength(55),
                HeaderCaptionLineBreakMode = LineBreakMode.WordWrap,
                VerticalContentAlignment = TextAlignment.Center,
                HorizontalContentAlignment = TextAlignment.Center,
                HorizontalHeaderAlignment = TextAlignment.Center,
                DisplayTemplate = new HabitWithProgressTemplateSelector( ViewModel, DateOnly.FromDateTime( day ) )
            };

            DGV_Habits.Columns.Add( templateColumn );
        }
    }

    private void UnderlineDataGridRow(object sender, TappedEventArgs eventArgs)
    {

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

#if ANDROID
                TouchBehavior touchBehavior = new()
                {
                    LongPressCommand = ViewModel.SelectRowCommand,
                    Command = ViewModel.EditHabitCommand
                };
                touchBehavior.Bind( TouchBehavior.LongPressCommandParameterProperty, "Item" );
                touchBehavior.Bind( TouchBehavior.CommandParameterProperty, "Item" );

                stack.Behaviors.Add( touchBehavior );
#else
                stack.BindTapGesture( "EditHabitCommand", commandSource: ViewModel, parameterPath: "Item", numberOfTapsRequired: 1 );
#endif

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

    private void DGV_Habits_SortByGoalName( object sender, CustomSortEventArgs e )
    {
        if (e.Column.FieldName == "Goal.Name")
        {
            string goalNameToCompare = e.Value1 as string;

            e.Result = goalNameToCompare == LocStrings.NoGoalSpecified ? 1 : 0;
        }
    }

    private void DXI_Reminder_Tapped( object sender, TappedEventArgs e )
    {
        if (LocalNotificationCenter.Current.IsSupported)
        {
            DXP_Reminder.IsOpen = true;
        }
        else
        {
            Snackbar.Make(
                LocStrings.DeviceDoesNotSupportNotifications,
                visualOptions: SnackbarHelper.DefaultOptions()
            ).Show();
            DXP_Reminder.IsOpen = false;
        }
    }

    private void SB_Save_Clicked( object sender, EventArgs e )
    {
        DXP_Reminder.IsOpen = false;
    }

    private void SB_GeneralReminder_Cancel_Clicked( object sender, EventArgs e )
    {
        DXP_Reminder.IsOpen = false;
    }
}