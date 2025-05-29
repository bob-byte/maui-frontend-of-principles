using LiveChartsCore.SkiaSharpView.Maui;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using DevExpress.Maui.Editors;
using DevExpress.Maui.DataGrid;
using LiveChartsCore.SkiaSharpView.SKCharts;
//using static Android.Icu.Text.CaseMap;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;


namespace Principles.Views;

public partial class HabitDetailView : ContentPageBase
{
    private readonly ILockDeviceOrientation m_deviceOrientationService;
    public HabitDetailView( HabitDetailViewModel viewModel )
	{
        BindingContext = viewModel;
        ViewModel = viewModel;
        ViewModel.PropertyChanged += ViewModelOnPropertyChanged;

        m_deviceOrientationService = DependencyService.Get<ILockDeviceOrientation>();

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
#if ANDROID
        m_deviceOrientationService.LockOrientation( DeviceOrientation.Portrait );
        Microsoft.Maui.Controls.Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
            .UseWindowSoftInputModeAdjust( WindowSoftInputModeAdjust.Resize );
#endif
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
#if ANDROID
        m_deviceOrientationService.UnlockOrientation();
        Microsoft.Maui.Controls.Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
           .UseWindowSoftInputModeAdjust( WindowSoftInputModeAdjust.Pan );
#endif
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
        
    }

    private void B_StreakOk_Clicked( object sender, EventArgs e )
    {
        
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

    private void G_Title_SizeChanged( object sender, EventArgs e )
    {
        double titleWidth = G_Title.Width;
        double buttonWidth = DXI_DeleteHabit.WidthRequest + DXI_ArchiveHabit.WidthRequest; // width = 40
        if (titleWidth != -1 && buttonWidth != -1)
        {
            double titleLabelWidth = titleWidth - buttonWidth - 10;
#if ANDROID
            L_TitleText.WidthRequest = titleLabelWidth;
#else
            L_TitleText.MaximumWidthRequest = titleLabelWidth;
#endif
        }
    }
}