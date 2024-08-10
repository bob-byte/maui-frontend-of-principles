using System.Text.Json.Serialization;

namespace SET.Core.Models;

public class GetGoogleTokenInfoResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName( "id_token" )]
    public string IdToken { get; set; }
}
