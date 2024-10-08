using System.Text.Json.Serialization;

namespace Principles.Core.Models;

public class GetGoogleTokenInfoResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName( "id_token" )]
    public string IdToken { get; set; }
}
