
namespace Principles.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
    private readonly IGoogleAuthService m_googleAuthService;

    [ObservableProperty]
    private ObservableCollectionEx<AppFeature> m_appFeatures;

    public StartupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_googleAuthService = serviceProvider.GetRequiredService<IGoogleAuthService>();

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
                Uri authUrl = new( uriString: "https://appleid.apple.com/auth/authorize" );
                Uri callbackUrl = new LaunchUriBuilder( LaunchType.OAuth2Redirect ).Build();

                result = await WebAuthenticator.AuthenticateAsync( authUrl, callbackUrl );
            }

            //TODO: send to server id token and access token and get from it JWT token
            //TODO: call await m_settingsService.SetAuthAccessTokenAsync( response.Token ).DefaultConfigureAwait();
            //TODO: go to initial view
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
