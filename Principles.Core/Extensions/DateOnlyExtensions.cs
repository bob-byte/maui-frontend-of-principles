using System;

namespace Principles.Core.Extensions;

public static class DateOnlyExtensions
{
    public static int DaysUntil(this DateOnly it, DateOnly other)
    {
        var zeroTime = TimeOnly.FromTimeSpan( TimeSpan.Zero );
        DateTime itAsDateTime = it.ToDateTime(zeroTime);
        DateTime otherAsDateTime = other.ToDateTime(zeroTime);

        int result = (otherAsDateTime - itAsDateTime).Days;
        return result;
    }
}

