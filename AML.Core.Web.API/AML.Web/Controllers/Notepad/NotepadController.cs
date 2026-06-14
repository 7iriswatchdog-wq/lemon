using AML.Core.ServiceContract.Notepad;
using AML.DTO.DTO.Notepad;
using AML.Web.Helper;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using AML.Core.DataContract.Enum;
using System;

namespace AML.Web.Controllers.Notepad
{
    public class NotepadController : Controller
    {
        private readonly INotepadService _notepadService;
        private readonly IHttpClientHandler _httpClientHandler;
        private readonly IFileUploader _fileUploader;

        public NotepadController(INotepadService notepadService, IHttpClientHandler httpClientHandler, IFileUploader fileUploader)
        {
            _notepadService = notepadService;
            _httpClientHandler = httpClientHandler;
            _fileUploader = fileUploader;
        }

        public IActionResult Index()
        {
            return View("Notepadv2");
        }

        public IActionResult Notepadv2()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetNote(int id)
        {
            var notepad = await _notepadService.GetNoteByIdAsync(id);
            if (notepad == null) return NotFound();
            return Json(notepad);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] NotepadDTO model)
        {
            int userId = _httpClientHandler.GetUserId();
            model.UserId = userId;
            var result = await _notepadService.SaveNotepadAsync(model);
            return Json(new { success = result, id = model.Id, subject = model.Subject });
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachment()
        {
            var files = Request.Form.Files;
            int userId = _httpClientHandler.GetUserId();
            int notepadId = 0;
            int.TryParse(Request.Form["notepadId"], out notepadId);
            
            foreach (var file in files)
            {
                var doc = _fileUploader.UploadFile(0, ItemType.Notepad, 0, file);
                if (doc != null)
                {
                    var attachment = new NotepadAttachmentDTO
                    {
                        NotepadId = notepadId,
                        FileName = doc.DocOriginalName,
                        FilePath = doc.DocPath,
                        FileType = doc.DocType,
                        FileSize = file.Length,
                        UploadedOn = DateTime.Now
                    };
                    await _notepadService.AddAttachmentAsync(userId, attachment);
                }
            }
            return RedirectToAction("Index", new { id = notepadId > 0 ? (int?)notepadId : null });
        }

        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _notepadService.GetAttachmentAsync(id);
            if (attachment == null) return NotFound();

            // Normalize path for Windows/Linux compatibility and handle double slashes
            string normalizedPath = attachment.FilePath.Replace("/", "\\").Replace("\\\\", "\\");
            
            if (!System.IO.File.Exists(normalizedPath))
            {
                // Try original path if normalized fails
                if (!System.IO.File.Exists(attachment.FilePath))
                {
                    return NotFound();
                }
                normalizedPath = attachment.FilePath;
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(normalizedPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/octet-stream", attachment.FileName);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttachment([FromQuery] int id)
        {
            try
            {
                var result = await _notepadService.DeleteAttachmentAsync(id);
                return Json(new { success = result, message = result ? "File deleted successfully" : "Failed to delete file. It may have already been removed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNote([FromQuery] int id)
        {
            try
            {
                var result = await _notepadService.DeleteNoteAsync(id);
                return Json(new { success = result, message = result ? "Note deleted successfully" : "Failed to delete note." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error: " + ex.Message });
            }
        }
    }
}
