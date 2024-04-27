namespace SET.MAUI.ViewModels;

public interface IViewModelBase : IQueryAttributable
{
    public INavigationService Navigation { get; }

    public IAsyncRelayCommand InitializeAsyncCommand { get; }

    public bool IsBusy { get; }

    public bool IsInitialized { get; }

    Task InitializeAsync(object? parameter = null);
}