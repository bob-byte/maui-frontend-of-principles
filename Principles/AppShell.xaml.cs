namespace Principles;

public partial class AppShell : Shell
{
    private readonly INavigationService m_navigationService;
    private readonly ISettingsService m_settingsService;
    public WeakReferenceMessenger ReferenceMessenger { get; }
    public LocalizationResourceManager LocManager
        => LocalizationResourceManager.Instance;

    public AppShell( IServiceProvider serviceProvider )
    {
        BindingContext = this;
        m_navigationService = serviceProvider.GetRequiredService<INavigationService>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        InitRouting();
        InitializeComponent();
        ReferenceMessenger = WeakReferenceMessenger.Default;
        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            OnPropertyChanged( nameof( LocManager ) );
        } );
    }

    protected async override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null)
        {
            await InitializeAppAsync();
        }
    }

    private async Task InitializeAppAsync()
    {
        await m_settingsService.GetAuthAccessTokenAsync();

        bool isLoggedIn = !string.IsNullOrWhiteSpace( m_settingsService.AuthAccessToken );
        if (isLoggedIn)
        {
            await m_navigationService.NavigateToMainAsync<ProgressOfHabitsViewModel>();
        }
        else
        {
            await m_navigationService.NavigateToAsync<StartupViewModel>( isAbsoluteRoute: true );
            await m_navigationService.NavigateToAsync<AppBenefitsViewModel>();
        }

        IVersionCheckerService versionCheckerService = ServiceLocator.Current!.GetRequiredService<IVersionCheckerService>();
        bool shouldShowPopup = await versionCheckerService.ShouldShowPopup();

        if (shouldShowPopup)
        {
            IDialogService dialogService = ServiceLocator.Current!.GetRequiredService<IDialogService>();
            await dialogService.ShowPopupAsync<UpdatePopupViewModel>();
        }

        if (VersionTracking.IsFirstLaunchEver)
        {
            try
            {
                string token = await m_settingsService.GetAuthAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace( token ))
                {
                    IReminderService reminderService = ServiceLocator.Current!.GetRequiredService<IReminderService>();
                    await reminderService.TryToRecoverAllUserRemindersAsync();
                }
            }
            catch (Exception ex)
            {
                ILoggingService loggingService = ServiceLocator.Current!.GetRequiredService<ILoggingService>();
                loggingService.LogError( ex, ex.Message );
            }
        }

        if (m_settingsService.IsAdsEnabled)
        {
            IAdService adService = ServiceLocator.Current!.GetRequiredService<IAdService>();
            await adService.GetAccessToTrackAsync();
        }
    }

    private static void InitRouting()
    {
        RegisterRoute( typeof( SignupView ) );
        RegisterRoute( typeof( LoginView ) );
        RegisterRoute( typeof( EditHabitView ) );
        RegisterRoute( typeof( SettingsView ) );
        RegisterRoute( typeof( ForgetPasswordView ) );
        RegisterRoute( typeof( ChangePasswordView ) );
        RegisterRoute( typeof( HabitDetailView ) );
        RegisterRoute( typeof( StartupView ) );
        RegisterRoute( typeof( AppBenefitsView ) );
        RegisterRoute( typeof( TasksPageView ) );
    }

    private static void RegisterRoute( Type viewType )
    {
        string route = viewType
            .Name
            .Replace( oldValue: "View", newValue: "" )
            .ToLower( CultureInfo.GetCultureInfo( name: "en" ) );

        Routing.RegisterRoute( route, viewType );
    }

    private async void OnMenuFilterClicked(object sender, EventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            string filterName = menuItem.CommandParameter?.ToString();

            if (!string.IsNullOrEmpty(filterName))
            {
                Current.FlyoutIsPresented = false;
                await Current.GoToAsync($"//tasks?filter={filterName}");
            }
        }
    }
}