using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PdfDocument = SelectPdf.PdfDocument;
using AML.Core.DataContract.Enum;
using AML.Core.ServiceContract.Common;
using AML.ViewModel.ViewModels.Common;
namespace AML.Web.Helper
{
    public interface IExportDataService
    {
        byte[] HtmlToPDF(string html, string customHeader);
        string ListToCSV<T>(List<T> list, string separator);
        FileResult ExportData<T>(List<T> list, string html, int operationTypeID, string downloadFileName);
        FileResult ExportDataWithHeader<T>(List<T> list, string html, int operationTypeID, string downloadFileName, byte[] ms);
        FileResult ExportDetails<T>(T model, string html, int operationTypeID, string downloadFileName);

    }

    public class ExportDataService : PdfPageEventHelper, IExportDataService
    {
        private readonly IConfiguration configuration;
        private ICommonService commonService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private string baseURL = string.Empty;
        public ExportDataService(IConfiguration _configuration, ICommonService _commonService, IHttpContextAccessor _httpContextAccessor)
        {
            configuration = _configuration;
            commonService = _commonService;
            httpContextAccessor = _httpContextAccessor;
            baseURL = configuration.GetSection("OHSBaseApiUrl:BaseUrl").Value;
        }
        //public byte[] HtmlToPDF(string html, string customHeader)
        //{
        //    try
        //    {
        //        var bytes = System.Text.Encoding.UTF8.GetBytes(html);
        //        HtmlToPdf converter = new HtmlToPdf();
        //        converter.Options.PdfPageSize = PdfPageSize.Letter;
        //        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
        //        converter.Options.MarginLeft = 15;
        //        converter.Options.MarginRight = 15;
        //        converter.Options.MarginTop = 20;
        //        converter.Options.MarginBottom = 15;               
        //        // footer set tings
        //        converter.Options.DisplayFooter = true;

        //      converter.Footer.Height = 50;


        //        // page numbers can be added using a PdfTextSection object
        //        PdfTextSection text = new PdfTextSection(0, 10, "Page: {page_number} of {total_pages}", new iTextSharp.text.Font("Arial", 8));
        //        text.HorizontalAlign = PdfTextHorizontalAlign.Right;
        //        converter.Footer.Add(text);

        //        // create a new pdf document converting html
        //        SelectPdf.PdfDocument doc = converter.ConvertHtmlString(html);

        //        // create memory stream to save PDF
        //        MemoryStream pdfStream = new MemoryStream();
        //        // save pdf document into a MemoryStream
        //        doc.Save(pdfStream);
        //        // reset stream position
        //        pdfStream.Position = 0;

        //        byte[] buffer = new byte[16 * 1024];
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            int read;
        //            while ((read = pdfStream.Read(buffer, 0, buffer.Length)) > 0)
        //            {
        //                ms.Write(buffer, 0, read);
        //            }
        //            return ms.ToArray();
        //        }
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
        public string ListToCSV<T>(List<T> list, string separator)
        {
            StringBuilder csv = new StringBuilder();
            try
            {

                if (list == null || list.Count == 0) return null;

                //get type from 0th member
                Type t = list[0].GetType();
                string newLine = Environment.NewLine;


                //make a new instance of the class name we figured out to get its props
                object o = Activator.CreateInstance(t);
                //gets all properties
                PropertyInfo[] props = o.GetType().GetProperties();

                //foreach of the properties in class above, write out properties
                //this is the header row
                csv.Append(string.Join(separator, props.Select(d => d.Name).ToArray()));
                csv.Append(newLine);

                //this acts as datarow
                foreach (T item in list)
                {
                    try
                    {
                        //this acts as datacolumn
                        var row = string.Join(separator, props.Select(d => Convert.ToString(item.GetType()
                                                                        .GetProperty(d.Name)
                                                                        .GetValue(item, null))
                                                                        )
                                                                .ToArray());
                        csv.Append(row + newLine);
                    }
                    catch
                    {

                    }
                }
                return Convert.ToString(csv);
            }
            catch
            {
                return null;
            }
        }
        public byte[] ListToWS<T>(List<T> list, string separator, string wsName)
        {
            StringBuilder csv = new StringBuilder();
            try
            {

                if (list == null || list.Count == 0) return null;

                //get type from 0th member
                Type t = list[0].GetType();

                //make a new instance of the class name we figured out to get its props
                object o = Activator.CreateInstance(t);
                //gets all properties
                PropertyInfo[] props = o.GetType().GetProperties();
                var columnHeadrs = props.Select(d => d.Name).ToArray();
                //foreach of the properties in class above, write out properties
                //this is the header row         
                byte[] result;

                using (var package = new ExcelPackage())
                {
                    // add a new worksheet to the empty workbook

                    var worksheet = package.Workbook.Worksheets.Add(wsName); //Worksheet name
                    var colFromHex = System.Drawing.Color.FromArgb(240, 240, 240); // #f0f0f0
                    using (var cells = worksheet.Cells[1, 1, 1, columnHeadrs.Count()]) //(1,1) (1,5)
                    {
                        cells.Style.Font.Bold = true;
                        cells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cells.Style.Fill.BackgroundColor.SetColor(colFromHex);
                    }

                    //First add the headers
                    for (var i = 0; i < columnHeadrs.Count(); i++)
                    {
                        worksheet.Cells[1, i + 1].Value = columnHeadrs[i];
                    }

                    //Add values                  
                    //foreach (T item in list)
                    for (var j = 0; j < list.Count; j++)
                    {
                        for (var i = 0; i < columnHeadrs.Count(); i++)
                        {
                            worksheet.Cells[j + 2, i + 1].Value = Convert.ToString(list[j].GetType().GetProperty(columnHeadrs[i]).GetValue(list[j]));
                        }
                    }
                    result = package.GetAsByteArray();
                }
                return result;
            }
            catch
            {
                return null;
            }
        }
       
        public FileResult ExportData<T>(List<T> list, string html, int operationTypeID, string downloadFileName)
        {
            try
            {
                if (operationTypeID == (int)OperationType.Copy)
                {
                    var pdfResult = HtmlToPDF(html, downloadFileName);
                    FileResult fileResult = new FileContentResult(pdfResult, "application/pdf");
                    fileResult.FileDownloadName = downloadFileName + ".pdf";
                    return fileResult;
                }
                else if (operationTypeID == (int)OperationType.CSV)
                {
                    string csv = ListToCSV<T>(list, ",");
                    //FileResult fileResult = new FileContentResult(Encoding.ASCII.GetBytes(csv), "application/octet-stream");
                    FileResult fileResult = new FileContentResult(Encoding.ASCII.GetBytes(csv), "application/octet-stream");
                    fileResult.FileDownloadName = downloadFileName + ".csv";
                    return fileResult;
                }
                else if (operationTypeID == (int)OperationType.Excel)
                {
                    var excelSheet = ListToWS<T>(list, ",", downloadFileName);
                    FileResult fileResult = new FileContentResult(excelSheet, "application/ms-excel");
                    fileResult.FileDownloadName = downloadFileName + ".xlsx";
                    return fileResult;
                }
                else if (operationTypeID == (int)OperationType.PDF)
                {
                    var pdfResult = HtmlToPDF(html, downloadFileName);
                    FileResult fileResult = new FileContentResult(pdfResult, "application/pdf");
                    fileResult.FileDownloadName = downloadFileName + ".pdf";
                    return fileResult;
                }
                else
                {
                    var pdfResult = HtmlToPDF(html, downloadFileName);
                    FileResult fileResult = new FileContentResult(pdfResult, "application/pdf");
                    fileResult.FileDownloadName = downloadFileName + ".pdf";
                    return fileResult;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public FileResult ExportDataWithHeader<T>(List<T> list, string html, int operationTypeID, string downloadFileName, byte[] ms)
        {
            try
            {
                if (operationTypeID == (int)OperationType.Copy)
                {
                    var pdfResult = HtmlToPDF(html, downloadFileName);
                    FileResult fileResult = new FileContentResult(pdfResult, "application/pdf");
                    fileResult.FileDownloadName = downloadFileName + ".pdf";
                    return fileResult;
                }
                else if (operationTypeID == (int)OperationType.CSV)
                {
                    string csv = ListToCSV<T>(list, ",");
                    //FileResult fileResult = new FileContentResult(Encoding.ASCII.GetBytes(csv), "application/octet-stream");
                    FileResult fileResult = new FileContentResult(Encoding.ASCII.GetBytes(csv), "application/octet-stream");
                    fileResult.FileDownloadName = downloadFileName + ".csv";
                    return fileResult;
                }
                else if (operationTypeID == (int)OperationType.Excel)
                {
                    var excelSheet = ListToWS<T>(list, ",", downloadFileName);
                    FileResult fileResult = new FileContentResult(excelSheet, "application/ms-excel");
                    fileResult.FileDownloadName = downloadFileName + ".xlsx";
                    return fileResult;
                }
                else if (operationTypeID == (int)OperationType.PDF)
                {
                    var pdfResult = HtmlToPDF(html, downloadFileName);
                    FileResult fileResult = new FileContentResult(ms, "application/pdf");
                    fileResult.FileDownloadName = downloadFileName + ".pdf";
                    return fileResult;
                }
                else
                {
                    var pdfResult = HtmlToPDF(html, downloadFileName);
                    FileResult fileResult = new FileContentResult(pdfResult, "application/pdf");
                    fileResult.FileDownloadName = downloadFileName + ".pdf";
                    return fileResult;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public FileResult ExportDetails<T>(T model, string html, int operationTypeID, string downloadFileName)
        {
            try
            {
                var pdfResult = HtmlToPDFforChecklistLogs(html);
                FileResult fileResult = new FileContentResult(pdfResult, "application/pdf");
                fileResult.FileDownloadName = downloadFileName + ".pdf";
                return fileResult;

            }
            catch
            {
                return null;
            }
        }

        




        public byte[] HtmlToPDF(string html, string customHeader)
        {
            try
            {
                byte[] bPDF = null;

                MemoryStream ms = new MemoryStream();
                TextReader txtReader = new StringReader(html);

                // 1: create object of a itextsharp document class  
                Document doc = new Document(PageSize.A4, 10, 10, 10, 10);


                // 2: we create a itextsharp pdfwriter that listens to the document and directs a XML-stream to a file  
                PdfWriter oPdfWriter = PdfWriter.GetInstance(doc, ms);

                iTextSharp.text.Font mainFont = FontFactory.GetFont("Arial", 10, new iTextSharp.text.BaseColor(153, 153, 153)); // #999
                oPdfWriter.PageEvent = new ExportDataService(configuration, commonService, httpContextAccessor);


                // 3: we create a worker parse the document  
                HTMLWorker htmlWorker = new HTMLWorker(doc);

                // 4: we open document and start the worker on the document  
                doc.Open();
                StyleSheet styleSheet = new StyleSheet();
                styleSheet.LoadStyle("bg-grey", "background-color", "#F4F4F4");
                styleSheet.LoadStyle("bg-red", "background-color", "red");
                styleSheet.LoadStyle("bg-green", "background-color", "green");
                styleSheet.LoadStyle("bg-orange", "background-color", "orange");
                styleSheet.LoadStyle("border-red", "border", "1px solid red");
                styleSheet.LoadStyle("border-orange", "border", "1px solid orange");
                styleSheet.LoadStyle("border-green", "border", "1px solid green");
                styleSheet.LoadStyle("margin-l-5", "margin-left", "3pt");
                styleSheet.LoadStyle("margin-r-5", "margin-right", "3pt");
                styleSheet.LoadTagStyle("td", "background-color", "red");
                htmlWorker.SetStyleSheet(styleSheet);
                htmlWorker.StartDocument();


                // 5: parse the html into the document  
                htmlWorker.Parse(txtReader);

                // 6: close the document and the worker  
                htmlWorker.EndDocument();

                htmlWorker.Close();
                doc.Close();
                bPDF = ms.ToArray();
                //Page Number
                iTextSharp.text.Font blackFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                using (MemoryStream stream = new MemoryStream())
                {
                    PdfReader reader = new PdfReader(bPDF);
                    using (PdfStamper stamper = new PdfStamper(reader, stream))
                    {
                        int pages = reader.NumberOfPages;
                        for (int i = 1; i <= pages; i++)
                        {
                            ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_RIGHT, new Phrase("Page " + i.ToString() + " of " + pages, blackFont), 568f, 15f, 0);
                        }
                    }
                    bPDF = stream.ToArray();
                }
                return bPDF;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Event Listener on end of the page
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="document"></param>
        public override void OnEndPage(PdfWriter writer, Document document)
        {

            PdfContentByte canvas = writer.DirectContent;
            iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(document.PageSize);
            rect.Border = (iTextSharp.text.Rectangle.BOX); // left, right, top, bottom border
            rect.BorderWidth = 2; // a width of 5 user units
            rect.BorderColor = BaseColor.BLACK; // a black border
            rect.UseVariableBorders = true; // the full width will be visible
            rect.Left += document.LeftMargin - 5;
            rect.Right -= document.RightMargin - 5;
            rect.Top -= document.TopMargin - 5;
            rect.Bottom += document.BottomMargin - 5;
            canvas.Rectangle(rect);
            canvas.Stroke();
        }

        /// <summary>
        /// Html to Pdf
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public byte[] HtmlToPDFforChecklistLogs(string html)
        {
            try
            {

                var bytes = System.Text.Encoding.UTF8.GetBytes(html);
                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.Letter;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                converter.Options.MarginLeft = 30;
                converter.Options.MarginRight = 30;
                converter.Options.MarginTop = 20;
                converter.Options.MarginBottom = 20;
                // create a new pdf document converting an url
                PdfDocument doc = converter.ConvertHtmlString(html);

                // create memory stream to save PDF
                MemoryStream pdfStream = new MemoryStream();
                // save pdf document into a MemoryStream
                doc.Save(pdfStream);
                // reset stream position
                pdfStream.Position = 0;

                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = pdfStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
