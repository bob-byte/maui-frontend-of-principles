namespace Principles.Core.Services;

public class ServiceOfTask : BaseRemoteService, IServiceOfTask
{
    private readonly IUrlBuilder _urlBuilder;

    public ObservableCollectionEx<TaskItem> StoredUserTasks { get; set; } = new();

    public ServiceOfTask(IUrlBuilder urlBuilder, IServiceProvider serviceProvider) 
        : base(serviceProvider)
    {
        _urlBuilder = urlBuilder;
    }

    // --- СТВОРЕННЯ ---
    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        return await RequestProvider.PostAsync<TaskItem, TaskItem>(
            _urlBuilder.Tasks, 
            task, 
            SettingsService.AuthAccessToken);
    }

    // --- ЗАВАНТАЖЕННЯ (Фільтри) ---
    public async Task<List<TaskItem>> GetTasksAsync(DateOnly date)
    {
        return await RequestProvider.GetAsync<List<TaskItem>>(
            _urlBuilder.TasksByDate(date), 
            SettingsService.AuthAccessToken).DefaultConfigureAwait();
    }

    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        try
        {
            return await RequestProvider.GetAsync<List<TaskItem>>(
                $"{_urlBuilder.Tasks}/all",
                SettingsService.AuthAccessToken).DefaultConfigureAwait();
        }
        catch (ExtendedHttpRequestException ex) when (ex.HttpCode == System.Net.HttpStatusCode.NotFound)
        {
            return new List<TaskItem>(); 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] Сталася помилка: {ex.Message}");
            return new List<TaskItem>();
        }
    }

    public async Task<List<TaskItem>> GetInboxTasksAsync()
    {
        try
        {
            return await RequestProvider.GetAsync<List<TaskItem>>(
                $"{_urlBuilder.Tasks}/inbox", 
                SettingsService.AuthAccessToken).DefaultConfigureAwait();
        }
        catch (ExtendedHttpRequestException ex) when (ex.HttpCode == System.Net.HttpStatusCode.NotFound)
        {
            return new List<TaskItem>(); 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] Помилка завантаження Inbox: {ex.Message}");
            return new List<TaskItem>();
        }
    }

    // --- ОНОВЛЕННЯ ---
    public async Task UpdateStatusAsync(long id, bool isCompleted)
    {
        await RequestProvider.PutAsync(
            $"{_urlBuilder.Tasks}/{id}/status", 
            new { IsCompleted = isCompleted }, 
            SettingsService.AuthAccessToken);
    }

    public async Task UpdateTaskAsync(long id, TaskItem task)
    {
        await RequestProvider.PutAsync(
            $"{_urlBuilder.Tasks}/{id}", 
            task, 
            SettingsService.AuthAccessToken);
    }

    // --- ВИДАЛЕННЯ ---
    public async Task DeleteTaskAsync(long id)
    {
        await RequestProvider.DeleteAsync(
            $"{_urlBuilder.Tasks}/{id}", 
            SettingsService.AuthAccessToken);
    }
}