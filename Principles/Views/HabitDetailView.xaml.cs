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

        InitializeComponent();
    }

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
    private void DX_Calendar_CustomDayCellAppearance( object sender, CustomSelectableCellAppearanceEventArgs e )
    {
        if (BindingContext is HabitDetailViewModel vm &&
        vm.CompletedDates.Contains( DateOnly.FromDateTime( e.Date ) ))
        {
            e.BackgroundColor = (Application.Current!.Resources["Primary"] as Color)!;
            e.TextColor = Colors.White;
        } 
    }
}