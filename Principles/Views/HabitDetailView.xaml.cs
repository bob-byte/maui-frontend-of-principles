using LiveChartsCore.SkiaSharpView.Maui;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using DevExpress.Maui.Editors;
using DevExpress.Maui.DataGrid;

using LiveChartsCore.Measure;
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

        CC_Streaks.AnimationsSpeed = TimeSpan.FromSeconds( 2 );
        
        viewModel.ReferenceMessenger.Register<MsgThatProgressOfHabitUpdated>( this, ( _, _ ) =>
        {
            DX_Calendar.Refresh();
        } );
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DX_Calendar.BindingContext = ViewModel.Habit.Progresses;
        
        m_deviceOrientationService.LockOrientation( DeviceOrientation.Portrait );
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        m_deviceOrientationService.UnlockOrientation();
    }

    private void ViewModelOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if (e.PropertyName == nameof(ViewModel.IsInitialized) && ViewModel.IsInitialized)
        {
            DX_Calendar.Refresh();
        }
    }

    private HabitDetailViewModel ViewModel { get; }

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