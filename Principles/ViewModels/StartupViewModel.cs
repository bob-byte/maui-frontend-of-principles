using Principles.Core.Models;

using System.Net;

namespace Principles.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
    private readonly IGoogleAuthService m_googleAuthService;
    private readonly IAppleAuthService m_appleAuthService;
    private readonly IReminderService m_reminderService;

    [ObservableProperty]
    private ObservableCollectionEx<AppFeature> m_appFeatures;

    public StartupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_googleAuthService = serviceProvider.GetRequiredService<IGoogleAuthService>();
        m_reminderService = serviceProvider.GetRequiredService<IReminderService>();
        m_appleAuthService = serviceProvider.GetRequiredService<IAppleAuthService>();

        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            UpdateAppFeatures();
        } );

        AppFeatures = InitializeAppFeatures();
    }

    private ObservableCollectionEx<AppFeature> InitializeAppFeatures()
    {
        return new ObservableCollectionEx<AppFeature>
        {
            new() { Title = LocStrings.TransformAreasOfLifeTitle, Description = LocStrings.TransformAreasOfLifeDescription },
            new() { Title = LocStrings.ChatWithHelperTitle, Description = LocStrings.ChatWithHelperDescription },
            new() { Title = LocStrings.GroupHabitsByGoalsTitle, Description = LocStrings.GroupHabitsByGoalsDescription },
            new() { Title = LocStrings.GetRecommendationsByAITitle, Description = LocStrings.GetRecommendationsByAIDescription },
            new() { Title = LocStrings.BecomeTruePersonalityTitle, Description = LocStrings.BecomeTruePersonalityDescription }
        };
    }

    private void UpdateAppFeatures()
    {
        AppFeatures.Clear();
        ObservableCollectionEx<AppFeature> updatedFeatures = InitializeAppFeatures();
        foreach (AppFeature feature in updatedFeatures)
        {
            AppFeatures.Add( feature );
        }
    }

    [RelayCommand]
    public async Task ContinueWithGoogleAsync()
    {
        try
        {
            await m_googleAuthService.AuthorizeAsync();
        }
        catch (Exception ex)
        {
            await HandleExceptionWhenGoogleAuthAsync( ex ).DefaultConfigureAwait();
            return;
        }

        await ExecuteWithRetryAsync( m_reminderService.TryToRecoverAllUserRemindersAsync );

        try
        {
            await Navigation.GoToInitialViewAsync( SyncTrigger.AuthCompleted );
        }
        catch (Exception ex)
        {
            LoggingService.LogError( ex, ex.Message );
        }
    }

    private async Task HandleExceptionWhenGoogleAuthAsync( Exception ex )
    {
        string? errorMsg = null;

        if (ex is TimeoutException || ex.InnerException is TimeoutException)
        {
            errorMsg = LocStrings.OperationTimeoutMessage;
        }
        else if (ConnectivityExceptionClassifier.IsConnectivityFailure( ex ))
        {
            errorMsg = LocStrings.NoInternetConnection;
        }
        else if (ex is not TaskCanceledException)
        {
            errorMsg = LocStrings.SomethingWentWrong;
            LoggingService.LogError( ex, ex.Message );
        }

        if (!string.IsNullOrWhiteSpace( errorMsg ))
        {
            await DialogService.ShowErrorAsync( errorMsg );
        }
    }

    [RelayCommand]
    public async Task ContinueWithAppleAsync()
    {
        try
        {
            await m_appleAuthService.AuthorizeAsync();
        }
        catch (Exception ex)
        {
            await HandleExceptionWhenAppleAuthAsync( ex ).DefaultConfigureAwait();
            return;
        }

        await ExecuteWithRetryAsync( m_reminderService.TryToRecoverAllUserRemindersAsync );

        try
        {
            await Navigation.GoToInitialViewAsync( SyncTrigger.AuthCompleted );
        }
        catch (Exception ex)
        {
            LoggingService.LogError( ex, ex.Message );
        }
    }

    private async Task HandleExceptionWhenAppleAuthAsync( Exception ex )
    {
        bool isCancelledByUser = ex.Message.Contains( "1001" );
        if (isCancelledByUser)
        {
            return;
        }

        string? errorMsg = null;

        if (ex is TimeoutException || ex.InnerException is TimeoutException)
        {
            errorMsg = LocStrings.OperationTimeoutMessage;
        }
        else if (ConnectivityExceptionClassifier.IsConnectivityFailure( ex ))
        {
            errorMsg = LocStrings.NoInternetConnection;
        }
        else if (ex is not TaskCanceledException)
        {
            errorMsg = LocStrings.SomethingWentWrong;
            LoggingService.LogError( ex, ex.Message );
        }

        if (!string.IsNullOrWhiteSpace( errorMsg ))
        {
            await DialogService.ShowErrorAsync( errorMsg );
        }
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
