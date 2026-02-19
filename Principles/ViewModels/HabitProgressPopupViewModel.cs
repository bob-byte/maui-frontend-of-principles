using System;
using System.Windows.Input;

using CommunityToolkit.Maui.Markup;

using Microsoft.Maui.Controls;

namespace Principles.ViewModels;
public partial class HabitProgressPopupViewModel : BaseViewModel
{
    public UserHabit Habit { get; private set; }
    public DateTime Day { get; }
    public int ValuePercent => Value * 10;
    public double? ProgressPercent => Habit.TargetPerOneTime == 0 ? 0 : (double)Value / Habit.MaxRate;
    public Action<object?>? RequestClose { get; set; }
    public double MaxValue => (double)(Habit.ProgressMarkVariaty == ProgressMarkVariaty.Numeric ? Habit.MaxRate : 10);
    public bool IsSliderEnabled => IsYes;
    private bool _isYes = true;
    public bool IsYes
    {
        get => _isYes;
        set
        {
            if (SetProperty( ref _isYes, value ))
            {
                OnPropertyChanged( nameof( IsSliderEnabled ) );
            }
        }
    }
    private int _value;
    public int Value
    {
        get => IsYes ? _value : 0;
        set
        {
            _value = value;
            OnPropertyChanged();
            OnPropertyChanged( nameof( ProgressPercent ) );
            OnPropertyChanged( nameof( ValuePercent ) );
        }
    }
    public HabitProgressPopupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        
    }

    public override async Task InitializePopupAsync( IDictionary<string, object> query )
    {
        await base.InitializePopupAsync( query );
        
        if ( query.TryGetValue( "Habit", out object? habitObj ) && habitObj is UserHabit habit && query.TryGetValue( "Habit", out object? progerssObj ) && progerssObj is ProgressOfHabit progress )
        {
            Habit = habit;
            Value = progress.Value;
        }
    }

    [RelayCommand]
    private async Task SaveAsync( )
    {
        RequestClose?.Invoke( "save" );
    }
    [RelayCommand]
    private async Task CancelAsync()
    {
        RequestClose?.Invoke( "cancel" );
    }
    [RelayCommand]
    private async Task SkipDayAsync()
    {
        RequestClose?.Invoke( "skip" );
    }
    [RelayCommand]
    private async Task IncreaseNumericValueAsync()
    {
        if (Value < Habit.MaxRate) Value++;
    }
    [RelayCommand]
    private async Task DecreaseNumericValueAsync()
    {
        if (Value > 0) Value--;
    }
}

