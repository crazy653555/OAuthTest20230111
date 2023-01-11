namespace OAuth.Line.Core
{
    public class LineLoginService
    {
        public string GenerateLineLoginUrl(string clientId, string redirectUri, string state)
        {
            var url = $"https://access.line.me/oauth2/v2.1/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state={state}&scope=openid%20profile";
            return url;
        }
    }
}