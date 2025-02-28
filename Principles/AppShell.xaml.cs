namespace Principles;

public partial class AppShell : Shell
{
    private readonly INavigationService m_navigationService;
    private readonly ISettingsService m_settingsService;
    public WeakReferenceMessenger ReferenceMessenger { get; }

    public AppShell( IServiceProvider serviceProvider )
    {
        BindingContext = this;
        m_navigationService = serviceProvider.GetRequiredService<INavigationService>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        InitRouting();
        InitializeComponent();
        LoadLocalizationData();
        ReferenceMessenger = WeakReferenceMessenger.Default;

        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            LoadLocalizationData();
        } );
    }

    private void LoadLocalizationData()
    {
        SC_Helper.Title = LocStrings.Helper;
        SC_Progress.Title = LocStrings.Progress;
        SC_Profile.Title = LocStrings.Profile;
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