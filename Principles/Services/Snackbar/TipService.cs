using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Services;
public class TipService : ITipService
{
    public async Task ShowAsync( string message, string? actionText = null, Action? action = null )
    {
        var backgroundColor = (Color)Application.Current.Resources["LightPrimary"];
        var textColor = Colors.Black;
        var actionTextColor = Colors.White;

        var snackbarOptions = new SnackbarOptions
        {
            BackgroundColor = backgroundColor,
            TextColor = textColor,
            ActionButtonTextColor = actionTextColor,
            CornerRadius = new CornerRadius( 8 ),
            CharacterSpacing = 0
        };

        var snackbar = Snackbar.Make(
            message,
            async () =>
            {
                action?.Invoke();
                await Task.CompletedTask;
            },
            actionText ?? "OK",
            TimeSpan.FromSeconds( 10 ), 
            snackbarOptions
        );

        await snackbar.Show();
    }
}
