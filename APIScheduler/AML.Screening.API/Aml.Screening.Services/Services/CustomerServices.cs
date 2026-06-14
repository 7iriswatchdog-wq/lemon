using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using Aml.Screening.ServiceContracts.ServiceContracts;
using Aml.Screening.Services.Extensions;
using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver.Core.Events;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Aml.Screening.Services.Services
{
    public class CustomerServices : ICustomerServices
    {
        /// <summary>
        /// The service 
        /// </summary>
        private ScreeningServices _service;
        

        public CustomerServices(IConfiguration configuration)
        {
            _service = new ScreeningServices(configuration);
        }
        /// <summary>
        /// Screens the customer asynchronous.
        /// </summary>
        /// <param name="customerDto">The customer dto.</param>
        /// <returns></returns>
        public async Task<ServiceResponse<CASELOGMATCH>> ScreenCustomerAsync(CustomerDto customerDto)
        {
            try
            {
                double lowRange = 0;
                double midRange = 0;
                double highRange = 0;
                string riskStatus = "LOWRISK";
                RiskServices riskService = new RiskServices();
                var screendto = customerDto.ToOnlineCustomerScreenDto();
                var risk = riskService.calculateCustomerRisk(screendto);
                lowRange = 3 * 1.49;
                midRange = 3 * 2.24;
                highRange = 3 * 3;
                if (risk > lowRange && risk <= midRange) riskStatus = "MEDIUMRISK";
                else if (risk > midRange && risk <= highRange) riskStatus = "HIGHRISK";
                //Console.WriteLine($"Starts Checking with id:  {DateTime.Now}");
                //Console.WriteLine($"Caseid : {screendto.CaseId}");

                //LogFile("starts Checking with id", screendto.CaseId);
                //var idScreen = await _service.CheckId(screendto);


                //if (!string.IsNullOrWhiteSpace(idScreen.MATCHUID))
                //{
                //    Console.WriteLine($"Ends Checking with id :  {DateTime.Now}");
                //    Console.WriteLine($"Matchduid :  {idScreen.MATCHUID}");


                //   // LogFile("Ends Checking with id",idScreen.MATCHUID);
                //    ///To write an insert to case generation table with matching details in msql DB
                //    return new ServiceResponse<CASELOGMATCH>()
                //    {
                //        RiskScore = risk.ToString(),
                //        RiskStatus = riskStatus,
                //        CaseId = screendto.CaseId,
                //        Data = idScreen,
                //        ResponseCode = Resource.PositiveMatchCode,
                //        ResponseMessage = Resource.PositiveMatchDesc
                //    };
                //}
                //Console.WriteLine($"Starts Checking with exact name : {DateTime.Now}");
                //Console.WriteLine($"Caseid : { screendto.CaseId}");
                //LogFile("Starts Checking with exact name", screendto.CaseId);

               // LogFile("Starts Checking with exact name", screendto.CaseId);
                var exactNameScreen = await _service.CheckExactName(screendto);
                
                if (!string.IsNullOrWhiteSpace(exactNameScreen.MATCHUID))
                {
                    Console.WriteLine($"Ends Checking with exact name : {DateTime.Now}");
                    Console.WriteLine($"Matchuid :  {exactNameScreen.MATCHUID}");

                    //LogFile("Ends Checking with exact name", exactNameScreen.MATCHUID);
                    ///To write an insert to case generation table with matching details in msql DB
                    return new ServiceResponse<CASELOGMATCH>()
                    {
                        RiskScore = risk.ToString(),
                        RiskStatus = riskStatus,
                        CaseId = screendto.CaseId,
                        Data = exactNameScreen,
                        ResponseCode = Resource.PositiveMatchCode,
                        ResponseMessage = Resource.PositiveMatchDesc
                    };
                }
                //Console.WriteLine($"Starts Checking with Fuzzy name : {DateTime.Now}");
                //Console.WriteLine($"Caseid : {screendto.CaseId}");

                //LogFile("Starts Checking with Fuzzy name", screendto.CaseId);
                //var fuzzyNameScreen = await _service.CheckFuzzyName(screendto);

                //if (fuzzyNameScreen != null)
                //{
                //    Console.WriteLine($"Ends Checking with Fuzzy name :  {DateTime.Now}");
                //    Console.WriteLine($"FuzzyNameCount :  {fuzzyNameScreen.Count()}");

                //    //LogFile("Ends Checking with Fuzzy name", fuzzyNameScreen);
                //    return new ServiceResponse<CASELOGMATCH>()
                //    {
                //        RiskScore = risk.ToString(),
                //        RiskStatus = riskStatus,
                //        CaseId = screendto.CaseId,
                //        Data = fuzzyNameScreen.First(),
                //        ResponseCode = Resource.PositiveMatchCode,
                //        ResponseMessage = Resource.PositiveMatchDesc
                //    };
                //}
                return new ServiceResponse<CASELOGMATCH>()
                {
                    RiskScore = risk.ToString(),
                    RiskStatus = riskStatus,
                    CaseId = "",
                    ResponseCode = Resource.NoMatchCode,
                    ResponseMessage = Resource.NoMatchDesc
                };                
            }
            catch (Exception)
            {
                throw;
            }
        }
        //private async void LogFile(string message2, object data)
        //{
        //    string data3 = bool.TryParse(data.ToString(), out bool result) ? result.ToString() : "";

        //    string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        //    message += Environment.NewLine;
        //    message += "-----------------------------------------------------------";
        //    message += Environment.NewLine;
        //    message += message2;
        //    message += Environment.NewLine;

        //    message += string.Format("Message: {0}", data3);



        //    if (!System.IO.File.Exists(@"APISchedulerLogs.txt"))
        //    {
        //        using (System.IO.StreamWriter sw = System.IO.File.CreateText(@"APISchedulerLogs.txt"))
        //        {
        //            sw.WriteLine(message);
        //        }
        //    }
        //    else
        //    {
        //        using (System.IO.StreamWriter sw = System.IO.File.AppendText(@"APISchedulerLogs.txt"))
        //        {
        //            sw.WriteLine(message);
        //        }
        //    }



        //}
    }
}
