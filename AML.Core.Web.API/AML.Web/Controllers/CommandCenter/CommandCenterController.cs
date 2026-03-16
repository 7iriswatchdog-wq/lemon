using AML.Web.CustomFilters;
using AML.Web.Helper;
using Microsoft.AspNetCore.Mvc;

namespace AML.Web.Controllers.CommandCenter
{
    [SessionAuthorize]
    public class CommandCenterController : Controller
    {
        private IHttpClientHandler _clientHandler;

        public CommandCenterController(IHttpClientHandler clientHandler)
        {
            _clientHandler = clientHandler;
        }

        public IActionResult Index()
        {
            return RedirectToAction("ClientCaseSummary");
        }

        public IActionResult ClientCaseSummary()
        {
            return View();
        }

        public IActionResult ClientOnboarding()
        {
            return View();
        }

        public IActionResult UserActivity()
        {
            return View();
        }

        public IActionResult ScreeningVolume()
        {
            return View();
        }

        public IActionResult AccessRightsAudit()
        {
            return View();
        }

        public IActionResult PlatformHealth()
        {
            return View();
        }
    }
}
