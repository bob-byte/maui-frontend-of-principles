using Principles.Core.Models;

namespace Principles.Core.Services;
public interface IServiceOfTask
{
    ObservableCollectionEx<TaskItem> StoredUserTasks { get; set; }

    Task<TaskItem> CreateAsync(TaskItem task);
    Task<List<TaskItem>> GetTasksAsync(DateOnly date);
    Task<List<TaskItem>> GetAllTasksAsync();
    Task UpdateStatusAsync(long id, bool isCompleted);
    Task UpdateTaskAsync(long id, TaskItem task);
    Task DeleteTaskAsync(long id);
    Task<List<TaskItem>> GetInboxTasksAsync();
}