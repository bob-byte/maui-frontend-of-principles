using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Principles.Core.Models;

namespace Principles.ViewModels;

public enum TaskListType
{
    Today,
    Overdue,
    Upcoming,
    Inbox,
    Someday,
    Completed
}

public partial class TasksViewModel : BaseViewModel
{
    private readonly IServiceOfTask _taskService;
    private List<TaskItem> _allTasks = new();

    // --- ДИНАМІЧНІ ЕЛЕМЕНТИ ІНТЕРФЕЙСУ ---
    [ObservableProperty] private ObservableCollectionEx<TaskItem> _displayedTasks = new();
    [ObservableProperty] private string _pageTitle;
    [ObservableProperty] private string _headerText;
    [ObservableProperty] private double _progressValue; 
    [ObservableProperty] private TaskListType _currentListType = TaskListType.Today;

    // --- ЗАВДАННЯ ДЛЯ РЕДАГУВАННЯ / СТВОРЕННЯ ---
    [ObservableProperty] private TaskItem _editingTask;
    [ObservableProperty] private TaskItem _currentTask = new();

    public TasksViewModel(IServiceOfTask taskService, IServiceProvider serviceProvider) 
        : base(serviceProvider)
    {
        _taskService = taskService;
        
        PrepareNewTask(); 
        Task.Run(LoadAllTasksAsync); // Спростили синтаксис виклику
    }

    // --- ЛОГІКА ДАТИ ТА ЧАСУ ---
    public string SelectedDateText
    {
        get
        {
            if (!CurrentTask.Date.HasValue) return "Date";
            
            var date = CurrentTask.Date.Value;
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            if (date == today) return "Today";
            if (date == today.AddDays(1)) return "Tomorrow";
            
            return date.ToString("MMM dd, yyyy");
        }
    }

    public string SelectedTimeText => CurrentTask.Time?.ToString("HH:mm") ?? "Time";

    public DateTime SelectedDate
    {
        get => CurrentTask.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
        set 
        {
            CurrentTask.Date = DateOnly.FromDateTime(value);
            OnPropertyChanged(nameof(CurrentTask));
            OnPropertyChanged(nameof(SelectedDateText));
        }
    }

    public TimeSpan SelectedTime
    {
        get => CurrentTask.Time?.ToTimeSpan() ?? TimeSpan.Zero;
        set 
        {
            CurrentTask.Time = TimeOnly.FromTimeSpan(value);
            OnPropertyChanged(nameof(CurrentTask));
            OnPropertyChanged(nameof(SelectedTimeText));
        }
    }

    // --- МЕТОД ЗМІНИ ВКЛАДКИ ---
    public void ChangeListType(TaskListType newListType)
    {
        CurrentListType = newListType;
        PrepareNewTask(); 
        ApplyFilterAndCalculateProgress(); 
    }

    // --- ГОЛОВНИЙ ФІЛЬТР ---
    private void ApplyFilterAndCalculateProgress()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var categoryTasks = CurrentListType switch
        {
            TaskListType.Today => _allTasks.Where(t => t.Date == today),
            TaskListType.Overdue => _allTasks.Where(t => t.Date < today),
            TaskListType.Upcoming => _allTasks.Where(t => t.Date > today),
            TaskListType.Inbox => _allTasks.Where(t => !t.IsCompleted), 
            TaskListType.Someday => _allTasks.Where(t => !t.Date.HasValue),
            TaskListType.Completed => _allTasks.Where(t => t.IsCompleted),
            _ => _allTasks
        };

        int total = categoryTasks.Count();
        int completed = categoryTasks.Count(t => t.IsCompleted);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            PageTitle = CurrentListType.ToString();
            HeaderText = $"{completed} of {total} completed";
            ProgressValue = total == 0 ? 0 : (double)completed / total;

            DisplayedTasks.Clear();
            
            var tasksToShow = CurrentListType == TaskListType.Completed 
                ? categoryTasks 
                : categoryTasks.Where(t => !t.IsCompleted);

            foreach (var task in tasksToShow)
            {
                DisplayedTasks.Add(task);
            }
        });
    }

    // --- ЗАВАНТАЖЕННЯ ДАНИХ ---
    public async Task LoadAllTasksAsync()
    {
        var tasks = await _taskService.GetAllTasksAsync(); 
        _allTasks = tasks.ToList();
        ApplyFilterAndCalculateProgress();
    }

    // --- СТВОРЕННЯ НОВОГО ЗАВДАННЯ ---
    private void PrepareNewTask()
    {
        CurrentTask = new TaskItem
        {
            Date = CurrentListType switch
            {
                TaskListType.Today => DateOnly.FromDateTime(DateTime.Now),
                TaskListType.Upcoming => DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TaskListType.Overdue => DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                _ => null 
            }
        };

        OnPropertyChanged(nameof(CurrentTask));
        OnPropertyChanged(nameof(SelectedDateText));
        OnPropertyChanged(nameof(SelectedTimeText));
    }

    // --- ЗБЕРЕЖЕННЯ (Нове завдання) ---
    [RelayCommand]
    private async Task SaveTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentTask.Name)) return;

        var result = await _taskService.CreateAsync(CurrentTask);
        if (result != null)
        {
            _allTasks.Add(result);
            _taskService.UpsertStoredTask(result);
            ApplyFilterAndCalculateProgress(); 
            PrepareNewTask(); 
        }
    }

    // --- ЗБЕРЕЖЕННЯ (Існуюче завдання) ---
    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (EditingTask == null) return;

        try
        {
            await _taskService.UpdateTaskAsync(EditingTask.Id, EditingTask);

            var index = _allTasks.FindIndex(t => t.Id == EditingTask.Id);
            if (index != -1)
            {
                _allTasks[index] = EditingTask;
            }
            _taskService.UpsertStoredTask(EditingTask);

            ApplyFilterAndCalculateProgress();
            EditingTask = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка при збереженні: {ex.Message}");
        }
    }

    // --- ВИДАЛЕННЯ ---
    public async Task DeleteCurrentTaskAsync()
    {
        if (EditingTask == null) return;

        try
        {
            await _taskService.DeleteTaskAsync(EditingTask.Id);

            var taskToRemove = _allTasks.FirstOrDefault(t => t.Id == EditingTask.Id);
            if (taskToRemove != null)
            {
                _allTasks.Remove(taskToRemove);
            }
            _taskService.RemoveStoredTask(EditingTask.Id);

            ApplyFilterAndCalculateProgress();
            EditingTask = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка при видаленні: {ex.Message}");
        }
    }

    // --- ЧЕКБОКС (Виконання) ---
    public async Task UpdateTaskStatusAsync(TaskItem task, bool isCompleted)
    {
        if (task.Id == 0)
        {
            task.IsCompleted = isCompleted;
        }
        else
        {
            await _taskService.UpdateStatusAsync(task.Id, isCompleted);
            task.IsCompleted = isCompleted;
        }

        var index = _allTasks.FindIndex(t => t.Id == task.Id);
        if (index != -1) _allTasks[index].IsCompleted = isCompleted;
        _taskService.UpsertStoredTask(task);
        
        ApplyFilterAndCalculateProgress();
    }
}