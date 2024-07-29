namespace SET.MAUI.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollectionEx<AppFeature> m_appFeatures;

    public StartupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
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
    public Task ContinueWithGoogleAsync()
    {
        return DialogService.ShowErrorAsync( LocStrings.NotYetImplemented );
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