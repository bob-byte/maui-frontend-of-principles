
namespace Principles.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
    private readonly IGoogleAuthService m_googleAuthService;
    private readonly ISettingsService m_settingsService;

    private readonly string _authUrl = "https://appleid.apple.com/auth/authorize";
    private readonly string _clientId = "com.set.principles.auth";
    private readonly string _redirectUri = "https://principles.top/api/auth/apple";

    [ObservableProperty]
    private ObservableCollectionEx<AppFeature> m_appFeatures;

    public StartupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_googleAuthService = serviceProvider.GetRequiredService<IGoogleAuthService>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();

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
    public async Task ContinueWithAppleAsync()
    {
        try
        {
            WebAuthenticatorResult result = null;

            if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Version.Major >= 13)
            {
                // Make sure to enable Apple Sign In in both the
                // entitlements and the provisioning profile.
                var options = new AppleSignInAuthenticator.Options
                {
                    IncludeEmailScope = true,
                    IncludeFullNameScope = true,
                };
                result = await AppleSignInAuthenticator.AuthenticateAsync( options );
            }
            else
            {
                string encodedResponseType = Uri.EscapeDataString( "code id_token" );
                string encodedScope = Uri.EscapeDataString( "openid name email" );
                string responseMode = Uri.EscapeDataString( "form_post" );

                Uri authUrl = new Uri(
                    $"{_authUrl}?client_id={_clientId}" +
                    $"&redirect_uri={_redirectUri}" +
                    $"&response_type={encodedResponseType}" +
                    $"&scope={encodedScope}" +
                    $"&response_mode={responseMode}"
                );

                Uri callbackUrl = new Uri( _redirectUri );

                result = await WebAuthenticator.AuthenticateAsync( authUrl, callbackUrl );
            }

            if (result != null)
            {
                var appToken = result?.Properties.GetValueOrDefault( "app_token" );
                await m_settingsService.SetAuthAccessTokenAsync( appToken );

                await Navigation.GoToInitialViewAsync();
            }
            else
            {
                throw new Exception( "Authentication failed: No result returned." );
            }
        }
        catch (Exception ex)
        {
            if (ex is not TaskCanceledException)
            {
                LoggingService.LogError( ex, ex.Message );
            }
        }
    }

    [RelayCommand]
    public async Task ContinueWithGoogleAsync()
    {
        try
        {
            await m_googleAuthService.AuthorizeAsync();
            await Navigation.GoToInitialViewAsync();
        }
        catch (Exception ex)
        {
            if (ex is not TaskCanceledException)
            {
                LoggingService.LogError( ex, ex.Message );
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
