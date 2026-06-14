using AML.Core.ServiceContract.ComplianceHub;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using Microsoft.AspNetCore.Mvc;

namespace AML.Web.Controllers.ComplianceHub
{
    [SessionAuthorize]
    [Route("Compliance")]
    public class ComplianceHubController : Controller
    {
        private readonly IComplianceHubService _service;
        private readonly IHttpClientHandler _clientHandler;

        public ComplianceHubController(IComplianceHubService service, IHttpClientHandler clientHandler)
        {
            _service = service;
            _clientHandler = clientHandler;
        }

        private int ClientId => _clientHandler.GetClientId();

        [HttpGet("")]
        [HttpGet("Hub")]
        public IActionResult Hub()
        {
            var vm = _service.BuildHub(ClientId, action => Url.Action(action, "ComplianceHub"));
            return View("Hub", vm);
        }

        [HttpGet("KycExpiry")]        public IActionResult KycExpiry(int days = 365) => View("Reports/KycExpiry", _service.GetKycExpiry(ClientId, days));
        [HttpGet("PeriodicReview")]   public IActionResult PeriodicReview()           => View("Reports/PeriodicReview", _service.GetPeriodicReview(ClientId));
        [HttpGet("DataCompleteness")] public IActionResult DataCompleteness()         => View("Reports/DataCompleteness", _service.GetDataCompleteness(ClientId));
        [HttpGet("OnboardingFunnel")] public IActionResult OnboardingFunnel(int months = 12) => View("Reports/OnboardingFunnel", _service.GetOnboardingFunnel(ClientId, months));
        [HttpGet("ScreeningGap")]     public IActionResult ScreeningGap()             => View("Reports/ScreeningGap", _service.GetScreeningGap(ClientId));
        [HttpGet("SanctionsMatrix")]  public IActionResult SanctionsMatrix()          => View("Reports/SanctionsMatrix", _service.GetSanctionsMatrix(ClientId));
        [HttpGet("PepInventory")]     public IActionResult PepInventory()             => View("Reports/PepInventory", _service.GetPepInventory(ClientId));
        [HttpGet("AdverseMedia")]     public IActionResult AdverseMedia()             => View("Reports/AdverseMedia", _service.GetAdverseMedia(ClientId));
        [HttpGet("CrossBorderMap")]   public IActionResult CrossBorderMap()           => View("Reports/CrossBorderMap", _service.GetCrossBorderMap(ClientId));
        [HttpGet("ChannelProductMix")]public IActionResult ChannelProductMix()        => View("Reports/ChannelProductMix", _service.GetChannelProductMix(ClientId));
        [HttpGet("Whitelist")]        public IActionResult Whitelist()                => View("Reports/Whitelist", _service.GetWhitelistGovernance(ClientId));
        [HttpGet("Proliferation")]    public IActionResult Proliferation()            => View("Reports/Proliferation", _service.GetProliferationRegister(ClientId));
    }
}
