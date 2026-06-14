using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.TransactionMonitor;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.TransactionMonitor;
using AML.ViewModel.ViewModels.TransactionMonitor;
using AML.Web.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AML.Web.Helper;

namespace AML.Web.Controllers
{
    [Route("api/tms")]
    [ApiController]
    public class TmsAPIController : Controller
    {
        private readonly ITransactionMonitorService _transactionMonitorService;
        private readonly ICustomerCaseService _customerCaseService;
        private readonly IMapper _mapper;
        private ICommonService _commonService;
        private readonly string baseURL;
        private readonly string c6BaseURL;
        private readonly int checkThreshold;
        private readonly IServiceScopeFactory scopeFactory;

        public TmsAPIController(ITransactionMonitorService transactionMonitorService, IMapper mapper, ICustomerCaseService customerCaseService,
            IConfiguration configuration, ICommonService commonService, IServiceScopeFactory scopeFactory)
        {
            _transactionMonitorService = transactionMonitorService;
            _customerCaseService = customerCaseService;
            _mapper = mapper;
            _commonService = commonService;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            c6BaseURL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            checkThreshold = configuration.GetSection("C6BaseApiUrl").GetSection("Threshold").Value.ParseInt();
            this.scopeFactory = scopeFactory;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API Working");
        }
        [JwtAuthorize]
        [HttpPost("case")]
        public IActionResult InsertTmsCase([FromBody] TMSCaseModel model)
        {
            var result = _transactionMonitorService.InsertTMSMaster(_mapper.Map<TMSCaseModelDTO>(model));
            if (result.Status == 200)
                return Ok("TMS Case Created");
            else
                return Ok("Error creating TMS Case");
        }
        [JwtAuthorize]
        [HttpGet("rules")]
        public JsonResult GetRules()
        {
            var result = _transactionMonitorService.GetRules().Result;

            return Json(result);
        }

        private CustomerCaseDTO GetCustomerCaseDTO(dynamic TmsObject, string CustomerId = null)
        {
            Type TmsObjectType = TmsObject.GetType();
            PropertyInfo TmsObjectPropertyBName = TmsObjectType.GetProperty("Bankname");
            PropertyInfo TmsObjectPropertyCOI = TmsObjectType.GetProperty("CountryOfIncorporation");
            PropertyInfo TmsObjectPropertyDOB = TmsObjectType.GetProperty("Dob");

            CustomerCaseDTO customerCaseDTO = new CustomerCaseDTO
            {
                CustomerId = CustomerId ?? TmsObject.CustomerId,
                CustomerType = CustomerId == "" ? "C" : "I",
                FirstName = TmsObjectPropertyBName == null ? TmsObject.Name : TmsObject.Bankname,
                Nationality = TmsObjectPropertyCOI == null ? TmsObject.Nationality : TmsObject.CountryOfIncorporation
            };

            if (TmsObjectPropertyDOB != null && TmsObject.Dob != "")
            {
                customerCaseDTO.DOB = Convert.ToDateTime(TmsObject.Dob);
            }

            return customerCaseDTO;
        }

        [JwtAuthorize]
        [HttpPost("screen")]
        public async Task<JsonResult> ScreenAsync([FromBody] TMSScreeningModel screeningModel)
        {
            List<Task<CustomerCaseDTO>> apiResp = new List<Task<CustomerCaseDTO>>();
            if (ModelState.IsValid)
            {
                try
                {
                    var _ccDTO = new List<CustomerCaseDTO>
                    {
                        GetCustomerCaseDTO(screeningModel.Remitter),
                        GetCustomerCaseDTO(screeningModel.Beneficiary),
                        GetCustomerCaseDTO(screeningModel.Remitter.Bank, screeningModel.Remitter.Bank.Id),
                        GetCustomerCaseDTO(screeningModel.Beneficiary.Bank, screeningModel.Beneficiary.Bank.Id),
                        GetCustomerCaseDTO(screeningModel.Vendor, $"{screeningModel.Remitter.CustomerId}-{screeningModel.Beneficiary.CustomerId}-Vendor"),
                    };



                    foreach (var model in _ccDTO)
                    {
                        var result = _customerCaseService.Create(model);

                        if (result.Result == "0")
                        {
                            model.IsMatched = -1;
                            apiResp.Add(Task.Run(() => model));
                            continue;
                        }

                        model.Id = Convert.ToInt32(result.Result);

                        string body = string.Empty;
                        using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html")) { body = reader.ReadToEnd(); };

                        if (result.Status == StaticResource.SuccessStatusCode)
                        {
                            if (model.CustomerType == "I") apiResp.Add(_commonService.CustomerScreeningCall(model, baseURL, c6BaseURL, "API", body, checkThreshold, result.Result));
                            else if (model.CustomerType == "C") apiResp.Add(_commonService.CustomerScreeningCall(model, baseURL, c6BaseURL, "API", body, checkThreshold, result.Result));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("There was an error while screening: {0}", ex.Message));

                    Response.StatusCode = 500;

                    return Json(new
                    {
                        message = "Something went wrong while processing your request.",
#if DEBUG
                        devMessage = ex.Message,
                        devStacktrace = ex.StackTrace,
#endif
                    });
                }

                return Json(new
                {
                    Results = (await Task.WhenAll(apiResp)).Select((resp) =>
                    {
                        return new
                        {
                            resp.CustomerId,
                            IsMatched = resp.IsMatched == -1 ? "Record exists" : resp.IsMatched == 1 ? "Yes" : "No",
#if (DEBUG)
                            resp.MatchScore,
                            apiResults = resp.ApiResultsjson?.Take(5)
#endif
                        };
                    }).ToArray(),
                });
            }
            else
            {
                Response.StatusCode = 400;

                return Json(new
                {
                    message = "Invalid Request Token"
                });
            }
        }


        [JwtAuthorize]
        [HttpPost("monitor")]
        public async Task<JsonResult> TranMonitorAsync([FromBody] TransactionMonitorAPIModel[] tmsModel)
        {
            List<Task<TMSInsertStatusModel>> apiResp = new List<Task<TMSInsertStatusModel>>();
            TMSInsertStatusModel resp = new TMSInsertStatusModel();


            //List<TMSNewMasterDTO> tarns = tmsModel;


            if (tmsModel.Count() > 5)
            {
                return await Task.FromResult(Json(new
                {
                    message = "Cannot add more than 5 transactions in a batch!"
                }));
            }
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var model in tmsModel)
                    {


                        var result = _transactionMonitorService.InsertTransactionMonitor(_mapper.Map<TransactionMonitorAPIDTO>(model));

                        resp.TranRefno = model.TranRefno;
                        resp.Status = result.Status;
                        resp.Message = result.Message;

                        apiResp.Add(Task.Run(() => resp));

                    //    using var scope = scopeFactory.CreateScope();

                    //    var dbContext = scope.ServiceProvider.GetRequiredService<ITransactionMonitorService>();

                    //    var TMSRule = dbContext.GetAllTMSRules();

                    //    var Transactions = tmsModel;

                    //    //if (Transactions?.Count() == 0 || Transactions == null || TMSRule?.Count() == 0 || TMSRule == null)
                    //    //{
                    //    //    return;
                    //    //}

                    //    //Console.WriteLine($"Got {TMSRule.Count} rules from DB, and {Transactions.Count} transactions from DB.");

                    //    foreach (var rule in TMSRule)
                    //    {
                    //        bool isMasterRuleHit = true;

                    //        if (rule.TMSRuleParameters[0].TMSOperatorGRP == "AND")
                    //        {
                    //            isMasterRuleHit = true;
                    //        }
                    //        else
                    //        {
                    //            isMasterRuleHit = false;
                    //        }

                    //        List<TMSNewMasterDTO> MasterHits = new List<TMSNewMasterDTO>();

                    //        foreach (var ruleParam in rule.TMSRuleParameters)
                    //        {
                    //            int intCompareValue;

                    //            bool success = int.TryParse(ruleParam.TMSRuleDetCompareValue, out intCompareValue);
                    //            if (success)
                    //            {
                    //                ruleParam.TMSRuleDetDynamicCompareValue = intCompareValue;
                    //            }
                    //            else
                    //            {
                    //                ruleParam.TMSRuleDetDynamicCompareValue = ruleParam.TMSRuleDetCompareValue;
                    //            }

                    //            (bool isParamRuleHit, List<TMSNewMasterDTO> hits) = Builder.CheckRule(ruleParam, MasterHits.Count() > 0 ? MasterHits : Transactions);

                    //            Console.WriteLine($"isRuleHit: {isParamRuleHit}");
                    //            Console.WriteLine($"Got {hits.Count()} hit transactions");

                    //            if (isParamRuleHit)
                    //            {
                    //                MasterHits = hits;
                    //            }

                    //            if (ruleParam.TMSOperatorGRP == "AND")
                    //            {
                    //                isMasterRuleHit = isMasterRuleHit && isParamRuleHit;
                    //            }
                    //            else
                    //            {
                    //                isMasterRuleHit = isMasterRuleHit || isParamRuleHit;
                    //            }
                    //        }

                    //        if (isMasterRuleHit)
                    //        {
                    //            dbContext.InsertRuleViolatedTransactions(MasterHits, rule);
                    //        }
                    //    }

                    //    dbContext.SetMonitoredStatus(Transactions.Where(val => val.tmsstatus == 0).ToList());

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("There was an error while inserting: {0}", ex.Message));

                    Response.StatusCode = 500;

                    return await Task.FromResult(Json(new
                    {
                        message = "Something went wrong while processing your request.",
#if DEBUG
                        devMessage = ex.Message,
                        devStacktrace = ex.StackTrace,
#endif
                    }));
                }
                //if (result != 0)
                //    return Task.FromResult(Json(new
                //    {
                //        message = "Transaction Inserted successfully!"
                //    }));
                //else return Task.FromResult(Json(new
                //{
                //    message = "Something went wrong while processing your request."
                //}));

                return Json(new
                {
                    Results = (await Task.WhenAll(apiResp)).Select((resp) =>
                    {
                        return new
                        {
                            resp.TranRefno,
                            resp.Status,
                            resp.Message,

#if (DEBUG)

#endif
                        };
                    }).ToArray(),
                });


            }
            else
            {
                Response.StatusCode = 400;

                return await Task.FromResult(Json(new
                {
                    message = "Invalid Request Token"
                }));
            }


        }


    }
}
