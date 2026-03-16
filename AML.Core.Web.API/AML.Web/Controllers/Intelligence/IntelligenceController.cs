using AML.Web.CustomFilters;
using AML.Web.Helper;
using Microsoft.AspNetCore.Mvc;

namespace AML.Web.Controllers.Intelligence
{
    [SessionAuthorize]
    public class IntelligenceController : Controller
    {
        private IHttpClientHandler _clientHandler;

        public IntelligenceController(IHttpClientHandler clientHandler)
        {
            _clientHandler = clientHandler;
        }

        public IActionResult Index()
        {
            return RedirectToAction("BehavioralPatterns");
        }

        public IActionResult BehavioralPatterns()
        {
            return View();
        }

        public IActionResult NetworkAnalysis()
        {
            return View();
        }

        public IActionResult TrendForecasting()
        {
            return View();
        }

        public IActionResult GeoRiskHeatmap()
        {
            return View();
        }
    }
}
