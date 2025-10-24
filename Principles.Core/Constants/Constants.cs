using System;
using System.Text.Json.Serialization;

namespace Principles.Constants;

public static class Constants
{
    static Constants()
    {
        JsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    }

    public static JsonSerializerOptions JsonOptions { get; }
    
    public const int MAX_TAPS_TO_SHOW_ADS = 5;
}

