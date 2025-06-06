namespace Principles.Services;

public class TipService : ITipService
{
    public async Task ShowSnackbarAsync( string message )
    {
        await ShowSnackbarAsync( message, Timeout.InfiniteTimeSpan );
    }

    public async Task ShowSnackbarAsync( string message, TimeSpan duration )
    {
        SnackbarOptions snackbarOptions = SnackbarHelper.DefaultOptions();

        ISnackbar snackbar = Snackbar.Make(
            message,
            actionButtonText: LocStrings.OK,
            duration: duration, 
            visualOptions: snackbarOptions
        );

        await snackbar.Show();
    }
}
