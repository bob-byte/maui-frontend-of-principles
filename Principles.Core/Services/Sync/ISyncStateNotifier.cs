namespace Principles.Core.Services;

public interface ISyncStateNotifier
{
    void NotifyUserChanged( User user );
}
