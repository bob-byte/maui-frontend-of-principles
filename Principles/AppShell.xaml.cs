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
            InitializeApp();
        }
    }

    private void InitializeApp() 
    {
        ISettingsService settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
        INavigationService navigationService = ServiceLocator.Current!.GetRequiredService<INavigationService>();

        //we don't await execution because otherwise helper view will be shown for 1 second
        settingsService.GetAuthAccessTokenAsync().GetAwaiter().GetResult();

        bool isLoggedIn = !string.IsNullOrWhiteSpace( settingsService.AuthAccessToken );
        if (isLoggedIn)
        {
            navigationService.NavigateToMainAsync<ProgressOfHabitsViewModel>();
        }
        else
        {
            navigationService
                .NavigateToAsync<StartupViewModel>( isAbsoluteRoute: true )
                .ContinueWith( _ => navigationService.NavigateToAsync<AppBenefitsViewModel>() );
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
