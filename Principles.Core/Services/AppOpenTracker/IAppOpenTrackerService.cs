namespace Principles.Core.Services;

public interface IAppOpenTrackerService
{
    void TrackAppOpen();
    DateTime? GetLastOpenDate();
    DateTime? GetLastMissedDate();
    int DaysSinceLastOpen();
}