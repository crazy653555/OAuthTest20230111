namespace OAuth.Line.Core.LineLogin;

public class LineLoginConfig
{
    /// <summary>
    /// OAuth Server Url
    /// </summary>
    public string OAuthEndpoint { get; set; }

    /// <summary>
    /// AccessToken Server Url
    /// </summary>
    public string AccessTokenEndpoint { get; set; }

    /// <summary>
    /// 帳號
    /// </summary>
    public string ChannelId { get; set; }

    /// <summary>
    /// 密碼
    /// </summary>
    public string ChannelSecret { get; set; }

    /// <summary>
    /// Redirect Rul
    /// </summary>
    public string ReturnPath { get; set; }

}