
using DevExpress.Maui.Controls;
using DevExpress.Maui.Editors;


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

    void TE_Frequency_Focused( object sender, FocusEventArgs e )
    {
        if (e.IsFocused)
        {
            OpenFrequencyPopup();
        }
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
                    TE_RepeatsOfSeveralTimesPerPeriod.Text = "3";
                    TE_RepeatsOfSeveralDays.Text = frequency.IntervalLengthInDays.ToString();
                    CE_EverySeveralDays.IsChecked = true;
                    break;
                }

            case FrequencyType.SeveralTimesPerPeriod:
                {
                    TE_RepeatsOfSeveralDays.Text = "3";
                    TE_RepeatsOfSeveralTimesPerPeriod.Text = frequency.Repeats.ToString();
                    CE_SeveralTimesPerPeriod.IsChecked = true;
                    break;
                }

            default:
                {
                    TE_RepeatsOfSeveralDays.Text = "3";
                    TE_RepeatsOfSeveralTimesPerPeriod.Text = "3";
                    CE_EveryDay.IsChecked = true;

                    break;
                }
        }

        DXP_Frequency.BindingContext = BindingContext;
        DXP_Frequency.IsOpen = true;
    }

    async void SB_SaveFrequencyPopup_Clicked( object sender, EventArgs e )
    {
        FrequencyOfHabit frequency = ViewModel.Habit.Frequency!;
        bool hasErrors = false;
        if (CE_EveryDay.IsChecked == true)
        {
            frequency.IntervalLengthInDays = 1;
            frequency.Repeats = 1;
            frequency.Type = FrequencyType.EveryDay;
        }
        else if (CE_EverySeveralDays.IsChecked == true)
        {
            try
            {
                int previousInterval = frequency.IntervalLengthInDays;
                frequency.IntervalLengthInDays = Convert.ToUInt16( TE_RepeatsOfSeveralDays.Text );

                frequency.Type = frequency.IntervalLengthInDays > 1
                        ? FrequencyType.EverySeveralDays
                        : FrequencyType.EveryDay;

                frequency.Repeats = 1;
            }
            catch
            {
                string errorMsg = $"{LocStrings.CannotParse} \"{TE_RepeatsOfSeveralDays.Text}\" {LocStrings.ToInteger.ToLower()}";
                ViewModel.LoggingService.LogError( errorMsg );
                await ViewModel.DialogService.ShowErrorAsync( errorMsg );
                hasErrors = true;
            }
        }
        else if (CE_SeveralTimesPerPeriod.IsChecked == true)
        {
            try
            {
                frequency.Type = FrequencyType.SeveralTimesPerPeriod;
                int repeats = Convert.ToUInt16( TE_RepeatsOfSeveralTimesPerPeriod.Text );

                switch (ViewModel.SelectedPeriodOfHabit.Type)
                {
                    case PeriodTypeOfHabit.Week:
                        {
                            frequency.IntervalLengthInDays = 7;
                            if(repeats > 7)
                            {
                                repeats = 7;
                            }

                            break;
                        }

                    case PeriodTypeOfHabit.Month:
                        {
                            frequency.IntervalLengthInDays = 30;
                            if (repeats > 30)
                            {
                                repeats = 30;
                            }

                            break;
                        }

                    case PeriodTypeOfHabit.Year:
                        {
                            frequency.IntervalLengthInDays = 365;
                            if (repeats > 365)
                            {
                                repeats = 365;
                            }

                            break;
                        }
                }

                frequency.Repeats = repeats;
            }
            catch
            {
                await ViewModel.DialogService.ShowErrorAsync( $"{LocStrings.CannotParse} \"{TE_RepeatsOfSeveralDays.Text}\" {LocStrings.ToInteger.ToLower()}" );
                hasErrors = true;
            }
        }

        if (!hasErrors)
        {
            ViewModel.UpdateFrequencyRepresentation();
            DXP_Frequency.IsOpen = false;
        }
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

    void TE_AreasOfLife_SelectionChanged( object sender, EventArgs e )
    {
        TE_AreasOfLife.IsLabelFloating = TE_AreasOfLife.SelectedItems?.Count == 0;

        m_doExecuteReloadOfRecommendedHabits = true;
    }

    void TE_AreasOfLife_TextChanged( object sender, AutoCompleteEditTextChangedEventArgs e )
    {
        if (e.Reason == AutoCompleteEditTextChangeReason.UserInput && !string.IsNullOrEmpty( TE_AreasOfLife.Text ))
        {
            TE_AreasOfLife.Text = string.Empty;
        }
    }

    void TE_AreasOfLife_Tap( object sender, HandledEventArgs e )
    {
        try
        {
            TE_AreasOfLife.IsDropDownOpen = !TE_AreasOfLife.IsDropDownOpen;
        }
        catch
        {
            //do nothing
        }
    }

    void RepeatsOfSeveralDays_Focused( System.Object sender, Microsoft.Maui.Controls.FocusEventArgs e )
    {
        if (e.IsFocused)
        {
            CE_EverySeveralDays.IsChecked = true;
        }
    }

    void EverySeveralDaysFrequency_Tapped( System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e )
    {
        CE_EverySeveralDays.IsChecked = true;
    }

    async void SB_Save_Clicked( System.Object sender, System.EventArgs e )
    {
        if (ViewModel.SaveCommand.CanExecute( null ))
        {
            await ViewModel.SaveCommand.ExecuteAsync( null );
        }
        else
        {
            DXS_ErrorMessages.Children.Clear();

            foreach (string errMsg in ViewModel.NameOfHabit.Errors)
            {
                Label errorLabel = new()
                {
                    FontSize = 16,
                    FontFamily = "MonaSansMedium",
                    LineBreakMode = LineBreakMode.WordWrap,
                    HorizontalOptions = LayoutOptions.Start,
                    HorizontalTextAlignment = TextAlignment.Start,
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
                    FontFamily = "MonaSansMedium",
                    LineBreakMode = LineBreakMode.WordWrap,
                    HorizontalOptions = LayoutOptions.Start,
                    HorizontalTextAlignment = TextAlignment.Start,
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

    async void SB_RecomendedHabit_Clicked( System.Object sender, System.EventArgs e )
    {
        if (BS_RecommendedHabits.State == BottomSheetState.Hidden)
        {
            bool doShowRecommendedHabits = true;

            if (m_doExecuteReloadOfRecommendedHabits || ViewModel.RecommendedHabits.Count == 0)
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

    private void OnCheckEditChangedInFrequencyPopup( object sender, EventArgs e )
    {
        var selectedCheckEdit = sender as CheckEdit;

        if (selectedCheckEdit != null && selectedCheckEdit.IsChecked == true)
        {
            var checkEdits = new List<CheckEdit> { CE_EveryDay, CE_EverySeveralDays, CE_SeveralTimesPerPeriod };

            foreach (var checkEdit in checkEdits)
            {
                if (checkEdit != selectedCheckEdit)
                {
                    checkEdit.IsChecked = false;
                    checkEdit.IsEnabled = true;
                }
                else
                {
                    checkEdit.IsEnabled = false;
                }
            }
        }
    }

    private void TE_RepeatsOfSeveralDays_TextChanged( object sender, EventArgs e )
    {
        var enteredText = sender as TextEdit;

        if (enteredText?.Text?.Length > 2)
        {
            enteredText.Text = enteredText.Text.Substring( startIndex: 0, length: 2 );
        }
    }

    private void TE_RepeatsOfSeveralTimesPerPeriod_TextChanged( object sender, EventArgs e )
    {
        var enteredText = sender as TextEdit;

        if (enteredText?.Text?.Length > 2)
        {
            enteredText.Text = enteredText.Text.Substring( startIndex: 0, length: 2 );
        }
    }

    void SeveralTimesPerPeriodFrequency_Tapped( object sender, TappedEventArgs e )
    {
        CE_SeveralTimesPerPeriod.IsChecked = true;
    }

    void SeveralTimesPerPeriodFrequency_Focused( object sender, FocusEventArgs e )
    {
        if (e.IsFocused)
        {
            CE_SeveralTimesPerPeriod.IsChecked = true;
        }
    }

    void CBE_Period_Tap( System.Object sender, System.ComponentModel.HandledEventArgs e )
    {
        CE_SeveralTimesPerPeriod.IsChecked = true;
    }

    void TE_Frequency_Tap( System.Object sender, System.ComponentModel.HandledEventArgs e )
    {
        OpenFrequencyPopup();
    }
}
