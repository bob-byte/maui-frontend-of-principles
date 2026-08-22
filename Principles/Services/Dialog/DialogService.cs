namespace Principles.Services;

public class DialogService : IDialogService
{
    public bool IsPopupOpen { get; private set; }

    public Task ShowAlertAsync( string msg, string title, string buttonLabel )
    {
        return Shell.Current.DisplayAlertAsync( title, msg, buttonLabel );
    }

    public Task ShowErrorAsync( string msg )
    {
        return Shell.Current.DisplayAlertAsync( title: LocStrings.Error, msg, cancel: LocStrings.OK );
    }

    public Task<bool> ShowAlertWithTwoBtnsAsync( string msg, string title, string accept, string cancel )
    {
        return Shell.Current.DisplayAlertAsync( title, msg, accept, cancel );
    }

    public Task<bool> ShowConfirmAsync( string msg, string title )
    {
        return Shell.Current.DisplayAlertAsync( title, msg, LocStrings.Yes, LocStrings.No );
    }

    public Task<object?> ShowPopupAsync<TPopupViewModel>(
        IDictionary<string, object>? parameters = null, 
        object? options = null
    )
    {
        IPopupService popupService = ServiceLocator.Current.GetRequiredService<IPopupService>();
        PopupOptions popupOptions = options as PopupOptions ?? new()
        {
            Shape = null,
            CanBeDismissedByTappingOutsideOfPopup = true
        };
        
        //to not trigger HandleDisappearingOfPageAsync of current page
        IsPopupOpen = true;
        return popupService.ShowPopupAsync<TPopupViewModel>( Shell.Current, popupOptions, parameters )
            .ContinueWith( t => (object?)t.Result );
    }

    public Task<object?> ShowPopupAsync<TPopupViewModel, TResult>(
        IDictionary<string, object>? parameters = null, 
        object? options = null
    )
    {
        IPopupService popupService = ServiceLocator.Current.GetRequiredService<IPopupService>();
        PopupOptions popupOptions = options as PopupOptions ?? new()
        {
            Shape = null,
            CanBeDismissedByTappingOutsideOfPopup = true
        };

        //to not trigger HandleDisappearingOfPageAsync of current page
        IsPopupOpen = true;

        return popupService.ShowPopupAsync<TPopupViewModel, TResult>( Shell.Current, popupOptions, parameters )
            .ContinueWith( t => (object?)t.Result );
    }

    public async Task ClosePopupAsync()
    {
        IPopupService popupService = ServiceLocator.Current.GetRequiredService<IPopupService>();
        
        await popupService.ClosePopupAsync(Shell.Current);

        //to trigger HandlePageAppearingAsync of next pages
        IsPopupOpen = false;
    }

    public async Task ClosePopupAsync<TResult>(TResult result)
    {
        IPopupService popupService = ServiceLocator.Current.GetRequiredService<IPopupService>();
        await popupService.ClosePopupAsync(Shell.Current, result);

        //to trigger HandlePageAppearingAsync of next pages
        IsPopupOpen = false;
    }
}
