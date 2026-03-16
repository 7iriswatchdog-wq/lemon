using AML.Web.CustomFilters;
using AML.Web.Helper;
using Microsoft.AspNetCore.Mvc;

namespace AML.Web.Controllers.Governance
{
    [SessionAuthorize]
    public class GovernanceController : Controller
    {
        private IHttpClientHandler _clientHandler;

        public GovernanceController(IHttpClientHandler clientHandler)
        {
            _clientHandler = clientHandler;
        }

        public IActionResult Index()
        {
            return RedirectToAction("SlaMonitoring");
        }

        public IActionResult SlaMonitoring()
        {
            return View();
        }

        public IActionResult ConfigAudit()
        {
            return View();
        }

        public IActionResult ComplianceCalendar()
        {
            return View();
        }

        public IActionResult AlertCenter()
        {
            return View();
        }
    }
}
