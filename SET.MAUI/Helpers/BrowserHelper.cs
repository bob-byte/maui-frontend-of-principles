using System;
namespace SET.MAUI.Helpers;

public static class BrowserHelper
{
    public static async Task OpenUrl( string url )
    {
        try
        {
            await Launcher.Default.OpenAsync( url );
        }
        catch(Exception ex)
        {
            IDialogService dialogService = ServiceLocator.Current!.GetRequiredService<IDialogService>();
            ISettingsService settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
            string msg = settingsService.IsDebug ? ex.ToString() : ex.Message;

            await dialogService.ShowErrorAsync( msg ).DefaultConfigureAwait();
        }
    }
}

