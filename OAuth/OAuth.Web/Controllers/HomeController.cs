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
        private readonly LineLoginConfig _lineLoginConfigOptions;
        private readonly LineLoginService _lineLoginService;

        private string _lineLoginRedirectUri
        {
            get
            {
                var requestScheme = Request.Scheme;
                var requestHost = Request.Host;
                var requestPathBase = Request.PathBase;
                var returnPath = _lineLoginConfigOptions.ReturnPath;

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
            _lineLoginConfigOptions = lineLoginConfigOptions.Value;
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
            var lineLoginUrl = _lineLoginService.GenerateLineLoginUrl(_lineLoginConfigOptions.ChannelId, UrlEncoder.Default.Encode(_lineLoginRedirectUri), state);

            //return Redirect(lineLoginUrl);

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