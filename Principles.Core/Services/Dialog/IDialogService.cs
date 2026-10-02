namespace Principles.Core.Services;

public interface IDialogService
{
    bool IsPopupOpen { get; }
    Task ShowAlertAsync( string msg, string title, string buttonLabel );
    Task<bool> ShowAlertWithTwoBtnsAsync( string msg, string title, string accept, string cancel );
    Task<bool> ShowConfirmAsync( string msg, string title );
    Task ShowErrorAsync( string msg );
    Task<object?> ShowPopupAsync<TPopupViewModel>(
        IDictionary<string, object>? parameters = null, 
        object? options = null
    );
    Task<object?> ShowPopupAsync<TPopupViewModel, TResult>(
        IDictionary<string, object>? parameters = null, 
        object? options = null
    );
    /// <summary>
    /// Closes current popup
    /// </summary>
    Task ClosePopupAsync();
    /// <summary>
    /// Closes current popup
    /// </summary>
    Task ClosePopupAsync<TResult>(TResult result);
}
