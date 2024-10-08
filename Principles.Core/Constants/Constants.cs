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
}

