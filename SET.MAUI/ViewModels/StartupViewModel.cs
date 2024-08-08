using SET.Core.Models.Enums;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Services;
using Google.Apis.Calendar.v3;
using DevExpress.Maui.Core.Internal;
namespace SET.MAUI.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollectionEx<AppFeature> m_appFeatures;

    public StartupViewModel( IServiceProvider serviceProvider, IConfiguration configuration)
        : base( serviceProvider )
    {
        Configuration = configuration;
        m_appFeatures = new ObservableCollectionEx<AppFeature>
        {
            new() {  Title = LocStrings.TransformAreasOfLifeTitle, Description =  LocStrings.TransformAreasOfLifeDescription},
            new() {  Title = LocStrings.ChatWithHelperTitle, Description = LocStrings.ChatWithHelperDescription },
            new() {  Title = LocStrings.GroupHabitsByGoalsTitle, Description = LocStrings.GroupHabitsByGoalsDescription },
            new() {  Title = LocStrings.GetRecommendationsByAITitle, Description = LocStrings.GetRecommendationsByAIDescription },
            new() {  Title = LocStrings.BecomeTruePersonalityTitle, Description = LocStrings.BecomeTruePersonalityDescription }
        };
    }

    public IConfiguration Configuration { get; set; }

    [RelayCommand]
    public async Task ContinueWithGoogleAsync()
    {
        await Authorize();
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

    public BaseClientService.Initializer Initializer { get; private set; }
    public async Task Authorize()
    {
        if (Initializer != null) return;

        DevicePlatform platform = DeviceInfo.Current.Platform;
        string clientSecretKey = platform == DevicePlatform.Android
            ? "Google:ClientSecrets:Android"
            : "Google:ClientSecrets:IOS";

        string? clientSecret = Configuration[clientSecretKey];

        if (string.IsNullOrEmpty( clientSecret ))
        {
            throw new InvalidOperationException( "Client secret is missing." );
        }

        UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = clientSecret
            },
            new[] { CalendarService.Scope.CalendarEvents }, 
            "user1111",
            CancellationToken.None,
            codeReceiver: new CodeReceiver(this)
        );

        Initializer = new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        };


        string[] serverResponse = credential.Token.AccessToken.Split( ' ' );
        SettingsService.AuthAccessToken = serverResponse[0];
        SettingsService.UserId = serverResponse[1];
        await Navigation.GoToInitialViewAsync();
    }
    private class CodeReceiver : ICodeReceiver
    {
        private readonly StartupViewModel m_startupViewModel;

        public CodeReceiver( StartupViewModel startupViewModel )
        {
            m_startupViewModel = startupViewModel;
        }
        public string RedirectUri => new LaunchUriBuilder( LaunchType.OAuth2Redirect ).Build().AbsoluteUri;

        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync( AuthorizationCodeRequestUrl url,
            CancellationToken taskCancellationToken )
        {
            await Launcher.Default.OpenAsync( url.Build().AbsoluteUri );
            LaunchResult result = await LaunchUriHelper.LaunchResult;
            if (result.Type != LaunchType.OAuth2Redirect) throw new OperationCanceledException();

            return new AuthorizationCodeResponseUrl
            {
                Code = result.Query.Get( "code" ),
                State = result.Query.Get( "state" ),
                Error = result.Query.Get( "error" ),
                ErrorDescription = result.Query.Get( "error_description" ),
                ErrorUri = result.Query.Get( "error_uri" )
            };
        }
    }
}
