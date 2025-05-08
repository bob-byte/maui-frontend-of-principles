namespace Principles.Core.Services;
public interface INetworkService
{
    public Task CheckInitialConnectionAsync();
    private void Connectivity_ConnectivityChanged( object? sender, ConnectivityChangedEventArgs e ) { } 
}
