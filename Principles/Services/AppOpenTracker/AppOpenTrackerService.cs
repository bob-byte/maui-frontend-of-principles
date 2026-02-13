using Microsoft.Maui.Storage;
using System.Globalization;

namespace Principles.Services;

public class AppOpenTrackerService : IAppOpenTrackerService
{
    private const string LAST_OPEN_KEY = "LastOpenDate";
    private const string LAST_MISSED_KEY = "LastMissedDate";
    private const string DATE_FORMAT = "yyyy-MM-dd";

    public void TrackAppOpen()
    {
        DateTime today = DateTime.Now.Date;
        DateTime? lastOpen = GetLastOpenDate();

        if (lastOpen.HasValue)
        {
            int daysSince = (today - lastOpen.Value).Days;

            if (daysSince > 1)
            {
                DateTime lastMissedDate = today.AddDays(-1);
                Preferences.Set(LAST_MISSED_KEY, lastMissedDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture));
            }
        }

        Preferences.Set(LAST_OPEN_KEY, today.ToString(DATE_FORMAT, CultureInfo.InvariantCulture));
    }

    public DateTime? GetLastOpenDate()
    {
        if (!Preferences.ContainsKey(LAST_OPEN_KEY))
        {
            return null;
        }

        string stored = Preferences.Get(LAST_OPEN_KEY, string.Empty);
        if (DateTime.TryParseExact(stored, DATE_FORMAT, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime parsed))
        {
            return parsed;
        }

        return null;
    }

    public DateTime? GetLastMissedDate()
    {
        if (!Preferences.ContainsKey(LAST_MISSED_KEY))
        {
            return null;
        }

        string stored = Preferences.Get(LAST_MISSED_KEY, string.Empty);
        if (DateTime.TryParseExact(stored, DATE_FORMAT, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime parsed))
        {
            return parsed;
        }

        return null;
    }

    public int DaysSinceLastOpen()
    {
        DateTime? lastOpen = GetLastOpenDate();
        int result = lastOpen == null ? 0 : (DateTime.Now.Date - lastOpen.Value.Date).Days;
        return result;
    }
}
