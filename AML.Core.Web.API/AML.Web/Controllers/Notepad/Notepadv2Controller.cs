using AML.Core.ServiceContract.Notepad;
using AML.DTO.DTO.Notepad;
using AML.Web.Helper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace AML.Web.Controllers.Notepad
{
    [Route("notepadv2")]
    public class Notepadv2Controller : Controller
    {
        private readonly INotepadService _notepadService;
        private readonly IHttpClientHandler _httpClientHandler;
        private readonly IExportDataService _exportService;

        public Notepadv2Controller(INotepadService notepadService, IHttpClientHandler httpClientHandler, IExportDataService exportService)
        {
            _notepadService = notepadService;
            _httpClientHandler = httpClientHandler;
            _exportService = exportService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("Notepadv2");
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetNotepadv2Records(string search, int page = 1)
        {
            try
            {
                int clientId = _httpClientHandler.GetClientId();
                var data = await _notepadService.GetNotepadv2Async(clientId, search);
                return Json(new { data = data, total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateNotepadv2Details([FromBody] Notepadv2DTO model)
        {
            try
            {
                model.updated_by = _httpClientHandler.GetUserId();
                model.client_id = _httpClientHandler.GetClientId();
                var result = await _notepadService.SaveNotepadv2Async(model);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateNotepadv2Record([FromBody] Notepadv2DTO model)
        {
            try
            {
                model.created_by = _httpClientHandler.GetUserId();
                model.client_id = _httpClientHandler.GetClientId();
                model.created_on = DateTime.Now;
                var result = await _notepadService.SaveNotepadv2Async(model);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteNotepadv2ById([FromQuery] int id)
        {
            try
            {
                var result = await _notepadService.DeleteNotepadv2Async(id);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("Export")]
        public async Task<IActionResult> ExportToExcel(string search)
        {
            try
            {
                int clientId = _httpClientHandler.GetClientId();
                var data = await _notepadService.GetNotepadv2Async(clientId, search);
                
                string fileName = $"Notepad_Records_{DateTime.Now:yyyyMMddHHmmss}";
                return _exportService.ExportData<Notepadv2DTO>(data.ToList(), "", 2, fileName); // 2 is for Excel
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }
    }
}
