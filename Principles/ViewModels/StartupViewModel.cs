
using System.Net;

namespace Principles.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
    private readonly IGoogleAuthService m_googleAuthService;
    private readonly IAppleAuthService m_appleAuthService;
    private readonly IReminderService m_reminderService;

    [ObservableProperty]
    private ObservableCollectionEx<AppFeature> m_appFeatures;

    public StartupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_googleAuthService = serviceProvider.GetRequiredService<IGoogleAuthService>();
        m_reminderService = serviceProvider.GetRequiredService<IReminderService>();
        m_appleAuthService = serviceProvider.GetRequiredService<IAppleAuthService>();

        m_appFeatures = new ObservableCollectionEx<AppFeature>
        {
            new() {  Title = LocStrings.TransformAreasOfLifeTitle, Description =  LocStrings.TransformAreasOfLifeDescription},
            new() {  Title = LocStrings.ChatWithHelperTitle, Description = LocStrings.ChatWithHelperDescription },
            new() {  Title = LocStrings.GroupHabitsByGoalsTitle, Description = LocStrings.GroupHabitsByGoalsDescription },
            new() {  Title = LocStrings.GetRecommendationsByAITitle, Description = LocStrings.GetRecommendationsByAIDescription },
            new() {  Title = LocStrings.BecomeTruePersonalityTitle, Description = LocStrings.BecomeTruePersonalityDescription }
        };
    }

    [RelayCommand]
    public async Task ContinueWithGoogleAsync()
    {
        try
        {
            await m_googleAuthService.AuthorizeAsync();
            await m_reminderService.TryToRecoverAllUserRemindersAsync();
            await Navigation.GoToInitialViewAsync();
        }
        catch (Exception ex)
        {
            await HandleExceptionWhenGoogleAuthAsync( ex ).DefaultConfigureAwait();
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
            errorMsg = LocStrings.SomethingWentWrongWhenUserAuthsUsingExternalService;
        }
            
        bool doShowAlert = !string.IsNullOrWhiteSpace( errorMsg );
        if (doShowAlert)
        {
            LoggingService.LogError( ex, ex.Message );
            await DialogService.ShowErrorAsync( errorMsg! );
        }
    }

    [RelayCommand]
    public async Task ContinueWithAppleAsync()
    {
        try
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Version.Major >= 13)
            {
                await m_appleAuthService.AuthorizeAsync();
                await m_reminderService.TryToRecoverAllUserRemindersAsync();
                await Navigation.GoToInitialViewAsync();
            }
            else
            {
                await DialogService.ShowErrorAsync( LocStrings.AppleAuthIsNotSupportedForCurrentDevice );
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionWhenAppleAuthAsync( ex ).DefaultConfigureAwait();
        }
    }

    private async Task HandleExceptionWhenAppleAuthAsync( Exception ex )
    {
        bool isCancelledByUser = ex.Message.Contains( "1001" );
        if (!isCancelledByUser)
        {
            LoggingService.LogError( ex, ex.Message );

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
                errorMsg = LocStrings.SomethingWentWrongWhenUserAuthsUsingExternalService;
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
