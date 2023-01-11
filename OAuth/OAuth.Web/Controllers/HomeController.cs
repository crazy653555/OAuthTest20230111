using Microsoft.AspNetCore.Mvc;
using OAuth.Web.Models;
using System.Diagnostics;
using System.Text.Encodings.Web;
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

            throw new Exception();
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