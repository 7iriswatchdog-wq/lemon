using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Enum;
using AML.Core.RepositoryContract.EtlBatch;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.RepositoryContract.InternalWatchList;
using AML.Core.ServiceContract.InternalWatchList;
using AML.Core.ServiceContract.Sanction;
using AML.DTO.DTO.FreeSource;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.EtlBatch;
using AML.DTO.DTO.InternalWatchListExcel;
using AML.DTO.DTO.Sanction;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.InternalWathcList;
using AML.ViewModel.ViewModels.Sanction;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NToastNotify;

namespace AML.Web.Controllers.InternalWatchList
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class InternalWatchListController : Controller
    {
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IHttpClientHandler _clientHandler;
        private readonly ICountryService _countryService;
        //private readonly ISanctionService _sanctionService;
        //private ICustomerCaseService _customerCaseService
        private IInternalWatchListService _internalWatchListService;
        IEtlLogRepository _etlLogRepository;
        IFileUploader _fileUploader;
        private readonly IInternalWatchListMongoRepository _mongoRepository;
        public InternalWatchListController(
            IMapper mapper,
            IToastNotification toastNotification,
            IHttpClientHandler clientHandler,
            ICountryService countryService, IEtlLogRepository etlLogRepository,
            IFileUploader fileUploader, IInternalWatchListService internalWatchListService,
            IInternalWatchListMongoRepository mongoRepository
        )
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _countryService = countryService;
            _internalWatchListService = internalWatchListService;
            _fileUploader = fileUploader;
            _etlLogRepository = etlLogRepository;
            _mongoRepository = mongoRepository;
        }
        public IActionResult Index()
        {
            return View(new WatchListModel());
        }
        public IActionResult WatchList()
        {
            return View();
        }
        //[HttpPost("custompagination")]
        //public JsonResult CustomPagination(DataTableModel model)
        //{
        //    List<SanctionWatchModel> watchList = _mapper.Map<List<SanctionWatchModel>>(_sanctionService.GetAll());
        //    if (!string.IsNullOrEmpty(model.search.value))
        //    {
        //        watchList = watchList.Where(m => m.FirstName.ToLower().Contains(model.search.value.ToLower())
        //        || m.LastName.ToLower().Contains(model.search.value.ToLower())
        //        || m.MiddleName.ToLower().Contains(model.search.value.ToLower())).ToList();
        //    }

        //    var data = watchList.Skip(model.start).Take(model.length).ToList();
        //    var response = Json(new
        //    {
        //        // this is what datatables wants sending back
        //        model.draw,
        //        recordsTotal = watchList.Count,//totalResultsCount,
        //        recordsFiltered = watchList.Count,//filteredResultsCount,
        //        data = data
        //    });
        //    return response;
        //}

        
        public IActionResult Create()
        {
            WatchListModel model = new WatchListModel();
            model.CreatedBy = _clientHandler.GetUserId();
            var clientId = _clientHandler.GetClientId();
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            return View(model);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(WatchListModel model)
        {
            var clientId = _clientHandler.GetClientId();
            try
            {
                if (ModelState.IsValid)
                {
                    var apiresponse = "";
                    if (model.Source == "UAE IEC LIST")
                    {
                        var request = new { fullname = model.FullName, dob = model.DOB, nationality = model.Nationality, type = model.Source, category = model.Type, IDNUMBER = model.IdNumber, REMARKS = model.Remarks };
                        apiresponse = _clientHandler.PostAsync(request, ScreeningService.ADDTOBLACKLIST).Result;

                        if (!string.IsNullOrEmpty(apiresponse))
                        {
                            SourceUploadLogsDTO uploadLogsDTO = new SourceUploadLogsDTO();
                            uploadLogsDTO.Source = model.Source;
                            uploadLogsDTO.TotalRecords = 1;
                            _internalWatchListService.InsertUploadLogs(uploadLogsDTO);
                        }
                    }
                    else
                    {
                        var request2 = new { fullname = model.FullName, dob = model.DOB, nationality = model.Nationality, type = model.Source, category = model.Type, IDNUMBER = model.IdNumber, CLIENTID = clientId, REMARKS = model.Remarks };
                        apiresponse = _clientHandler.PostAsync(request2, ScreeningService.ADDTOBLACKLIST).Result;
                    }
                    //model.Nationality = _mapper.Map<CountryModel>(_countryService.GetDetails(model.NationalityId)).Name;

                    //var result = _sanctionService.Create(_mapper.Map<WatchListDTO>(model));

                    if (!string.IsNullOrEmpty(apiresponse))
                    {
                        var uid = model.UID ?? Guid.NewGuid().ToString();
                        _mongoRepository.InsertBlockList(new NAMELIST
                        {
                            UID = uid,
                            FULLNAME = model.FullName,
                            NATIONALITY = model.Nationality,
                            TYPE = model.Source,
                            CATEGORY = model.Type,
                            REMARKS = model.Remarks,
                            IDDETAILS = new List<IDDETAIL> { new IDDETAIL { IDNUMBER = model.IdNumber } },
                            DOB = new List<DOBLIST> { new DOBLIST { DOB = model.DOB } },
                            STATUS = "A",
                            CREATEDON = DateTime.UtcNow.AddHours(4).ToString("dd/MM/yyyy HH:mm:ss"),
                            CREATEDDATE = DateTime.UtcNow,
                            UPDATEDDATE = DateTime.UtcNow
                        });

                        return Json(new { success = true, message = "Internal Watchlist added successfully", uid = uid });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Something went wrong with the API response." });
                    }
                }
                return Json(new { success = false, message = "Invalid model state." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult BlockListUpload() 
        {

            return View();
        }

        public IActionResult Upload(WatchListModel model)
        {
            var clientId = _clientHandler.GetClientId();
            WatchListModel watchlistModel = new WatchListModel();
            if (model.Document!=null)
            {
                DocumentsModel _documentsModel = _fileUploader.UploadFile(model.ClientId, ItemType.InternalWatchList, _clientHandler.GetBranchId(), model.Document.fileUpload);
                _documentsModel.AddedBy = _clientHandler.GetUserId();
                var totalrecords = 0;
                var apiresponse2 = "";
                
                List<InternalWatcListExcelDTO> _custExcelDataResponse = _internalWatchListService.LoadInternalWatchExcelData(_documentsModel.DocFullPath, 1).Result;
                foreach (var item in _custExcelDataResponse)
                {
                    if (item.Type == "INDIVIDUAL" || item.Type == "CORPORATE" || item.Type == "Individual" || item.Type == "Corporate")
                    {
                        if (item.Source == "UAE IEC LIST")
                        {
                            var request = new { fullname = item.FullName, dob = item.DOB, nationality = item.Nationality, type = item.Source, category = item.Type, REMARKS = item.REMARKS };
                            apiresponse2 = _clientHandler.PostAsync(request, ScreeningService.ADDTOBLACKLIST).Result;


                            if (!string.IsNullOrEmpty(apiresponse2))
                            {
                                model.Source = item.Source;
                                totalrecords++;

                                // Log to MongoDB (Fixed: Added missing logging for UAE IEC LIST)
                                _mongoRepository.InsertBlockList(new NAMELIST
                                {
                                    UID = Guid.NewGuid().ToString(),
                                    FULLNAME = item.FullName,
                                    NATIONALITY = item.Nationality,
                                    TYPE = item.Source,
                                    CATEGORY = item.Type,
                                    REMARKS = item.REMARKS,
                                    DOB = new List<DOBLIST> { new DOBLIST { DOB = item.DOB } },
                                    STATUS = "A",
                                    CREATEDON = DateTime.UtcNow.AddHours(4).ToString("dd/MM/yyyy HH:mm:ss"),
                                    CREATEDDATE = DateTime.UtcNow,
                                    UPDATEDDATE = DateTime.UtcNow
                                });
                            }


                        }
                        else
                        {
                            var request = new { fullname = item.FullName, dob = item.DOB, nationality = item.Nationality, type = item.Source, category = item.Type, CLIENTID = clientId, REMARKS = item.REMARKS };
                            var apiresponse = _clientHandler.PostAsync(request, ScreeningService.ADDTOBLACKLIST).Result;

                            if (!string.IsNullOrEmpty(apiresponse))
                            {
                                // Log to MongoDB
                                _mongoRepository.InsertBlockList(new NAMELIST
                                {
                                    UID = Guid.NewGuid().ToString(),
                                    FULLNAME = item.FullName,
                                    NATIONALITY = item.Nationality,
                                    TYPE = item.Source,
                                    CATEGORY = item.Type,
                                    REMARKS = item.REMARKS,
                                    DOB = new List<DOBLIST> { new DOBLIST { DOB = item.DOB } },
                                    STATUS = "A",
                                    CREATEDON = DateTime.UtcNow.AddHours(4).ToString("dd/MM/yyyy HH:mm:ss"),
                                    CREATEDDATE = DateTime.UtcNow,
                                    UPDATEDDATE = DateTime.UtcNow
                                });
                            }
                        }
                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage("Type should be individual or corporate");

                        return View("ViewExcelContent", _custExcelDataResponse);
                    }

                }
                if (!string.IsNullOrEmpty(apiresponse2))
                {
                    SourceUploadLogsDTO uploadLogsDTO = new SourceUploadLogsDTO();
                    uploadLogsDTO.Source = model.Source;
                    uploadLogsDTO.TotalRecords = totalrecords;

                    _internalWatchListService.InsertUploadLogs(uploadLogsDTO);
                }


                watchlistModel.CreatedBy = _clientHandler.GetUserId();
                
                watchlistModel.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
                watchlistModel.ExcelData = _mapper.Map<List<ExcelData>>(_custExcelDataResponse);
                EtlBatchDTO _etlBatchDTO = new EtlBatchDTO()
                {
                    FileName = _documentsModel.DocName,
                    FileFullPath = _documentsModel.DocFullPath,
                    TotalRows = watchlistModel.ExcelData.Count,
                    RowsRecorded = 0,
                    AddedBy = _documentsModel.AddedBy,
                    Type = (int)LogModulle.InternalWatchlist
                };
                int resp = _etlLogRepository.Create(_etlBatchDTO).Result;
                if (resp > 0)
                {
                    return Json(new { success = true, total = totalrecords, message = "Bulk Watch List uploaded successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Something went wrong during logs recording." });
                }
            }
            return Json(new { success = false, message = "No document provided." });
        }
        [HttpGet]
        public ActionResult DownloadInternalWatchList(string filename)
        {
            string filePath = @"Files/Internal_WatchList.xlsx";
            string fileName = "Internal WatchList.xlsx";
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/force-download", fileName);
        }
        public ActionResult DownloadUAEIECLISTInternalWatchList(string filename)
        {
            string filePath = @"Files/UaeIceList_Internal_WatchList.xlsx";
            string fileName = "UAE IEC List Internal WatchList.xlsx";
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/force-download", fileName);
        }
        [HttpGet]
        public ActionResult DownloadOtherSample(string filename)
        {
            string filePath = @"Files/Other_InternalWatchList.xlsx";
            string fileName = "Other Internal WatchList.xlsx";
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/force-download", fileName);
        }

        [HttpGet("InternalWatchList/Edit/{Uid}")]
        public IActionResult Edit(string Uid)
        {
            var clientId = _clientHandler.GetClientId();
            var request = new { UID = Uid, client_id=clientId };
            var apiresponse2 = _clientHandler.PostAsync(request, ScreeningService.GETINTERNALWATCHLISTBYUID).Result;
            WatchListModel model = JsonConvert.DeserializeObject<WatchListModel>(apiresponse2);

            
            model.CreatedBy = _clientHandler.GetUserId();
            
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(WatchListModel model)
        {
            var clientId = _clientHandler.GetClientId();
            try
            {
                if (ModelState.IsValid)
                {
                    var apiresponse = "";
                    if (model.Source == "UAE IEC LIST")
                    {
                        var request = new { FULLNAME = model.FullName, DOB = model.DOB, NATIONALITY = model.Nationality, TYPE = model.Source, CATEGORY = model.Type, IDNUMBER = model.IdNumber, REMARKS = model.Remarks,UID=model.UID };
                        apiresponse = _clientHandler.PostAsync(request, ScreeningService.UPDATETOBLACKLIST).Result;


                    }
                    else
                    {
                        var request2 = new { fullname = model.FullName, dob = model.DOB, nationality = model.Nationality, type = model.Source, category = model.Type, IDNUMBER = model.IdNumber, CLIENTID = clientId, REMARKS = model.Remarks };
                        apiresponse = _clientHandler.PostAsync(request2, ScreeningService.ADDTOBLACKLIST).Result;

                    }
                    //model.Nationality = _mapper.Map<CountryModel>(_countryService.GetDetails(model.NationalityId)).Name;

                    //var result = _sanctionService.Create(_mapper.Map<WatchListDTO>(model));
                    if (!string.IsNullOrEmpty(apiresponse))
                    {
                        // Log to MongoDB (Update history)
                        _mongoRepository.InsertBlockList(new NAMELIST
                        {
                            UID = model.UID ?? Guid.NewGuid().ToString(),
                            FULLNAME = model.FullName,
                            NATIONALITY = model.Nationality,
                            TYPE = model.Source,
                            CATEGORY = model.Type,
                            REMARKS = model.Remarks,
                            IDDETAILS = new List<IDDETAIL> { new IDDETAIL { IDNUMBER = model.IdNumber } },
                            DOB = new List<DOBLIST> { new DOBLIST { DOB = model.DOB } },
                            STATUS = "A",
                            CREATEDON = DateTime.UtcNow.AddHours(4).ToString("dd/MM/yyyy HH:mm:ss"),
                            CREATEDDATE = DateTime.UtcNow,
                            UPDATEDDATE = DateTime.UtcNow
                        });

                        _toastNotification.AddSuccessToastMessage("Internal Watchlist Updated successfully");
                        return RedirectToAction("GetBlocklistUpdateLogs", "Report");
                    }
                    else
                    {
                        _toastNotification.AddSuccessToastMessage("Something went wrong.please try again.");
                        model.CreatedBy = _clientHandler.GetUserId();
                        model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
                        return View(model);
                    }


                }
                else
                {
                    model.CreatedBy = _clientHandler.GetUserId();
                    model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
                    return View(model);
                }
                _toastNotification.AddSuccessToastMessage("Watch List added successfully");
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Status"] = 500;
                return View(model);
            }
        }


    }
}
