using System;
namespace SET.Core.Models;

public static class ProgressValue
{
    public const int UNKNOWN = -1;
    public const int NO = 0;
    public const int YES_AUTO = 1;
    public const int YES_MANUAL = 2;
    public const int SKIP = 3;

    public static int NextToggled( int value, bool isSkipEnabled = false, bool areQuestionMarksEnabled = false )
    {
        int result = value switch
        {
            YES_AUTO => YES_MANUAL,
            YES_MANUAL => isSkipEnabled ? SKIP : NO,
            SKIP => NO,
            NO => areQuestionMarksEnabled ? UNKNOWN : YES_MANUAL,
            UNKNOWN => YES_MANUAL,
            _ => YES_MANUAL
        };

        return result;
    }
}

