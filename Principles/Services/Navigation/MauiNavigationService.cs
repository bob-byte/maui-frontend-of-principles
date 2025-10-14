using Microsoft.Extensions.Configuration;

using Principles.ViewModels;
using Principles.Views;

using System.Globalization;
using System.Reflection;
using System.Web;

namespace Principles.Core.Services;

public class MauiNavigationService : INavigationService
{
    private readonly IConfiguration m_config;
    private readonly ISettingsService m_settingsService;

    public MauiNavigationService(IConfiguration config, IUrlBuilder urlBuilder, ISettingsService settingsService)
    {
        m_config = config;
        UrlBuilder = urlBuilder;
        m_settingsService = settingsService;
    }

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace( m_settingsService.AuthAccessToken );

    public IUrlBuilder UrlBuilder { get; }

    public Task GoToInitialViewAsync()
    {
        return IsLoggedIn ?
            NavigateToMainAsync<ProgressOfHabitsViewModel>() :
            NavigateToAsync<StartupViewModel>( isAbsoluteRoute: true );
    }

    public async Task NavigateToMainAsync<TViewModel>() where TViewModel : BaseViewModel
    {
        await InternalNavigateToAsync(typeof(TViewModel), routeParameters: null, isMainRoute: true, isAbsoluteRoute: false);
    }

    public Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel
    {
        return NavigateToAsync<TViewModel>( isAbsoluteRoute: false );
    }

    public async Task NavigateToAsync<TViewModel>( bool isAbsoluteRoute ) where TViewModel : BaseViewModel
    {
        await InternalNavigateToAsync(typeof(TViewModel), routeParameters: null, isMainRoute: false, isAbsoluteRoute );
    }

    public Task NavigateToAsync<TViewModel>( IDictionary<string, object> routeParameters ) where TViewModel : BaseViewModel
    {
        return NavigateToAsync<TViewModel>( isAbsoluteRoute: false, routeParameters );
    }

    public Task NavigateToAsync<TViewModel>( bool isAbsoluteRoute, IDictionary<string, object> routeParameters ) where TViewModel : BaseViewModel
    {
        return InternalNavigateToAsync( typeof( TViewModel ), routeParameters, isMainRoute: false, isAbsoluteRoute );
    }

    public Task NavigateToAsync<TViewModel>( long? id ) where TViewModel : BaseViewModel
    {
        return NavigateToAsync<TViewModel>( isAbsoluteRoute: false, id );
    }

    public Task NavigateToAsync<TViewModel>( bool isAbsoluteRoute, long? id ) where TViewModel : BaseViewModel
    {
        Dictionary<string, object> parameters = null;
        if( id != null )
        {
            parameters = new();
            parameters.Add( key: "Id", value: id );
        }

        return InternalNavigateToAsync( typeof( TViewModel ), parameters, isMainRoute: false, isAbsoluteRoute );
    }

    public Task GoBackAsync()
    {
        return Shell.Current.GoToAsync(state: "..", animate: true);
    }

    public Task GoBackAsync(IDictionary<string, object> routeParameters)
    {
        return Shell.Current.GoToAsync(state: "..", animate: true, routeParameters);
    }

    private static Task InternalNavigateToAsync( Type viewModelType, IDictionary<string, object> routeParameters, bool isMainRoute, bool isAbsoluteRoute )
    {
        string route = viewModelType.Name.
            Replace( oldValue: "ViewModel", newValue: "" ).
            ToLower( CultureInfo.GetCultureInfo( name: "en" ) );

        string absolutePrefix;
        if (isMainRoute)
        {
            absolutePrefix = "//main/";
        }
        else
        {
            absolutePrefix = isAbsoluteRoute ? "///" : "";
        }
        route = $"{absolutePrefix}{route}";

        ShellNavigationState shellNavigation = new( route );

        bool doAnimation = true;
        Task navigateTask = routeParameters?.Count >= 1 ?
            Shell.Current.GoToAsync( shellNavigation, doAnimation, routeParameters ) : 
            Shell.Current.GoToAsync( shellNavigation, doAnimation );
        return navigateTask;
    }
}