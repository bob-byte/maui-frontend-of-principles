
using DevExpress.Maui.Controls;
using DevExpress.Maui.Editors;

using Microsoft.VisualBasic;

using SET.Core.Models;

namespace SET.MAUI.Views;

public partial class EditHabitView : ContentPageBase
{
    private bool m_doExecuteReloadOfRecommendedHabits;

    public EditHabitView( EditHabitViewModel viewModel )
    {
        m_doExecuteReloadOfRecommendedHabits = true;

        BindingContext = viewModel;
        ViewModel = viewModel;
        ViewModel.PropertyChanged += SelectHabitTypeChip;
        ViewModel.PropertyChanged += ViewModel_OnIsInitializedChanged;

        InitializeComponent();
    }

    private void ViewModel_OnIsInitializedChanged( object sender, PropertyChangedEventArgs e )
    {
        if (e.PropertyName == nameof( BaseViewModel.IsInitialized ))
        {
            if (ViewModel.IsInitialized)
            {
                NE_Priority.MaxValue = ViewModel.UserHabits.Count;
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ViewModel.IsNewHabit)
        {
            await Task.Delay( 1500 );
            await ME_NameOfHabit.DisplaySnackbar( LocStrings.DescriptionOfAiLampTap, duration: TimeSpan.FromSeconds( 6 ), visualOptions: SnackbarHelper.DefaultOptions() );
        }
    }

    private void SelectHabitTypeChip( object? sender, PropertyChangedEventArgs eventArgs )
    {
        UserHabit habit = ViewModel.Habit;
        if (eventArgs.PropertyName == nameof( EditHabitViewModel.Habit ) && habit != null)
        {
            switch (habit.Type)
            {
                default:
                    {
                        CCG_Types.SelectChip( C_WithoutExceptionsType );
                        break;
                    }
                case TypeOfHabit.IntegrallyWise:
                    {
                        CCG_Types.SelectChip( C_IntegrallyWiseType );
                        break;
                    }
            }
        }
    }

    private EditHabitViewModel ViewModel { get; }

    void Frequency_Tapped( object sender, TappedEventArgs e )
    {
        OpenFrequencyPopup();
    }

    void FrequencyIcon_Clicked( object sender, EventArgs e )
    {
        OpenFrequencyPopup();
    }

    private void OpenFrequencyPopup()
    {
        FrequencyOfHabit frequency = ViewModel.Habit.Frequency;
        switch (frequency.Type)
        {
            case FrequencyType.EverySeveralDays:
                {
                    E_RepeatsOfSeveralTimesPerPeriod.Text = "3";
                    E_RepeatsOfSeveralDays.Text = frequency.IntervalLengthInDays.ToString();
                    RB_EverySeveralDays.IsChecked = true;
                    break;
                }

            case FrequencyType.SeveralTimesPerPeriod:
                {
                    E_RepeatsOfSeveralDays.Text = "3";
                    E_RepeatsOfSeveralTimesPerPeriod.Text = frequency.Repeats.ToString();
                    RB_SeveralTimesPerPeriod.IsChecked = true;
                    break;
                }

            default:
                {
                    E_RepeatsOfSeveralDays.Text = "3";
                    E_RepeatsOfSeveralTimesPerPeriod.Text = "3";
                    RB_EveryDay.IsChecked = true;

                    break;
                }
        }

        DXP_Frequency.BindingContext = BindingContext;
        DXP_Frequency.IsOpen = true;
    }

    async void SB_SaveFrequencyPopup_Clicked( object sender, EventArgs e )
    {
        FrequencyOfHabit frequency = ViewModel.Habit.Frequency;
        bool hasErrors = false;
        if (RB_EveryDay.IsChecked)
        {
            frequency.IntervalLengthInDays = 1;
            frequency.Repeats = 1;
            frequency.Type = FrequencyType.EveryDay;
        }
        else if (RB_EverySeveralDays.IsChecked)
        {
            try
            {
                int previousInterval = frequency.IntervalLengthInDays;
                frequency.IntervalLengthInDays = Convert.ToUInt16( E_RepeatsOfSeveralDays.Text );

                int maxIntervalLengthInDays = HabitConstants.AVERAGE_NUMBER_OF_DAYS_TO_AUTOMATE_HABIT;
                if (frequency.IntervalLengthInDays <= maxIntervalLengthInDays)
                {
                    frequency.Type = frequency.IntervalLengthInDays > 1
                        ? FrequencyType.EverySeveralDays
                        : FrequencyType.EveryDay;

                    frequency.Repeats = 1;
                }
                else
                {
                    frequency.IntervalLengthInDays = previousInterval;

                    string errorMsg = $"{maxIntervalLengthInDays} {LocStrings.IsMaxValue.ToLower()}";
                    ViewModel.LoggingService.LogError( errorMsg );
                    await ViewModel.DialogService.ShowErrorAsync( errorMsg );
                    hasErrors = true;
                }
            }
            catch
            {
                string errorMsg = $"{LocStrings.CannotParse} \"{E_RepeatsOfSeveralDays.Text}\" {LocStrings.ToInteger.ToLower()}";
                ViewModel.LoggingService.LogError( errorMsg );
                await ViewModel.DialogService.ShowErrorAsync( errorMsg );
                hasErrors = true;
            }
        }
        else if (RB_SeveralTimesPerPeriod.IsChecked)
        {
            try
            {
                frequency.Type = FrequencyType.SeveralTimesPerPeriod;
                frequency.Repeats = Convert.ToUInt16( E_RepeatsOfSeveralTimesPerPeriod.Text );
                switch (ViewModel.SelectedPeriodOfHabit.Type)
                {
                    case PeriodTypeOfHabit.Week:
                        {
                            frequency.IntervalLengthInDays = 7;
                            break;
                        }

                    case PeriodTypeOfHabit.Month:
                        {
                            frequency.IntervalLengthInDays = 30;
                            break;
                        }

                    case PeriodTypeOfHabit.Year:
                        {
                            frequency.IntervalLengthInDays = 365;
                            break;
                        }
                }
            }
            catch
            {
                await ViewModel.DialogService.ShowErrorAsync( $"{LocStrings.CannotParse} \"{E_RepeatsOfSeveralDays.Text}\" {LocStrings.ToInteger.ToLower()}" );
                hasErrors = true;
            }
        }

        if (!hasErrors)
        {
            ViewModel.UpdateFrequencyRepresentation();
            DXP_Frequency.IsOpen = false;
        }
    }

    void SeveralTimesPerPeriodFrequency_Tapped( System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e )
    {
        RB_SeveralTimesPerPeriod.IsChecked = true;
    }

    //TODO: replace to ViewModel
    void C_WithoutExceptionsType_Tap( System.Object sender, System.ComponentModel.HandledEventArgs e )
    {
        ViewModel.Habit.Type = TypeOfHabit.WithoutExceptions;
        C_WithoutExceptionsType.DisplaySnackbar(
            LocStrings.WithoutExceptionsHabitTypeShortDescription,
            duration: Timeout.InfiniteTimeSpan,
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    void C_IntegrallyWiseType_Tap( System.Object sender, System.ComponentModel.HandledEventArgs e )
    {
        ViewModel.Habit.Type = TypeOfHabit.IntegrallyWise;
        C_IntegrallyWiseType.DisplaySnackbar(
            LocStrings.IntegrallyWiseHabitTypeShortDescription,
            duration: Timeout.InfiniteTimeSpan,
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    void TE_AreasOfLife_EndIconClicked( System.Object sender, System.EventArgs e )
    {
        TE_AreasOfLife.IsDropDownOpen = !TE_AreasOfLife.IsDropDownOpen;
    }

    void TE_AreasOfLife_SelectionChanged( System.Object sender, System.EventArgs e )
    {
        if (TE_AreasOfLife.SelectedItems?.Count == 0)
        {
            TE_AreasOfLife.IsLabelFloating = true;
        }
        else
        {
            TE_AreasOfLife.IsLabelFloating = false;
        }

        m_doExecuteReloadOfRecommendedHabits = true;
    }

    void RepeatsOfSeveralDays_Focused( System.Object sender, Microsoft.Maui.Controls.FocusEventArgs e )
    {
        if (e.IsFocused)
        {
            RB_EverySeveralDays.IsChecked = true;
        }
    }

    void EverySeveralDaysFrequency_Tapped( System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e )
    {
        RB_EverySeveralDays.IsChecked = true;
    }

    async void SB_Save_Clicked( System.Object sender, System.EventArgs e )
    {
        if (ViewModel.SaveCommand.CanExecute( null ))
        {
            await ViewModel.SaveCommand.ExecuteAsync( null );
        }
        else
        {
            //L_ErrorMessages.Text = string.Empty;
            DXS_ErrorMessages.Children.Clear();

            foreach (string errMsg in ViewModel.NameOfHabit.Errors)
            {
                Label errorLabel = new()
                {
                    FontSize = 16,
                    LineBreakMode = LineBreakMode.WordWrap,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
                errorLabel.Text += $"{errMsg}";
                DXS_ErrorMessages.Children.Add( errorLabel );
            }

            foreach (string errMsg in ViewModel.ReasonToFollow.Errors)
            {
                Label errorLabel = new()
                {
                    FontSize = 16,
                    LineBreakMode = LineBreakMode.WordWrap,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
                errorLabel.Text += $"{errMsg}";
                DXS_ErrorMessages.Children.Add( errorLabel );
            }

            DXP_Errors.MaximumWidthRequest = PageWidth - 40;
            DXP_Errors.MaximumHeightRequest = PageHeight - 60;
            HSL_ErrorMessages.MaximumWidthRequest = DXP_Errors.MaximumWidthRequest - (HSL_ErrorMessages.Margin.Left + HSL_ErrorMessages.Margin.Right);
            DXS_ErrorMessages.MaximumWidthRequest = HSL_ErrorMessages.MaximumWidthRequest - (DXI_Error.WidthRequest + HSL_ErrorMessages.ItemSpacing);

            DXP_Errors.IsOpen = true;
        }
    }

    void SB_CloseErrorsPopup_Clicked( System.Object sender, System.EventArgs e )
    {
        DXP_Errors.IsOpen = false;
    }

    async void ME_NameOfHabit_AiIconClicked( System.Object sender, System.EventArgs e )
    {
        if (BS_RecommendedHabits.State == BottomSheetState.Hidden)
        {
            bool doShowRecommendedHabits = true;

            if (m_doExecuteReloadOfRecommendedHabits)
            {
                bool canReload = ViewModel.ReloadRecommendedHabitsCommand.CanExecute( ShowSnackbarOfSuccessfulRecomendedHabitsReload );

                if (canReload)
                {
                    bool isConfirmed = await ViewModel.DialogService.ShowConfirmAsync( LocStrings.ConfirmMessageOnRecommededHabitsView, LocStrings.ConfirmTitleOnRecommendedHabitsView );
                    doShowRecommendedHabits = isConfirmed;

                    if (isConfirmed)
                    {
                        ViewModel.ReloadRecommendedHabitsCommand.Execute( ShowSnackbarOfSuccessfulRecomendedHabitsReload );
                        m_doExecuteReloadOfRecommendedHabits = false;
                    }
                }
                else
                {
                    doShowRecommendedHabits = false;

                    await ViewModel.DialogService.ShowErrorAsync( LocStrings.CannotReloadHabits );
                }
            }

            if (doShowRecommendedHabits)
            {
                BS_RecommendedHabits.State = BottomSheetState.HalfExpanded;
            }
        }
    }

    private async void ShowSnackbarOfSuccessfulRecomendedHabitsReload()
    {
        if (BS_RecommendedHabits.State == BottomSheetState.Hidden)
        {
            ISnackbar snackbar = Snackbar.Make(
                LocStrings.RecommendedHabitsSuccessfullyLoaded,
                action: () => BS_RecommendedHabits.State = BottomSheetState.HalfExpanded,
                actionButtonText: LocStrings.Inspect,
                duration: TimeSpan.FromSeconds( 5 ),
                visualOptions: SnackbarHelper.DefaultOptions()
            );
            await snackbar.Show();
        }
    }

    void TGR_RecommendedHabit_Tapped( System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e )
    {
        if (ViewModel.SelectedRecommendedHabitCommand.CanExecute( e.Parameter ))
        {
            ViewModel.SelectedRecommendedHabitCommand.Execute( e.Parameter );
            BS_RecommendedHabits.State = BottomSheetState.Hidden;
        }
    }

    void E_RepeatsOfSeveralTimesPerPeriod_Focused( System.Object sender, Microsoft.Maui.Controls.FocusEventArgs e )
    {
        if (e.IsFocused)
        {
            RB_SeveralTimesPerPeriod.IsChecked = true;
        }
    }

    void G_SaveHabit_SizeChanged( System.Object sender, System.EventArgs e )
    {
        double titleWidth = G_Title.Width;
        double buttonWidth = G_SaveHabit.Width;
        if (titleWidth != -1 && buttonWidth != -1)
        {
            double titleLabelWidth = titleWidth - buttonWidth - 10;
            L_TitleText.WidthRequest = titleLabelWidth;
        }
    }

    void TGR_AreasOfHabit_Tapped( System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e )
    {
        TE_AreasOfLife.IsDropDownOpen = !TE_AreasOfLife.IsDropDownOpen;
    }

    void NE_Priority_EndIconClicked( System.Object sender, System.EventArgs e )
    {
        if (BS_AllHabits.State == BottomSheetState.Hidden)
        {
            bool isAddedCurrentHabit = ViewModel.UserHabits.Any( h => h.Id == ViewModel.Habit.Id );
            if (!isAddedCurrentHabit)
            {
                ViewModel.UserHabits.Add( ViewModel.Habit );
            }

            ViewModel.Habit.Name = ViewModel.NameOfHabit.Value;
            BS_AllHabits.State = BottomSheetState.HalfExpanded;
        }
        else
        {
            BS_AllHabits.State = BottomSheetState.Hidden;
        }
    }

    void DXC_SelectPriority_CompleteItemDragDrop( System.Object sender, DevExpress.Maui.CollectionView.CompleteItemDragDropEventArgs e )
    {
        for (int priority = 1; priority <= ViewModel.UserHabits.Count; priority++)
        {
            ViewModel.UserHabits[priority - 1].Priority = priority;
        }
    }

    void NE_Priority_UpIconClicked( System.Object sender, System.ComponentModel.HandledEventArgs e )
    {
        if (!e.Handled)
        {
            UserHabit habit = ViewModel.Habit;
            if (ViewModel.ReorderUserHabitsCommand.CanExecute( null ))
            {
                if (habit.Priority < ViewModel.UserHabits.Count)
                {
                    ViewModel.Habit.Priority++;
                }
                else
                {
                    habit.Priority = 1;
                }

                ViewModel.ReorderUserHabitsCommand.Execute( null );
            }
        }

    }

    void NE_Priority_DownIconClicked( System.Object sender, System.ComponentModel.HandledEventArgs e )
    {
        UserHabit habit = ViewModel.Habit;

        if (ViewModel.ReorderUserHabitsCommand.CanExecute( null ))
        {
            if (habit.Priority == 1)
            {
                habit.Priority = ViewModel.UserHabits.Count;
            }
            else
            {
                habit.Priority--;
            }

            ViewModel.ReorderUserHabitsCommand.Execute( null );
        }
    }
}