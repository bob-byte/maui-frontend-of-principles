namespace Principles.Services;

public interface IDialogService
{
    bool IsPopupOpen { get; }
    Task ShowAlertAsync( string msg, string title, string buttonLabel );
    Task<bool> ShowAlertWithTwoBtnsAsync( string msg, string title, string accept, string cancel );
    Task<bool> ShowConfirmAsync( string msg, string title );
    Task ShowErrorAsync( string msg );
    Task<IPopupResult> ShowPopupAsync<TPopupViewModel>(
        IDictionary<string, object>? parameters = null, 
        PopupOptions options = null
    ) where TPopupViewModel : BaseViewModel;
    Task<IPopupResult<TResult>> ShowPopupAsync<TPopupViewModel, TResult>(
        IDictionary<string, object>? parameters = null, 
        PopupOptions options = null
    ) where TPopupViewModel : BaseViewModel;
    /// <summary>
    /// Closes current popup
    /// </summary>
    Task ClosePopupAsync();
    /// <summary>
    /// Closes current popup
    /// </summary>
    Task ClosePopupAsync<TResult>(TResult result);
}