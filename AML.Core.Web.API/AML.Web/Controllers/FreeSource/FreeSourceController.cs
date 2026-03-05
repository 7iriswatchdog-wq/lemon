using AML.Core.DataContract.Enum;
using AML.Core.ServiceContract.FreeSource;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.FreeSource;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;

namespace AML.Web.Controllers.FreeSource
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class FreeSourceController : Controller
    {
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IHttpClientHandler _clientHandler;
        IFileUploader _fileUploader;
        private IFreeSourceService _freeSourceService;

        public FreeSourceController(IMapper mapper, IHttpClientHandler clientHandler, IFileUploader fileUploader, IFreeSourceService freeSourceService,
            IConfiguration configuration, IToastNotification toastNotification)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _fileUploader = fileUploader;
            _freeSourceService = freeSourceService;
        }
        // GET: FreeSource
        public ActionResult Index()
        {
            return View();
        }

        // GET: FreeSource/Upload
        //public ActionResult Upload(string Source)
        //{
        //    FreeSourceModel model = new FreeSourceModel();
        //    model.Source = Source;
        //    model.Document = new DocumentUploadModel();

        //    DocumentUploadModel _docUpload = new DocumentUploadModel { itemId = (int)ItemType.freeSource, branchId = _clientHandler.GetBranchId() };
        //    model.Document.branchId = _clientHandler.GetBranchId();
        //    model.CreatedBy = _clientHandler.GetUserId();
        //    return View(model);
        //}

        //// POST: FreeSource/Upload
        //[HttpPost]
        //public ActionResult Upload(FreeSourceModel model)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            if (model.Document.fileUpload != null)
        //            {
        //                DocumentsModel _documentsModel = _fileUploader.UploadFile((ItemType)model.Document.itemId, model.Document.branchId, model.Document.fileUpload);
        //                _documentsModel.AddedBy = _clientHandler.GetUserId();

        //                string res = _freeSourceService.ReadFile(_documentsModel.DocFullPath, model.Source);

        //                _toastNotification.AddSuccessToastMessage(model.Source + " document uploaded successfully");

        //            }
        //            else
        //            {
        //                _toastNotification.AddErrorToastMessage(model.Source + " document not selected");
        //            }

        //            return RedirectToAction(nameof(Index));
        //        }
        //        else
        //        {
        //            return View(model);
        //        }
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}


    }
}
