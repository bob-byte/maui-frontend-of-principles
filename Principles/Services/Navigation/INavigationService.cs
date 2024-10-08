using Principles.ViewModels;

namespace Principles.Services
{
    public interface INavigationService
    {
        IUrlBuilder UrlBuilder { get; }

        Task NavigateToMainAsync<TViewModel>() where TViewModel : BaseViewModel;
        Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel;
        Task NavigateToAsync<TViewModel>( long? id ) where TViewModel : BaseViewModel;
        Task NavigateToAsync<TViewModel>( IDictionary<string, object> routeParameters ) where TViewModel : BaseViewModel;
        Task NavigateToAsync<TViewModel>( bool isAbsoluteRoute ) where TViewModel : BaseViewModel;
        Task NavigateToAsync<TViewModel>( bool isAbsoluteRoute, long? id ) where TViewModel : BaseViewModel;
        Task NavigateToAsync<TViewModel>(bool isAbsoluteRoute, IDictionary<string, object> routeParameters) where TViewModel : BaseViewModel;

        Task GoBackAsync();
        Task GoToInitialViewAsync();
    }
}