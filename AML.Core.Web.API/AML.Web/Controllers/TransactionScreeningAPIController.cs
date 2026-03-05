using AML.DTO.DTO.TransactionScreening;
using AML.ViewModel.ViewModels.TransactionScreening;
using AML.Core.ServiceContract.TransactionScreening;
using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.Common;
using AML.Web.Helpers;
using AutoMapper;
using NLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AML.Web.Helper;
using Microsoft.Extensions.Logging;
using AML.Core.ServiceContract.CustomerCase;
using AML.DTO.DTO.Common;
using System.Threading;
using AML.DTO.DTO.TransactionMonitor;
using System.Diagnostics;
using AML.Core.ServiceContract.TransactionMonitor;

namespace AML.Web.Controllers
{
    [Route("api/transScreen")]
    [ApiController]
    public class TransactionScreeningAPIController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly ITransactionScreeningService _transactionScreening;
        private readonly ITransactionMonitorService _transactionMonitoring;
        private readonly string baseURL;
        private readonly string c6BaseURL;
        private readonly IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;
        private int checkThreshold;
        private ICustomerCaseService _customerCaseService;
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public TransactionScreeningAPIController(IMapper mapper, IConfiguration configuration, ICommonService commonService, ITransactionScreeningService transactionScreening, IHttpClientHandler clientHandler, ICustomerCaseService customerCaseService, ITransactionMonitorService transactionMonitoring)
        {
            _mapper = mapper;
            _commonService = commonService;
            _customerCaseService = customerCaseService;
            _clientHandler = clientHandler;
            _configuration = configuration;
            _transactionScreening = transactionScreening;
            _transactionMonitoring = transactionMonitoring;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            c6BaseURL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            checkThreshold = Convert.ToInt32(_configuration.GetSection("C6BaseApiUrl:Threshold").Value);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API Working");
        }
        [JwtAuthorize]
        [HttpPost("ScreenFromDB")]
        public IActionResult ScreenFromDB()
        {
            new Thread(() => _commonService.ScreenTMSFromDB(c6BaseURL)).Start();

            return Ok(new { message = "Screening from DB started." });
        }
        [JwtAuthorize]
        [HttpPost("screen")]
        public async Task<JsonResult> tranScreenAsync([FromBody] ScreeningModel[] screeningModel)
        {
            List<Task<TranScreenDTO>> apiResp = new List<Task<TranScreenDTO>>();

            IsWhiteListedCheckDTO IsWhiteListed = new IsWhiteListedCheckDTO();

            if (screeningModel.Count() > 5)
            {
                return Json(new
                {
                    message = "Cannot add more than 5 transactions in a batch!"
                });
            }
            if (ModelState.IsValid)
            {
                int CreatedBy = 0;
                try
                {
                    foreach (ScreeningModel s in screeningModel)
                    {
                        //Client Id and User Id
                        ClientMasterDTO clientMasterDTO = _customerCaseService.GetAllClients().Find(val => val.ClientName == s.CompanyName);

                        if (clientMasterDTO == null)
                        {
                            log.Error($"Coud not get customerid of {s.CompanyName}");
                            throw new Exception(string.Format("Could not get id of '{0}'", s.CompanyName));
                        }
                        if (int.TryParse(_customerCaseService.GetUserIdForAPI(s.UserId), out int createdById))
                        {
                            CreatedBy = createdById;
                        }
                        else
                        {
                            CreatedBy = -1;
                        }

                        if (s.CreatedBy == -1)
                        {
                            throw new Exception($"User '{s.UserId}' could not be foud.");
                        }
                        //End 
                        var _ccDTO = new List<TranScreenDTO>
                    {

                        GetTranScreenDTO(s.Remitter, null, s.TranRefNo),
                        GetTranScreenDTO(s.Beneficiary,null, s.TranRefNo),
                        GetTranScreenDTO(s.Remitter.Bank, $"{s.Remitter.CustomerId}-Bank", s.TranRefNo),
                        GetTranScreenDTO(s.Beneficiary.Bank, $"{s.Beneficiary.CustomerId}-Bank", s.TranRefNo),
                        GetTranScreenDTO(s.Vendor, $"{s.Remitter.CustomerId}-{s.Beneficiary.CustomerId}-Vendor", s.TranRefNo),
                    };


                        foreach (var model in _ccDTO)
                        {
                            //Check if the Customer Id provided in API is an alreday whitelisted onboarded customer
                            IsWhiteListed = _transactionScreening.checkIfCusWhiteListed(model.CustRefNo);
                            if (IsWhiteListed.IsNotNullOrEmpty()) { model.IsWhiteListed = IsWhiteListed.IsWhiteListed; }
                            model.Threshold = checkThreshold;
                            model.ClientId = clientMasterDTO.ClientId;
                            model.CreatedBy = CreatedBy;
                            var result = _transactionScreening.InsertTransactionScreening(model);

                            if (result.Result == 0)
                            {
                                model.IsMatched = -1;
                                apiResp.Add(Task.Run(() => model));
                                continue;
                            }

                            model.Id = result.Result;

                            string body = string.Empty;
                            using (StreamReader reader = new StreamReader(@"Views/TransactionScreening/TranRiskEmailBody.html")) { body = reader.ReadToEnd(); };

                            if (result.Status == StaticResource.SuccessStatusCode)
                            {

                                apiResp.Add(_commonService.TransactionScreeningCall(model, baseURL, c6BaseURL, "API", body, result.Result, "Free"));


                            }
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

                //return Json(apiResp);

                return Json(new
                {
                    Results = (await Task.WhenAll(apiResp)).Select((resp) =>
                    {
                        return new
                        {
                            resp.TranRefNo,
                            resp.CustRefNo,
                            resp.Name,
                            IsMatched = resp.IsMatched == -1 ? "Record exists" : resp.Status != 10 ? resp.IsMatched == 1 ? "Yes" : "No" : "Something went wrong while processing your request.",

#if (DEBUG)
                            resp.MatchName,
                            resp.MatchScore,
                            apiResults = resp.ApiResjson?.Take(5)
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

        [HttpPost("status")]
        public async Task<IActionResult> Status([FromBody] ScreeningModel model)
        {
            string resp = string.Empty;

            TranstatusCheckDTO apiResp = new TranstatusCheckDTO();
            try
            {
                apiResp = _commonService.TransactionStatusCheck(model.TranRefNo);

                if (apiResp.IsNotNullOrEmpty())
                {

                    resp = apiResp.status;

                }
                else
                    resp = "not found";
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("There was an error checking for status: {0}", ex.Message));
                return StatusCode(500, new
                {
                    message = "Something went wrong while processing your request.",
#if DEBUG
                    devMessage = ex.Message,
                    devStacktrace = ex.StackTrace,
#endif
                });
            }
            return Ok(new
            {
                TransactionStatus = resp
            });
        }


        private TranScreenDTO GetTranScreenDTO(dynamic TmsObject, string CustomerId = null, string TranRefNo = null)
        {

            Type TmsObjectType = TmsObject.GetType();
            //PropertyInfo TmsObjectPropertyRemitter = TmsObjectType.GetProperty("Remitter");
            PropertyInfo TmsObjectPropertyBName = TmsObjectType.GetProperty("Bankname");
            PropertyInfo TmsObjectPropertyCOI = TmsObjectType.GetProperty("CountryOfIncorporation");
            PropertyInfo TmsObjectPropertyDOB = TmsObjectType.GetProperty("Dob");
            try
            {


                TranScreenDTO tranScreenDTO = new TranScreenDTO
                {
                    TranRefNo = TranRefNo,
                    CustRefNo = CustomerId ?? TmsObject.CustomerId,
                    CustType = CustomerId == null ? "I" : "C",
                    Name = TmsObjectPropertyBName == null ? TmsObject.Name : TmsObject.Bankname,
                    Country = TmsObjectPropertyCOI == null ? TmsObject.Nationality : TmsObject.CountryOfIncorporation
                };



                if (TmsObjectPropertyDOB != null && TmsObject.Dob != "")
                {
                    tranScreenDTO.DOB = Convert.ToDateTime(TmsObject.Dob);
                }
                //tranScreenDTO.CreatedBy = _clientHandler.GetUserId();
                //tranScreenDTO.UpdatedBy = _clientHandler.GetUserId();

                return tranScreenDTO;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return null;
            }

        }

        [HttpPost("monitor")]
        public Task<JsonResult> TranMonitorAsync([FromBody] TMSNewMasterDTO newTransactionDTO, bool debug)
        {
            try
            {
                ClientMasterDTO clientMasterDTO = _customerCaseService.GetAllClients().Find(val => val.ClientName == newTransactionDTO.CompanyName);

                if (clientMasterDTO == null)
                {
                    log.Error($"Coud not get customerid of {newTransactionDTO.CompanyName}");
                    throw new Exception(string.Format("Could not get id of '{0}'", newTransactionDTO.CompanyName));
                }
                else {
                    newTransactionDTO.ClientId = clientMasterDTO.ClientId;
                }

                if (int.TryParse(_customerCaseService.GetUserIdForAPI(newTransactionDTO.UserId), out int createdById))
                {
                    newTransactionDTO.createdby = createdById;
                }
                else
                {
                    newTransactionDTO.createdby = -1;
                }

                if (newTransactionDTO.createdby == -1)
                {
                    throw new Exception($"User '{newTransactionDTO.createdby}' could not be foud.");
                }
                newTransactionDTO.assignedto = newTransactionDTO.assignedto ?? 0;
                newTransactionDTO.createdby = newTransactionDTO.createdby ?? 0;

                var transactionId = _transactionMonitoring.InsertTransactionMonitor(_mapper.Map<TransactionMonitorAPIDTO>(newTransactionDTO)).Result;

                newTransactionDTO.id = transactionId;

                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Start();
                var TMSRule = _transactionMonitoring.GetAllTMSRules(newTransactionDTO.ClientId).Where(rule => rule.TMSRuleIsActive == 1 && rule.Client_Id == newTransactionDTO.ClientId).ToList();
                
                var Transactions = _transactionMonitoring.GetAllTransactionsForTMS(newTransactionDTO);
                stopwatch.Stop();
                log.Info($"Got {Transactions.Count()} transactions from DB (took {stopwatch.ElapsedMilliseconds}ms)");

                Console.WriteLine($"Got {TMSRule.Count} rules from DB, and {Transactions.Count} transactions from DB.");

                var hitRulesCount = 0;
                var hitRules = new List<string>();

                foreach (var rule in TMSRule)
                {
                    if (rule.TMSRuleDescription == "Multiple") 
                    {
                        Transactions = _transactionMonitoring.GetAllTransactionsBycustomerIdforTMS(newTransactionDTO);
                    }


                    log.Debug($"Checking rule: {rule.TMSRuleName}");

                    bool isMasterRuleHit = true;

                    if (rule.TMSRuleParameters[0].TMSOperatorGRP == "AND" || rule.TMSRuleParameters[0].TMSOperatorGRP == null)
                    {
                        log.Debug("Param has and grouping, setting isMasterRuleHit = true");
                        isMasterRuleHit = true;
                    }
                    else
                    {
                        log.Debug("Param has or grouping, setting isMasterRuleHit = false");
                        isMasterRuleHit = false;
                    }

                    List<TMSNewMasterDTO> MasterHits = new List<TMSNewMasterDTO>();

                    foreach (var ruleParam in rule.TMSRuleParameters)
                    {
                        log.Debug("Processing rule parameter");

                        int intCompareValue;

                        bool success = int.TryParse(ruleParam.TMSRuleDetCompareValue, out intCompareValue);

                        if (success)
                        {
                            ruleParam.TMSRuleDetDynamicCompareValue = intCompareValue;
                        }
                        else
                        {
                            ruleParam.TMSRuleDetDynamicCompareValue = ruleParam.TMSRuleDetCompareValue;
                        }
                        if (ruleParam.TMSRuleDetPercentValue != 0 && ruleParam.TMSRuleDetPercentValue != null) 
                        {
                            ruleParam.TMSRuleDetCompareIsPercent= true;
                        }

                        Stopwatch stopwatchRules = Stopwatch.StartNew();
                        stopwatchRules.Start();
                        (bool isRuleHit, List<TMSNewMasterDTO> hits) = Builder.CheckRule(ruleParam, MasterHits.Count() > 0 ? MasterHits : Transactions, ruleParam.TMSRuleDetCustomCheckerString, ruleParam.TMSRuleDetCustomGetterString);
                        stopwatchRules.Stop();
                        log.Info($"Got {Transactions.Count()} transactions from DB (took {stopwatch.ElapsedMilliseconds}ms)");

                        bool isParamRuleHit = isRuleHit && hits?.Where(x => x.TranRefno == newTransactionDTO.TranRefno).Count() > 0;

                        log.Debug($"Is param rule hit: {isParamRuleHit}");

                        if (ruleParam.TMSOperatorGRP == "AND" || ruleParam.TMSOperatorGRP == null)
                        {
                            isMasterRuleHit = isMasterRuleHit && isParamRuleHit;
                        }
                        else
                        {
                            isMasterRuleHit = isMasterRuleHit || isParamRuleHit;
                        }

                        log.Debug($"is master rule hit: {isMasterRuleHit}");

                        if (isMasterRuleHit && hits != null)
                        {
                            log.Debug($"Add {hits?.Count()} transactions to master list");

                            MasterHits.AddRange(hits.Where(hit => hit.tmsstatus == 0 && MasterHits.FindAll(masterHit => masterHit.TranRefno == hit.TranRefno).Count() == 0));
                        }
                    }

                    if (isMasterRuleHit)
                    {
                        hitRules.Add(rule.TMSRuleName);
                        hitRulesCount++;

                        log.Info($"Insert rule violated transactions");

                        new Thread(delegate ()
                        {
                            Stopwatch stopwatch = Stopwatch.StartNew();
                            stopwatch.Start();
                            _transactionMonitoring.InsertRuleViolatedTransactions(MasterHits, rule);
                            stopwatch.Stop();
                            log.Info($"Inserted rule violated transactions (took {stopwatch.ElapsedMilliseconds}ms)");
                        }).Start();
                    }
                }

                log.Info($"Update monitor status");

                new Thread(delegate ()
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    stopwatch.Start();
                    _transactionMonitoring.SetMonitoredStatus(Transactions.Where(val => val.tmsstatus == 0).ToList());
                    stopwatch.Stop();
                    log.Info($"Updated monitor status (took {stopwatch.ElapsedMilliseconds}ms)");
                }).Start();

                return Task.FromResult(Json(new
                {
                    status = 200,
                    isMonitorHit = hitRulesCount > 0,
                    rulesViolatedCount = hitRulesCount,
                    rulesViolated = hitRules
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");

                return Task.FromResult(Json(new
                {
                    status = 500,
                    message = ex.Message
                }));
            }


        }

    }
}
