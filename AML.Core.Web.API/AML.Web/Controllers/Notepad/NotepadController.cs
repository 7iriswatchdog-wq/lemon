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

        public async Task<IActionResult> Index()
        {
            int userId = _httpClientHandler.GetUserId();
            var notepad = await _notepadService.GetNotepadAsync(userId);
            if (notepad == null)
            {
                notepad = new NotepadDTO { UserId = userId, Content = "" };
            }
            return View(notepad);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] NotepadDTO model)
        {
            int userId = _httpClientHandler.GetUserId();
            model.UserId = userId;
            var result = await _notepadService.SaveNotepadAsync(model);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachment()
        {
            var files = Request.Form.Files;
            int userId = _httpClientHandler.GetUserId();
            
            foreach (var file in files)
            {
                var doc = _fileUploader.UploadFile(0, ItemType.Notepad, 0, file);
                if (doc != null)
                {
                    var attachment = new NotepadAttachmentDTO
                    {
                        FileName = doc.DocOriginalName,
                        FilePath = doc.DocPath,
                        FileType = doc.DocType,
                        FileSize = file.Length,
                        UploadedOn = DateTime.Now
                    };
                    await _notepadService.AddAttachmentAsync(userId, attachment);
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _notepadService.GetAttachmentAsync(id);
            if (attachment == null || !System.IO.File.Exists(attachment.FilePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(attachment.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/octet-stream", attachment.FileName);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var result = await _notepadService.DeleteAttachmentAsync(id);
            return Json(new { success = result });
        }
    }
}
