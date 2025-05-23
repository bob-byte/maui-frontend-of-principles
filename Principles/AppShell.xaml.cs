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
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null)
        {
            //we don't await execution because otherwise helper view will be shown for 1 second
            m_settingsService.GetAuthAccessTokenAsync().GetAwaiter().GetResult();
            m_navigationService.GoToInitialViewAsync();
        }
    }

    private static void InitRouting()
    {
        RegisterRoute( typeof( SignupView ) );
        RegisterRoute( typeof( LoginView ) );
        RegisterRoute( typeof( EditHabitView ) );
        RegisterRoute( typeof( SettingsView ) );
        RegisterRoute( typeof( ForgetPasswordView ) );
        RegisterRoute( typeof( HabitDetailView ) );
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