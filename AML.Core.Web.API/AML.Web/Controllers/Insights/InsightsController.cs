using AML.Web.CustomFilters;
using AML.Web.Helper;
using Microsoft.AspNetCore.Mvc;

namespace AML.Web.Controllers.Insights
{
    [SessionAuthorize]
    public class InsightsController : Controller
    {
        private IHttpClientHandler _clientHandler;

        public InsightsController(IHttpClientHandler clientHandler)
        {
            _clientHandler = clientHandler;
        }

        public IActionResult Index()
        {
            return RedirectToAction("ScreeningCaseStatus");
        }

        public IActionResult ScreeningCaseStatus()
        {
            return View();
        }

        public IActionResult MatchAnalysis()
        {
            return View();
        }

        public IActionResult PepSanctions()
        {
            return View();
        }

        public IActionResult RiskClassification()
        {
            return View();
        }

        public IActionResult CorporateOwnership()
        {
            return View();
        }

        public IActionResult TransactionMonitoring()
        {
            return View();
        }

        public IActionResult SuspiciousTransaction()
        {
            return View();
        }

        public IActionResult InternalWatchlist()
        {
            return View();
        }

        public IActionResult CaseAuditTrail()
        {
            return View();
        }

        public IActionResult DocumentCompliance()
        {
            return View();
        }
    }
}
