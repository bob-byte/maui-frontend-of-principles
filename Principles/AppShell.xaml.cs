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

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null)
        {
            m_settingsService.GetAuthAccessTokenAsync().GetAwaiter().GetResult();
            
            if (VersionTracking.IsFirstLaunchForCurrentVersion)
            {
                m_navigationService.NavigateToAsync<AppBenefitsViewModel>( isAbsoluteRoute: true );
            }
            else
            {
                m_navigationService.GoToInitialViewAsync();
            }
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
    }

    private static void RegisterRoute( Type viewType )
    {
        string route = viewType
            .Name
            .Replace( oldValue: "View", newValue: "" )
            .ToLower( CultureInfo.GetCultureInfo( name: "en" ) );

        Routing.RegisterRoute( route, viewType );
    }
}