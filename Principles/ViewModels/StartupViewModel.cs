
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
            if (ex is not TaskCanceledException)
            {
                LoggingService.LogError( ex, ex.Message );
            }
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
                throw new NotSupportedException( message: LocStrings.AppleAuthIsNotSupportedForCurrentDevice );
            }
        }
        catch (Exception ex)
        {
            bool isCancelledByUser = ex is TaskCanceledException || ex.Message.Contains( "error 1001" );
            if (!isCancelledByUser)
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
