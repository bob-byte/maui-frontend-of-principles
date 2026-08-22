using Principles.Core.Models;

namespace Principles.ViewModels;

public class SyncGateViewModel : BaseViewModel
{
    private readonly ISyncOrchestrator m_syncOrchestrator;

    private SyncTrigger m_trigger = SyncTrigger.Startup;

    public SyncGateViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_syncOrchestrator = serviceProvider.GetRequiredService<ISyncOrchestrator>();
        Title = LocStrings.Loading;
    }

    public override void ApplyQueryAttributes( IDictionary<string, object> query )
    {
        base.ApplyQueryAttributes( query );

        if (!query.TryGetValue( nameof( SyncTrigger ), out object? triggerValue ))
        {
            m_trigger = SyncTrigger.Startup;
            return;
        }

        if (triggerValue is SyncTrigger trigger)
        {
            m_trigger = trigger;
            return;
        }

        if (triggerValue is string triggerText && Enum.TryParse( triggerText, ignoreCase: true, out SyncTrigger parsedTrigger ))
        {
            m_trigger = parsedTrigger;
            return;
        }

        m_trigger = SyncTrigger.Startup;
    }

    public override async Task InitializePageAsync( IDictionary<string, object> query )
    {
        string authToken = SettingsService.AuthAccessToken ?? await SettingsService.GetAuthAccessTokenAsync();
        if (string.IsNullOrWhiteSpace( authToken ))
        {
            await Navigation.NavigateToAsync<StartupViewModel>( isAbsoluteRoute: true );
            return;
        }

        SyncRunResult syncResult = await m_syncOrchestrator.RunAsync( m_trigger );
        if (syncResult.Status == SyncRunStatus.FailedAuthentication)
        {
            await LogoutAsync();
            return;
        }

        await Navigation.NavigateToMainAsync<ProgressOfHabitsViewModel>();
    }
}
