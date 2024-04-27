namespace SET.Core.Services;

public interface IDialogService
{
    Task ShowAlertAsync( string msg, string title, string buttonLabel );
    Task<bool> ShowAlertWithTwoBtnsAsync( string msg, string title, string accept, string cancel );
    Task<bool> ShowConfirmAsync( string msg, string title );
    Task ShowErrorAsync( string msg );
}