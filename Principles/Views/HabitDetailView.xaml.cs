using DevExpress.Maui.Controls;
using DevExpress.Maui.DataGrid;
using DevExpress.Maui.Editors;

using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Maui;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
//using static Android.Icu.Text.CaseMap;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

using SkiaSharp;


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
        
        var animationSpeed = TimeSpan.FromSeconds( 1.5 );
        
        CC_Streaks.AnimationsSpeed = animationSpeed;
        CC_Stability.AnimationsSpeed = animationSpeed;
        CC_HabitByWeekDays.AnimationsSpeed = animationSpeed;
        
        viewModel.ReferenceMessenger.Register<MsgThatProgressOfHabitUpdated>( this, async ( _, _ ) =>
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync( DX_Calendar.Refresh );
            }
            catch(Exception ex)
            {
                viewModel.LoggingService.LogCriticalError( ex );
            }
        } );
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
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