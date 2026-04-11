using System.Text.Json.Serialization;

namespace Principles;

internal sealed class LookupResponse
{
    [JsonPropertyName("resultCount")]
    public int ResultCount {get;set;}

    [JsonPropertyName("results")]
    public List<LookupResult> Results {get;set;} = [];
}