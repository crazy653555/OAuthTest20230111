using Microsoft.AspNetCore.Mvc;
using OAuth.Web.Models;
using System.Diagnostics;
using OAuth.Line.Core.LineLogin;

namespace OAuth.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LineLoginService _lineLoginService;
        private readonly LineLoginConfig _lineLoginConfig;

        //private string _lineLoginRedirectUri
        //{
        //    get { return $"{Request.Scheme}://{Request.Host}{Request.PathBase}{_lineLoginConfig.ReturnPath}"; }
        //}

        public HomeController(
            ILogger<HomeController> logger,
            LineLoginService lineLoginService,
            LineLoginConfig lineLoginConfig
            )
        {
            _logger = logger;
            _lineLoginService = lineLoginService;
            _lineLoginConfig = lineLoginConfig;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LineLogin()
        {
            var state = "3393";

            // 轉到 Line Login 登入網址
            //var lineLoginUrl = _lineLoginService.GenerateLineLoginUrl(_lineLoginConfig.ChannelId, UrlEncoder.Default.Encode(_lineLoginRedirectUri), state);

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