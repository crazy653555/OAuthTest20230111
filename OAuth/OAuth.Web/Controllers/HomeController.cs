using Microsoft.AspNetCore.Mvc;
using OAuth.Web.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OAuth.Line.Core.LineLogin;

namespace OAuth.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LineLoginConfig _lineLoginConfig;
        private readonly LineLoginService _lineLoginService;

        private string _lineLoginRedirectUri
        {
            get
            {
                var requestScheme = Request.Scheme;
                var requestHost = Request.Host;
                var requestPathBase = Request.PathBase;
                var returnPath = _lineLoginConfig.ReturnPath;

                return $"{requestScheme}://{requestHost}{requestPathBase}{returnPath}";
            }
        }

        public HomeController(
            ILogger<HomeController> logger,
            IOptions<LineLoginConfig> lineLoginConfigOptions,
            LineLoginService lineLoginService

        )
        {
            _logger = logger;
            _lineLoginConfig = lineLoginConfigOptions.Value;
            _lineLoginService = lineLoginService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LineLogin()
        {
            var state = "3393";

            // 轉到 Line Login 登入網址
            var lineLoginUrl = _lineLoginService.GenerateLineLoginUrl(_lineLoginConfig.ChannelId, UrlEncoder.Default.Encode(_lineLoginRedirectUri), state);

            return Redirect(lineLoginUrl);
        }


        /// <summary>
        /// Line Login 的 callback action
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task<IActionResult> LineLoginCallback([FromQuery(Name = "code")] string code,
            [FromQuery(Name = "state")] string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }

            // 驗證 state 簽章

            // 透過 code 取得 access token
            var accessToken = await _lineLoginService.GetAccessTokenAsync(code, _lineLoginConfig.ChannelId, _lineLoginConfig.ChannelSecret, _lineLoginRedirectUri);

            // 取得 id token 物件後，將相關資訊塞到 cookie 中
            if (TryParseIdToken(accessToken.IdToken, out var idToken))
            {
                //呼叫 _lineNotifyBindingService.UpdateLoginAsync 


                HttpContext.Response.Cookies.Append("AccessToken", accessToken.AccessToken);
                HttpContext.Response.Cookies.Append("ExpiresIn", accessToken.ExpiresIn.ToString());
                HttpContext.Response.Cookies.Append("IdToken", accessToken.IdToken);
                HttpContext.Response.Cookies.Append("RefreshToken", accessToken.RefreshToken);
                HttpContext.Response.Cookies.Append("Scope", accessToken.Scope);
                HttpContext.Response.Cookies.Append("TokenType", accessToken.TokenType);

                return RedirectToAction("Index");
            }

            return BadRequest();
        }

        /// <summary>
        /// 取得 JwtToken 的 payload 部分，型別為 IdToken
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <param name="idToken"></param>
        /// <returns></returns>
        public bool TryParseIdToken(string jwtToken, out IdToken idToken)
        {
            try
            {
                var payloadString = jwtToken.Split(".")[1];
                var result = JsonSerializer.Deserialize<IdToken>(Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(payloadString)));
                if (result is not null)
                {
                    idToken = result;
                    return true;
                }
            }
            catch { }

            idToken = null;

            return false;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}