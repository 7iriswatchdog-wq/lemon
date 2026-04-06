
using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Enum;
using AML.Core.Repository;
using AML.Core.Service.CustomerCategory;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.Core.ServiceContract.CustomerScreening;
using AML.Core.ServiceContract.Sanction;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.CustomerCategory;
using AML.ViewModel.ViewModels.Report;
using AML.ViewModel.ViewModels.Sanction;
using AML.ViewModel.ViewModels.User;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AML.Web.Controllers.Sanction
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class SanctionScreeningController : Controller
    {
        private IMapper _mapper;
        private ICountryService _countryService;
        private IScreeningService _screeningService;
        private IHttpClientHandler _clientHandler;
        private IViewRenderService _viewRenderService;
        private IExportDataService _exportService;
        private ICustomerScreeningService _customerScreeningService;
        private ICustomerCaseService _customerCaseService;
        private ICustomerCategoryService _customerCategoryService;
        private int clientId = 0;
        public SanctionScreeningController(IMapper mapper, ICountryService countryService, IScreeningService screeningService, IHttpClientHandler clientHandler, IViewRenderService viewRenderService, IExportDataService exportService, ICustomerScreeningService customerScreeningService, ICustomerCaseService customerCaseService, ICustomerCategoryService customerCategoryService)
        {
            _mapper = mapper;
            _countryService = countryService;
            _screeningService = screeningService;
            _clientHandler = clientHandler;
            _viewRenderService = viewRenderService;
            _exportService = exportService;
            _customerScreeningService = customerScreeningService;
            _customerCaseService = customerCaseService;
            clientId = clientHandler.GetClientId();
            _customerCategoryService = customerCategoryService;
        }
        public IActionResult Index()
        {
            ScreeningSearchModel model = new ScreeningSearchModel();
            model.CustomerCategories = new SelectList(
   _mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result)
       .Where(x => x.Name == "INDIVIDUAL" || x.Name == "CORPORATE")
       .ToList(),
   "Code",
   "Name"
);
            model.SelectionProperties = new[] { "Exact Match", "Partial Match", "Phonetic Match" };
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            return View(model);
        }

        [HttpPost]
        public IActionResult SearchList(ScreeningSearchModel model)
        {
            string response = string.Empty;

            model.clientId = _clientHandler.GetClientId();
            var searchType = "E";//changing from P to F as in front end no options showing
            if (model.SelectionProperty != null)
            {
                var selection = String.Concat(model.SelectionProperty.Where(c => !Char.IsWhiteSpace(c))).ToLower();
                if (selection == "exactmatch")
                    searchType = "E";
                else if (selection == "partialmatch")
                    searchType = "P";
                else if (selection == "phoneticmatch")
                    searchType = "F";
            }

            string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = model.DOB,
                customerfullname = model.Name,
                customernationality = model.Nationality,
                searchtype = searchType,
                customertype=model.customerType
            }, ScreeningService.BACKLIST_SCREENING).Result;
            List<ApiResultModel> apiResultModel = new List<ApiResultModel>();
            if (!string.IsNullOrEmpty(data))
            {
                 apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                    response = AMLUtility.FormatJsonToPlainText(data);
                    model.DataList = apiResultModel;
                    dynamic mdob;
                    try
                    {
                        mdob = Convert.ToDateTime(apiResultModel[0].matchdob);
                    }
                    catch
                    {
                        mdob = Convert.ToDateTime(DateTime.Now);
                    }
                    //Insert Logs
                    _customerScreeningService.InsertSanctionScreeningLogs(new DTO.DTO.Sanction.SanctionScreeningLogDTO {
                        CustomerName = model.Name,
                        Nationality = model.Nationality,
                        DOB = Convert.ToDateTime(model.DOB),
                        SearchType = searchType,
                        CustomerType=model.customerType,
                        MatchName = apiResultModel[0].matchname,
                        MatchScore = Convert.ToInt32(apiResultModel[0].matchscore),
                        MatchUID = apiResultModel[0].matchuid,
                        MatchCategory = apiResultModel[0].matchcategory,
                        MatchType = apiResultModel[0].matchtype,
                        MatchNationality = apiResultModel[0].matchnationality,
                        MatchIDNum = apiResultModel[0].matchidnumber,
                        MatchDOB = mdob,
                        CreatedBy = _clientHandler.GetUserId(),
                        ClientId = _clientHandler.GetClientId()
                });
            }
            
            model.SearchLogs = _mapper.Map<List<SanctionScreeningLogModel>>(_customerScreeningService.GetSanctionScreeningLogs(model.Name,model.Nationality,model.DOB,model.customerType,clientId));

            model.CaseLogs = _mapper.Map<List<CaseModel>>(_customerScreeningService.GetCaseLogs(model.Name, model.Nationality, model.DOB, model.customerType, clientId));


            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            model.CustomerCategories = new SelectList(
              _mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result)
                  .Where(x => x.Name == "INDIVIDUAL" || x.Name == "CORPORATE")
                  .ToList(),
              "Code",
              "Name"
            );
            return View("Index",model);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPDF(ScreeningSearchModel model)
        {
            string response = string.Empty;
            var searchType = "F";
            if (model.SelectionProperty != null)
            {
                var selection = String.Concat(model.SelectionProperty.Where(c => !Char.IsWhiteSpace(c))).ToLower();
                if (selection == "exactmatch")
                    searchType = "E";
                else if (selection == "partialmatch")
                    searchType = "P";
                else if (selection == "phoneticmatch")
                    searchType = "F";
            }

            string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = model.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                customerfullname = model.Name,
                customernationality = model.Nationality,
                searchtype = searchType
            }, ScreeningService.BACKLIST_SCREENING).Result;
            if (!string.IsNullOrEmpty(data))
            {
                List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                
                int fileType = (int)OperationType.PDF;
                var result = await _viewRenderService.RenderToStringAsync("SanctionScreening/ExportSearchList", apiResultModel);
                var file = _exportService.ExportData<ApiResultModel>(apiResultModel, result, fileType, "Sanction_Screening_" + DateTime.Now.Ticks);
                if (file != null)
                {
                    return file;
                }
            }
            return Json(response);
        }
        [HttpGet]
        public FileResult DownloadPDFs(ScreeningSearchModel model)
        {
             string response = string.Empty;
            var searchType = "F";
            if (model.SelectionProperty != null)
            {
                var selection = String.Concat(model.SelectionProperty.Where(c => !Char.IsWhiteSpace(c))).ToLower();
                if (selection == "exactmatch")
                    searchType = "E";
                else if (selection == "partialmatch")
                    searchType = "P";
                else if (selection == "phoneticmatch")
                    searchType = "F";
            }
            
            var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());
            string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = model.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                customerfullname = model.Name,
                customernationality = model.Nationality,
                searchtype = searchType
            }, ScreeningService.BACKLIST_SCREENING).Result;
            if (!string.IsNullOrEmpty(data))
            {
                List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);

                int fileType = (int)OperationType.PDF;
                //var result = await _viewRenderService.RenderToStringAsync("SanctionScreening/ExportSearchList", apiResultModel);
                //var file = _exportService.ExportData<ApiResultModel>(apiResultModel, result, fileType, "Sanction_Screening_" + DateTime.Now.Ticks);
                var logos = "wwwroot/img/" + clientData.DocumentFileName;
                var companyName = clientData.ClientName;

                string body = string.Empty;
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
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
                    PdfPCell compname = new PdfPCell(new Phrase(companyName.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 17, Font.BOLD)));
                    compname.FixedHeight = 40f;
                    compname.VerticalAlignment = 1;
                    compname.HorizontalAlignment = 1;
                    compname.Border = 0;
                    logo.AddCell(compname);
                    string url = logos;
                    Image tif = Image.GetInstance(url);
                    tif.ScalePercent(1f);
                    tif.SpacingBefore = 20f;
                    logo.AddCell(tif);
                    document.Add(logo);

                    #region headers
                    //PdfPTable header = new PdfPTable(1);
                    //header.TotalWidth = 550f;
                    //header.LockedWidth = true;
                    //header.HorizontalAlignment = Element.ALIGN_LEFT;//0=Left, 1=Centre, 2=Right
                    //header.SpacingAfter = 30f;
                    //header.DefaultCell.Border = 0;
                    ////header.DefaultCell.ExtraParagraphSpace= 1;
                    //PdfPCell hd = new PdfPCell(new Phrase("Report               :   Case Report"));
                    //PdfPCell _hd = new PdfPCell(new Phrase("\n"));
                    //PdfPCell dateRange = new PdfPCell(new Phrase("Date Range       :   " + startDate + "  to  " + endDate));
                    //PdfPCell _dateRange = new PdfPCell(new Phrase("\n"));
                    //PdfPCell filters = new PdfPCell(new Phrase("Filters Applied   :   " + filter));
                    //PdfPCell _filters = new PdfPCell(new Phrase("\n"));
                    //PdfPCell createdBy = new PdfPCell(new Phrase("Created by        :   " + clientData.ClientName));
                    //PdfPCell _createdBy = new PdfPCell(new Phrase("\n"));
                    //PdfPCell caseStat = new PdfPCell(new Phrase("Case Status      :   " + caseStatus));
                    //PdfPCell _caseStat = new PdfPCell(new Phrase("\n"));

                    ////hd.HorizontalAlignment = Element.ALIGN_LEFT;
                    ////hd.FixedHeight = 20f;
                    ////hd.VerticalAlignment = 1;
                    //hd.Border = 0;
                    //dateRange.Border = 0;
                    //filters.Border = 0;
                    //createdBy.Border = 0;
                    //caseStat.Border = 0;
                    //_hd.Border = 0;
                    //_dateRange.Border = 0;
                    //_filters.Border = 0;
                    //_createdBy.Border = 0;
                    //_caseStat.Border = 0;
                    //header.AddCell(hd);
                    //header.AddCell(_hd);
                    //header.AddCell(dateRange);
                    //header.AddCell(_dateRange);
                    //header.AddCell(filters);
                    //header.AddCell(_filters);
                    //header.AddCell(createdBy);
                    //header.AddCell(_createdBy);
                    //header.AddCell(caseStat);
                    //header.AddCell(_caseStat);
                    //document.Add(header);

                    #endregion

                    var userName = _clientHandler.GetUserId();

                    PdfPTable headTab = new PdfPTable(1);
                    headTab.TotalWidth = 550f;
                    headTab.LockedWidth = true;
                    headTab.DefaultCell.Border = 0;
                    headTab.HorizontalAlignment = 1;
                    headTab.DefaultCell.FixedHeight = 30f;
                    float[] widths1 = new float[] { 3f };
                    headTab.SetWidths(widths1);
                    headTab.AddCell("Computer generated report; hence no signature is required. ");
                    headTab.AddCell(new Phrase("Date of Extraction  :   " + DateTime.Now));
                    headTab.AddCell(new Phrase("Extracted By : " + userName));
                    document.Add(headTab);



                    PdfPTable table = new PdfPTable(9);
                    table.TotalWidth = 550f;
                    table.LockedWidth = true;
                    float[] widths = new float[] { 0.5f, 1.3f, .8f, .9f, 1.3f, 1f, 1.4f, 1f, 1f};
                    table.SetWidths(widths);
                    table.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                    table.SpacingAfter = 30f;
                    PdfPCell cell1 = new PdfPCell(new Phrase("#", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell1.HorizontalAlignment = 1;
                    cell1.VerticalAlignment = 1;
                    cell1.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell1.FixedHeight = 30f;
                    table.AddCell(cell1);

                    PdfPCell cell2 = new PdfPCell(new Phrase("NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell2.HorizontalAlignment = 1;
                    cell2.VerticalAlignment = 1;
                    cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell2.FixedHeight = 30f;
                    table.AddCell(cell2);
                    PdfPCell cell3 = new PdfPCell(new Phrase("SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell3.HorizontalAlignment = 1;
                    cell3.VerticalAlignment = 1;
                    cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell3.FixedHeight = 30f;
                    table.AddCell(cell3);
                    PdfPCell cell4 = new PdfPCell(new Phrase("UID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell4.HorizontalAlignment = 1;
                    cell4.VerticalAlignment = 1;
                    cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell4.FixedHeight = 30f;
                    table.AddCell(cell4);
                    PdfPCell cell5 = new PdfPCell(new Phrase("CATEGORY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell5.HorizontalAlignment = 1;
                    cell5.VerticalAlignment = 1;
                    cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell5.FixedHeight = 30f;
                    table.AddCell(cell5);
                    PdfPCell cell6 = new PdfPCell(new Phrase("TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
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
                    PdfPCell cell8 = new PdfPCell(new Phrase("ID NUMBER", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell8.HorizontalAlignment = 1;
                    cell8.VerticalAlignment = 1;
                    cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell8.FixedHeight = 30f;
                    table.AddCell(cell8);
                    PdfPCell cell9 = new PdfPCell(new Phrase("DOB", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell9.HorizontalAlignment = 1;
                    cell9.VerticalAlignment = 1;
                    cell9.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell9.FixedHeight = 30f;
                    table.AddCell(cell9);

                    for (int i = 0; i < apiResultModel.Count; i++)
                    {
                        table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(apiResultModel[i].matchname, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(apiResultModel[i].matchscore, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(apiResultModel[i].matchuid, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(apiResultModel[i].matchcategory.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(apiResultModel[i].matchtype, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(apiResultModel[i].matchnationality, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(apiResultModel[i].matchidnumber, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        table.AddCell(new Phrase(apiResultModel[i].matchdob, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));

                    }
                    document.Add(table);




                    document.Add(new Paragraph("\n"));
                    iTextSharp.text.pdf.draw.LineSeparator line1 = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1);
                    //document.Add(new Chunk(line1));

                    PdfPTable footer2 = new PdfPTable(1);
                    footer2.TotalWidth = 550f;
                    footer2.LockedWidth = true;
                    footer2.DefaultCell.Border = 0;
                    footer2.AddCell("Computer generated report; hence no signature is required. ");
                    footer2.AddCell(new Phrase("Date of Extraction  :   " + DateTime.Now));
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


                    byte[] datas = memoryStream.ToArray();


                    var result = datas.ToString();




                    List<CaseReportListModel> list = new List<CaseReportListModel>();
                    var file = _exportService.ExportDataWithHeader<CaseReportListModel>(list, result, fileType, "SanctionScreening_" + DateTime.Now.Ticks, datas);
                    if (file != null)
                    {
                        return file;
                    }
                }
            }
            return null;
        }

    }
}
