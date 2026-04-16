using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Enum;
using AML.Core.RepositoryContract.EtlBatch;
using AML.Core.RepositoryContract.InternalWatchList;
using AML.Core.Service.CustomerCase;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.InternalWatchList;
using AML.Core.ServiceContract.Sanction;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.EtlBatch;
using AML.DTO.DTO.FreeSource;
using AML.DTO.DTO.InternalWatchListExcel;
using AML.DTO.DTO.Sanction;
using AML.ViewModel.ViewModels.ApiAuthentication;
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
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.DataValidation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Linq;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;

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
        private ICustomerCaseService _customerCaseService;
        public InternalWatchListController(
            IMapper mapper,
            IToastNotification toastNotification,
            IHttpClientHandler clientHandler, ICustomerCaseService customerCaseService,
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
            _customerCaseService = customerCaseService;
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

                            DataSetsScreeinglogsModel uploadLogsDTO = new DataSetsScreeinglogsModel();

                            uploadLogsDTO.Datasets = model.Source;
                            uploadLogsDTO.Cumulative = "1";
                            uploadLogsDTO.CreatedOn = DateTime.Now;

                            // Initialize
                            uploadLogsDTO.Delta = "0";
                            uploadLogsDTO.Individual = "0";
                            uploadLogsDTO.Corporate = "0";

                            // ? Based on Type
                            if (model.Type == "INDIVIDUAL")
                            {
                                uploadLogsDTO.Delta = "+1";
                                uploadLogsDTO.Individual = "+1";
                            }
                            else if (model.Type == "CORPORATE")
                            {
                                uploadLogsDTO.Delta = "+1";
                                uploadLogsDTO.Corporate = "+1";
                            }
                            uploadLogsDTO.CreatedOn = DateTime.Now;
                            // Call service
                            _customerCaseService.InsertDatasetsScreeninglogs(uploadLogsDTO);
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
            try
            {
                if (model.Document != null)
                {
                    DocumentsModel _documentsModel = _fileUploader.UploadFile(model.ClientId, ItemType.InternalWatchList, _clientHandler.GetBranchId(), model.Document.fileUpload);
                    _documentsModel.AddedBy = _clientHandler.GetUserId();
                    var totalrecords = 0;
                    var apiresponse2 = "";

                    int individualCount = 0;
                    int corporateCount = 0;
                    string lastSource = "";

                    List<InternalWatcListExcelDTO> _custExcelDataResponse = _internalWatchListService.LoadInternalWatchExcelData(_documentsModel.DocFullPath, 1).Result;
                    foreach (var item in _custExcelDataResponse)
                    {
                        // Map user-friendly Excel labels back to API internal codes
                        if (!string.IsNullOrEmpty(item.Type))
                        {
                            if (item.Type.Equals("Corporate", StringComparison.OrdinalIgnoreCase)) item.Type = "CORPORATE";
                            else if (item.Type.Equals("Individual", StringComparison.OrdinalIgnoreCase)) item.Type = "INDIVIDUAL";
                        }

                        if (!string.IsNullOrEmpty(item.Source))
                        {
                            if (item.Source.Equals("Central Bank Watch List", StringComparison.OrdinalIgnoreCase)) item.Source = "CBWL";
                            else if (item.Source.Equals("Block List", StringComparison.OrdinalIgnoreCase)) item.Source = "INTERNAL";
                            else if (item.Source.Equals("UAE IEC", StringComparison.OrdinalIgnoreCase)) item.Source = "UAE IEC LIST";
                        }

                        if (item.Type == "INDIVIDUAL" || item.Type == "CORPORATE")
                        {
                            if (item.Source == "UAE IEC LIST")
                            {
                                var request = new { fullname = item.FullName, dob = item.DOB, nationality = item.Nationality, type = item.Source, category = item.Type, REMARKS = item.REMARKS };
                                apiresponse2 = _clientHandler.PostAsync(request, ScreeningService.ADDTOBLACKLIST).Result;

                                if (!string.IsNullOrEmpty(apiresponse2))
                                {
                                    model.Source = item.Source;
                                    totalrecords++;
                                    lastSource = item.Source;

                                    if (item.Type == "INDIVIDUAL") individualCount++;
                                    else if (item.Type == "CORPORATE") corporateCount++;

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
                                    totalrecords++;

                                    if (item.Type == "INDIVIDUAL") individualCount++;
                                    else if (item.Type == "CORPORATE") corporateCount++;

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
                            return Json(new { success = false, message = $"Invalid Type '{item.Type}' in row. Must be 'Individual' or 'Corporate'." });
                        }
                    }

                    if (totalrecords > 0 && !string.IsNullOrEmpty(lastSource))
                    {
                        DataSetsScreeinglogsModel datasetLog = new DataSetsScreeinglogsModel
                        {
                            Datasets = lastSource,
                            Delta = $"+{totalrecords}",
                            Individual = $"+{individualCount}",
                            Corporate = $"+{corporateCount}",
                            Cumulative = totalrecords.ToString(),
                            CreatedOn = DateTime.Now
                        };
                        _customerCaseService.InsertDatasetsScreeninglogs(datasetLog);
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
                    return resp > 0
                        ? Json(new { success = true, total = totalrecords, message = "Bulk Watch List uploaded successfully" })
                        : Json(new { success = false, message = "Something went wrong during logs recording." });
                }
                return Json(new { success = false, message = "No document provided." });
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = $"Upload failed: {innerMsg}" });
            }
        }
        [HttpGet]
        public ActionResult DownloadInternalWatchList(string filename)
        {
            string filePath = @"Files/Internal_WatchList.xlsx";
            string fileName = "Internal WatchList.xlsx";
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/force-download", fileName);
        }
        [HttpGet]
        public ActionResult DownloadUAEIECLISTInternalWatchList()
        {
            var clientId = _clientHandler.GetClientId();
            var nationalities = _countryService.GetAll(clientId).Select(x => x.Name).ToList();

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Blocklist Template");
                var hiddenSheet = package.Workbook.Worksheets.Add("Data");
                hiddenSheet.Hidden = eWorkSheetHidden.VeryHidden;

                // Headers
                string[] headers = { "Type", "Full Name", "Nationality/Country of Incorporation", "Date of Birth/Incorporation", "Data Source", "Id Number", "Remarks" };
                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.Cells[1, i + 1].Value = headers[i];
                    sheet.Cells[1, i + 1].Style.Font.Bold = true;
                    sheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Populate Hidden Sheet
                // Column A: Type
                hiddenSheet.Cells[1, 1].Value = "Individual";
                hiddenSheet.Cells[2, 1].Value = "Corporate";
                
                // Column B: Nationalities
                for (int i = 0; i < nationalities.Count; i++)
                {
                    hiddenSheet.Cells[i + 1, 2].Value = nationalities[i];
                }
                
                // Column C: Source
                hiddenSheet.Cells[1, 3].Value = "Central Bank Watch List";
                hiddenSheet.Cells[2, 3].Value = "Block List";
                hiddenSheet.Cells[3, 3].Value = "UAE IEC";

                // Data Validations
                // 1. Type (Col A)
                var typeValidation = sheet.DataValidations.AddListValidation("A2:A1000");
                typeValidation.Formula.ExcelFormula = "Data!$A$1:$A$2";
                typeValidation.ShowErrorMessage = true;
                typeValidation.ErrorTitle = "Invalid Type";
                typeValidation.Error = "Please select from the dropdown";

                // 2. Nationality (Col C)
                var nationalityValidation = sheet.DataValidations.AddListValidation("C2:C1000");
                nationalityValidation.Formula.ExcelFormula = $"Data!$B$1:$B${nationalities.Count}";
                nationalityValidation.ShowErrorMessage = true;
                nationalityValidation.ErrorTitle = "Invalid Nationality";
                nationalityValidation.Error = "Please select from the dropdown";

                // 3. Source (Col E)
                var sourceValidation = sheet.DataValidations.AddListValidation("E2:E1000");
                sourceValidation.Formula.ExcelFormula = "Data!$C$1:$C$3";
                sourceValidation.ShowErrorMessage = true;
                sourceValidation.ErrorTitle = "Invalid Source";
                sourceValidation.Error = "Please select from the dropdown";

                // 4. DOB (Col D)
                sheet.Cells["D2:D1000"].Style.Numberformat.Format = "yyyy-mm-dd";
                var dobValidation = sheet.DataValidations.AddDateTimeValidation("D2:D1000");
                dobValidation.Operator = ExcelDataValidationOperator.greaterThan;
                dobValidation.Formula.Value = new DateTime(1900, 1, 1);
                dobValidation.ShowErrorMessage = true;
                dobValidation.ErrorTitle = "Invalid Date";
                dobValidation.Error = "Please enter a valid date (yyyy-mm-dd)";

                sheet.Cells.AutoFitColumns();
                sheet.Column(2).Width = 30; // Full Name
                sheet.Column(7).Width = 40; // Remarks

                var fileBytes = package.GetAsByteArray();
                string fileName = "Internal_WatchList_Template.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
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
            var request = new { UID = Uid, client_id = clientId };
            var apiresponse2 = _clientHandler.PostAsync(request, ScreeningService.GETINTERNALWATCHLISTBYUID).Result;

            // Deserialize into a list since the API returns an array
            var apiList = JsonConvert.DeserializeObject<List<dynamic>>(apiresponse2);
            var item = apiList?.FirstOrDefault();

            WatchListModel model = new WatchListModel();
            if (item != null)
            {
                model.UID = (string)item.uid;
                model.FullName = (string)item.fullname;
                model.DOB = (string)item.dob;
                model.Nationality = (string)item.nationality == "0" ? null : (string)item.nationality;
                model.Source = (string)item.type; // API 'type' field holds the Source (CBWL, etc.)
                model.Type = (string)item.category; // API 'category' field holds the Type (INDIVIDUAL, etc.)
                model.IdNumber = (string)item.idnumber;
                model.Remarks = (string)item.remarks;
            }

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
