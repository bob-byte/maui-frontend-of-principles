namespace Principles.Services;

public class DialogService : IDialogService
{
    private readonly ILoggingService m_loggingService;

    public DialogService( ILoggingService loggingService )
    {
        m_loggingService = loggingService;
    }

    public Task ShowAlertAsync( string msg, string title, string buttonLabel )
    {
        return Shell.Current.DisplayAlert( title, msg, buttonLabel );
    }

    public Task ShowErrorAsync( string msg )
    {
        return Shell.Current.DisplayAlert( title: LocStrings.Error, msg, cancel: LocStrings.OK );
    }

    public Task<bool> ShowAlertWithTwoBtnsAsync( string msg, string title, string accept, string cancel )
    {
        return Shell.Current.DisplayAlert( title, msg, accept, cancel );
    }

    public Task<bool> ShowConfirmAsync( string msg, string title )
    {
        return Shell.Current.DisplayAlert( title, msg, LocStrings.Yes, LocStrings.No );
    }
}
