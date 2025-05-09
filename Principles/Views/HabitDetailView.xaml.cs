using LiveChartsCore.SkiaSharpView.Maui;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using DevExpress.Maui.Editors;
using DevExpress.Maui.DataGrid;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace Principles.Views;

public partial class HabitDetailView : ContentPageBase
{
	public HabitDetailView( HabitDetailViewModel viewModel )
	{
        BindingContext = viewModel;
        ViewModel = viewModel;
        ViewModel.PropertyChanged += ViewModelOnPropertyChanged;

        InitializeComponent();
        
        viewModel.ReferenceMessenger.Register<MsgThatProgressOfHabitUpdated>( this, ( _, _ ) =>
        {
            DX_Calendar.Refresh();
        } );
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DX_Calendar.BindingContext = ViewModel.Habit.Progresses;
    }

    private void ViewModelOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if (e.PropertyName == nameof(ViewModel.IsInitialized) && ViewModel.IsInitialized)
        {
            DX_Calendar.Refresh();
        }
    }

    private HabitDetailViewModel ViewModel { get; }

    private void DXI_StreakInfo_Tapped( object sender, TappedEventArgs e )
    {
        DXP_StreakTip.IsOpen = true;
    }

    private void B_StreakOk_Clicked( object sender, EventArgs e )
    {
        DXP_StreakTip.IsOpen = false;
    }

    private void DXI_StabilityInfo_Tapped( object sender, TappedEventArgs e )
    {
        DXP_StabilityTip.IsOpen = true;
    }

    private void B_OkStability_Clicked( object sender, EventArgs e )
    {
        DXP_StabilityTip.IsOpen = false;
    }
    private void B_OkDayByDayWeeks_Clicked( object sender, EventArgs e )
    {
        DXP_DayByDayWeeksTip.IsOpen = false;
    }

    private void DXI_HabitByDayweeks_Tapped( object sender, TappedEventArgs e )
    {
        DXP_DayByDayWeeksTip.IsOpen = true;
    }

    private void DXI_CalendarInfo_Tapped( object sender, TappedEventArgs e )
    {
        DXP_CalendarInfoTip.IsOpen = true;
    }

    private void B_OkCalendarInfo_Clicked( object sender, EventArgs e )
    {
        DXP_CalendarInfoTip.IsOpen = false;
    }
}