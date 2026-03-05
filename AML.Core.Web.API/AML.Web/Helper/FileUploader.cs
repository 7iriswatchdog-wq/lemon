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
        public FileUploader(ICommonService commonService, IWebHostEnvironment environment, IHttpClientHandler clientHandler, IConfiguration configuration)
        {
            _commonService = commonService;
            _environment = environment;
            _clientHandler = clientHandler;
            _configuration = configuration;
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
            string path = GetRootPath(itemType, 0);
            //string path = GetBasePath(itemType, ClientId);


            if (fileName.IsNullOrEmpty())
            {
                fileName = string.Concat(GetCurrentUnixTimestampMillis().ToString(), Path.GetFileNameWithoutExtension(formFile.FileName), Path.GetExtension(formFile.FileName));
                //fileName = string.Concat("logo_", ClientId,".png");
            }

            try
            {
                if (formFile != null && formFile.Length > 0)
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
            }
            catch { }
            return new DocumentsModel
            {
                DocName = fileName,
                DocOriginalName = formFile.FileName,
                DocPath = string.Concat(path, "/", fileName),
                DocType = Path.GetExtension(fileName).Replace(".", string.Empty),
                DocFullPath = string.Concat(_configuration.GetSection("FileUploadPath").Value, path, "/", fileName),
            };
        }
        public DocumentsModel UploadFile(int ClientId,ItemType itemType, int branchId, IFormFile formFile, string fileName = null)
        {
            //string path = GetRootPath(itemType, 0);
            string path = GetBasePath(itemType, ClientId);


            if (fileName.IsNullOrEmpty())
            {
                fileName = string.Concat(GetCurrentUnixTimestampMillis().ToString(), Path.GetFileNameWithoutExtension(formFile.FileName), Path.GetExtension(formFile.FileName));
                //fileName = string.Concat("logo_", ClientId,".png");
            }

            try
            {
                if (formFile != null && formFile.Length > 0)
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
            }
            catch { }
            return new DocumentsModel
            {
                DocName = fileName,
                DocOriginalName = formFile.FileName,
                DocPath = string.Concat(path, "/", fileName),
                DocType = Path.GetExtension(fileName).Replace(".", string.Empty),
                DocFullPath = string.Concat(_configuration.GetSection("FileUploadPath").Value, path, "/", fileName),
            };
        }
    }
}
