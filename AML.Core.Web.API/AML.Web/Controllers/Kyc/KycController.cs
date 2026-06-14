using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Enum;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.Kyc;
using AML.DTO.DTO.CaseDocument;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Kyc;
using AML.ViewModel.ViewModels.CaseDocument;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.Corporate;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.Report;
using AML.ViewModel.ViewModels.RiskAPI;
using AML.Web.Helper;
using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AML.Core.Service.Common.CommonService;

namespace AML.Web.Controllers.Kyc
{
    public class KycController : Controller
    {
        private IMapper _mapper;
        private IKycService _kycService;
        private readonly IToastNotification _toastNotification;
        private ICountryService _countryService;
        private ICustomerCaseService _customerCaseService;
        private ICommonService _commonService;
        private IConfiguration _configuration;
        private string baseURL = string.Empty;
        private string baseC6URL = string.Empty;
        private int checkThreshold = 0;
        private IHttpClientHandler _clientHandler;
        private List<ApiResultModel> matchrecordsList = new List<ApiResultModel>();
        private CorporateScreeningModel res = new CorporateScreeningModel();
        private IViewRenderService _viewRenderService;
        private RiskAPIController _riskAPIController;
        private readonly IStringLocalizer<KycController> _localizer;
        private string culture = CultureInfo.CurrentCulture.Name;
        IFileUploader _fileUploader;
        private ICaseDocumentService _caseDocumentService;
        public KycController(IConfiguration configuration, IMapper mapper, IKycService kycService,
            IToastNotification toastNotification, ICountryService countryService, ICustomerCaseService customerCaseService, ICommonService commonService, IHttpClientHandler clientHandler, IViewRenderService viewRenderService, RiskAPIController riskAPIController,
            IStringLocalizer<KycController> localizer, IFileUploader fileUploader, ICaseDocumentService caseDocumentService)
        {
            _mapper = mapper;
            _kycService = kycService;
            _toastNotification = toastNotification;
            _countryService = countryService;
            _customerCaseService = customerCaseService;
            _commonService = commonService;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            baseC6URL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            checkThreshold = configuration.GetSection("C6BaseApiUrl").GetSection("Threshold").Value.ParseInt();
            _configuration = configuration;
            _clientHandler = clientHandler;
            _viewRenderService = viewRenderService;
            _riskAPIController = riskAPIController;
            _localizer = localizer;
            _fileUploader = fileUploader;
            _caseDocumentService = caseDocumentService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("Kyc/Corporate")]
        public IActionResult CreateCorporateKyc()
        {
            var ClientId = _clientHandler.GetClientId();
            var CustomerType = "C";
            CorporateKycModel model = new CorporateKycModel();
            model.EntityAddress = new Address();
            // var list = _mapper.Map<List<CountryModel>>(_countryService.GetAll(ClientId));
            var countrylist = _mapper.Map<List<CountryModel>>(_countryService.GetAll(ClientId)); //Country list from country master table
            model.CountryofIncorporationList = new SelectList(countrylist, "Name", "Name");
            //var list = _mapper.Map<List<CountryModel>>(_countryService.GetAllCountryRiskConfig(culture)); //Countryof incorporation from Risk Table
            //model.CountryofIncorporationList = new SelectList(list, "Name", "Name");
            
            model.EntityAddress.CountryList = new SelectList(countrylist, "Name", "Name");

            model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, ClientId)), "ProductName", "ProductName");
            model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, ClientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.PartnerNationalityList = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAllPartnerNationality(ClientId)), "Name", "Name");//Partner Nationality from risk Table
            model.BusinessTypeList = new SelectList(_mapper.Map<List<BusinessNature>>(_kycService.GetBusinessType(culture, CustomerType, ClientId)), "BusinessName", "BusinessName");
            model.EntityType = new SelectList(_mapper.Map<List<LegalStatusModel>>(_kycService.GetLegalStatus(culture, CustomerType, ClientId)), "LegalStatus", "LegalStatus");
            model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, ClientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            model.DateofIncorporation = new DateTime();
            return View(model);
        }
        [HttpPost("Kyc/Corporate")]
        public async Task<ActionResult> CreateCorporateKyc(CorporateKycModel model)
        {
            Console.WriteLine("Start KYC for corporate");

            checkThreshold = model.Threshold;

            CorporateScreeningModel corporateScreeningModel = new CorporateScreeningModel();
            corporateScreeningModel.CorporateDetailList = new List<CorporateDetailsModel>();
            List<CorporateDetailsModel> corplist = corporateScreeningModel.CorporateDetailList;

            Console.WriteLine("Create corporate details list");

            CorporateDetailsModel corpdetaillist = new CorporateDetailsModel()
            {
                CustomerCode = "",
                CompanyName = model.FullName,
                LicenseName = "",
                LicenseNumber = model.CommercialLicense.LicenseNumber,
                LicenseIssueDate = model.CommercialLicense.LicenseIssueDate,
                LicenseExpiryDate = model.CommercialLicense.LicenseExpiryDate,
                AccomplishedDate = model.DateofIncorporation,
                AccomplishedCountry = model.PlaceofIncorporation != "0" ? model.PlaceofIncorporation : " ",
                MobileNo = model.Telephone,
                CreatedBy = _clientHandler.GetUserId(),
                IsMatched = false,
                MatchScore = 0,
                IsWhiteListed = "NO",
                isRemoved = false,
                ApiResultJsonCorp = "",
                CustomerDetailList = null,
                C6Threshold = model.C6Threshold,
                Threshold = model.Threshold,
            };
            var Firstname = model.FullName;
            Console.WriteLine("Adding main corporate to corporate list");

            corplist.Add(corpdetaillist);
            corplist[0].CustomerDetailList = new List<CustomeDetailsModel>();

            Console.WriteLine("Adding shareholders to main corporate");

            foreach (PersonDetails person in model.Partners.Where(m => m.IsRemoved == false))
            {
                CustomeDetailsModel partnerDetails = new CustomeDetailsModel()
                {
                    LastName = person.Name,
                    Nationality = person.Nationality != "0" ? person.Nationality : "",
                    Designation = person.Designation,
                    EmiratesIdNumber = person.EmiratesID,
                    EmiratesIdExpiry = person.EmiratesIDExpiry,
                    CustomerIdType = "Passport Number",
                    CustomerIdNumber = person.PassportNumber,
                    CustomerIdExpiry = person.PassportExpiry,
                    SharePercent = person.Percentage,
                    CustomerType = "Shareholder",
                    CorporateType = "Shareholder",
                    C6Threshold = model.C6Threshold,
                    Threshold = model.Threshold,

                };
                corplist[0].CustomerDetailList.Add(partnerDetails);
            }

            Console.WriteLine($"{corplist[0].CustomerDetailList.Count()} customers in main corporate customer list");

            Console.WriteLine("Adding senior management to main corporate");

            foreach (var person in model.SeniorManagements.Where(m => m.IsRemoved == false))
            {
                CustomeDetailsModel SeniorManagementList = new CustomeDetailsModel()
                {
                    LastName = person.Name,
                    Nationality = person.Nationality != "0" ? person.Nationality : "",
                    Designation = person.Designation,
                    EmiratesIdNumber = person.EmiratesID,
                    EmiratesIdExpiry = person.EmiratesIDExpiry,
                    CustomerIdType = "Passport Number",
                    CustomerIdNumber = person.PassportNumber,
                    CustomerIdExpiry = person.PassportExpiry,
                    SharePercent = person.Percentage,
                    CustomerType = "Senior Management",
                    C6Threshold = model.C6Threshold,
                    Threshold = model.Threshold,
                };
                corplist[0].CustomerDetailList.Add(SeniorManagementList);
            }
            Console.WriteLine($"{corplist[0].CustomerDetailList.Count()} customers in main corporate customer list");
            Console.WriteLine("Adding authorised signatories to main corporate");

            foreach (var person in model.AuthorisedSignatories.Where(m => m.IsRemoved == false))
            {
                CustomeDetailsModel AuthorisedSignatories = new CustomeDetailsModel()
                {

                    LastName = person.Name,
                    Nationality = person.Nationality != "0" ? person.Nationality : "",
                    Designation = person.Designation,
                    EmiratesIdNumber = person.EmiratesID,
                    EmiratesIdExpiry = person.EmiratesIDExpiry,
                    CustomerIdType = "Passport Number",
                    CustomerIdNumber = person.PassportNumber,
                    CustomerIdExpiry = person.PassportExpiry,
                    SharePercent = person.Percentage,
                    CustomerType = "Authorised Signatories",
                    C6Threshold = model.C6Threshold,
                    Threshold = model.Threshold
                };
                corplist[0].CustomerDetailList.Add(AuthorisedSignatories);
            }

            Console.WriteLine($"{corplist[0].CustomerDetailList.Count()} customers in main corporate customer list");

            var custId = "";
            var partnerId = new List<string>();
            var SeniorMgtId = new List<string>();
            var AuthorisedId = new List<string>();
            var GroupEntityId = new List<string>();
            var customeridlist = new List<string>();
            var responseList = new List<string>();


            Console.WriteLine("Adding group entities");

            foreach (var item in model.GroupEntity.Where(_ => _.IsRemoved == false))
            {
                CorporateDetailsModel groupentitylist = new CorporateDetailsModel()
                {
                    CompanyName = item.EntityName,
                    Nationality = item.PlaceofIncorporation,
                    AccomplishedCountry = item.PlaceofIncorporation != "0" ? item.PlaceofIncorporation : " ",
                    CreatedBy = _clientHandler.GetUserId(),
                    CorporateType = "Group Entity",
                    C6Threshold = model.C6Threshold,
                    Threshold = model.Threshold
                };
                corplist.Add(groupentitylist);



            }

            Console.WriteLine($"Added {corplist.Count() - 1} group entities");

            int i = 0;
            int entity = 0;


            foreach (var corporate in corplist)
            {
                corporate.ClientId = _clientHandler.GetClientId();
                corporate.FirstName = Firstname;
                corporate.GroupEntityOf = custId;
                var customerCodeprefix= _mapper.Map<ClientMasterDTO>(_customerCaseService.GetCustomerCodeprefixByclient(corporate.ClientId));
                corporate.customerCodeprefix = customerCodeprefix.Prefix;

                if (string.IsNullOrEmpty(corporate.CompanyName)) { continue; }

                var responseLists = new List<Tuple<string, string>>();

                Console.WriteLine($"Send corporate {corporate.CompanyName} for screening");

                if (corporate.CorporateType != "Group Entity")
                {
                    (responseLists, _) = _customerCaseService.SaveCorporateScreening(_mapper.Map<CorporateScreeningDTO>(corporate));

                    custId = responseLists[0].Item1;
                    model.CustomerId = custId;
                    customeridlist.Add(responseLists[0].Item1);

                    //Document upload 
                    if (model.CaseDocumentsL != null)
                    {
                        foreach (var item in model.CaseDocumentsL)
                        {
                            if (item.Document != null)
                            {
                                CaseDocumentModel _caseDoc = new CaseDocumentModel();
                                _caseDoc.CaseId = _customerCaseService.GetCaseId(custId).ToString();
                                _caseDoc.CreatedBy = _clientHandler.GetUserId();
                                _caseDoc.CreatedOn = DateTime.Now;
                                _caseDoc.ClientId = _clientHandler.GetClientId();
                                DocumentsModel _documentsModel = _fileUploader.UploadFile(_caseDoc.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), item.Document);
                                _caseDoc.DocumentFileName = item.Document.FileName;
                                _caseDoc.DocumentFullPath = _documentsModel.DocFullPath;
                                _caseDoc.IssuedDateOnDB = item.IssuedDate;
                                _caseDoc.ExpiryDateOnDB = item.ExpiryDate;
                                _caseDoc.DocumentName = item.DocumentName;
                                var resp = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
                            }
                        }
                    }


                    int sign = 0, share = 0, srMngr = 0;
                    foreach (var li in responseLists)
                    {
                        if (li.Item2 == "Shareholder")
                        {
                            partnerId.Add(li.Item1);
                            customeridlist.Add(li.Item1);

                            CaseDocumentModel _caseDoc = new CaseDocumentModel();
                            if (model.Partners[share].CaseDocumentsL != null)
                            {
                                foreach (var item in model.Partners[share].CaseDocumentsL)
                                {
                                    if (item.Document != null)
                                    {
                                        _caseDoc.CaseId = _customerCaseService.GetCaseId(li.Item1).ToString();
                                        _caseDoc.CreatedBy = _clientHandler.GetUserId();
                                        _caseDoc.CreatedOn = DateTime.Now;
                                        _caseDoc.ClientId = _clientHandler.GetClientId();
                                        DocumentsModel _documentsModel = _fileUploader.UploadFile(_caseDoc.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), item.Document);
                                        _caseDoc.DocumentFileName = item.Document.FileName;
                                        _caseDoc.DocumentFullPath = _documentsModel.DocFullPath;
                                        _caseDoc.CreatedBy = _clientHandler.GetUserId();
                                        _caseDoc.CreatedOn = DateTime.Now;
                                        _caseDoc.IssuedDateOnDB = item.IssuedDate;
                                        _caseDoc.ExpiryDateOnDB = item.ExpiryDate;
                                        _caseDoc.DocumentName = item.DocumentName;
                                        var resp = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
                                    }
                                }
                            }


                            share++;
                        }
                        else if (li.Item2 == "Senior Management")
                        {
                            SeniorMgtId.Add(li.Item1);
                            customeridlist.Add(li.Item1);

                            CaseDocumentModel _caseDoc = new CaseDocumentModel();
                            if (model.SeniorManagements[srMngr].CaseDocumentsL != null)
                            {
                                foreach (var item in model.SeniorManagements[srMngr].CaseDocumentsL)
                                {
                                    if (item.Document != null)
                                    {
                                        _caseDoc.CaseId = _customerCaseService.GetCaseId(li.Item1).ToString();
                                        _caseDoc.CreatedBy = _clientHandler.GetUserId();
                                        _caseDoc.CreatedOn = DateTime.Now;
                                        _caseDoc.ClientId = _clientHandler.GetClientId();
                                        DocumentsModel _documentsModel = _fileUploader.UploadFile(_caseDoc.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), item.Document);
                                        _caseDoc.DocumentFileName = item.Document.FileName;
                                        _caseDoc.DocumentFullPath = _documentsModel.DocFullPath;
                                        _caseDoc.CreatedBy = _clientHandler.GetUserId();
                                        _caseDoc.CreatedOn = DateTime.Now;
                                        _caseDoc.IssuedDateOnDB = item.IssuedDate;
                                        _caseDoc.ExpiryDateOnDB = item.ExpiryDate;
                                        _caseDoc.DocumentName = item.DocumentName;
                                        var resp = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
                                    }
                                }
                            }

                            srMngr++;
                        }
                        else if (li.Item2 == "Authorised Signatories")
                        {
                            AuthorisedId.Add(li.Item1);
                            customeridlist.Add(li.Item1);

                            CaseDocumentModel _caseDoc = new CaseDocumentModel();
                            if (model.AuthorisedSignatories[sign].CaseDocumentsL != null)
                            {
                                foreach (var item in model.AuthorisedSignatories[sign].CaseDocumentsL)
                                {
                                    if (item.Document != null)
                                    {
                                        _caseDoc.CaseId = _customerCaseService.GetCaseId(li.Item1).ToString();
                                        _caseDoc.CreatedBy = _clientHandler.GetUserId();
                                        _caseDoc.CreatedOn = DateTime.Now;
                                        _caseDoc.ClientId = _clientHandler.GetClientId();
                                        DocumentsModel _documentsModel = _fileUploader.UploadFile(_caseDoc.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), item.Document);
                                        _caseDoc.DocumentFileName = item.Document.FileName;
                                        _caseDoc.DocumentFullPath = _documentsModel.DocFullPath;
                                        _caseDoc.CreatedBy = _clientHandler.GetUserId();
                                        _caseDoc.CreatedOn = DateTime.Now;
                                        _caseDoc.IssuedDateOnDB = item.IssuedDate;
                                        _caseDoc.ExpiryDateOnDB = item.ExpiryDate;
                                        _caseDoc.DocumentName = item.DocumentName;
                                        var resp = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
                                    }
                                }
                            }


                            sign++;
                        }
                    }

                    //foreach (var li in responseLists)
                    //{
                    //    if (li.Item2 == "Shareholder")
                    //    {
                    //        partnerId.Add(li.Item1);
                    //        customeridlist.Add(li.Item1);
                    //    }
                    //    else if (li.Item2 == "Senior Management")
                    //    {
                    //        SeniorMgtId.Add(li.Item1);
                    //        customeridlist.Add(li.Item1);
                    //    }
                    //    else if (li.Item2 == "Authorised Signatories")
                    //    {
                    //        AuthorisedId.Add(li.Item1);
                    //        customeridlist.Add(li.Item1);
                    //    }
                    //}
                    var kycresult = _kycService.CreateCorporate(_mapper.Map<CorporateKycDTO>(model));
                    Console.WriteLine($"Add main corporate details to DB result: {kycresult.Result}");
                }
                else
                {
                    Console.WriteLine("Corporate is group entity");
                    corporate.GroupEntityOf = custId;

                    corporate.customerCodeprefix = customerCodeprefix.Prefix;
                    responseList = _customerCaseService.GroupEntityScreeningDetails(_mapper.Map<CorporateScreeningDTO>(corporate));
                    foreach (var GroupEntity in responseList)
                    {
                        GroupEntityId.Add(GroupEntity);
                        customeridlist.Clear();
                        customeridlist.Add(GroupEntity);
                        if (model.GroupEntity[entity].CaseDocumentsL != null)
                        {
                            foreach (var item in model.GroupEntity[entity].CaseDocumentsL)
                            {
                                if (item.Document != null)
                                {
                                    CaseDocumentModel _caseDoc = new CaseDocumentModel();
                                    _caseDoc.CaseId = _customerCaseService.GetCaseId(GroupEntity).ToString();
                                    _caseDoc.CreatedBy = _clientHandler.GetUserId();
                                    _caseDoc.CreatedOn = DateTime.Now;
                                    _caseDoc.ClientId = _clientHandler.GetClientId();
                                    DocumentsModel _documentsModel = _fileUploader.UploadFile(_caseDoc.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), item.Document);
                                    _caseDoc.DocumentFileName = item.Document.FileName;
                                    _caseDoc.DocumentFullPath = _documentsModel.DocFullPath;
                                    _caseDoc.CreatedBy = _clientHandler.GetUserId();
                                    _caseDoc.CreatedOn = DateTime.Now;
                                    _caseDoc.IssuedDateOnDB = item.IssuedDate;
                                    _caseDoc.ExpiryDateOnDB = item.ExpiryDate;
                                    _caseDoc.DocumentName = item.DocumentName;
                                    var resp = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
                                }
                            }
                        }


                        entity++;
                    }
                    var kycresult = _kycService.CreateCorporate(_mapper.Map<CorporateKycDTO>(model));
                    Console.WriteLine($"Add main corporate details to DB result: {kycresult.Result}");
                }

                foreach (string _i in customeridlist)
                {
                    Console.WriteLine($"Screen corporate with id: {_i}");

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    };

                    var x = await _commonService.CustomerScreeningCall(_i, baseURL, baseC6URL, "CORPORATE", body,model.Threshold);
                    Console.WriteLine($"Got match score: {x.MatchScore}");

                    if (x.sendMail == 1)
                    {
                        Console.WriteLine("Sending email.");
                        int fileType = 4;
                        CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
                        using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                        {
                            var appBaseUrl = MyHttpContext.AppBaseUrl;
                            body = body.Replace("{customerID}", x.CustomerId.ToString());
                            body = body.Replace("{caseID}", x.Id.ToString());
                            body = body.Replace("{baseUrl}", appBaseUrl);
                            corporateScreeningModel.Url = appBaseUrl;

                            Document document = new Document(PageSize.A4, 15, 15, 15, 15);
                            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                            document.Open();
                            document.Add(new Paragraph("\n"));
                            PdfPTable logo = new PdfPTable(2);
                            logo.TotalWidth = 550f;
                            float[] logowidth = new float[] { 3f, 0.5f };
                            logo.SetWidths(logowidth);
                            logo.LockedWidth = true;
                            logo.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                            logo.DefaultCell.VerticalAlignment = 1;
                            logo.DefaultCell.HorizontalAlignment = 1;
                            logo.SpacingBefore = 20f;
                            logo.SpacingAfter = 30f;
                            logo.DefaultCell.Border = 0;
                            var companyName = HttpContext.Session.GetString("SessCompanyName");
                            PdfPCell compname = new PdfPCell(new Phrase(companyName, new Font(Font.FontFamily.TIMES_ROMAN, 17, Font.BOLD)));
                            compname.FixedHeight = 40f;
                            compname.VerticalAlignment = 1;
                            compname.HorizontalAlignment = 1;
                            compname.Border = 0;
                            logo.AddCell(compname);
                            string url = "wwwroot/img/logo.png";
                            Image tif = Image.GetInstance(url);
                            tif.ScalePercent(1f);
                            tif.SpacingBefore = 20f;
                            logo.AddCell(tif);
                            document.Add(logo);


                            PdfPTable header = new PdfPTable(1);
                            header.TotalWidth = 550f;
                            header.LockedWidth = true;
                            header.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                            header.SpacingAfter = 30f;
                            header.DefaultCell.Border = 0;
                            PdfPCell hd = new PdfPCell(new Phrase("Potential Hits"));
                            hd.HorizontalAlignment = 1;
                            hd.FixedHeight = 20f;
                            hd.VerticalAlignment = 1;
                            hd.Border = 0;
                            header.AddCell(hd);
                            document.Add(header);


                            if (x.CustomerType == "C")
                            {
                                PdfPTable headTab = new PdfPTable(1);
                                headTab.TotalWidth = 550f;
                                headTab.LockedWidth = true;
                                headTab.DefaultCell.Border = 0;
                                headTab.HorizontalAlignment = 1;
                                float[] widths1 = new float[] { 3f };
                                headTab.SetWidths(widths1);
                                headTab.SpacingAfter = 20f;
                                headTab.AddCell("Company Name : " + corporate.CompanyName);
                                headTab.AddCell("License Number : " + corporate.LicenseNumber);
                                headTab.AddCell("Mobile Number : " + corporate.MobileNo);
                                headTab.AddCell("Country Of Incorporation:" + corporate.AccomplishedCountry);
                                if (corporate.CorporateType == "Group Entity")
                                    headTab.AddCell("Corporate Typ:" + corporate.CorporateType);
                                var dateAndTime = corporate.AccomplishedDate;
                                var dob = dateAndTime.Date;
                                headTab.AddCell("Date Of Incorporation : " + dob.Day + "/" + dob.Month + "/" + dob.Year);
                                document.Add(headTab);
                            }
                            else
                            {
                                PdfPTable headTab = new PdfPTable(1);
                                headTab.TotalWidth = 550f;
                                headTab.LockedWidth = true;
                                headTab.DefaultCell.Border = 0;
                                headTab.HorizontalAlignment = 1;
                                float[] widths1 = new float[] { 3f };
                                headTab.SetWidths(widths1);
                                headTab.SpacingAfter = 20f;

                                headTab.AddCell("Full Name : " + corplist[0].CustomerDetailList[0].LastName);
                                headTab.AddCell("Customer Type:" + corplist[0].CustomerDetailList[0].CustomerType);
                                headTab.AddCell("Share Percent : " + corplist[0].CustomerDetailList[0].SharePercent);
                                headTab.AddCell("Designation:" + corplist[0].CustomerDetailList[0].Designation);
                                document.Add(headTab);
                            }

                            PdfPTable table = new PdfPTable(8);
                            table.TotalWidth = 550f;
                            table.LockedWidth = true;
                            float[] widths = new float[] { 0.5f, 1f, 1f, 1.5f, 3f, 1f, 1f, 1f };
                            table.SetWidths(widths);
                            table.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                            table.SpacingAfter = 30f;
                            PdfPCell cell1 = new PdfPCell(new Phrase("#", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                            cell1.HorizontalAlignment = 1;
                            cell1.VerticalAlignment = 1;
                            cell1.BackgroundColor = BaseColor.LIGHT_GRAY;
                            cell1.FixedHeight = 30f;
                            table.AddCell(cell1);
                            PdfPCell cell2 = new PdfPCell(new Phrase("ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                            cell2.HorizontalAlignment = 1;
                            cell2.VerticalAlignment = 1;
                            cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                            cell2.FixedHeight = 30f;
                            table.AddCell(cell2);
                            PdfPCell cell3 = new PdfPCell(new Phrase("TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                            cell3.HorizontalAlignment = 1;
                            cell3.VerticalAlignment = 1;
                            cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                            cell3.FixedHeight = 30f;
                            table.AddCell(cell3);
                            PdfPCell cell4 = new PdfPCell(new Phrase("CATEGORY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                            cell4.HorizontalAlignment = 1;
                            cell4.VerticalAlignment = 1;
                            cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                            cell4.FixedHeight = 30f;
                            table.AddCell(cell4);
                            PdfPCell cell5 = new PdfPCell(new Phrase("NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                            cell5.HorizontalAlignment = 1;
                            cell5.VerticalAlignment = 1;
                            cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                            cell5.FixedHeight = 30f;
                            table.AddCell(cell5);
                            PdfPCell cell6 = new PdfPCell(new Phrase("SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                            cell6.HorizontalAlignment = 1;
                            cell6.VerticalAlignment = 1;
                            cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                            cell6.FixedHeight = 30f;
                            table.AddCell(cell6);
                            PdfPCell cell7 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                            cell7.HorizontalAlignment = 1;
                            cell7.VerticalAlignment = 1;
                            cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                            cell7.FixedHeight = 30f;
                            table.AddCell(cell7);
                            PdfPCell cell8 = new PdfPCell(new Phrase("DOB", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                            cell8.HorizontalAlignment = 1;
                            cell8.VerticalAlignment = 1;
                            cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
                            cell8.FixedHeight = 30f;
                            table.AddCell(cell8);
                            if (x.ApiResultsjson == null)
                            {
                                downloadModel.ApiResultsjson = x.ApiResultsjsonCorp;
                            }
                            else
                            {
                                downloadModel.ApiResultsjson = x.ApiResultsjson;
                            }
                            for (int a = 0; a < downloadModel.ApiResultsjson.Count; a++)
                            {
                                table.AddCell(new Phrase((a + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                                table.AddCell(new Phrase(downloadModel.ApiResultsjson[a].matchuid, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                                table.AddCell(new Phrase(downloadModel.ApiResultsjson[a].matchtype, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                                table.AddCell(new Phrase(downloadModel.ApiResultsjson[a].matchcategory, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                                table.AddCell(new Phrase(downloadModel.ApiResultsjson[a].matchname, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                                table.AddCell(new Phrase(downloadModel.ApiResultsjson[a].matchscore, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                                table.AddCell(new Phrase(downloadModel.ApiResultsjson[a].nationality, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                                table.AddCell(new Phrase(downloadModel.ApiResultsjson[a].matchdob, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                            }
                            document.Add(table);

                            PdfPTable footTab = new PdfPTable(3);
                            footTab.TotalWidth = 550f;
                            footTab.LockedWidth = true;
                            footTab.DefaultCell.Border = 0;
                            float[] footWidth = new float[] { 20f, 40f, 40 };
                            footTab.SetWidths(footWidth);
                            footTab.HorizontalAlignment = 1;
                            footTab.SpacingAfter = 30f;
                            footTab.AddCell("Created By : ");
                            string username = HttpContext.Session.GetString("SessUsername");
                            footTab.AddCell(username);
                            footTab.AddCell("");
                            footTab.AddCell("Created On : ");
                            footTab.AddCell(DateTime.Now.ToString());
                            footTab.AddCell("");
                            footTab.AddCell("Url : ");
                            footTab.AddCell(appBaseUrl);
                            footTab.AddCell("");
                            document.Add(footTab);

                            PdfPTable footer1 = new PdfPTable(1);
                            footer1.TotalWidth = 550f;
                            footer1.LockedWidth = true;
                            footer1.DefaultCell.Border = 0;
                            footer1.AddCell("Please click on the VIEW report under CASE Tab for full details ");

                            document.Add(footer1);


                            document.Add(new Paragraph("\n"));
                            iTextSharp.text.pdf.draw.LineSeparator line1 = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1);
                            document.Add(new Chunk(line1));

                            PdfPTable footer2 = new PdfPTable(1);
                            footer2.TotalWidth = 550f;
                            footer2.LockedWidth = true;
                            footer2.DefaultCell.Border = 0;
                            footer2.AddCell("Computer generated report; hence no signature is required. ");
                            document.Add(footer2);

                            PdfContentByte content = writer.DirectContent;
                            Rectangle rectangle = new Rectangle(document.PageSize);
                            rectangle.Left += document.LeftMargin;
                            rectangle.Right -= document.RightMargin;
                            rectangle.Top -= document.TopMargin;
                            rectangle.Bottom += document.BottomMargin;
                            content.SetColorStroke(GrayColor.BLACK);
                            content.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                            content.Stroke();


                            document.Close();


                            byte[] data = memoryStream.ToArray();



                            if (!((x.MatchScore >= 0 && x.MatchScore < checkThreshold)))
                            {
                                var emailSent = await _commonService.SendHtmlFormattedEmailWithAttachment("Case creation Alert", body, res.ToString(), data);
                            }

                        }
                        i++;

                    }

                    if (x.IsMatched == 1 && x.MatchScore >= checkThreshold)
                    {
                        Console.WriteLine(string.Concat(_localizer["Customer blocked, Case created for "], Firstname));
                        _toastNotification.AddWarningToastMessage(string.Concat(_localizer["Customer blocked, Case created for "], x.FirstName, " ", x.LastName, ". \n"));
                        //resultString = string.Concat(resultString, "Customer blocked, Case created for ", x.FirstName, " ", x.LastName, ". \n");
                    }

                    else if (x.IsMatched == 0 && x.MatchScore == 0)
                    {
                        Console.WriteLine(string.Concat(_localizer["Customer Approved as there were no cases found for"], " ", x.FirstName, " ", x.LastName));
                        //         corporateScreeningModel.IsMatchedInt = 2;
                        _toastNotification.AddInfoToastMessage(string.Concat(_localizer["Customer Approved as there were no cases found for"], " ", x.FirstName, " ", x.LastName, ". \n"));
                        //return View(model);
                    }

                    else if (x.MatchScore >= 0 && x.MatchScore < checkThreshold)
                    {
                        Console.WriteLine(string.Concat(_localizer["Customer Approved as the matchscore is less than threshold"], " ", x.FirstName, " ", x.LastName));

                        if (x.CustomerType == "C")
                        {
                            foreach (var item in x.ApiResultsjson)
                            {
                                var matchrecords = new ApiResultModel();
                                matchrecords.matchuid = item.matchuid;
                                matchrecords.matchtype = item.matchtype;
                                matchrecords.matchcategory = item.matchcategory;
                                matchrecords.matchname = item.matchname;
                                matchrecords.matchscore = item.matchscore;
                                matchrecords.nationality = item.nationality;
                                matchrecordsList.Add(matchrecords);
                            }

                            res.ApiResultJson = matchrecordsList;
                        }
                        _toastNotification.AddInfoToastMessage(string.Concat(_localizer["Customer Approved as the matchscore is less than threshold for"], " ", x.FirstName, " ", x.LastName, ". \n"));
                    }
                    else
                    {
                        Console.WriteLine(string.Concat(_localizer["Customer Approved for"], x.LastName));

                        _toastNotification.AddInfoToastMessage(string.Concat(_localizer["Customer Approved for"], x.LastName, ". \n"));
                    }
                }
            }
            CorporateKycModel kycmodel = new CorporateKycModel();
            var ClientId = _clientHandler.GetClientId();

            //To check if risk assessment is enabled for the client.
            var result = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(ClientId));

            if (result != null)
            {
                Console.WriteLine("Generate risk");
                var list = _mapper.Map<List<CountryModel>>(_countryService.GetAll(ClientId));
                kycmodel.EntityAddress = new Address
                {
                    CountryList = new SelectList(list, "Name", "Name")
                };

                var isPep = "";

                //if (model.PEPStatus == 1)
                //{
                //    isPep = "Yes";
                //}
                //else
                //{
                //    isPep = "No";
                //}
                KycIndividualDTO imodel = new KycIndividualDTO();
                //model.IsPeP = isPep;
                var str1 = _kycService.GetRiskLovId(imodel, _mapper.Map<CorporateKycDTO>(model), "C", culture, ClientId);
                if (str1.Result == null)
                {
                    _toastNotification.AddWarningToastMessage(_localizer["Unable to calculate risk due to insufficient data."]);
                    return View(model);
                }
                var spStr1 = str1.Result.Split('Ø');
                var entlovId = spStr1[2];
                var buslovId = spStr1[4];
                var incorplovId = spStr1[3];
                var productlovId = spStr1[11];
                var deliverylovId = spStr1[12];
                var nationality1lovId = spStr1[6];
                var nationality2lovId = spStr1[7];
                var nationality3lovId = spStr1[8];
                var nationality4lovId = spStr1[9];
                var nationality5lovId = spStr1[10];
                var modeofpaymentlovId= spStr1[16];
                // var IsPeplovId = spStr1[14];

                var str = _kycService.GetRiskTypeId(imodel, _mapper.Map<CorporateKycDTO>(model), "C", culture, ClientId);

                if (str.Result == null)
                {
                    _toastNotification.AddWarningToastMessage(_localizer["Unable to calculate risk due to insufficient data."]);
                    return View(kycmodel);
                }
                var spStr = str.Result.Split('Ø');
                var entId = spStr[2];
                var busId = spStr[4];
                var incorpId = spStr[3];
                var productId = spStr[11];
                var deliveryId = spStr[12];
                var nationality1Id = spStr[6];
                var nationality2Id = spStr[7];
                var nationality3Id = spStr[8];
                var nationality4Id = spStr[9];
                var nationality5Id = spStr[10];
                var modeofpaymentId= spStr[16];
                //var IsPepId = spStr[14];
                Console.WriteLine(
                    $"entId: {entId}\n" +
                    $"busId: {busId}\n" +
                    $"incorpId: {incorpId}\n" +
                    $"productId: {productId}\n" +
                    $"deliveryId: {deliveryId}\n" +
                    $"nationality1Id: {nationality1Id}\n" +
                    $"nationality2Id: {nationality2Id}\n" +
                    $"nationality3Id: {nationality3Id}\n" +
                    $"nationality4Id: {nationality4Id}\n" +
                    $"nationality5Id: {nationality5Id}\n"
                );

                RiskAPIRequestModel riskModel = new RiskAPIRequestModel();
                riskModel.CustomerId = custId;
                riskModel.CustomerName = model.FullName;
                riskModel.MainNationality = model.PlaceofIncorporation;
                riskModel.ClientId = _clientHandler.GetClientId();
                riskModel.CreatedBy = _clientHandler.GetUserId();

                if (model.EntityAddress.Country == "0")
                {
                    riskModel.MainNationality = "";
                }
                else
                {
                    riskModel.MainNationality = model.EntityAddress.Country;
                }
                riskModel.RiskCategory = "C";


                var riskTypeList = new List<RiskTypeListModel>();
                if (entId != "0" || busId != "0" || incorpId != "0" || nationality1Id != "0" || nationality2Id != "0" || nationality3Id != "0" || nationality4Id != "0" || nationality5Id != "0" || productId != "0" || deliveryId != "0" || modeofpaymentId != "0")
                {
                    //for legal status of entity start
                    if (entId != "0")
                    {
                        var riskType3 = new RiskTypeListModel();
                        riskType3.Id = Convert.ToString(entlovId);
                        var riskItem3 = new RiskItemListModel();
                        riskItem3.Id = entId.ToString();
                        var riskItemList3 = new List<RiskItemListModel>();
                        riskItemList3.Add(riskItem3);
                        riskType3.RiskItemList = riskItemList3;
                        riskTypeList.Add(riskType3);
                    }
                    //for legal status of entity end
                    //for nature of business start
                    if (busId != "0")
                    {
                        var riskType1 = new RiskTypeListModel();
                        riskType1.Id = Convert.ToString(buslovId);
                        var riskItem1 = new RiskItemListModel();
                        riskItem1.Id = busId.ToString();
                        var riskItemList1 = new List<RiskItemListModel>();
                        riskItemList1.Add(riskItem1);
                        riskType1.RiskItemList = riskItemList1;
                        riskTypeList.Add(riskType1);
                    }
                    //for nature of business end
                    //for country of incorporation start
                    if (incorpId != "0")
                    {
                        var riskType4 = new RiskTypeListModel();
                        riskType4.Id = Convert.ToString(incorplovId);
                        var riskItem4 = new RiskItemListModel();
                        riskItem4.Id = incorpId.ToString();//Convert.ToString(1);
                        var riskItemList4 = new List<RiskItemListModel>();
                        riskItemList4.Add(riskItem4);
                        riskType4.RiskItemList = riskItemList4;
                        riskTypeList.Add(riskType4);
                    }
                    //for country of incorporation end
                    //nationality partner 1 start
                    if (nationality1Id != "0")
                    {
                        var riskType5 = new RiskTypeListModel();
                        riskType5.Id = Convert.ToString(nationality1lovId);
                        var riskItem5 = new RiskItemListModel();
                        riskItem5.Id = nationality1Id.ToString();//Convert.ToString(1);
                        var riskItemList5 = new List<RiskItemListModel>();
                        riskItemList5.Add(riskItem5);
                        riskType5.RiskItemList = riskItemList5;
                        riskTypeList.Add(riskType5);
                    }
                    //nationality partner 1 end
                    //nationality partner 2 start
                    if (nationality2Id != "0")
                    {
                        var riskType6 = new RiskTypeListModel();
                        riskType6.Id = Convert.ToString(nationality2lovId);
                        var riskItem6 = new RiskItemListModel();
                        riskItem6.Id = nationality2Id.ToString();//Convert.ToString(1);
                        var riskItemList6 = new List<RiskItemListModel>();
                        riskItemList6.Add(riskItem6);
                        riskType6.RiskItemList = riskItemList6;
                        riskTypeList.Add(riskType6);
                    }
                    //nationality partner 2 end
                    //nationality partner 3 start
                    if (nationality3Id != "0")
                    {
                        var riskType7 = new RiskTypeListModel();
                        riskType7.Id = Convert.ToString(nationality3lovId);
                        var riskItem7 = new RiskItemListModel();
                        riskItem7.Id = nationality3Id.ToString();//Convert.ToString(1);
                        var riskItemList7 = new List<RiskItemListModel>();
                        riskItemList7.Add(riskItem7);
                        riskType7.RiskItemList = riskItemList7;
                        riskTypeList.Add(riskType7);
                    }
                    //nationality partner 3 end
                    //nationality partner 4 start
                    if (nationality4Id != "0")
                    {
                        var riskType8 = new RiskTypeListModel();
                        riskType8.Id = Convert.ToString(nationality4lovId);
                        var riskItem8 = new RiskItemListModel();
                        riskItem8.Id = nationality4Id.ToString();//Convert.ToString(1);
                        var riskItemList8 = new List<RiskItemListModel>();
                        riskItemList8.Add(riskItem8);
                        riskType8.RiskItemList = riskItemList8;
                        riskTypeList.Add(riskType8);
                    }
                    //nationality partner 4 end
                    //nationality partner 5 start
                    if (nationality5Id != "0")
                    {
                        var riskType9 = new RiskTypeListModel();
                        riskType9.Id = Convert.ToString(nationality5lovId);
                        var riskItem9 = new RiskItemListModel();
                        riskItem9.Id = nationality5Id.ToString();//Convert.ToString(1);
                        var riskItemList9 = new List<RiskItemListModel>();
                        riskItemList9.Add(riskItem9);
                        riskType9.RiskItemList = riskItemList9;
                        riskTypeList.Add(riskType9);
                    }
                    //nationality partner 5 end
                    //product start
                    if (productId != "0")
                    {
                        var riskType10 = new RiskTypeListModel();
                        riskType10.Id = Convert.ToString(productlovId);
                        var riskItem10 = new RiskItemListModel();
                        riskItem10.Id = productId.ToString();//Convert.ToString(1);
                        var riskItemList10 = new List<RiskItemListModel>();
                        riskItemList10.Add(riskItem10);
                        riskType10.RiskItemList = riskItemList10;
                        riskTypeList.Add(riskType10);
                    }
                    //product end
                    //delivery start
                    if (deliveryId != "0")
                    {
                        var riskType11 = new RiskTypeListModel();
                        riskType11.Id = Convert.ToString(deliverylovId);
                        var riskItem11 = new RiskItemListModel();
                        riskItem11.Id = deliveryId.ToString();//Convert.ToString(1);
                        var riskItemList11 = new List<RiskItemListModel>();
                        riskItemList11.Add(riskItem11);
                        riskType11.RiskItemList = riskItemList11;
                        riskTypeList.Add(riskType11);
                    }
                    //delivery end
                    ////for pep start
                    //if (IsPepId != "0")
                    //{
                    //    var riskType2 = new RiskTypeListModel();
                    //    riskType2.Id = Convert.ToString(IsPeplovId);
                    //    var riskItem2 = new RiskItemListModel();
                    //    riskItem2.Id = IsPepId.ToString();
                    //    var riskItemList2 = new List<RiskItemListModel>();
                    //    riskItemList2.Add(riskItem2);
                    //    riskType2.RiskItemList = riskItemList2;
                    //    riskTypeList.Add(riskType2);
                    //}
                    ////for pep end
                    ///for mode of payment start
                    if (modeofpaymentId != "0")
                    {
                        var riskType12 = new RiskTypeListModel();
                        riskType12.Id = Convert.ToString(modeofpaymentlovId);
                        var riskItem12 = new RiskItemListModel();
                        riskItem12.Id = modeofpaymentId.ToString();//Convert.ToString(1);
                        var riskItemList12 = new List<RiskItemListModel>();
                        riskItemList12.Add(riskItem12);
                        riskType12.RiskItemList = riskItemList12;
                        riskTypeList.Add(riskType12);
                    }
                    //for mode of payment end

                    riskModel.RiskTypeList = riskTypeList;
                    var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                    var xyz = riskResult;
                    Console.WriteLine($"generated risk for customer: {JsonConvert.SerializeObject(riskModel, Formatting.Indented)}");
                    Console.WriteLine($"Finished generating risk for customer: {JsonConvert.SerializeObject(riskResult, Formatting.Indented)}");
                }
                //UBO Risk Creation
                Console.WriteLine("Risk for UBO");
            foreach (var corporate in corplist)
            {

                    if (corporate.CorporateType != "Group Entity")
                    {
                        Console.WriteLine("Individual UBO risk ");

                        var a = corporate.CustomerDetailList;

                        foreach (var item in a)
                        {

                            var UboId = "";

                            if (item.CustomerType != "Group Entity")
                            {
                                Console.WriteLine("Customer Type", item.CustomerType);
                                var isPeP = "";

                                string body = string.Empty;
                                RiskAPIRequestModel riskModels = new RiskAPIRequestModel();
                                if (item.CustomerType == "Shareholder")
                                {

                                    foreach (var id in partnerId)
                                    {
                                        riskModels.CustomerId = id;
                                        var x = await _commonService.CustomerScreeningCall(id, baseURL, baseC6URL, "CORPORATE", body, model.Threshold);


                                        //if (x.ApiResultsjson != null)
                                        //{
                                        //    if (x.MatchScore >= 95)
                                        //    {
                                        //        isPeP = "Yes";
                                        //    }
                                        //    else if (x.MatchScore < 95 && x.MatchScore >= 75)
                                        //    {
                                        //        isPeP = "No";
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    isPeP = "No";
                                        //}

                                        UboId = id;
                                        partnerId.Remove(id);
                                        break;
                                    }
                                }
                                else if (item.CustomerType == "Senior Management")
                                {
                                    foreach (var id in SeniorMgtId)
                                    {
                                        riskModels.CustomerId = id;
                                        var x = await _commonService.CustomerScreeningCall(id, baseURL, baseC6URL, "CORPORATE", body, model.Threshold);

                                        //if (x.ApiResultsjson != null)
                                        //{
                                        //    if (x.MatchScore >= 95)
                                        //    {
                                        //        isPeP = "Yes";
                                        //    }
                                        //    else if (x.MatchScore < 95 && x.MatchScore >= 75)
                                        //    {
                                        //        isPeP = "No";
                                        //    }

                                        //}
                                        //else
                                        //{
                                        //    isPeP = "No";
                                        //}

                                        UboId = id;
                                        SeniorMgtId.Remove(id);
                                        break;
                                    }
                                }
                                else if (item.CustomerType == "Authorised Signatories")
                                {
                                    foreach (var id in AuthorisedId)
                                    {
                                        riskModels.CustomerId = id;
                                        var x = await _commonService.CustomerScreeningCall(id, baseURL, baseC6URL, "CORPORATE", body);


                                        //if (x.ApiResultsjson != null)
                                        //{
                                        //    if (x.MatchScore >= 95)
                                        //    {
                                        //        isPeP = "Yes";
                                        //    }
                                        //    else if (x.MatchScore < 95 && x.MatchScore >= 75)
                                        //    {
                                        //        isPeP = "No";
                                        //    }

                                        //}
                                        //else
                                        //{
                                        //    isPeP = "No";
                                        //}

                                        UboId = id;
                                        AuthorisedId.Remove(id);
                                        break;
                                    }
                                }


                                CorporateKycDTO corpModel = new CorporateKycDTO();

                                item.IsPeP = isPeP;


                                var str2 = _kycService.GetRiskLovId(_mapper.Map<KycIndividualDTO>(item), corpModel, "I", culture, ClientId);
                                if (str2.Result == null)
                                {
                                    // _toastNotification.AddWarningToastMessage(_localizer["Unable to calculate risk due to insufficient data."]);
                                    return View(model);
                                }
                                var spStr2 = str2.Result.Split('Ø');
                                var natlovId = spStr2[1];
                                //var IspeplovId = spStr2[13];

                                var strs = _kycService.GetRiskTypeId(_mapper.Map<KycIndividualDTO>(item), corpModel, "I", culture, ClientId);

                                if (strs.Result == null)
                                {
                                    _toastNotification.AddWarningToastMessage(_localizer["Unable to calculate UBO risk due to insufficient data."]);
                                    return View(kycmodel);
                                }
                                var spStrS = strs.Result.Split('Ø');
                                var natId = spStrS[1];
                                //var IspepId = spStrS[13];



                                if (UboId != "")
                                {

                                    riskModels.CustomerName = item.LastName;
                                    riskModels.ClientId = _clientHandler.GetClientId();
                                    riskModels.CreatedBy = _clientHandler.GetUserId();
                                    riskModels.RiskCategory = "I";
                                    var riskTypeLists = new List<RiskTypeListModel>();
                                   
                                        if (natId != "0")
                                        {
                                            var riskType3 = new RiskTypeListModel();
                                            riskType3.Id = Convert.ToString(natlovId);
                                            var riskItem3 = new RiskItemListModel();
                                            riskItem3.Id = natId.ToString();
                                            var riskItemList3 = new List<RiskItemListModel>();
                                            riskItemList3.Add(riskItem3);
                                            riskType3.RiskItemList = riskItemList3;
                                            riskTypeLists.Add(riskType3);

                                            riskModels.RiskTypeList = riskTypeLists;
                                            var riskResults = _riskAPIController.KycRiskAssessment(riskModels);
                                            var xy = riskResults;
                                            Console.WriteLine($"Risk assessment result: {JsonConvert.SerializeObject(riskModels, Formatting.Indented)}");


                                    }

                                        //if (IspepId != "0")
                                        //{
                                        //    var riskType1 = new RiskTypeListModel();
                                        //    riskType1.Id = Convert.ToString(IspeplovId);
                                        //    var riskItem1 = new RiskItemListModel();
                                        //    riskItem1.Id = IspepId.ToString();
                                        //    var riskItemList1 = new List<RiskItemListModel>();
                                        //    riskItemList1.Add(riskItem1);
                                        //    riskType1.RiskItemList = riskItemList1;
                                        //    riskTypeLists.Add(riskType1);
                                        //}
                                       
                                    

                                }
                            }

                        }
                    }
                    //Group Entity Risk Creation
                    else if (corporate.CorporateType == "Group Entity" && corporate.CorporateType != null)
                    {
                        Console.WriteLine("Group Entity Risk");
                        KycIndividualDTO models = new KycIndividualDTO();
                        if (corporate.AccomplishedCountry == "UNITED ARAB EMIRATES")
                        {
                            corporate.PlaceofIncorporation = "Within UAE";
                        }
                        else if (corporate.AccomplishedCountry != "UNITED ARAB EMIRATES")
                        {
                            corporate.PlaceofIncorporation = "Outside UAE";
                        }
                        //var gstr = _kycService.GetRiskTypeId(models, _mapper.Map<CorporateKycDTO>(corporate), "C", culture);

                        RiskAPIRequestModel riskModels = new RiskAPIRequestModel();
                        var GroupEntId = "";
                        var isPeps = "";
                        foreach (var id in GroupEntityId)
                        {
                            string body = string.Empty;
                            riskModels.CustomerId = id;
                            var x = await _commonService.CustomerScreeningCall(id, baseURL, baseC6URL, "CORPORATE", body, model.Threshold);

                            if (x.ApiResultsjson != null)
                            {
                                if (x.MatchScore >= 95)
                                {
                                    isPeps = "Yes";
                                }
                                else if (x.MatchScore < 95 && x.MatchScore >= 75)
                                {
                                    isPeps = "No";
                                }

                            }
                            else
                            {
                                isPeps = "No";
                            }

                            GroupEntId = id;
                            GroupEntityId.Remove(id);
                            break;
                        }
                        //model.IsPeP = isPeps;
                        var gstr1 = _kycService.GetRiskLovId(models, _mapper.Map<CorporateKycDTO>(model), "C", culture, ClientId);
                        if (gstr1.Result == null)
                        {
                            _toastNotification.AddWarningToastMessage(_localizer["Unable to calculate UBO risk due to insufficient data."]);
                            return View(kycmodel);
                        }
                        var spStrS1 = gstr1.Result.Split('Ø');
                        var incorplovIds1 = spStrS1[3];
                        //var IsPeplovId1 = spStrS1[14];
                        var gstr = _kycService.GetRiskTypeId(models, _mapper.Map<CorporateKycDTO>(model), "C", culture, ClientId);
                        if (gstr.Result == null)
                        {
                            _toastNotification.AddWarningToastMessage(_localizer["Unable to calculate UBO risk due to insufficient data."]);
                            return View(kycmodel);
                        }
                        var spStrS = gstr.Result.Split('Ø');
                        var incorpIds = spStrS[3];
                        // var ispepIds = spStrS[14];

                        riskModels.CustomerName = corporate.CompanyName;
                        riskModels.ClientId = _clientHandler.GetClientId();
                        riskModels.CreatedBy = _clientHandler.GetUserId();
                        riskModels.RiskCategory = "C";

                        var GroupriskTypeList = new List<RiskTypeListModel>();
                        if (GroupEntId != "")
                        {
                            if (incorpIds != "0")
                            {
                                var riskType4 = new RiskTypeListModel();
                                riskType4.Id = Convert.ToString(incorplovIds1);
                                var riskItem4 = new RiskItemListModel();
                                riskItem4.Id = incorpIds.ToString();//Convert.ToString(1);
                                var riskItemList4 = new List<RiskItemListModel>();
                                riskItemList4.Add(riskItem4);
                                riskType4.RiskItemList = riskItemList4;
                                GroupriskTypeList.Add(riskType4);

                                riskModels.RiskTypeList = GroupriskTypeList;
                                var riskResults = _riskAPIController.KycRiskAssessment(riskModels);
                                var xy = riskResults;
                                Console.WriteLine($"Risk assessment result: {JsonConvert.SerializeObject(riskModels, Formatting.Indented)}");
                            }
                            //for pep start
                            //if (ispepIds != "0")
                            //{
                            //    var riskType2 = new RiskTypeListModel();
                            //    riskType2.Id = Convert.ToString(IsPeplovId1);
                            //    var riskItem2 = new RiskItemListModel();
                            //    riskItem2.Id = ispepIds.ToString();
                            //    var riskItemList2 = new List<RiskItemListModel>();
                            //    riskItemList2.Add(riskItem2);
                            //    riskType2.RiskItemList = riskItemList2;
                            //    GroupriskTypeList.Add(riskType2);
                            //}

                           
                            //responseList.Remove(id);

                        }
                    }

                }
            }
            kycmodel.EntityAddress = new Address();
            var CustomerType = "C";
            //var list1 = _mapper.Map<List<CountryModel>>(_countryService.GetAllCountryRiskConfig(culture)); //Countryof incorporation from Risk Table
            var countrylist = _mapper.Map<List<CountryModel>>(_countryService.GetAll(ClientId));
            kycmodel.CountryofIncorporationList = new SelectList(countrylist, "Name", "Name");
             //Country list from country master table
            kycmodel.EntityAddress.CountryList = new SelectList(countrylist, "Name", "Name");
            kycmodel.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, ClientId)), "ProductName", "ProductName");
            kycmodel.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, ClientId)), "DeliveryChannelName", "DeliveryChannelName");
            kycmodel.PartnerNationalityList = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAllPartnerNationality(ClientId)), "Name", "Name");//Partner Nationality from risk Table
            kycmodel.BusinessTypeList = new SelectList(_mapper.Map<List<BusinessNature>>(_kycService.GetBusinessType(culture, CustomerType, ClientId)), "BusinessName", "BusinessName");
            kycmodel.EntityType = new SelectList(_mapper.Map<List<LegalStatusModel>>(_kycService.GetLegalStatus(culture, CustomerType, ClientId)), "LegalStatus", "LegalStatus");
            kycmodel.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, ClientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");

            return View(kycmodel);
        }

        [HttpGet("Kyc/Individual")]
        public IActionResult CreateIndividualKyc()
        {
            var ClientId = _clientHandler.GetClientId();
            var CustomerType = "I";
            KycIndividualModel model = new KycIndividualModel();
            var list = _mapper.Map<List<CountryModel>>(_countryService.GetAll(ClientId));
            model.CountryList = new SelectList(list, "Name", "Name");
            model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, ClientId)), "ProductName", "ProductName");
            model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, ClientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.ProfessionalList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetProfessionalStatus(culture, CustomerType, ClientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, ClientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            model.ResidentialStatusList=new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_residence_status(culture, ClientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");

            return View(model);
        }

        private async Task sendEmailForGuardian(string body, CaseModel GuardianCase, CustomerCaseDTO x, KycIndividualModel model)
        {
            int fileType = 4;
            CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
            downloadModel.ApiResultsjson = x.ApiResultsjson;
            downloadModel.TotalRows = x.ApiResultsjson.Count;
            var res = await _viewRenderService.RenderToStringAsync("Report/CaseReportDetailDownload", downloadModel);
            List<CaseReportListModel> Guardianlist = new List<CaseReportListModel>();


            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {

                var appBaseUrl = MyHttpContext.AppBaseUrl;
                body = body.Replace("{customerID}", x.CustomerId.ToString());
                body = body.Replace("{caseID}", x.Id.ToString());
                body = body.Replace("{baseUrl}", appBaseUrl);
                GuardianCase.Url = appBaseUrl;
                GuardianCase.createdUserName = HttpContext.Session.GetString("SessUsername");


                Document document = new Document(PageSize.A4, 15, 15, 15, 15);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                document.Add(new Paragraph("\n"));

                PdfPTable logo = new PdfPTable(2);
                logo.TotalWidth = 550f;
                float[] logowidth = new float[] { 3f, 0.5f };
                logo.SetWidths(logowidth);
                logo.LockedWidth = true;
                logo.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                logo.DefaultCell.VerticalAlignment = 1;
                logo.DefaultCell.HorizontalAlignment = 1;
                logo.SpacingBefore = 20f;
                logo.SpacingAfter = 30f;
                logo.DefaultCell.Border = 0;
                var companyName = HttpContext.Session.GetString("SessCompanyName");
                PdfPCell compname = new PdfPCell(new Phrase(companyName, new Font(Font.FontFamily.TIMES_ROMAN, 17, Font.BOLD)));
                compname.FixedHeight = 40f;
                compname.VerticalAlignment = 1;
                compname.HorizontalAlignment = 1;
                compname.Border = 0;
                logo.AddCell(compname);
                string url = "wwwroot/img/logo.png";
                Image tif = Image.GetInstance(url);
                tif.ScalePercent(1f);
                tif.SpacingBefore = 20f;
                logo.AddCell(tif);
                document.Add(logo);


                PdfPTable header = new PdfPTable(1);
                header.TotalWidth = 550f;
                header.LockedWidth = true;
                header.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                header.SpacingAfter = 30f;
                header.DefaultCell.Border = 0;
                PdfPCell hd = new PdfPCell(new Phrase("Potential Hits for Guardian"));
                hd.HorizontalAlignment = 1;
                hd.FixedHeight = 20f;
                hd.VerticalAlignment = 1;
                hd.Border = 0;
                header.AddCell(hd);
                document.Add(header);


                PdfPTable headTab = new PdfPTable(1);
                headTab.TotalWidth = 550f;
                headTab.LockedWidth = true;
                headTab.DefaultCell.Border = 0;
                headTab.HorizontalAlignment = 1;
                headTab.DefaultCell.FixedHeight = 30f;
                float[] widths1 = new float[] { 3f };
                headTab.SetWidths(widths1);

                headTab.AddCell("Full Name : " + x.LastName);
                headTab.AddCell("Nationality : " + x.Nationality);
                //var dateAndTime = x.DOB;
                //var dob = dateAndTime.Date;
                //var dateAndTime = x.DOB;
                var dob = x.DOB.ToUIDDateFormat();

                //headTab.AddCell("Date Of Birth : " + dob.Day + "/" + dob.Month + "/" + dob.Year);
                headTab.AddCell("Date Of Birth : " + dob);
                headTab.AddCell("ID Type : " + model.CustomerIdType);
                headTab.AddCell("ID Number : " + model.CustomerIdNumber);
                headTab.AddCell("Mobile : " + x.Mobile);
                document.Add(headTab);

                PdfPTable table = new PdfPTable(8);
                table.TotalWidth = 550f;
                table.LockedWidth = true;
                float[] widths = new float[] { 0.5f, 1f, 1f, 1.5f, 3f, 1f, 1f, 1f };
                table.SetWidths(widths);
                table.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                table.SpacingAfter = 30f;
                PdfPCell cell1 = new PdfPCell(new Phrase("#", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell1.HorizontalAlignment = 1;
                cell1.VerticalAlignment = 1;
                cell1.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell1.FixedHeight = 30f;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase("ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell2.HorizontalAlignment = 1;
                cell2.VerticalAlignment = 1;
                cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell2.FixedHeight = 30f;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase("TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell3.HorizontalAlignment = 1;
                cell3.VerticalAlignment = 1;
                cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell3.FixedHeight = 30f;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("CATEGORY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell4.HorizontalAlignment = 1;
                cell4.VerticalAlignment = 1;
                cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell4.FixedHeight = 30f;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell6.HorizontalAlignment = 1;
                cell6.VerticalAlignment = 1;
                cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell6.FixedHeight = 30f;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell7.HorizontalAlignment = 1;
                cell7.VerticalAlignment = 1;
                cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell7.FixedHeight = 30f;
                table.AddCell(cell7);
                PdfPCell cell8 = new PdfPCell(new Phrase("DOB", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell8.HorizontalAlignment = 1;
                cell8.VerticalAlignment = 1;
                cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell8.FixedHeight = 30f;
                table.AddCell(cell8);
                for (int i = 0; i < downloadModel.ApiResultsjson.Count; i++)
                {
                    table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchuid, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchtype, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchcategory, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchname, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchscore, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].nationality, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchdob, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                }
                document.Add(table);

                PdfPTable footTab = new PdfPTable(3);
                footTab.TotalWidth = 550f;
                footTab.LockedWidth = true;
                footTab.DefaultCell.Border = 0;
                float[] footWidth = new float[] { 20f, 40f, 40 };
                footTab.SetWidths(footWidth);
                footTab.HorizontalAlignment = 1;
                footTab.SpacingAfter = 30f;
                footTab.AddCell("Created By : ");
                string username = HttpContext.Session.GetString("SessUsername");
                footTab.AddCell(username);
                footTab.AddCell("");
                footTab.AddCell("Created On : ");
                footTab.AddCell(DateTime.Now.ToString());
                footTab.AddCell("");
                footTab.AddCell("Url : ");
                footTab.AddCell(appBaseUrl);
                footTab.AddCell("");
                document.Add(footTab);

                PdfPTable footer1 = new PdfPTable(1);
                footer1.TotalWidth = 550f;
                footer1.LockedWidth = true;
                footer1.DefaultCell.Border = 0;
                footer1.AddCell("Please click on the VIEW report under CASE Tab for full details ");

                document.Add(footer1);


                document.Add(new Paragraph("\n"));
                iTextSharp.text.pdf.draw.LineSeparator line1 = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1);
                document.Add(new Chunk(line1));

                PdfPTable footer2 = new PdfPTable(1);
                footer2.TotalWidth = 550f;
                footer2.LockedWidth = true;
                footer2.DefaultCell.Border = 0;
                footer2.AddCell("Computer generated report; hence no signature is required.");
                document.Add(footer2);

                PdfContentByte content = writer.DirectContent;
                Rectangle rectangle = new Rectangle(document.PageSize);
                rectangle.Left += document.LeftMargin;
                rectangle.Right -= document.RightMargin;
                rectangle.Top -= document.TopMargin;
                rectangle.Bottom += document.BottomMargin;
                content.SetColorStroke(GrayColor.BLACK);
                content.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                content.Stroke();

                document.Close();
                byte[] data = memoryStream.ToArray();
                if (!(x.MatchScore >= 0 && x.MatchScore < checkThreshold))
                {
                    var emailSent = await _commonService.SendHtmlFormattedEmailWithAttachment("Case creation Alert", body, res.ToString(), data);
                }
            }
        }
        private async Task sendEmailForCustomer(string body, CaseModel caseModel, CustomerCaseDTO x, KycIndividualModel model)
        {
            int fileType = 4;
            CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
            if (x.ApiResultsjson != null)
            {
                downloadModel.ApiResultsjson = x.ApiResultsjson;
                downloadModel.TotalRows = x.ApiResultsjson.Count;
            }
            var res = await _viewRenderService.RenderToStringAsync("Report/CaseReportDetailDownload", downloadModel);
            List<CaseReportListModel> caselist = new List<CaseReportListModel>();


            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {

                var appBaseUrl = MyHttpContext.AppBaseUrl;
                body = body.Replace("{customerID}", x.CustomerId.ToString());
                body = body.Replace("{caseID}", x.Id.ToString());
                body = body.Replace("{baseUrl}", appBaseUrl);
                caseModel.Url = appBaseUrl;
                caseModel.createdUserName = HttpContext.Session.GetString("SessUsername");


                Document document = new Document(PageSize.A4, 15, 15, 15, 15);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();



                document.Add(new Paragraph("\n"));

                PdfPTable logo = new PdfPTable(2);
                logo.TotalWidth = 550f;
                float[] logowidth = new float[] { 3f, 0.5f };
                logo.SetWidths(logowidth);
                logo.LockedWidth = true;
                logo.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                logo.DefaultCell.VerticalAlignment = 1;
                logo.DefaultCell.HorizontalAlignment = 1;
                logo.SpacingBefore = 20f;
                logo.SpacingAfter = 30f;
                logo.DefaultCell.Border = 0;
                var companyName = HttpContext.Session.GetString("SessCompanyName");
                PdfPCell compname = new PdfPCell(new Phrase(companyName, new Font(Font.FontFamily.TIMES_ROMAN, 17, Font.BOLD)));
                compname.FixedHeight = 40f;
                compname.VerticalAlignment = 1;
                compname.HorizontalAlignment = 1;
                compname.Border = 0;
                logo.AddCell(compname);
                string url = "wwwroot/img/logo.png";
                Image tif = Image.GetInstance(url);
                tif.ScalePercent(1f);
                tif.SpacingBefore = 20f;
                logo.AddCell(tif);
                document.Add(logo);


                PdfPTable header = new PdfPTable(1);
                header.TotalWidth = 550f;
                header.LockedWidth = true;
                header.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                header.SpacingAfter = 30f;
                header.DefaultCell.Border = 0;
                PdfPCell hd = new PdfPCell(new Phrase("Potential Hits for Individual"));
                hd.HorizontalAlignment = 1;
                hd.FixedHeight = 20f;
                hd.VerticalAlignment = 1;
                hd.Border = 0;
                header.AddCell(hd);
                document.Add(header);


                PdfPTable headTab = new PdfPTable(1);
                headTab.TotalWidth = 550f;
                headTab.LockedWidth = true;
                headTab.DefaultCell.Border = 0;
                headTab.HorizontalAlignment = 1;
                headTab.DefaultCell.FixedHeight = 30f;
                float[] widths1 = new float[] { 3f };
                headTab.SetWidths(widths1);

                headTab.AddCell("Full Name : " + x.LastName);
                headTab.AddCell("Nationality : " + x.Nationality);
                //var dateAndTime = x.DOB;
                //var dob = dateAndTime.Date;
                //var dateAndTime = x.DOB;
                var dob = x.DOB.ToUIDDateFormat();

                //headTab.AddCell("Date Of Birth : " + dob.Day + "/" + dob.Month + "/" + dob.Year);
                headTab.AddCell("Date Of Birth : " + dob);
                headTab.AddCell("ID Type : " + model.CustomerIdType);
                headTab.AddCell("ID Number : " + model.CustomerIdNumber);
                headTab.AddCell("Mobile : " + x.Mobile);
                document.Add(headTab);

                PdfPTable table = new PdfPTable(8);
                table.TotalWidth = 550f;
                table.LockedWidth = true;
                float[] widths = new float[] { 0.5f, 1f, 1f, 1.5f, 3f, 1f, 1f, 1f };
                table.SetWidths(widths);
                table.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                table.SpacingAfter = 30f;
                PdfPCell cell1 = new PdfPCell(new Phrase("#", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell1.HorizontalAlignment = 1;
                cell1.VerticalAlignment = 1;
                cell1.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell1.FixedHeight = 30f;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase("ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell2.HorizontalAlignment = 1;
                cell2.VerticalAlignment = 1;
                cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell2.FixedHeight = 30f;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase("TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell3.HorizontalAlignment = 1;
                cell3.VerticalAlignment = 1;
                cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell3.FixedHeight = 30f;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("CATEGORY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell4.HorizontalAlignment = 1;
                cell4.VerticalAlignment = 1;
                cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell4.FixedHeight = 30f;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell6.HorizontalAlignment = 1;
                cell6.VerticalAlignment = 1;
                cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell6.FixedHeight = 30f;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell7.HorizontalAlignment = 1;
                cell7.VerticalAlignment = 1;
                cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell7.FixedHeight = 30f;
                table.AddCell(cell7);
                PdfPCell cell8 = new PdfPCell(new Phrase("DOB", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell8.HorizontalAlignment = 1;
                cell8.VerticalAlignment = 1;
                cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell8.FixedHeight = 30f;
                table.AddCell(cell8);
                if (downloadModel.ApiResultsjson != null)
                {
                    for (int i = 0; i < downloadModel.ApiResultsjson.Count; i++)
                    {
                        table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchuid, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchtype, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchcategory, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchname, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchscore, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].nationality, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchdob, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    }
                }
                document.Add(table);

                PdfPTable footTab = new PdfPTable(3);
                footTab.TotalWidth = 550f;
                footTab.LockedWidth = true;
                footTab.DefaultCell.Border = 0;
                float[] footWidth = new float[] { 20f, 40f, 40 };
                footTab.SetWidths(footWidth);
                footTab.HorizontalAlignment = 1;
                footTab.SpacingAfter = 30f;
                footTab.AddCell("Created By : ");
                string username = HttpContext.Session.GetString("SessUsername");
                footTab.AddCell(username);
                footTab.AddCell("");
                footTab.AddCell("Created On : ");
                footTab.AddCell(DateTime.Now.ToString());
                footTab.AddCell("");
                footTab.AddCell("Url : ");
                footTab.AddCell(appBaseUrl);
                footTab.AddCell("");
                document.Add(footTab);

                PdfPTable footer1 = new PdfPTable(1);
                footer1.TotalWidth = 550f;
                footer1.LockedWidth = true;
                footer1.DefaultCell.Border = 0;
                footer1.AddCell("Please click on the VIEW report under CASE Tab for full details ");

                document.Add(footer1);


                document.Add(new Paragraph("\n"));
                iTextSharp.text.pdf.draw.LineSeparator line1 = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1);
                document.Add(new Chunk(line1));

                PdfPTable footer2 = new PdfPTable(1);
                footer2.TotalWidth = 550f;
                footer2.LockedWidth = true;
                footer2.DefaultCell.Border = 0;
                footer2.AddCell("Computer generated report; hence no signature is required. ");
                document.Add(footer2);

                PdfContentByte content = writer.DirectContent;
                Rectangle rectangle = new Rectangle(document.PageSize);
                rectangle.Left += document.LeftMargin;
                rectangle.Right -= document.RightMargin;
                rectangle.Top -= document.TopMargin;
                rectangle.Bottom += document.BottomMargin;
                content.SetColorStroke(GrayColor.BLACK);
                content.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                content.Stroke();


                document.Close();
                byte[] data = memoryStream.ToArray();
                if (!(x.MatchScore >= 0 && x.MatchScore < checkThreshold))
                {
                    var emailSent = await _commonService.SendHtmlFormattedEmailWithAttachment("Case creation Alert", body, res.ToString(), data);
                }
            }
        }

        [HttpPost("Kyc/Individual")]
        public async Task<ActionResult> CreateIndividualKyc(KycIndividualModel model)
        {
            var ClientId = _clientHandler.GetClientId();
            model.ClientId = ClientId;
            model.CreatedBy = _clientHandler.GetUserId();
            var errors = ModelState.Select(x => x.Value.Errors)
                          .Where(y => y.Count > 0)
                          .ToList();

            var CustomerType = "I";

            var list = _mapper.Map<List<CountryModel>>(_countryService.GetAll(ClientId));
            model.CountryList = new SelectList(list, "Name", "Name");
            model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, ClientId)), "ProductName", "ProductName");
            model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, ClientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.ProfessionalList=new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetProfessionalStatus(culture, CustomerType, ClientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, ClientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");

            if (errors.Count() > 0)
            {
                Console.WriteLine($"ModelState errors: {string.Join(", ", errors)}");
            }
            Console.WriteLine("Model is valid.");
            checkThreshold = model.Threshold;

            if (ModelState.IsValid)
            {
                CaseModel caseModel = new CaseModel();
                caseModel.FirstName = "";
                caseModel.LastName = model.FullName;
                caseModel.Nationality = model.Nationality;
                caseModel.DOB = model.DOB.ToString("yyyy-MM-dd");
                caseModel.CustomerIdType = model.CustomerIdType;
                caseModel.CustomerIdNumber = model.CustomerIdNumber;
                caseModel.CustomerType = "I";
                caseModel.MatchCategory = "INDIVIDUAL";
                caseModel.CreatedBy = _clientHandler.GetUserId();
                caseModel.Mobile = model.Mobile;
                caseModel.Threshold = model.Threshold;
                caseModel.C6Threshold = model.C6Threshold;
                caseModel.ClientId = _clientHandler.GetClientId();
                //var customerCodeprefix= _mapper.Map<ClientMasterDTO>(_customerCaseService.GetCustomerCodeprefixByclient(model.ClientId));
                //caseModel.customerCodeprefix = customerCodeprefix.Prefix;
                ServiceResponse<string> result = _customerCaseService.Create(_mapper.Map<CustomerCaseDTO>(caseModel));
                Console.WriteLine($"Created customer case with id {result.Result}");
                model.CustomerId = result.Result.Split('Ø')[1];
                //ServiceResponse<string> result = _customerCaseService.CreatePrefix(_mapper.Map<CustomerCaseDTO>(caseModel));
                //Console.WriteLine($"Created customer case with id {result.Result}");


                CaseDocumentModel _caseDoc = new CaseDocumentModel();

                _caseDoc.CaseId = _customerCaseService.GetCaseId(model.CustomerId).ToString();
                if (model.CaseDocumentsL != null)
                {
                    foreach (var item in model.CaseDocumentsL)
                    {
                        if (item.Document != null)
                        {
                            _caseDoc.CreatedBy = _clientHandler.GetUserId();
                            _caseDoc.CreatedOn = DateTime.Now;
                            _caseDoc.ClientId = _clientHandler.GetClientId();
                            DocumentsModel _documentsModel = _fileUploader.UploadFile(model.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), item.Document);
                            _caseDoc.DocumentFileName = item.Document.FileName;
                            _caseDoc.DocumentFullPath = _documentsModel.DocFullPath;
                            _caseDoc.IssuedDateOnDB = item.IssuedDate;
                            _caseDoc.ExpiryDateOnDB = item.ExpiryDate;
                            _caseDoc.DocumentName = item.DocumentName;
                            var resp = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
                        }
                    }
                }

                //model.CustomerId = result.Result;
                var Lastname = caseModel.LastName;
                var CustomerId = model.CustomerId;
                var kycresult = _kycService.CreateIndividual(_mapper.Map<KycIndividualDTO>(model));
                Console.WriteLine($"KYC create individual result: {kycresult.Message}");

                if (kycresult.Status == 200)
                {    //Creating case for Guardian
                    if (model.GuardianName != null)
                    {
                        Console.WriteLine("Guardian name is not null creating case for guardian as well");

                        CaseModel GuardianCase = new CaseModel();

                        GuardianCase.LastName = model.GuardianName;
                        GuardianCase.CustomerType = "I";
                        caseModel.MatchCategory = "INDIVIDUAL";
                        GuardianCase.CreatedBy = _clientHandler.GetUserId();

                        var GuardianResult = _customerCaseService.CreatePrefix(_mapper.Map<CustomerCaseDTO>(GuardianCase));
                        Console.WriteLine($"Created guardian customer case with id {GuardianResult.Result}");

                        if (GuardianResult.Status == StaticResource.SuccessStatusCode)
                        {
                            string body = string.Empty;
                            using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                            {
                                body = reader.ReadToEnd();
                            };

                            Console.WriteLine("Sending guardian details for screening");
                            var x = await _commonService.CustomerScreeningCall(GuardianResult.Result, baseURL, baseC6URL, "INDIVIDUAL", body,model.Threshold);
                            Console.WriteLine($"Got a match score of {x.MatchScore}");

                            if (x.sendMail == 1)
                            {
                                Console.WriteLine("Send email for case creation");
                                await sendEmailForGuardian(body, GuardianCase, x, model);
                            }

                            if (x.IsMatched == 1 && x.MatchScore >= checkThreshold)
                            {
                                _toastNotification.AddWarningToastMessage(_localizer["Customer blocked, Case created"]);
                            }

                            else if (x.IsMatched == 0 && x.MatchScore == 0)
                            {
                                caseModel.IsMatched = 2;
                                var appBaseUrl = MyHttpContext.AppBaseUrl;
                                body = body.Replace("{baseUrl}", appBaseUrl);
                                caseModel.Url = appBaseUrl;
                                caseModel.createdUserName = HttpContext.Session.GetString("SessUsername");
                                _toastNotification.AddInfoToastMessage(string.Concat(_localizer["Customer Approved as there was no match found"], ". \n"));
                            }

                            else if (x.MatchScore >= 0 && x.MatchScore < checkThreshold)
                            {
                                caseModel.ApiResultJson = x.ApiResultsjson;
                                _toastNotification.AddInfoToastMessage(string.Concat(_localizer["Customer Approved as the matchscore is less than threshold"], ". \n"));
                            }

                            else
                            {
                                _toastNotification.AddSuccessToastMessage("Customer Approved");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No guardian details given");
                    }

                    if (result.Status == StaticResource.SuccessStatusCode)
                    {
                        string body = string.Empty;

                        using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                        {
                            body = reader.ReadToEnd();
                        };

                        Console.WriteLine("Sending customer details for screening");
                        var x = await _commonService.CustomerScreeningCall(model.CustomerId, baseURL, baseC6URL, "INDIVIDUAL", body,model.Threshold);
                        Console.WriteLine($"Got a match score of {x.MatchScore}");

                        if (x.sendMail == 1)
                        {
                            Console.WriteLine("Send email for case creation");
                            await sendEmailForCustomer(body, caseModel, x, model);
                        }

                        if (x.IsMatched == 1 && x.MatchScore >= checkThreshold)
                        {
                            _toastNotification.AddWarningToastMessage(_localizer["Customer blocked, Case created"]);
                        }
                        else if (x.IsMatched == 0 && x.MatchScore == 0)
                        {
                            caseModel.IsMatched = 2;
                            var appBaseUrl = MyHttpContext.AppBaseUrl;
                            body = body.Replace("{baseUrl}", appBaseUrl);
                            caseModel.Url = appBaseUrl;
                            caseModel.createdUserName = HttpContext.Session.GetString("SessUsername");
                            _toastNotification.AddInfoToastMessage(string.Concat(_localizer["Customer Approved as there was no match found"], ". \n"));

                        }
                        else if (x.MatchScore >= 0 && x.MatchScore < checkThreshold)
                        {
                            caseModel.ApiResultJson = x.ApiResultsjson;
                            _toastNotification.AddInfoToastMessage(string.Concat(_localizer["Customer Approved as the matchscore is less than threshold"], ". \n"));
                        }
                        else
                        {
                            _toastNotification.AddSuccessToastMessage(_localizer["Customer Approved"]);
                        }

                        Console.WriteLine("Customer screening compleated, Generating risk report");
               
                        //var isPep = "";

                        //if (model.PEPStatus == 1)
                        //{
                        //    isPep = "Yes";
                        //}
                        //else
                        //{
                        //    isPep = "No";
                        //}
                        //model.IsPep = isPep;
                        CorporateKycDTO corpModel = new CorporateKycDTO();
                        //To check if risk assessment is enabled for the client.
                        var results = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(ClientId));
                        if (results != null)
                        {
                            var str1 = _kycService.GetRiskLovId(_mapper.Map<KycIndividualDTO>(model), corpModel, "I", culture, ClientId);
                            if (str1.Result == null)
                            {
                                _toastNotification.AddWarningToastMessage(_localizer["Unable to calculate risk due to insufficient data."]);
                                return View(model);
                            }
                                var spStr1 = str1.Result.Split('Ø');
                                var proflovId = spStr1[0];
                                var natlovId = spStr1[1];
                                var reslovId = spStr1[5];
                                //var IspeplovId = spStr1[13];
                                var IndprodlovId = spStr1[13];
                                var InddelilovId = spStr1[14];
                                var IndmodeofpaymentlovId= spStr1[15];
                            var str = _kycService.GetRiskTypeId(_mapper.Map<KycIndividualDTO>(model), corpModel, "I", culture, ClientId);
                            if (str.Result == null)
                            {
                                _toastNotification.AddWarningToastMessage(_localizer["Unable to calculate risk due to insufficient data."]);
                                return View(model);
                            }
                            var spStr = str.Result.Split('Ø');
                            var profId = spStr[0];
                            var natId = spStr[1];
                            var resId = spStr[5];
                            //var IspepId = spStr[13];
                            var IndprodId = spStr[13];
                            var InddeliId = spStr[14];
                            var Indmodeofpaymentid= spStr[15];
                            RiskAPIRequestModel riskModel = new RiskAPIRequestModel();

                            riskModel.CustomerId = CustomerId;
                            riskModel.CustomerName = Lastname;


                            riskModel.ClientId = _clientHandler.GetClientId();
                            riskModel.CreatedBy = _clientHandler.GetUserId();
                            if (x.Nationality == "0")
                            {
                                riskModel.MainNationality = "";
                            }
                            else
                            {
                                riskModel.MainNationality = x.Nationality;
                            }
                            riskModel.RiskCategory = "I";

                            var riskTypeList = new List<RiskTypeListModel>();

                            //for profession start
                            if (profId != "0")
                            {
                                var riskType4 = new RiskTypeListModel();
                                riskType4.Id = Convert.ToString(proflovId);
                                var riskItem4 = new RiskItemListModel();
                                riskItem4.Id = profId.ToString();//Convert.ToString(1);
                                var riskItemList4 = new List<RiskItemListModel>();
                                riskItemList4.Add(riskItem4);
                                riskType4.RiskItemList = riskItemList4;
                                riskTypeList.Add(riskType4);
                            }
                            //for profession end
                            //for residence start
                            if (resId != "0")
                            {
                                var riskType5 = new RiskTypeListModel();
                                riskType5.Id = Convert.ToString(reslovId);
                                var riskItem5 = new RiskItemListModel();
                                riskItem5.Id = resId.ToString();//Convert.ToString(1);
                                var riskItemList5 = new List<RiskItemListModel>();
                                riskItemList5.Add(riskItem5);
                                riskType5.RiskItemList = riskItemList5;
                                riskTypeList.Add(riskType5);
                            }
                            //for residence end
                            //for nationality start
                            if (natId != "0")
                            {
                                var riskType3 = new RiskTypeListModel();
                                riskType3.Id = Convert.ToString(natlovId);
                                var riskItem3 = new RiskItemListModel();
                                riskItem3.Id = natId.ToString();
                                var riskItemList3 = new List<RiskItemListModel>();
                                riskItemList3.Add(riskItem3);
                                riskType3.RiskItemList = riskItemList3;
                                riskTypeList.Add(riskType3);
                            }
                            //for nationality end
                            //for product start
                            if (IndprodId != "0")
                            {
                                var riskType6 = new RiskTypeListModel();
                                riskType6.Id = Convert.ToString(IndprodlovId);
                                var riskItem6 = new RiskItemListModel();
                                riskItem6.Id = IndprodId.ToString();//Convert.ToString(1);
                                var riskItemList6 = new List<RiskItemListModel>();
                                riskItemList6.Add(riskItem6);
                                riskType6.RiskItemList = riskItemList6;
                                riskTypeList.Add(riskType6);
                            }
                            //for product end
                            //for delivery channel start
                            if (InddeliId != "0")
                            {
                                var riskType7 = new RiskTypeListModel();
                                riskType7.Id = Convert.ToString(InddelilovId);
                                var riskItem7 = new RiskItemListModel();
                                riskItem7.Id = InddeliId.ToString();//Convert.ToString(1);
                                var riskItemList7 = new List<RiskItemListModel>();
                                riskItemList7.Add(riskItem7);
                                riskType7.RiskItemList = riskItemList7;
                                riskTypeList.Add(riskType7);
                            }
                            //for delivery channel end
                            //for pep start
                            //if (IspepId != "0")
                            //{
                            //    var riskType1 = new RiskTypeListModel();
                            //    riskType1.Id = Convert.ToString(IspeplovId);
                            //    var riskItem1 = new RiskItemListModel();
                            //    riskItem1.Id = IspepId.ToString();
                            //    var riskItemList1 = new List<RiskItemListModel>();
                            //    riskItemList1.Add(riskItem1);
                            //    riskType1.RiskItemList = riskItemList1;
                            //    riskTypeList.Add(riskType1);
                            //}
                            //for pep end
                            if (Indmodeofpaymentid != "0")
                            {
                                var riskType8 = new RiskTypeListModel();
                                riskType8.Id = Convert.ToString(IndmodeofpaymentlovId);
                                var riskItem8 = new RiskItemListModel();
                                riskItem8.Id = Indmodeofpaymentid.ToString();//Convert.ToString(1);
                                var riskItemList8 = new List<RiskItemListModel>();
                                riskItemList8.Add(riskItem8);
                                riskType8.RiskItemList = riskItemList8;
                                riskTypeList.Add(riskType8);
                            }


                            riskModel.RiskTypeList = riskTypeList;
                            var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                            var xyz = riskResult;

                            Console.WriteLine($"Risk assessment result: {JsonConvert.SerializeObject(riskModel, Formatting.Indented)}");
                        }
                    }

                    else
                    {
                        _toastNotification.AddErrorToastMessage(_localizer["Customer creation failed"]);

                    }
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(_localizer["Customer creation failed"]);

                }

            }

            

            return View(model);
        }
    }
}
