using Microsoft.Maui.Controls;
using Principles.ViewModels;
using Principles.Core.Models;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Principles.Views;

[QueryProperty(nameof(FilterString), "filter")]
public partial class TasksPageView : ContentPage
{
    private TaskItem _activeTask;

    public TasksPageView(TasksViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // --- ПЕРЕХОДИ З БУРГЕР-МЕНЮ ---
    public string FilterString
    {
        set
        {
            if (BindingContext is TasksViewModel vm && Enum.TryParse(typeof(TaskListType), value, true, out var listType))
            {
                vm.ChangeListType((TaskListType)listType);
            }
        }
    }

    // --- ЧЕКБОКС ВИКОНАННЯ ---
    private async void OnTaskCompletionChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox &&
            checkBox.BindingContext is TaskItem task &&
            BindingContext is TasksViewModel viewModel)
        {
            await viewModel.UpdateTaskStatusAsync(task, e.Value);
        }
    }

    // --- ВІДКРИТТЯ ІСНУЮЧОГО ЗАВДАННЯ ---
    private void OnTaskTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is TaskItem tappedTask)
        {
            _activeTask = tappedTask; 
            EditTaskSheet.State = DevExpress.Maui.Controls.BottomSheetState.HalfExpanded;

            if (BindingContext is TasksViewModel viewModel)
            {
                viewModel.EditingTask = null;       
                viewModel.EditingTask = tappedTask; 

                if (tappedTask.Date.HasValue)
                {
                    DateTime taskDate = tappedTask.Date.Value.ToDateTime(TimeOnly.MinValue);
                    EditHiddenDatePicker.Date = taskDate;
                    
                    if (taskDate.Date == DateTime.Today) 
                        EditDateLabel.Text = (string)viewModel.LocManager["Today"];
                    else if (taskDate.Date == DateTime.Today.AddDays(1)) 
                        EditDateLabel.Text = (string)viewModel.LocManager["Tomorrow"];
                    else 
                        EditDateLabel.Text = taskDate.ToString("MMM dd, yyyy");
                    
                    EditDateLabel.TextColor = Colors.Black;
                }
                else
                {
                    EditDateLabel.Text = (string)viewModel.LocManager["AddDate"];
                    EditDateLabel.TextColor = Colors.Gray;
                    EditHiddenDatePicker.Date = DateTime.Today; 
                }

                // Ініціалізація часу
                if (tappedTask.Time.HasValue) 
                {
                    EditHiddenTimePicker.Time = tappedTask.Time.Value.ToTimeSpan(); 
                    EditTimeLabel.Text = tappedTask.Time.Value.ToString(@"hh\:mm");
                    EditTimeLabel.TextColor = Colors.Black;
                }
                else
                {
                    EditTimeLabel.Text = (string)viewModel.LocManager["AddTime"];
                    EditTimeLabel.TextColor = Colors.Gray;
                }
            }
        }
    }

    // --- ФОКУС ПРИКХОВАНИХ ПІКЕРІВ ---
    private void OnEditDateTapped(object sender, TappedEventArgs e) => EditHiddenDatePicker.Focus();
    
    private void OnEditTimeTapped(object sender, TappedEventArgs e) => EditHiddenTimePicker.Focus();

    // --- ЗМІНА ДАТИ ТА ЧАСУ В ШТОРЦІ ---
    private void OnHiddenDateSelected(object sender, DateChangedEventArgs e)
    {
        if (!e.NewDate.HasValue) return;

        DateTime selectedDate = e.NewDate.Value; 

        if (BindingContext is TasksViewModel vm && vm.EditingTask != null)
        {
            vm.EditingTask.Date = DateOnly.FromDateTime(selectedDate); 

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (selectedDate.Date == DateTime.Today) 
                    EditDateLabel.Text = (string)vm.LocManager["Today"];
                else if (selectedDate.Date == DateTime.Today.AddDays(1)) 
                    EditDateLabel.Text = (string)vm.LocManager["Tomorrow"];
                else 
                    EditDateLabel.Text = selectedDate.ToString("MMM dd, yyyy"); 
                
                EditDateLabel.TextColor = Colors.Black;
            });
        }
    }

    private void OnHiddenTimeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time))
        {
            TimeSpan selectedTime = (TimeSpan)EditHiddenTimePicker.Time;

            if (BindingContext is TasksViewModel vm && vm.EditingTask != null)
            {
                vm.EditingTask.Time = TimeOnly.FromTimeSpan(selectedTime);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                EditTimeLabel.Text = selectedTime.ToString(@"hh\:mm");
                EditTimeLabel.TextColor = Colors.Black;
            });
        }
    }

    // --- ВЗАЄМОДІЯ З КНОПКАМИ ---
    private async void OnAddButtonTapped(object sender, TappedEventArgs e)
    {
        AddTaskSheet.State = DevExpress.Maui.Controls.BottomSheetState.HalfExpanded;

        await Task.Delay(200); 

        MainThread.BeginInvokeOnMainThread(() =>
        {
            NewTaskNameEntry.Focus();
        });
    }

    private void OnCloseEditSheetClicked(object sender, EventArgs e)
    {
        EditTaskSheet.State = DevExpress.Maui.Controls.BottomSheetState.Hidden;
    }

    private void OnEditDescriptionClicked(object sender, EventArgs e)
    {
        EditNotesEditor.Focus();
    }

    private void OnDeleteTaskClicked(object sender, EventArgs e)
    {
        if (_activeTask == null) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                if (BindingContext is TasksViewModel vm)
                {
                    string title = (string)vm.LocManager["DeleteTaskTitle"]; 
                    string message = $"{(string)vm.LocManager["AreYouSureDelete"]} '{_activeTask.Name}'?"; 
                    string yesBtn = (string)vm.LocManager["YesDelete"]; 
                    string cancelBtn = (string)vm.LocManager["Cancel"]; 

                    bool isConfirmed = await Shell.Current.DisplayAlert(title, message, yesBtn, cancelBtn);
                    
                    if (isConfirmed)
                    {
                        vm.EditingTask = _activeTask; 
                        await vm.DeleteCurrentTaskAsync();
                        EditTaskSheet.State = DevExpress.Maui.Controls.BottomSheetState.Hidden;
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        Microsoft.Maui.Controls.Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
            .UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        Microsoft.Maui.Controls.Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
            .UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Pan);
    }
}