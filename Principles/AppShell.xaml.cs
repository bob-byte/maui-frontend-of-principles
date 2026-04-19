namespace Principles;

public partial class AppShell : Shell
{
    public WeakReferenceMessenger ReferenceMessenger { get; }
    public LocalizationResourceManager LocManager => LocalizationResourceManager.Instance;

    public AppShell()
    {
        BindingContext = this;
        InitRouting();
        InitializeComponent();
        ReferenceMessenger = WeakReferenceMessenger.Default;
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
