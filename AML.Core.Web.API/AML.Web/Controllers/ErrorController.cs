using Microsoft.AspNetCore.Mvc;

namespace AML.Web.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult PageNotFound()
        {
            return View();
        }
    }
}
