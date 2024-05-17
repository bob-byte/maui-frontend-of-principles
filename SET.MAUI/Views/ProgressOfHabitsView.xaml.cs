using CommunityToolkit.Maui.Behaviors;

using DevExpress.Maui.CollectionView;
using DevExpress.Maui.DataGrid;
using DevExpress.Maui.Editors;
using SET.MAUI.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Application = Microsoft.Maui.Controls.Application;
using SwipeView = Microsoft.Maui.Controls.SwipeView;
using DevExpress.Maui.Controls;
using System.Windows.Input;
using System.Threading;

namespace SET.MAUI.Views;

public partial class ProgressOfHabitsView : ContentPageBase
{
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

    private void ViewModel_PropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if (e.PropertyName == nameof( ViewModel.IsInitialized ) && ViewModel.IsInitialized && m_newDayEventTimer is null)
        {
            TimeSpan timeUntilMidnight = TimeUntilMidnight();
            m_newDayEventTimer = new Timer( OnNewDay, state: null, dueTime: timeUntilMidnight, period: TimeSpan.FromHours( 24 ) );

#if IOS
            m_timeZoneChangeObserver = new TimeZoneChangeObserver();
            m_timeZoneChangeObserver.StartObservingTimeZoneChanges( ( notification ) =>
            {
                TimeSpan timeUntilMidnight = TimeUntilMidnight();
                m_newDayEventTimer!.Change( timeUntilMidnight, TimeSpan.FromHours( 24 ) );
            } );
#elif ANDROID
            m_timeZoneChangeReceiver = new TimeZoneChangedReceiver( ( context, intent ) =>
            {
                TimeSpan timeUntilMidnight = TimeUntilMidnight();
                m_newDayEventTimer!.Change( timeUntilMidnight, TimeSpan.FromHours( 24 ) );
            } );
#endif
        }
    }

    private void OnNewDay( object? state )
    {
        DateTime todayAsDatetime = DateTime.Today;
        DateOnly today = DateOnly.FromDateTime( todayAsDatetime );

        string dayOfWeek = LocStrings.ResourceManager.GetString( name: $"{today.DayOfWeek}Short" )!.ToUpperInvariant();
        int dayOfMonth = today.Day;
        string colCaption = $"{dayOfWeek}{Environment.NewLine}{dayOfMonth}";

        bool isAlreadyAddedCol = DGV_Habits.Columns[1].Caption == colCaption || DGV_Habits.Columns[2].Caption == colCaption;
        if (!isAlreadyAddedCol)
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
                HeaderFontSize = 7,
                HeaderCaptionLineBreakMode = LineBreakMode.WordWrap,
                VerticalContentAlignment = TextAlignment.Center,
                HorizontalContentAlignment = TextAlignment.Center,
                HorizontalHeaderAlignment = TextAlignment.Center,
                DisplayTemplate = new HabitWithProgressTemplateSelector( ViewModel, today )
            };

            DGV_Habits.Columns.Insert( index: 1, templateColumn );
        }

        TimeSpan timeUntilMidnight = TimeUntilMidnight();
        m_newDayEventTimer!.Change( timeUntilMidnight, Timeout.InfiniteTimeSpan );
    }

    private TimeSpan TimeUntilMidnight()
    {
        DateTime now = TimeZoneInfo.ConvertTimeFromUtc( DateTime.UtcNow, TimeZoneInfo.Local );
        DateTime midnight = now.Date.AddDays( 1 );
        TimeSpan result = midnight - now;
        return result;
    }

    private ProgressOfHabitsViewModel ViewModel { get; }

    void SwipeItem_Tap( System.Object sender, DevExpress.Maui.DataGrid.SwipeItemTapEventArgs e )
    {
        ICommand command = ViewModel.DeleteHabitCommand;
        if (command.CanExecute( e.Item))
        {
            command.Execute( e.Item );
        }
    }
    private void DGV_Habits_LongPress( object sender, DataGridGestureEventArgs e )
    {
        if (e.Element == DataGridElement.Row)
        {
            if (DGV_Habits.SelectedRowHandle == e.RowHandle)
            {
                DGV_Habits.SelectedRowHandle = -1;
                ViewModel.SelectedHabit = null;
            }
            else
            {
                DGV_Habits.SelectedRowHandle = e.RowHandle;
                ViewModel.SelectedHabit = ViewModel.UserHabits[e.RowHandle];
            }
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
            //string dayOfWeek = LocStringsExtensions.Value( propName: $"{day.DayOfWeek}Short" );
            string dayOfWeek = LocStrings.ResourceManager.GetString( name: $"{day.DayOfWeek}Short" )!.ToUpperInvariant();
            int dayOfMonth = day.Day;
            string colCaption = $"{dayOfWeek}{Environment.NewLine}{dayOfMonth}";

            TemplateColumn templateColumn = new()
            {
                Caption = colCaption,
                HeaderFontSize = 7,
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
        DGV_Habits.Columns.Clear();
        TemplateColumn habitNameCol = new()
        {
            FixedStyle = FixedStyle.Start,
            Caption = " ",
            MinWidth = 200,

            DisplayTemplate = new DataTemplate( loadTemplate: () =>
            {
                Color primaryColor = (Color)Application.Current!.Resources["Primary"];
                Color normalTextColor = (Color)Application.Current.Resources["LightNormalText"];

                //SwipeView swipeView = new();
                //swipeView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetSwipeTransitionMode( SwipeTransitionMode.Drag );

                //SwipeItems swipeItems = new();
                //SwipeItemView deleteSwipeItem = new();
                //DXImage deleteImg = new()
                //{
                //    Source = "delete",
                //    TintColor = Colors.White,
                //    BackgroundColor = primaryColor,
                //    WidthRequest = 20,
                //    Margin = new Thickness(3)
                //};
                //deleteSwipeItem.Invoked += SwipeItem_Invoked;
                //deleteSwipeItem.Content = deleteImg;
                //swipeItems.Mode = SwipeMode.Reveal;
                ////deleteSwipeItem.BindCommand( "DeleteHabitCommand", ViewModel, "." );
                //swipeItems.Add( deleteSwipeItem );
                //swipeView.LeftItems = swipeItems;

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
                progressBar.ProgressLeftColor = (Color)Application.Current.Resources["SecondaryLight"];
                progressBar.Size = 30;
                progressBar.Thickness = 2.5f;
                progressBar.Margin = new Thickness( 3 );

                Label habitName = new()
                {
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    MaxLines = 2,
                    HeightRequest = 40
                };
                habitName.Bind( Label.TextProperty, "Item.Name", BindingMode.TwoWay );

                habitName.TextColor = normalTextColor;
                habitName.WidthRequest = 145;
                stack.Add( progressBar );
                stack.Add( habitName );
                //swipeView.Content = stack;

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
}