using Microsoft.AspNetCore.Mvc;
using OAuth.Web.Models;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OAuth.Line.Core.LineLogin;
using OAuth.Line.Core.Jwt;
using System.Security.Claims;

namespace OAuth.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LineLoginConfig _lineLoginConfig;
        private readonly LineLoginService _lineLoginService;
        private readonly JwtService _jwtService;
        private readonly JwtConfig _jwtConfig;

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
            IOptions<JwtConfig> jwtConfigOptions,
            LineLoginService lineLoginService,
            JwtService jwtService
        )
        {
            _logger = logger;
            _lineLoginConfig = lineLoginConfigOptions.Value;
            _jwtConfig = jwtConfigOptions.Value;
            _lineLoginService = lineLoginService;
            _jwtService = jwtService;
        }

        public async Task<IActionResult> Index()
        {
            // 如果有登入過，目前會把資料存在 cookie 中
            var accessToken = HttpContext.Request.Cookies["AccessToken"];
            var idToken = HttpContext.Request.Cookies["IdToken"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(idToken))
            {
                return View();
            }

            // 驗證 LineLogin 的 access token
            LineLoginVerifyAccessTokenResult accessTokenVerifyResult = null;
            try
            {
                accessTokenVerifyResult = await _lineLoginService.VerifyAccessTokenAsync(accessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View();
            }

            // 驗證 LineLogin 的 id token
            LineLoginVerifyIdTokenResult idTokenVerifyResult = null;
            try
            {
                idTokenVerifyResult = await _lineLoginService.VerifyIdTokenAsync(idToken, _lineLoginConfig.ChannelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View();
            }

            // 取得目前的 user profile
            var user = await _lineLoginService.GetUserProfileAsync(accessToken);

            // 檢查是否已綁定 Line Notify


            ViewBag.User = user;
            //ViewBag.IsLineNotifyBinded = isLineNotifyBinded;

            return View();
        }

        public IActionResult LineLogin()
        {
            // 產生一個包含簽章的 jtw token 來當作 state，以避免 CSRF 攻擊
            var state = _jwtService.GenerateToken(_jwtConfig.SignKey, _jwtConfig.Issuer, new Claim[] { }, DateTime.UtcNow.AddMinutes(10));

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
        public async Task<IActionResult> LineLoginCallback([FromQuery(Name = "code")] string code, [FromQuery(Name = "state")] string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }

            // 驗證 state 簽章
            var stateValidateResult = _jwtService.ValidateToken(state, _jwtConfig.Issuer, _jwtConfig.SignKey, out var exception);
            if (stateValidateResult is null)
            {
                _logger.LogError(exception.Message);
                return BadRequest();
            }

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

        /// <summary>
        /// 登出 Line Login
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> LineLogoutAsync()
        {
            var accessToken = HttpContext.Request.Cookies["AccessToken"];
            var idToken = HttpContext.Request.Cookies["IdToken"];

            try
            {
                // 撤銷 Line Login 的 access token
                await _lineLoginService.RevokeAccessTokenAsync(accessToken, _lineLoginConfig.ChannelId, _lineLoginConfig.ChannelSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            // 刪除 cookie 資料
            HttpContext.Response.Cookies.Delete("AccessToken");
            HttpContext.Response.Cookies.Delete("ExpiresIn");
            HttpContext.Response.Cookies.Delete("IdToken");
            HttpContext.Response.Cookies.Delete("RefreshToken");
            HttpContext.Response.Cookies.Delete("Scope");
            HttpContext.Response.Cookies.Delete("TokenType");

            return RedirectToAction("Index");
        }
    }
}