using Principles.Models;

using SkiaSharp.Extended.UI.Controls;

namespace Principles.ViewModels;

public partial class AppCarouselViewModel : BaseViewModel
{
    private readonly IGoogleAuthService m_googleAuthService;
    private readonly IAppleAuthService m_appleAuthService;
    private readonly IReminderService m_reminderService;

    [ObservableProperty]
    private ObservableCollectionEx<AppFeature> m_appFeatures;

    public AppCarouselViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_googleAuthService = serviceProvider.GetRequiredService<IGoogleAuthService>();
        m_reminderService = serviceProvider.GetRequiredService<IReminderService>();
        m_appleAuthService = serviceProvider.GetRequiredService<IAppleAuthService>();

        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            UpdateAppFeatures();
        } );

        m_appFeatures = InitializeAppFeatures();
    }

    private ObservableCollectionEx<AppFeature> InitializeAppFeatures()
    {
        return
    [
        new() { Title = LocStrings.TransformAreasOfLifeTitle, Description = LocStrings.TransformAreasOfLifeDescription, Animation = SKLottieImageSource.FromFile("transform_areas_carousel.json") as SKLottieImageSource },
        new() { Title = LocStrings.ChatWithHelperTitle, Description = LocStrings.ChatWithHelperDescription , Animation = SKLottieImageSource.FromFile("chat_ai_carousel.json") as SKLottieImageSource },
        new() { Title = LocStrings.GroupHabitsByGoalsTitle, Description = LocStrings.GroupHabitsByGoalsDescription, Animation = SKLottieImageSource.FromFile("group_habits_carousel.json") as SKLottieImageSource },
        new() { Title = LocStrings.GetRecommendationsByAITitle, Description = LocStrings.GetRecommendationsByAIDescription, Animation = SKLottieImageSource.FromFile("get_recommendations_carousel.json") as SKLottieImageSource},
        new() { Title = LocStrings.BecomeTruePersonalityTitle, Description = LocStrings.BecomeTruePersonalityDescription, Animation = SKLottieImageSource.FromFile("become_personality_carousel.json") as SKLottieImageSource}
    ];
    }

    private void UpdateAppFeatures()
    {
        m_appFeatures.Clear();
        ObservableCollectionEx<AppFeature> updatedFeatures = InitializeAppFeatures();
        foreach (AppFeature feature in updatedFeatures)
        {
            m_appFeatures.Add( feature );
        }
    }
}