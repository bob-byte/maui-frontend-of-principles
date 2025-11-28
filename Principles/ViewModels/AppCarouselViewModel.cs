using Principles.Models;

using SkiaSharp.Extended.UI.Controls;

namespace Principles.ViewModels;

public partial class AppCarouselViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollectionEx<AppFeature> m_appFeatures;

    public AppCarouselViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            UpdateAppFeatures();
        } );
        AppFeatures = new ObservableCollectionEx<AppFeature>();
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        await base.InitializeAsync( parameter );
        FillAppFeatures();
    }

    private void FillAppFeatures()
    {
        AppFeatures.Add( new()
        {
            Title = LocStrings.TransformAreasOfLifeTitle,
            Description = LocStrings.TransformAreasOfLifeDescription,
            Animation = SKLottieImageSource.FromFile( "transform_areas_carousel.json" ) as SKLottieImageSource
        } );
        AppFeatures.Add( new() 
        { 
            Title = LocStrings.ChatWithHelperTitle, 
            Description = LocStrings.ChatWithHelperDescription, 
            Animation = SKLottieImageSource.FromFile( "chat_ai_carousel.json" ) as SKLottieImageSource 
        } );
        AppFeatures.Add( new() 
        { 
            Title = LocStrings.GroupHabitsByGoalsTitle, 
            Description = LocStrings.GroupHabitsByGoalsDescription,
             Animation = SKLottieImageSource.FromFile( "group_habits_carousel.json" ) as SKLottieImageSource 
        } );
        AppFeatures.Add( new() 
        { 
            Title = LocStrings.GetRecommendationsByAITitle, 
            Description = LocStrings.GetRecommendationsByAIDescription, 
            Animation = SKLottieImageSource.FromFile( "get_recommendations_carousel.json" ) as SKLottieImageSource 
        } );
        AppFeatures.Add( new() 
        {
            Title = LocStrings.BecomeTruePersonalityTitle, 
            Description = LocStrings.BecomeTruePersonalityDescription, 
            Animation = SKLottieImageSource.FromFile( "become_personality_carousel.json" ) as SKLottieImageSource
        } );
    }

    private void UpdateAppFeatures()
    {
        AppFeatures.Clear();
        FillAppFeatures();
    }
}