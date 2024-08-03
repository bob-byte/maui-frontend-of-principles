
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
        ViewModel.PropertyChanged += ViewModel_HabitPropertyChanged;

        InitializeComponent();
    }

    private void ViewModel_HabitPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        UserHabit habit = ViewModel.Habit;
        if (e.PropertyName == nameof( EditHabitViewModel.Habit ) && habit != null)
        {
            habit.PropertyChanged += TypeOfHabit_PropertyChanged;

            switch (ViewModel.Habit.Type)
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

    private void TypeOfHabit_PropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if(e.PropertyName == nameof( UserHabit.Type ))
        {
            switch (ViewModel.Habit.Type)
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
                int repeats = Convert.ToUInt16( TE_RepeatsOfSeveralTimesPerPeriod.Text );

                switch (ViewModel.SelectedPeriodOfHabit.Type)
                {
                    case PeriodTypeOfHabit.Week:
                        {
                            if(repeats >= 7)
                            {
                                frequency.Type = FrequencyType.EveryDay;
                                repeats = 1;
                                frequency.IntervalLengthInDays = 1;
                            }
                            else
                            {
                                frequency.Type = FrequencyType.SeveralTimesPerPeriod;
                                frequency.IntervalLengthInDays = 7;
                            }

                            break;
                        }

                    case PeriodTypeOfHabit.Month:
                        {
                            if (repeats >= 30)
                            {
                                frequency.Type = FrequencyType.EveryDay;
                                frequency.IntervalLengthInDays = 1;
                                repeats = 1;
                            }
                            else
                            {
                                frequency.Type = FrequencyType.SeveralTimesPerPeriod;
                                frequency.IntervalLengthInDays = 30;
                            }

                            break;
                        }

                    case PeriodTypeOfHabit.Year:
                        {
                            if (repeats >= 365)
                            {
                                frequency.Type = FrequencyType.EveryDay;
                                frequency.IntervalLengthInDays = 1;
                                repeats = 1;
                            }
                            else
                            {
                                frequency.Type = FrequencyType.SeveralTimesPerPeriod;
                                frequency.IntervalLengthInDays = 365;
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
        //NameOfHabit.Value is not updated in some devices
        ViewModel.NameOfHabit.Value = ME_NameOfHabit.Text;

        if (ViewModel.SaveCommand.CanExecute( null ))
        {
            await ViewModel.SaveCommand.ExecuteAsync( null );
        }
        else if( !ViewModel.IsBusy && !ViewModel.IsLoadingHabitInfo )
        {
            DXS_ErrorMessages.Children.Clear();
            DXS_ErrorMessages.RowDefinitions.Clear();

            int numRow = 0;
            foreach (string errMsg in ViewModel.NameOfHabit.Errors)
            {
                Label errorLabel = new()
                {
                    Text = errMsg,
                    FontSize = 16,
                    FontFamily = "MonaSansMedium",
                    LineBreakMode = LineBreakMode.TailTruncation,
                    MaxLines = 5,
                    HorizontalOptions = LayoutOptions.Start,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };

                RowDefinition row = new()
                {
                    Height = new GridLength( value: 1, GridUnitType.Auto )
                };
                DXS_ErrorMessages.AddRowDefinition( row );
                DXS_ErrorMessages.Children.Add( errorLabel );
                DXS_ErrorMessages.SetRow( errorLabel, numRow );
                numRow++;
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

    void DXC_SelectPriority_CompleteItemDragDrop( System.Object sender, DevExpress.Maui.CollectionView.CompleteItemDragDropEventArgs e )
    {
        for (int priority = 1; priority <= ViewModel.UserHabits.Count; priority++)
        {
            ViewModel.UserHabits[priority - 1].Priority = priority;
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

    private void ME_HabitGoal_IconClicked( System.Object sender, System.EventArgs e )
    {
        ShowOrHideUserGoals();
    }

    private void ME_HabitGoal_Tap( object sender, HandledEventArgs e )
    {
        ShowOrHideUserGoals();
    }

    private void ShowOrHideUserGoals()
    {
        if(GoalsBottomSheet.State == BottomSheetState.Hidden)
        {
            GoalsBottomSheet.State = BottomSheetState.HalfExpanded;
            double bottomSheetHeight = PageHeight * GoalsBottomSheet.HalfExpandedRatio;
            double rowSpacing = G_Goals.RowSpacing * (G_Goals.RowDefinitions.Count - 1);
            double additionalSpacing = 15;
            double height = bottomSheetHeight - rowSpacing - L_GoalSelectionCenterHeader.HeightRequest - L_GoalRecommendation.HeightRequest - additionalSpacing;

            SKL_Goals.HeightRequest = height;
            SKL_Goals.WidthRequest = PageWidth - (G_Goals.Padding.Left + G_Goals.Padding.Right);
            DXCV_Goals.HeightRequest = height;
        }
        else
        {
            GoalsBottomSheet.State = BottomSheetState.Hidden;
        }
    }

    private void OnGoalAddTap( object sender, TappedEventArgs e )
    {
        DXP_GoalAdd.IsOpen = true;
        ViewModel.EditedGoal = new UserGoal()
        {
            Id = 0,
            Name = $"{LocStrings.Be} "
        };
    }

    private void OnGoalNameTap( object sender, TappedEventArgs e )
    {
        GoalsBottomSheet.State = BottomSheetState.Hidden;
    }

    private void OnME_GoalNameEndIconClicked( object sender, EventArgs e )
    {
        DXP_GoalAdd.IsOpen = true;

    }

    private async void Sb_SaveGoal_Clicked( object sender, EventArgs e )
    {
        //EditedGoal.Name is not updated in some devices
        ViewModel.EditedGoal.Name = ME_EditedGoal.Text;

        void closePopup() => DXP_GoalAdd.IsOpen = false;

        if (ViewModel.SaveGoalCommand.CanExecute( closePopup ))
        {
            await ViewModel.SaveGoalCommand.ExecuteAsync( closePopup );
        }
    }

    private void OnGoalNameTapped( object sender, HandledEventArgs e )
    {
        var multilineEdit = sender as MultilineEdit;
        if(multilineEdit is not null)
        {
            GoalsBottomSheet.State = BottomSheetState.Hidden;

            var goal = multilineEdit.BindingContext as UserGoal;
            ViewModel.OnGoalNameTapped( goal! );

            m_doExecuteReloadOfRecommendedHabits = true;
        }
    }
}
