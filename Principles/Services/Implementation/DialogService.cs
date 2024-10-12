using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        return Application.Current.MainPage.DisplayAlert( title, msg, buttonLabel );
    }

    public Task ShowErrorAsync( string msg )
    {
        return Application.Current.MainPage.DisplayAlert( title: LocStrings.Error, msg, cancel: LocStrings.OK );
    }

    public Task<bool> ShowAlertWithTwoBtnsAsync( string msg, string title, string accept, string cancel )
    {
        return Application.Current.MainPage.DisplayAlert( title, msg, accept, cancel );
    }

    public Task<bool> ShowConfirmAsync( string msg, string title )
    {
        return Application.Current.MainPage.DisplayAlert( title, msg, LocStrings.Yes, LocStrings.No );
    }
}
