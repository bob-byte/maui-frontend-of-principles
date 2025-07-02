namespace Principles.Services;

public class TipService : ITipService
{
    public async Task ShowSnackbarAsync( string message )
    {
        await ShowSnackbarAsync( message, Timeout.InfiniteTimeSpan );
    }

    public async Task ShowSnackbarAsync( string message, TimeSpan duration )
    {
        SnackbarOptions snackbarOptions = DefaultSnackbarOptions();

        ISnackbar snackbar = Snackbar.Make(
            message,
            actionButtonText: LocStrings.OK,
            duration: duration, 
            visualOptions: snackbarOptions
        );

        await snackbar.Show();
    }

    public async Task ShowToastAsync( string message, TipDuration duration )
    {
#if IOS
        TimeSpan snackbarDuration = duration switch
        {
            TipDuration.Short => TimeSpan.FromSeconds( 2 ),
            _ => TimeSpan.FromSeconds( 3.5 ),
        };
        await ShowSnackbarAsync( message, snackbarDuration );
#else
        ToastDuration toastDuration = duration switch
        {
            TipDuration.Short => ToastDuration.Short,
            _ => ToastDuration.Long,
        };
        await Toast.Make( message, toastDuration ).Show();
#endif
    }

    public async Task ShowToastAsync( string message )
    {
#if IOS
        await ShowSnackbarAsync( message, TimeSpan.FromSeconds( 2 ) );
#else
        await Toast.Make( message, ToastDuration.Short ).Show();
#endif
    }
    
    private static SnackbarOptions DefaultSnackbarOptions()
    {
        SnackbarOptions result = SnackbarHelper.DefaultOptions();
        return result;
    }
}
