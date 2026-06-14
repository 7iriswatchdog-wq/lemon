using AML.Core.ServiceContract.AmlTracker;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using Microsoft.AspNetCore.Mvc;

namespace AML.Web.Controllers.Reports
{
    [SessionAuthorize]
    [Route("Associations")]
    public class AssociationsController : Controller
    {
        private readonly IAmlTrackerService _trackerService;
        private readonly IHttpClientHandler _clientHandler;

        public AssociationsController(IAmlTrackerService trackerService, IHttpClientHandler clientHandler)
        {
            _trackerService = trackerService;
            _clientHandler = clientHandler;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var clientId = _clientHandler.GetClientId();
            var vm = _trackerService.GetCustomerAssociations(clientId);
            return View(vm);
        }
    }
}
