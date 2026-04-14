using Microsoft.AspNetCore.Mvc;
using AML.ViewModel.ViewModels.DataTable;
using AML.DTO.DTO.TransactionScreening;
using AML.ViewModel.ViewModels.TransactionScreening;
using AML.ViewModel.ViewModels.User;
using AML.ViewModel.ViewModels.Common;
using AML.Core.ServiceContract.TransactionScreening;
using AML.Core.ServiceContract.User;
using AML.Core.DataContract.Enum;
using System.Collections.Generic;
using AutoMapper;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using AML.Web.Helper;
using Newtonsoft.Json;
using System;
using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.Common;
using Microsoft.AspNetCore.Http;
using RestSharp;
using RestSharp.Authenticators;
using Microsoft.Extensions.Configuration;
using AML.Web.CustomFilters;

namespace AML.Web.Controllers.TransactionScreening
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class TransactionScreeningController : Controller
    {

        private IMapper _mapper;
        private ITransactionScreeningService _transactionScreeningService;
        private IHttpClientHandler _clientHandler;
        private IUserService _userService;
        IFileUploader _fileUploader;
        private ICommonService _commonService;
        private IConfiguration _configuration;
        private readonly string callbackEnabled;

        public TransactionScreeningController(IMapper mapper, IConfiguration configuration, ITransactionScreeningService transactionScreeningService, IHttpClientHandler clientHandler, IUserService userService, IFileUploader fileUploader, ICommonService commonService)
        {
            _mapper = mapper;
            _transactionScreeningService = transactionScreeningService;
            _clientHandler = clientHandler;
            _userService = userService;
            _fileUploader = fileUploader;
            callbackEnabled = configuration.GetSection("CallBackEnabled")?.Value ?? "False";
            _commonService = commonService;
        }

        [HttpGet("/transactioncase")]
        public IActionResult PendingTransactions()
        {
            return View();
        }
        [HttpGet("/transactioncase/process/{CaseId}")]
        public async Task<ActionResult> ProcessTransactionCase(int CaseId)
        {
            var clientId = _clientHandler.GetClientId();
            TransactionCaseProcessModel model = new TransactionCaseProcessModel();
            model.Case = new TranScreenCaseModel();

            TranScreenDTO _TranCaseDTO = _transactionScreeningService.GetTranCaseByID(CaseId);
            model.Case = _mapper.Map<TranScreenCaseModel>(_TranCaseDTO);

            List<TransactionCaseDocumentDTO> caseDocumentbyId = _transactionScreeningService.GetCaseDocumentByCaseId(CaseId);
            model.CaseDocuments = _mapper.Map<List<TransactionCaseDocumentModel>>(caseDocumentbyId);
            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            var id = _clientHandler.GetUserId();
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != id))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYTRANCASEID).Result;


            if (!string.IsNullOrEmpty(result))
            {
                List<MatchDetailsModel> jsonList = JsonConvert.DeserializeObject<List<MatchDetailsModel>>(result);
                model.DataList = jsonList;
            }
            return View(model);
        }
        [HttpPost("/transactioncase/pendingtransactions")]
        public JsonResult PendingTransactionsCustomPagination(DataTableModel model, string startDate, string endDate)
        {


            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }

            if (startDate == null)
            {
                startDate = System.DateTime.Now.ToString();
            }


            PendingTransactionRequestDTO req = new PendingTransactionRequestDTO();
            req.StartDate = startDate;
            req.EndDate = endDate;
            req.ClientId= _clientHandler.GetUserId();
            //List<PendingTransactionsDTO> resp = new List<PendingTransactionsDTO>();


            List<PendingTransactionsModel> abc = _mapper.Map<List<PendingTransactionsModel>>(_transactionScreeningService.GetPendingTransactions(req));


            int totalcount = abc.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                abc = abc.Where(m => m.tranrefno.ToLower().Contains(model.search.value.ToLower())).ToList();
            }
            int filteredcount = abc.Count;
            var data = abc.Skip(model.start).Take(model.length).ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data,
            });
            return response;
            //return null;
        }

        [HttpPost("/transactioncase/pendingcases")]
        public JsonResult PendingCasesCustomPagination(DataTableModel model, string tranrefno)
        {

            PendingCasesRequestDTO req = new PendingCasesRequestDTO();
            req.tranrefno = tranrefno;

            List<PendingCasesModel> abc = _mapper.Map<List<PendingCasesModel>>(_transactionScreeningService.GetIndividualPendingCases(req));


            int totalcount = abc.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {

                abc = abc.Where(m => m.custrefno.ToLower().Contains(model.search.value.ToLower())
              || m.matchscore.ToString().ToLower().Contains(model.search.value.ToLower())
              || m.id.ToString().ToLower().Contains(model.search.value.ToLower())
              || m.name.ToLower().Contains(model.search.value.ToLower())).ToList();

            }
            int filteredcount = abc.Count;
            var data = abc.Skip(model.start).Take(model.length).ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data,
            });
            return response;
            //return null;
        }

        [HttpGet("/transactioncase/pendingcases/{tranrefno}")]
        public IActionResult PendingCases(string tranrefno)
        {
            PendingCasesRequestModel model = new PendingCasesRequestModel();
            model.tranrefno = tranrefno;

            return View(model);
        }

        [HttpPost("/transactioncase/document")]
        public JsonResult Document(TransactionCaseDocumentModel model)
        {
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;
            model.ClientId = _clientHandler.GetClientId();
            if (model.Document != null)
            {
                DocumentsModel _documentsModel = _fileUploader.UploadFile(model.ClientId ,ItemType.caseDocument, _clientHandler.GetBranchId(), model.Document);
                model.DocumentFileName = _documentsModel.DocName;
                model.DocumentFullPath = _documentsModel.DocFullPath;
            }
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;
            var result = _transactionScreeningService.CreateCaseDocumentByCaseID(_mapper.Map<TransactionCaseDocumentDTO>(model));
            return Json(result);
        }

        [HttpGet]
        public ActionResult DownloadCaseDocument(string filePath, string fileName)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);



            return File(fileBytes, "application/force-download", fileName);
        }

        [HttpGet("/transactioncase/comment/{CaseId}")]
        public JsonResult Comment(int CaseId)
        {
            return Json(_transactionScreeningService.GetAllCaseCommentByCase(CaseId));
        }

        [HttpPost("/transactioncase/comment")]
        public JsonResult Comment(TransactionCaseCommentModel model)
        {
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;
            var result = _transactionScreeningService.CreateCaseCommentByCaseID(_mapper.Map<TransactionCaseCommentDTO>(model));

            return Json(_transactionScreeningService.GetAllCaseCommentByCase(model.CaseId));
        }

        [Route("/transactioncase/saveRemark")]
        [HttpPost]
        public JsonResult SaveRemark(int id, List<ApiRespModel> model)
        {
            var result = _commonService.UpdateTransactionCaseRemark(id, model);

            string response = string.Empty;
            response = result ? "Remarks Updated" : "Saving Remarks failed";

            return Json(response);
        }

        [HttpPost("/Transactioncase/assign")]
        public JsonResult Assign(TransactionCaseAssignmentModel model)
        {
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;
            var result = _transactionScreeningService.CreateTransactionCaseTransfer(_mapper.Map<TransactionCaseAssignmentDTO>(model));
            var userName = HttpContext.Session.GetString("SessUsername");
            var comment = string.Format("Transferred Case To {0}", model.TransferUser);

            if (model.Comment != "" && model.Comment != null)
            {
                TransactionCaseCommentModel remarkModel = new TransactionCaseCommentModel();
                remarkModel.CaseId = model.CaseId;
                remarkModel.Comment = model.Comment;
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                var remarkResult = _transactionScreeningService.CreateCaseCommentByCaseID(_mapper.Map<TransactionCaseCommentDTO>(remarkModel));
            }
            TransactionCaseCommentModel commentModel = new TransactionCaseCommentModel();
            commentModel.CaseId = model.CaseId;
            commentModel.Comment = comment;
            commentModel.CreatedBy = _clientHandler.GetUserId();
            var commentResult = _transactionScreeningService.CreateCaseCommentByCaseID(_mapper.Map<TransactionCaseCommentDTO>(commentModel));

            return Json(result);
        }


        private string MiddlewareApiCall(TranScreenDTO model)
        {
            // ######   TEST    #######
            string endpoint = "https://mproxy-test.klip.ae";
            var options = new RestClientOptions(endpoint)
            {
                Authenticator = new HttpBasicAuthenticator("digicomply", "4_qvuX3ek):q*bWSNh{EA)j.V5qf5cw"),
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            var client = new RestClient(options);

            var request = new RestRequest("api/v1/partner/digicomply/callback");

            request.AddJsonBody(new
            {
                CustomerId = model.CustRefNo,
                Status = (model.Status == 1 || model.Status == 2) ? "APPROVED" : "REJECTED",
                ProcessedDate = TimeZoneInfo.ConvertTime(
                    DateTime.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")
                ).ToString("yyyy-MM-dd HH:mm:ss")
            });

            request.AddHeader("partner-id", "DIGI_COMPLY");

            var response = client.Post(request);

            var requestToLog = new
            {
                resource = request.Resource,
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value
                }),
                method = request.Method.ToString(),
                uri = client.BuildUri(request),
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };

            Console.WriteLine(string.Format("Request: {0}, Response: {1}",
                JsonConvert.SerializeObject(requestToLog, Formatting.Indented),
                JsonConvert.SerializeObject(responseToLog, Formatting.Indented)));

            return JsonConvert.SerializeObject(responseToLog, Formatting.Indented);
        }

        [HttpPost("/transactioncase/close")]
        public JsonResult Close(CaseCloseModel model)
        {


            TranScreenDTO _CustomerCaseDTO = _transactionScreeningService.GetTranCaseByID(model.CaseId);
            _CustomerCaseDTO.Status = model.Action;
            _CustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
            _CustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.Now);
            _CustomerCaseDTO.Comments = model.Comment;
            var result = _transactionScreeningService.UpdateTransactionScreening(_CustomerCaseDTO);

            //Update White List
            //if (model.Action == 1)
            //{
            //    var res = _customerMasterService.UpdateWhiteList(_CustomerCaseDTO.CustomerMasterId, "YES");
            //    _customerMasterService.InsertCustomerWhiteListLogs(_CustomerCaseDTO.CustomerMasterId, _CustomerCaseDTO.CustomerId, _clientHandler.GetUserId(), "YES");
            //}



            string[] response = { string.Empty, string.Empty };
            if (model.Action == 2)
            {
                response[0] = "Transaction case approved.";
            }
            else if (model.Action == 3)
            {
                response[0] = "Transaction rejected.";
            }
            //response = model.Action == 2 ? "Customer case approved." : "Customer case rejected.";
            if (model.Action == 1)
                response[0] = "Customer Case Whitelisted";
            //_toastNotification.AddErrorToastMessage(reposne);
            if (model.Comment != "" && model.Comment != null)
            {
                TransactionCaseCommentModel remarkModel = new TransactionCaseCommentModel();
                remarkModel.CaseId = model.CaseId;
                remarkModel.Comment = model.Comment;
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                var remarkResult = _transactionScreeningService.CreateCaseCommentByCaseID(_mapper.Map<TransactionCaseCommentDTO>(remarkModel));
            }
            TransactionCaseCommentModel commentModel = new TransactionCaseCommentModel();
            commentModel.CaseId = model.CaseId;
            var userName = HttpContext.Session.GetString("SessUsername");
            var comment = "";
            switch (model.Action)
            {
                case 1:
                    comment = string.Format("Whitelisted Case");
                    break;
                case 2:
                    comment = string.Format("Approved Case");
                    break;
                case 3:
                    comment = string.Format("Rejected Case");
                    break;
                case 4:
                    //comment = string.Format("Transferred Case To {0}",model.TransferUser);
                    break;
                default:
                    comment = "";
                    break;
            }
            commentModel.Comment = comment;
            commentModel.CreatedBy = _clientHandler.GetUserId();
            var commentResult = _transactionScreeningService.CreateCaseCommentByCaseID(_mapper.Map<TransactionCaseCommentDTO>(commentModel));

            if (callbackEnabled == "True")
            {
                response[1] = MiddlewareApiCall(_CustomerCaseDTO);
            }

            return Json(response);
        }




    }
}
