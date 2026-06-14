using AML.Core.ServiceContract.Common;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AML.Core.DataContract.Enum;
using AML.ViewModel.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using AML.Core.Common.StaticResource;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace AML.Web.Helper
{
    public interface IFileUploader
    {
        DocumentsModel UploadFile(int ClientId,ItemType itemType, int branchId, IFormFile formFile, string fileName = null);
        DocumentsModel UploadLogo(int ClientId, ItemType itemType, int branchId, IFormFile formFile, string fileName = null);
    }
    public class FileUploader : IFileUploader
    {
        private ICommonService _commonService;
        private IWebHostEnvironment _environment;
        private IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Allowlist of file extensions accepted for any non-image upload (case docs, bulk upload, etc.)
        // Logos use a separate image-only allowlist below.
        private static readonly HashSet<string> AllowedDocExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".xlsx", ".xls", ".csv", ".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".tiff", ".tif" };
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg" };
        private const long MaxUploadBytes = 50L * 1024 * 1024; // 50 MB hard cap

        public FileUploader(ICommonService commonService, IWebHostEnvironment environment, IHttpClientHandler clientHandler, IConfiguration configuration)
        {
            _commonService = commonService;
            _environment = environment;
            _clientHandler = clientHandler;
            _configuration = configuration;
        }

        // Strip any directory separators / traversal sequences from a user-supplied filename.
        private static string SafeFileName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "upload";
            var bare = Path.GetFileName(raw);                       // strips path traversal
            foreach (var c in Path.GetInvalidFileNameChars())
                bare = bare.Replace(c.ToString(), "_");
            return string.IsNullOrWhiteSpace(bare) ? "upload" : bare;
        }

        private static bool IsAllowed(IFormFile f, HashSet<string> allowed, out string ext)
        {
            ext = (Path.GetExtension(f?.FileName ?? "") ?? "").ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && allowed.Contains(ext) && (f?.Length ?? 0) > 0 && f.Length <= MaxUploadBytes;
        }


        public string GetBasePath(ItemType itemType, int branchId)
        {
            string itemTypeName = itemType.ToString().Replace("_", "").Replace(" ", "").ToLower();

            if (branchId > 0)
                return $"{_configuration.GetSection("BasePathUploadFolder").Value}/{itemTypeName}/{branchId}/";
            else
                return $"{_configuration.GetSection("BasePathUploadFolder").Value}/{itemTypeName}/";
        }
        public string GetRootPath(ItemType itemType, int branchId)
        {
            string itemTypeName = itemType.ToString().Replace("_", "").Replace(" ", "").ToLower();

            
                return $"{this._environment.WebRootPath}/img/";
        }

        private long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }
        public DocumentsModel UploadLogo(int ClientId, ItemType itemType, int branchId, IFormFile formFile, string fileName = null)
        {
            if (formFile == null || formFile.Length == 0) return null;
            if (!IsAllowed(formFile, AllowedImageExtensions, out var imgExt))
                throw new InvalidOperationException("Logo upload rejected: only image files (png/jpg/jpeg/gif/webp/svg) up to 50 MB are allowed.");

            string path = GetRootPath(itemType, 0);
            var safeOriginal = SafeFileName(formFile.FileName);

            if (fileName.IsNullOrEmpty())
            {
                fileName = string.Concat(GetCurrentUnixTimestampMillis().ToString(), Path.GetFileNameWithoutExtension(safeOriginal), imgExt);
            }
            else
            {
                fileName = SafeFileName(fileName);
            }

            try
            {
                #region CREATE FOLDER IF NOT EXISTING
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                #endregion

                using (var stream = File.Create(Path.Combine(path, fileName)))
                {
                    formFile.CopyTo(stream);
                }
            }
            catch { }
            return new DocumentsModel
            {
                DocName = fileName,
                DocOriginalName = safeOriginal,
                DocPath = string.Concat(path, "/", fileName),
                DocType = Path.GetExtension(fileName).Replace(".", string.Empty),
                DocFullPath = string.Concat(_configuration.GetSection("FileUploadPath").Value, path, "/", fileName),
            };
        }
        public DocumentsModel UploadFile(int ClientId,ItemType itemType, int branchId, IFormFile formFile, string fileName = null)
        {
            if (formFile == null || formFile.Length == 0) return null;
            if (!IsAllowed(formFile, AllowedDocExtensions, out var fileExt))
                throw new InvalidOperationException($"File upload rejected: extension '{Path.GetExtension(formFile.FileName)}' is not allowed, or file exceeds 50 MB.");

            string path = GetBasePath(itemType, ClientId);
            var safeOriginal = SafeFileName(formFile.FileName);

            if (fileName.IsNullOrEmpty())
            {
                fileName = string.Concat(GetCurrentUnixTimestampMillis().ToString(), "_", Path.GetFileNameWithoutExtension(safeOriginal), fileExt);
            }
            else
            {
                fileName = SafeFileName(fileName);
            }

            try
            {
                #region CREATE FOLDER IF NOT EXISTING
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                #endregion

                using (var stream = File.Create(Path.Combine(path, fileName)))
                {
                    formFile.CopyTo(stream);
                }
            }
            catch { }
            return new DocumentsModel
            {
                DocName = fileName,
                DocOriginalName = safeOriginal,
                DocPath = string.Concat(path, "/", fileName),
                DocType = Path.GetExtension(fileName).Replace(".", string.Empty),
                DocFullPath = string.Concat(_configuration.GetSection("FileUploadPath").Value, path, "/", fileName),
            };
        }
    }
}
