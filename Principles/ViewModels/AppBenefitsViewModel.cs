using Principles.Models;

using SkiaSharp.Extended.UI.Controls;

namespace Principles.ViewModels;

public partial class AppBenefitsViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollectionEx<AppBenefit> m_appBenefits;

    public AppBenefitsViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            UpdateAppBenefits();
        } );
        AppBenefits = [];
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        await base.InitializeAsync( parameter );
        AppBenefits.Reload(GetAppBenefits());
    }

    public override async Task OnDisappearingAsync( object? parameter = null )
    {
        await base.OnDisappearingAsync( parameter );
        await Task.Delay(1000);
        AppBenefits.Clear();
    }

    private IEnumerable<AppBenefit> GetAppBenefits()
    {
        yield return new()
        {
            Title = LocStrings.TransformAreasOfLifeTitle,
            Description = LocStrings.TransformAreasOfLifeDescription,
            Animation = SKLottieImageSource.FromFile( "transform_areas_carousel.json" ) as SKLottieImageSource
        };
        yield return new() 
        { 
            Title = LocStrings.ChatWithHelperTitle, 
            Description = LocStrings.ChatWithHelperDescription, 
            Animation = SKLottieImageSource.FromFile( "chat_ai_carousel.json" ) as SKLottieImageSource 
        };
        yield return new() 
        { 
            Title = LocStrings.GroupHabitsByGoalsTitle, 
            Description = LocStrings.GroupHabitsByGoalsDescription,
             Animation = SKLottieImageSource.FromFile( "group_habits_carousel.json" ) as SKLottieImageSource 
        };
        yield return new() 
        { 
            Title = LocStrings.GetRecommendationsByAITitle, 
            Description = LocStrings.GetRecommendationsByAIDescription, 
            Animation = SKLottieImageSource.FromFile( "get_recommendations_carousel.json" ) as SKLottieImageSource 
        };
        yield return new() 
        {
            Title = LocStrings.BecomeTruePersonalityTitle, 
            Description = LocStrings.BecomeTruePersonalityDescription, 
            Animation = SKLottieImageSource.FromFile( "become_personality_carousel.json" ) as SKLottieImageSource
        };
    }

    private void UpdateAppBenefits()
    {
        AppBenefits.Clear();
        AppBenefits.AddRange(GetAppBenefits());
    }
}