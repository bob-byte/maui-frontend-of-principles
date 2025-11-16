
using System.Net;

namespace Principles.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
    private readonly IGoogleAuthService m_googleAuthService;
    private readonly IAppleAuthService m_appleAuthService;
    private readonly IReminderService m_reminderService;

    public StartupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_googleAuthService = serviceProvider.GetRequiredService<IGoogleAuthService>();
        m_reminderService = serviceProvider.GetRequiredService<IReminderService>();
        m_appleAuthService = serviceProvider.GetRequiredService<IAppleAuthService>();
    }



    [RelayCommand]
    public async Task ContinueWithGoogleAsync()
    {
        try
        {
            await m_googleAuthService.AuthorizeAsync();
        }
        catch (Exception ex)
        {
            await HandleExceptionWhenGoogleAuthAsync( ex ).DefaultConfigureAwait();
            return;
        }

        await ExecuteWithRetryAsync( m_reminderService.TryToRecoverAllUserRemindersAsync );
        
        try
        {
            await Navigation.GoToInitialViewAsync();
        }
        catch(Exception ex)
        {
            LoggingService.LogError( ex, ex.Message );
        }
    }

    private async Task HandleExceptionWhenGoogleAuthAsync(Exception ex)
    {
        string? errorMsg = null;
            
        if (ex is TimeoutException || ex.InnerException is TimeoutException)
        {
            errorMsg = LocStrings.OperationTimeoutMessage;
        }
        else if (ex is HttpRequestException or AggregateException or WebException)
        {
            errorMsg = LocStrings.NoInternetConnection;
        }
        else if (ex is not TaskCanceledException)
        {
            errorMsg = LocStrings.SomethingWentWrong;
            LoggingService.LogError( ex, ex.Message );
        }
        
        bool doShowAlert = !string.IsNullOrWhiteSpace( errorMsg );
        if (doShowAlert)
        {
            await DialogService.ShowErrorAsync( errorMsg! );
        }
    }

    [RelayCommand]
    public async Task ContinueWithAppleAsync()
    {
        try
        {
            await m_appleAuthService.AuthorizeAsync();
        }
        catch (Exception ex)
        {
            await HandleExceptionWhenAppleAuthAsync( ex ).DefaultConfigureAwait();
            return;
        }

        await ExecuteWithRetryAsync( m_reminderService.TryToRecoverAllUserRemindersAsync );

        try
        {
            await Navigation.GoToInitialViewAsync();
        }
        catch(Exception ex)
        {
            LoggingService.LogError( ex, ex.Message );
        }
    }

    private async Task HandleExceptionWhenAppleAuthAsync( Exception ex )
    {
        bool isCancelledByUser = ex.Message.Contains( "1001" );
        if (!isCancelledByUser)
        {
            string? errorMsg = null;
                
            if (ex is TimeoutException || ex.InnerException is TimeoutException)
            {
                errorMsg = LocStrings.OperationTimeoutMessage;
            }
            else if (ex is HttpRequestException or AggregateException or WebException)
            {
                errorMsg = LocStrings.NoInternetConnection;
            }
            else if (ex is not TaskCanceledException)
            {
                errorMsg = LocStrings.SomethingWentWrong;
                LoggingService.LogError( ex, ex.Message );
            }
                
            bool doShowAlert = !string.IsNullOrWhiteSpace( errorMsg );
            if (doShowAlert)
            {
                await DialogService.ShowErrorAsync( errorMsg! );
            }
        }
    }

    [RelayCommand]
    public Task OpenLoginViewAsync()
    {
        return Navigation.NavigateToAsync<LoginViewModel>();
    }

    [RelayCommand]
    public Task OpenSignUpViewAsync()
    {
        return Navigation.NavigateToAsync<SignupViewModel>();
    }
}
