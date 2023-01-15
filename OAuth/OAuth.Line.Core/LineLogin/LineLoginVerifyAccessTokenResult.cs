using System.Text.Json.Serialization;

namespace OAuth.Line.Core.LineLogin;

public class LineLoginVerifyAccessTokenResult
{
    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("client_id")]
    public string CliendId { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}