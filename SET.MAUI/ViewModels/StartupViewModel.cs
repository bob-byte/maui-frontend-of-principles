namespace SET.MAUI.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollectionEx<AppFeatures> m_appFeatures;
    public StartupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        AppFeatures = new ObservableCollectionEx<AppFeatures>
        {
            new AppFeatures {  Title = LocStrings.TransformAreasOfLifeTitle, Description =  LocStrings.TransformAreasOfLifeDescription},
            new AppFeatures {  Title = LocStrings.GroupHabitsByGoalsTitle, Description = LocStrings.GroupHabitsByGoalsDescription },
            new AppFeatures {  Title = LocStrings.GetRecommendationsByAITitle, Description = LocStrings.GetRecommendationsByAIDescription },
            new AppFeatures {  Title = LocStrings.ChatWithHelperTitle, Description = LocStrings.ChatWithHelperDescription },
            new AppFeatures {  Title = LocStrings.BecomeTruePersonalityTitle, Description = LocStrings.BecomeTruePersonalityDescription }
        };
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