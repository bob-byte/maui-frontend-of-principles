namespace Principles.ViewModels;

public interface IViewModelBase : IQueryAttributable
{
    public INavigationService Navigation { get; }

    public IDialogService DialogService { get; }

    public bool IsBusy { get; }

    public bool IsInitialized { get; }
    Task HandlePageAppearingAsync( object? parameter = null );
    Task HandleDisappearingOfPageAsync( object? parameter = null );
}