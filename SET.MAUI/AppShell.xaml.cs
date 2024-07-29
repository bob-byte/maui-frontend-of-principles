namespace SET.MAUI;

public partial class AppShell : Shell
{
    private readonly INavigationService m_navigationService;

    public AppShell( INavigationService navigationService )
    {
        m_navigationService = navigationService;

        InitRouting();
        InitializeComponent();
    }

    protected override async void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        
        if (Handler is not null)
        {
            await m_navigationService.GoToInitialViewAsync();
        }
    }

    private static void InitRouting()
    {
        RegisterRoute( typeof( SignupView ) );
        RegisterRoute( typeof( LoginView ) );
        RegisterRoute( typeof( EditHabitView ) );
        RegisterRoute( typeof( SettingsView ) );
        RegisterRoute( typeof( UserAgreementView ) );
        RegisterRoute( typeof( PrivacyPolicyView ) );
        RegisterRoute( typeof( ForgetPasswordView ) );
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