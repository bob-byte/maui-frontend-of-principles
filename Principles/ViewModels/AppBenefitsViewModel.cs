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
            ReloadAppBenefits();
        } );

        AppBenefits = [];
        ReloadAppBenefits();
    }

    public override async Task InitializePageAsync( IDictionary<string, object> query )
    {
        await base.InitializePageAsync( query );
        ReloadAppBenefits();
    }

    public override async Task HandleDisappearingOfPageAsync( object? parameter = null )
    {
        await base.HandleDisappearingOfPageAsync( parameter );

        //delay to not show use clearing
        await Task.Delay( 1000 );
        AppBenefits.Clear();
    }

    public async Task NavigateToNextViewAsync()
    {
        if (IsLoggedIn)
        {
            await Navigation.GoBackAsync();
        }
        else
        {
            await Navigation.NavigateToAsync<StartupViewModel>( isAbsoluteRoute: true );
        }
    }

    private List<AppBenefit> GetAppBenefits()
    {
        List<AppBenefit> result =
        [ 
            new()
            {
                Title = LocStrings.TransformAreasOfLifeTitle,
                Description = LocStrings.TransformAreasOfLifeDescription,
                Animation = SKLottieImageSource.FromFile( "transform_areas_carousel.json" ) as SKLottieImageSource
            },
            new()
            {
                Title = LocStrings.ChatWithHelperTitle,
                Description = LocStrings.ChatWithHelperDescription,
                Animation = SKLottieImageSource.FromFile( "chat_ai_carousel.json" ) as SKLottieImageSource
            },
            new()
            {
                Title = LocStrings.GroupHabitsByGoalsTitle,
                Description = LocStrings.GroupHabitsByGoalsDescription,
                 Animation = SKLottieImageSource.FromFile( "group_habits_carousel.json" ) as SKLottieImageSource
            },
            new()
            {
                Title = LocStrings.GetRecommendationsByAITitle,
                Description = LocStrings.GetRecommendationsByAIDescription,
                Animation = SKLottieImageSource.FromFile( "get_recommendations_carousel.json" ) as SKLottieImageSource
            },
            new()
            {
                Title = LocStrings.BecomeTruePersonalityTitle,
                Description = LocStrings.BecomeTruePersonalityDescription,
                Animation = SKLottieImageSource.FromFile( "become_personality_carousel.json" ) as SKLottieImageSource
            }
        ];
        return result;
    }

    public void ReloadAppBenefits()
    {
        AppBenefits.Reload( GetAppBenefits() );
    }
}