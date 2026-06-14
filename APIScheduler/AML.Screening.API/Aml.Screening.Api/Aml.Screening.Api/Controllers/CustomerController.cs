using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using Aml.Screening.MongoModal.UnitOfWork;
using Aml.Screening.ServiceContracts.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Aml.Screening.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        public readonly ICustomerServices _customerServices;
        private IConfiguration _configuration;
        private readonly UnitOfWork_CaseLogRepository _unitOfWorkCaseLog;


        public CustomerController(ICustomerServices customerServices, IConfiguration configuration)
        {
            _customerServices = customerServices;
            _configuration = configuration;
            _unitOfWorkCaseLog = new UnitOfWork_CaseLogRepository(configuration);
        }                   
        [HttpPost]
        public async Task<IActionResult> ScreenCustomer([FromBody]CustomerDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(customerDto);
            }
            //Console.WriteLine("api call");
            Console.WriteLine($"Started Api call from api scheduler : {DateTime.Now}");
            
            Console.WriteLine($"Caseid :{customerDto.CASEID}");
            Console.WriteLine($"CustomerName :  {customerDto.CUSTOMERFULLNAME}");
            Console.WriteLine($"CustomerCategory : {customerDto.CUSTOMERCATEGORY}");
            Console.WriteLine($"WhiteListingDate : {customerDto.WHITELISTINGDATE}");
            Console.WriteLine($"WhiteListing : {customerDto.WHITELISTINGDATE}");

            //Console.WriteLine("CustomerNationality : ", customerDto.CUSTOMERNATIONALITY);


            //LogFile("Started Api call from api scheduler",customerDto);

            var result = await _customerServices.ScreenCustomerAsync(customerDto);
                return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> PendingCases([FromBody] CaseRequestDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(customerDto);
            }
            //Console.WriteLine("api call");
            Console.WriteLine($"Started Api call from api scheduler : {DateTime.Now}");

            Console.WriteLine($"Caseid :{customerDto.CASEID}");
   
            //Console.WriteLine("CustomerNationality : ", customerDto.CUSTOMERNATIONALITY);


            //LogFile("Started Api call from api scheduler",customerDto);
            var activeFilter = Builders<CASELOG>.Filter.Eq(x => x.CASEID, customerDto.CASEID);
            var matchrecords = Builders<CASELOG>.Filter.Where(x => x.MATCHRECORDS[0].MATCHTYPE != "KYC6");
            FilterDefinition<CASELOG> filter;
            filter = Builders<CASELOG>.Filter.And(activeFilter, matchrecords);
            var result = await _unitOfWorkCaseLog.FindAllLimitAsync(filter);
            if (result != null && result.Count() > 0) {

                foreach (var item in result) {

                    _unitOfWorkCaseLog.DeleteAsync(item._id);

                }


            }

            return Ok(result);
        }

        //private async void LogFile(string message2, CustomerDto data)
        //{
        //    string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        //    message += Environment.NewLine;
        //    message += "-----------------------------------------------------------";
        //    message += Environment.NewLine;
        //    message += message2;
        //    message += Environment.NewLine;

        //    message += string.Format("Caseid: {0}", data.CASEID);
        //    message += Environment.NewLine;
        //    message += string.Format("CustomerName: {0}", data.CUSTOMERFULLNAME);
        //    message += Environment.NewLine;
        //    message += string.Format("CustomerCategory: {0}", data.CUSTOMERCATEGORY);
        //    message += Environment.NewLine;
        //    message += string.Format("CustomerNationality: {0}", data.CUSTOMERNATIONALITY);
        //    message += Environment.NewLine;
        //    message += string.Format("Customercode: {0}", data.CUSTOMERCODE);
        //    message += Environment.NewLine;
        //    message += string.Format("Customerdob: {0}", data.CUSTOMERDOB);
        //    message += Environment.NewLine;



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
