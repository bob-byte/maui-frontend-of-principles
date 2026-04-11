using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace Principles;

internal sealed class LookupResult
{
    [JsonPropertyName("currentVersionReleaseDate")]
    public DateTime CurrentVersionReleaseDate { get; set; }

    [JsonPropertyName("releaseNotes")]
    public string ReleaseNotes { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("trackViewUrl")]
    public string TrackViewUrl { get; set; } = string.Empty;
}