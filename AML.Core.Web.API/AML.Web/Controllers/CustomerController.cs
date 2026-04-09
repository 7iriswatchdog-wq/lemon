using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using AML.Core.Common.StaticResource;
using AML.Core.Service.CaseComment;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.CustomerCase;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.FreeSource;
using AML.ViewModel.ViewModels.ApiAuthentication;
using AML.ViewModel.ViewModels.CaseComment;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.Risk;
using AML.ViewModel.ViewModels.RiskAPI;
using AML.Web.Helper;
using AML.Web.Helpers;

using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Org.BouncyCastle.Bcpg;

namespace AML.Web.Controllers
{
    [Route("api/customer")]
    public class CustomerController : Controller
    {
        private IMapper _mapper;
        private IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;
        private ICustomerCaseService _customerCaseService;
        private string baseURL = string.Empty;
        private string c6BaseURL = string.Empty;
        private string secret = string.Empty;
        private ICommonService _commonService;
        private int checkThreshold = 0;
        private RiskAPIController _riskAPIController;
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private ICaseCommentService _caseCommentService;

        public CustomerController(IMapper mapper, IHttpClientHandler clientHandler, ICommonService commonService,
            IConfiguration configuration, ICustomerCaseService customerCaseService, RiskAPIController riskAPIController, ICaseCommentService caseCommentService
            )
        {
            _customerCaseService = customerCaseService;
            _configuration = configuration;
            _clientHandler = clientHandler;
            _mapper = mapper;
            _commonService = commonService;
            _riskAPIController = riskAPIController;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            c6BaseURL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            secret = configuration.GetSection("JwtAppSettings").GetSection("Secret").Value;
            _caseCommentService = caseCommentService;
        }

        // Get api/customer
        [JwtAuthorize]
        [HttpGet]
        public IActionResult Get(CaseModel model)
        {
            return Ok("API Working");
        }

        // POST api/customer
        [JwtAuthorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerCaseApiModel model)
        {
            string resp = string.Empty;
            CustomerCaseDTO apiResp = new CustomerCaseDTO();
            if (ModelState.IsValid)
            {
                try
                {
                    model.CustomerType = model.CustomerType.IsNotNullOrEmpty() ? model.CustomerType : "I";
                    var result = _customerCaseService.Create(_mapper.Map<CustomerCaseDTO>(model));

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    };

                    if (result.Status == 200)
                    {
                        apiResp = await _commonService.CustomerScreeningCall(result.Result, baseURL, c6BaseURL, "API", body, model.Threshold);
                        resp = "Customer case added successfully";
                    }
                    else
                    {
                        resp = "Customer case creation failed";
                    }
                }
                catch { }
            }
            return Ok(new
            {
                id = apiResp.Id,
                customerId = apiResp.CustomerId,
                dob = apiResp.CustDOB,
                matchScore = apiResp.MatchScore,
                isMatched = apiResp.IsMatched,
                firstName = apiResp.FirstName,
                middleName = apiResp.MiddleName,
                lastName = apiResp.LastName,
                nationality = apiResp.Nationality,
                riskScore = apiResp.RiskScore,
                customerIdType = apiResp.CustomerIdType,
                customerIdNumber = apiResp.CustomerIdNumber,
                mobile = apiResp.Mobile,
                category = apiResp.MatchCategory
            });
        }
        // POST api/customer/Strict
        [JwtAuthorize]
        [HttpPost("Strict")]
        public async Task<IActionResult> PostStrict([FromBody] CustomerApiModel model)
        {
            string resp = string.Empty;
            CustomerCaseDTO apiResp = new CustomerCaseDTO();
            if (ModelState.IsValid)
            {
                try
                {
                    model.CustomerType = model.CustomerType.IsNotNullOrEmpty() ? model.CustomerType : "I";
                    var result = _customerCaseService.Create(_mapper.Map<CustomerCaseDTO>(model));

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    };

                    if (result.Status == 200)
                    {
                        apiResp = await _commonService.CustomerScreeningCall(result.Result, baseURL, c6BaseURL, "API", body);
                        resp = "Customer case added successfully";
                    }
                    else
                    {
                        resp = "Customer case creation failed";
                    }
                }
                catch { }
            }
            return Ok(new
            {
                id = apiResp.Id,
                customerId = apiResp.CustomerId,
                dob = apiResp.CustDOB,
                matchScore = apiResp.MatchScore,
                isMatched = apiResp.IsMatched,
                firstName = apiResp.FirstName,
                middleName = apiResp.MiddleName,
                lastName = apiResp.LastName,
                nationality = apiResp.Nationality,
                riskScore = apiResp.RiskScore,
                customerIdType = apiResp.CustomerIdType,
                customerIdNumber = apiResp.CustomerIdNumber,
                mobile = apiResp.Mobile,
                category = apiResp.MatchCategory
            });
        }




        [JwtAuthorize]
        // POST api/customer/status
        [HttpPost("status")]
        public async Task<IActionResult> Status([FromBody] CustomerCaseApiModel model)
        {
            string resp = string.Empty;
            CustomerCaseDTO apiResp = new CustomerCaseDTO();
            try
            {
                //  model.CustomerType = model.CustomerType.IsNotNullOrEmpty() ? model.CustomerType : "I";
                //     var result = _customerCaseService.Create(_mapper.Map<CustomerCaseDTO>(model));
                apiResp = _commonService.CustomerStatusCheck(model.CustomerId);

                if (apiResp.IsNotNullOrEmpty())
                {
                    if (apiResp.IsDelete == 0)
                    {
                        //resp = "Customer case existing";
                        if (apiResp.Status == 0)
                            resp = "Pending";
                        else if ((apiResp.Status == 2) || (apiResp.Status == 5))
                            resp = "Approved";
                        else if (apiResp.Status == 3)
                            resp = "Rejected";
                    }

                }
                else
                    resp = "not found";
            }
            catch { }

            //  CreatedAndUpdatedByNames createdAndUpdatedByNames = _commonService.GetCreatedByAndUpdateByNameFromId(apiResp.CreatedBy, apiResp.UpdatedBy);

            //     string status = (((apiResp.Status == 2) || (apiResp.Status == 5)) ? "Approved" : "Rejected");
            return Ok(new
            {
                CustomerStatus = resp,
                //createdBy = createdAndUpdatedByNames.CreatedBy,
                //createdDate = apiResp.CreatedOnDB,
                //updatedBy = createdAndUpdatedByNames.UpdatedBy,
                //updatedDate = apiResp.UpdatedOnDB
            });
        }
#if DEBUG
        private readonly Random _random = new Random();

        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }
        public string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26;

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

        public string RandomCID()
        {
            var passwordBuilder = new StringBuilder();

            // 4-Letters lower case   
            passwordBuilder.Append(RandomString(4, true));

            // 4-Digits between 1000 and 9999  
            passwordBuilder.Append(RandomNumber(1000, 9999));

            // 2-Letters upper case  
            passwordBuilder.Append(RandomString(2));
            return string.Join("", passwordBuilder.ToString().ToCharArray().OrderBy(x => _random.Next()));
        }
#endif

        [JwtAuthorize]
        // POST api/customer/screening
        [HttpPost("ScreenFromDB")]
        public IActionResult ScreenFromDB()
        {
            new Thread(() => _commonService.ScreenCustomersFromDB(c6BaseURL)).Start();

            return Ok(new { message = "Screening from DB started." });
        }

        // POST api/customer/screening
        [JwtAuthorize]
        [HttpPost("Screening")]
        public async Task<IActionResult> Screening([FromBody] CustomerCaseApiModel model)
        {
            if (model.Threshold < 50)
            {
                return BadRequest(new
                {
                    message = "Threshold must be more than 50"
                });
            }

            log.Debug("Start API screening");
            CustomerCaseDTO apiResp = new CustomerCaseDTO();
            if (ModelState.IsValid)
            {
                try
                {
                    checkThreshold = model.Threshold;
                    log.Info($"Setting threshold to: {checkThreshold}");
                    model.CustomerType = model.CustomerType.IsNotNullOrEmpty() ? model.CustomerType : "I";
                    log.Info($"CustomerType: {model.CustomerType}");
                    ClientMasterDTO clientMasterDTO = _customerCaseService.GetAllClients().Find(val => val.ClientName == model.CompanyName);

                    if (clientMasterDTO == null)
                    {
                        log.Error($"Coud not get customerid of {model.CompanyName}");
                        throw new Exception(string.Format("Could not get id of '{0}'", model.CompanyName));
                    }
//#if DEBUG
//                    log.Debug("Debug mode, Generating random CID");
//                    model.CustomerId = RandomCID();
//#endif
                    model.ClientId = clientMasterDTO.ClientId;

                    CustomerCaseDTO _ccDTO = _mapper.Map<CustomerCaseDTO>(model);
                    log.Debug("Storing initial details in db");

                    if (int.TryParse(_customerCaseService.GetUserIdForAPI(model.UserId), out int createdById))
                    {
                        _ccDTO.CreatedBy = createdById;
                    }
                    else
                    {
                        _ccDTO.CreatedBy = -1;
                    }

                    if (_ccDTO.CreatedBy == -1)
                    {
                        throw new Exception($"User '{model.UserId}' could not be foud.");
                    }

                    var result = _customerCaseService.Create(_ccDTO);

                    log.Debug($"Got new case id {result.Result}");

                    if (result.Result == "0" && result.Result != "")
                    {
                        log.Debug("Record already exists");
                        return Ok(new
                        {
                            //matchScore = apiResp.MatchScore,
                            CustomerId = model.CustomerId,
                            isMatched = "Record Exists"
                        });
                    }

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html")) { body = reader.ReadToEnd(); };

                    if (result.Result != "")
                    {
                        log.Debug("Send customer details for screening");
                        if (model.CustomerType == "I")
                            apiResp = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, c6BaseURL, "API", body, checkThreshold, result.Result.Split('Ø')[1]);
                        else if (model.CustomerType == "C")
                            apiResp = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, c6BaseURL, "API", body, model.Threshold, result.Result.Split('Ø')[1]);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);

                    return BadRequest(new
                    {
                        message = ex.Message
                    });
                }

                log.Debug("Send response json");

                return Ok(new
                {
                    //matchScore = apiResp.MatchScore,
                    isMatched = apiResp.IsMatched == 1 ? "Yes" : "No",
                    matchScore = apiResp.MatchScore,
                    customerId = apiResp.CustomerId,
                });
            }
            else
            {
                log.Warn("Model invalid", ModelState.Keys
                    .Where(i => ModelState[i].Errors.Count > 0)
                    .Select(k => new KeyValuePair<string, string>(k, ModelState[k].Errors.First().ErrorMessage)));
                return BadRequest(new
                {
                    message = "Invalid Request Token"
                });
            }
        }
        [JwtAuthorize]
        [HttpPost("ScreeningForSourceList")]
        public async Task<IActionResult> ScreeningForSourceList([FromBody] CustomerCaseApiModel model)
        {
            if (model.Threshold < 50)
            {
                return BadRequest(new
                {
                    message = "Threshold must be more than 50"
                });
            }

            log.Debug("Start API screening");
            CustomerCaseDTO apiResp = new CustomerCaseDTO();
            if (ModelState.IsValid)
            {
                try
                {
                    checkThreshold = model.Threshold;
                    log.Info($"Setting threshold to: {checkThreshold}");
                    model.CustomerType = model.CustomerType.IsNotNullOrEmpty() ? model.CustomerType : "I";
                    log.Info($"CustomerType: {model.CustomerType}");
                    ClientMasterDTO clientMasterDTO = _customerCaseService.GetAllClients().Find(val => val.ClientName == model.CompanyName);

                    if (clientMasterDTO == null)
                    {
                        log.Error($"Coud not get customerid of {model.CompanyName}");
                        throw new Exception(string.Format("Could not get id of '{0}'", model.CompanyName));
                    }
                    //#if DEBUG
                    //                    log.Debug("Debug mode, Generating random CID");
                    //                    model.CustomerId = RandomCID();
                    //#endif
                    model.ClientId = clientMasterDTO.ClientId;

                    CustomerCaseDTO _ccDTO = _mapper.Map<CustomerCaseDTO>(model);
                    log.Debug("Storing initial details in db");

                    if (int.TryParse(_customerCaseService.GetUserIdForAPI(model.UserId), out int createdById))
                    {
                        _ccDTO.CreatedBy = createdById;
                    }
                    else
                    {
                        _ccDTO.CreatedBy = -1;
                    }

                    if (_ccDTO.CreatedBy == -1)
                    {
                        throw new Exception($"User '{model.UserId}' could not be foud.");
                    }

                    var result = _customerCaseService.Create(_ccDTO);

                    log.Debug($"Got new case id {result.Result}");

                    if (result.Result == "0" && result.Result != "")
                    {
                        log.Debug("Record already exists");
                        return Ok(new
                        {
                            //matchScore = apiResp.MatchScore,
                            CustomerId = model.CustomerId,
                            isMatched = "Record Exists"
                        });
                    }

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html")) { body = reader.ReadToEnd(); };

                    if (result.Result != "")
                    {
                        log.Debug("Send customer details for screening");
                        // For PureGold Only sanction screening needed

                        string response = string.Empty;
                        var searchType = "F";
                        string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
                        {
                            customerdob = model.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                            customerfullname = string.Concat(_ccDTO.FirstName, " ", _ccDTO.MiddleName, " ", _ccDTO.LastName),
                            customernationality = model.Nationality,
                            searchtype = searchType
                        }, ScreeningService.BACKLIST_SCREENING).Result;
                        var matchrecordsList = new List<MatchRecordsDTO>();
                        var stat = 0;
                        string matchScore = string.Empty;
                        if (!string.IsNullOrEmpty(data))
                        {
                            log.Debug("Send response json");
                            List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                            //logic for matching score comparing with threshold
                            foreach (var item in apiResultModel)
                            {

                                var matchrecords = new MatchRecordsDTO();
                                if (item.IsNotNullOrEmpty())
                                {

                                    if (Convert.ToInt32(item.matchscore) >= checkThreshold)
                                    {
                                        matchScore = item.matchscore;
                                        matchrecordsList.Add(matchrecords);
                                        stat++;
                                    }


                                }
                            }
                            bool isMatchedApiResult = false;
                            if (stat >= 1)
                            {
                                isMatchedApiResult = true;
                            }




                            if (apiResultModel.Count > 0)
                            {
                                _commonService.UpdateSanctionRecords(_ccDTO, apiResultModel, _ccDTO.CustomerId.ToString());

                                return Ok(new
                                {

                                    isMatched = (isMatchedApiResult) ? "Yes" : "No",
                                    matchScore = apiResultModel.FirstOrDefault().matchscore,
                                    customerId = _ccDTO.CustomerId
                                });
                            }




                        }


                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);

                    return BadRequest(new
                    {
                        message = ex.Message
                    });
                }



                return Ok(new
                {
                    //matchScore = apiResp.MatchScore,
                    isMatched = apiResp.IsMatched == 1 ? "Yes" : "No",
                    matchScore = apiResp.MatchScore,
                    customerId = apiResp.CustomerId,
                });
            }
            else
            {
                log.Warn("Model invalid", ModelState.Keys
                    .Where(i => ModelState[i].Errors.Count > 0)
                    .Select(k => new KeyValuePair<string, string>(k, ModelState[k].Errors.First().ErrorMessage)));
                return BadRequest(new
                {
                    message = "Invalid Request Token"
                });
            }
        }
        public async Task<ShareholderResults> ScreenShareHolder(CustomerCaseWithShareholderApiModel model, ShareholderDetails shareholder, int shareholderIndex, string body)
        {
            var _ccDTO = _mapper.Map<CustomerCaseDTO>(new CustomerCaseApiModel
            {
                CustomerId = model.CustomerId + "-sh" + shareholderIndex,
                FirstName = shareholder.FirstName,
                MiddleName = shareholder.MiddleName,
                LastName = shareholder.LastName,
                MatchCategory = model.MatchCategory,
                CustomerIdNumber = model.CustomerIdNumber,
                Nationality = shareholder.Nationality,
                DOB = shareholder.DOB,
                CustomerType = "I",
                UserId = model.UserId,
                CompanyName = model.CompanyName,
                Threshold = model.Threshold,
                CompanyCode = model.CompanyCode,
                ClientId = model.ClientId,
            });

            var resultShareholder = _customerCaseService.Create(_ccDTO);

            _ccDTO.IsPep = true;
            _ccDTO.IsSan = true;
            _ccDTO.IsRre = true;
            _ccDTO.IsIns = true;
            _ccDTO.IsDd = true;
            _ccDTO.IsPoi = true;
            _ccDTO.IsRel = true;

            var apiRespShareHolder = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, c6BaseURL, "API", body, checkThreshold, resultShareholder.Result.ParseInt());

            return new ShareholderResults
            {
                isMatched = apiRespShareHolder.IsMatched == 1 ? "Yes" : "No",
                matchScore = apiRespShareHolder.MatchScore,
                customerId = apiRespShareHolder.CustomerId
            };
        }
        // POST api/customer/screening/withShareholder
        [JwtAuthorize]
        [HttpPost("Screening/withShareholder")]
        public async Task<IActionResult> ScreeningWithShareholder([FromBody] CustomerCaseWithShareholderApiModel model)
        {
            CustomerCaseDTO apiResp = new CustomerCaseDTO();
            CustomerCaseDTO apiRespShareHolder = new CustomerCaseDTO();
            List<ShareholderResults> apiRespShareHolderList = new List<ShareholderResults>();

            if (model.Shareholders == null)
            {
                model.Shareholders = new List<ShareholderDetails>();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    checkThreshold = model.Threshold;
                    log.Info($"Setting threshold to: {checkThreshold}");
                    model.CustomerType = model.CustomerType.IsNotNullOrEmpty() ? model.CustomerType : "I";
                    log.Info($"CustomerType: {model.CustomerType}");
                    ClientMasterDTO clientMasterDTO = _customerCaseService.GetAllClients().Find(val => val.ClientName == model.CompanyName);

                    if (clientMasterDTO == null)
                    {
                        log.Error($"Coud not get customerid of {model.CompanyName}");
                        throw new Exception(string.Format("Could not get id of '{0}'", model.CompanyName));
                    }
#if DEBUG
                    log.Debug("Debug mode, Generating random CID");
                    model.CustomerId = RandomCID();
#endif
                    model.ClientId = clientMasterDTO.ClientId;

                    CustomerCaseDTO _ccDTO = _mapper.Map<CustomerCaseDTO>(model);
                    log.Debug("Storing initial details in db");

                    if (int.TryParse(_customerCaseService.GetUserIdForAPI(model.UserId), out int createdById))
                    {
                        _ccDTO.CreatedBy = createdById;
                    }
                    else
                    {
                        _ccDTO.CreatedBy = -1;
                    }

                    if (_ccDTO.CreatedBy == -1)
                    {
                        throw new Exception($"User '{model.UserId}' could not be foud.");
                    }

                    var result = _customerCaseService.Create(_ccDTO);
                    log.Debug($"Got new case id {result.Result}");
                    if (result.Result == "0" && result.Result != "")
                    {
                        log.Debug("Record already exists");
                        return Ok(new
                        {
                            //matchScore = apiResp.MatchScore,
                            CustomerId = model.CustomerId,
                            isMatched = "Record Exists"
                        });
                    }
                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html")) { body = reader.ReadToEnd(); };

                    if (result.Result != "")
                    {
                        log.Debug("Send customer details for screening");
                        if (model.CustomerType == "I")
                            apiResp = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, c6BaseURL, "API", body, checkThreshold, result.Result.ParseInt());
                        else if (model.CustomerType == "C")
                        {
                            apiResp = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, c6BaseURL, "API", body, model.Threshold, result.Result.ParseInt());
                            // await _commonService.CustomerScreeningCall(_ccDTO, baseURL, c6BaseURL, "API", body, model.Threshold, result.Result);
                            List<Task<ShareholderResults>> taskList = new List<Task<ShareholderResults>>();
                            foreach (var shareholder in model.Shareholders.Select((value, i) => new { i, value }))
                            {
                                taskList.Add(ScreenShareHolder(model, shareholder.value, shareholder.i, body));
                            }
                            apiRespShareHolderList = (await Task.WhenAll(taskList)).ToList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);

                    return BadRequest(new
                    {
                        message = ex.Message
                    });
                }

                if (model.CustomerType == "I")
                {
                    return Ok(new
                    {
                        isMatched = apiResp.IsMatched == 1 ? "Yes" : "No",
                        matchScore = apiResp.MatchScore,
                        customerId = apiResp.CustomerId
                    });
                }
                else
                {
                    return Ok(new
                    {
                        isMatched = apiResp.IsMatched == 1 ? "Yes" : "No",
                        matchScore = apiResp.MatchScore,
                        customerId = apiResp.CustomerId,
                        shareholders = apiRespShareHolderList
                    });
                }
            }
            else
            {
                log.Warn("Model invalid", ModelState.Keys
                    .Where(i => ModelState[i].Errors.Count > 0)
                    .Select(k => new KeyValuePair<string, string>(k, ModelState[k].Errors.First().ErrorMessage)));
                return BadRequest(ModelState);
            }
        }

        // POST api/customer/CustomerScreening
        [JwtAuthorize]
        [HttpPost("CustomerScreening")]
        public async Task<IActionResult> FreeSourceScreening([FromBody] ScreenCaseModel model)
        {
            CustomerCaseDTO apiResp = new CustomerCaseDTO();
            if (ModelState.IsValid)
            {
                try
                {
                    model.CustomerType = model.CustomerType.IsNotNullOrEmpty() ? model.CustomerType : "I";
                    model.MatchCategory = model.MatchCategory.IsNotNullOrEmpty() ? model.MatchCategory : "INDIVIDUAL";
                    var result = _customerCaseService.Create(_mapper.Map<CustomerCaseDTO>(model));

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    };

                    if (result.Status == StaticResource.SuccessStatusCode)
                    {
                        apiResp = await _commonService.CustomerScreeningCall(result.Result, baseURL, c6BaseURL, "API", body);
                    }
                }
                catch { }
                CreatedAndUpdatedByNames createdAndUpdatedByNames = _commonService.GetCreatedByAndUpdateByNameFromId(apiResp.CreatedBy, apiResp.UpdatedBy);

                return Ok(new
                {
                    isMatched = apiResp.IsMatched == 1 ? "Yes" : "No",
                    matchScore = apiResp.MatchScore,
                    customerId = apiResp.CustomerId
                });
            }
            else
                return BadRequest(new
                {
                    message = "Invalid Request token"
                });
        }

        // creating new function for scheduler maintaining


        private async Task<string> GetApprovedRiskEmailBody()
        {
            using (StreamReader reader = new StreamReader(@"Views/Risk/ApprovedRiskEmailBody.html"))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task<string> GetApprovedScreenLogEmailBody(int count, int totalCount, string companyName, StringBuilder html_table)
        {
            using (StreamReader reader = new StreamReader(@"Views/Risk/ApprovedScreenLogEmailBody.html"))
            {
                var body = await reader.ReadToEndAsync();
                body = body.Replace("{Date}", DateTime.Now.ToString());
                body = body.Replace("{count}", count.ToString());
                body = body.Replace("{TotalCount}", totalCount.ToString());
                string actionDisplay = count == 0 ? "No Action Required" : "Action Required";
                string alertDisplay = count == 0 ? "No further action is required from your end." : "Please clear the alerts from the case management section.";
                body = body.Replace("{alertDisplay}", alertDisplay);
                body = body.Replace("{ActionDisplay}", actionDisplay);
                body = body.Replace("{Html_Table}", html_table.ToString());
                body = body.Replace("{CompanyName}", companyName);
                return body;
            }
        }

        [HttpGet("NewApprovedListScreening")]
        public async Task<IActionResult> NewScreenApprovedList()
        {
            var clients = await _customerCaseService.GetAllClientsAsync();
            string respData = "null";
            foreach (var client in clients.Result)
            {
                string schedulerRunId = $"{client.ClientId}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 5)}";
                ClientMasterDTO clientMasterDTO = client; // Assuming ClientDTO has ClientId property
                var result = await _customerCaseService.GetCasebyApprovedStatusAsync(client.ClientId);
                var count = 0;
                var potentialhits = new StringBuilder("<p>The Potential hits are :</p>");
                var html_table = new StringBuilder("<table><thead><tr><th>S.No</th><th>Case ID</th><th>Customer Name</th><th>Date of Initial Screening</th></tr></thead><tbody>");

                try
                {
                    foreach (var item in result.Result)
                    {
                        Console.WriteLine("Starts Api calling", item.ToString(), DateTime.Now);
                        log.Info("Starts Api calling",item.ToString(), DateTime.Now);
                        //LogFile("Starts Api calling", item);
                        
                        var apiResp = await _commonService.ApprovedListScreeningCall(item.CustomerId, baseURL, c6BaseURL, "API", await GetApprovedRiskEmailBody());
                        Console.WriteLine("Ends Api calling",apiResp);
                        log.Info("Ends Api calling", item.ToString(), DateTime.Now);
                       // LogFile("Ends Api calling",apiResp);
                        if (apiResp.IsMatched == 1)
                        {
                            count++;
                            html_table.Append($"<tr><td>{count}</td><td>{item.CustomerId}</td><td>{item.LastName}</td><td>{item.CreatedOn}</td></tr>");
                        }
                    }

                    html_table.Append("</tbody></table>");

                    if (count == 0)
                    {
                        potentialhits.Clear();
                        html_table.Clear();
                    }

                    var loginfo = $"{count}/{result.Result.Count()} cases added to approved list on {DateTime.Now}";
                   

                    _customerCaseService.InsertDigiSchedulerLogs(count, result.Result.Count(), client.ClientId, schedulerRunId);

                    //var emailBody = await GetApprovedScreenLogEmailBody(count, result.Result.Count(), clientMasterDTO.ClientName, html_table);
                    //_commonService.SendEmailLog(emailBody, client.ClientId, count == 0 ? "No Action Required" : "Action Required");

                    respData += $"{count}/{result.Result.Count()} hits";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception:", ex);
                    // Log or handle the exception appropriately
                }
            }
            return Ok(respData);
            //return Ok(new
            //{
            //    SchedulerLog = respData
            //});
        }



        // end (march 29 2024)


        [HttpGet("ApprovedListScreening")]
        //    [JwtAuthorize]
        public async Task<IActionResult> ScreenApprovedList()
        {
            var clients = _customerCaseService.GetAllClients();
            string respData = "null";
            string potentialhits = "<p>The Potential hits are :</p>";
            string html_table = "<table><thead><tr><th>S.No</th><th>Case ID</th><th>Customer Name</th><th>Date of Initial Screening</th></tr></thead><tbody>";
            foreach (var client in clients)
            {
                string schedulerRunId = $"{client.ClientId}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 5)}";
                //ClientMasterDTO clientMasterDTO = clients.Find(val => val.ClientId == client.ClientId);

                ClientMasterDTO clientMasterDTO = client;

                var result = _customerCaseService.GetCasebyApprovedStatus(client.ClientId);
                CustomerCaseDTO apiResp = new CustomerCaseDTO();
                var count = 0;
                string body = string.Empty;
                try
                {

                    foreach (var item in result)
                    {
                        apiResp = null;
                        using (StreamReader reader = new StreamReader(@"Views/Risk/ApprovedRiskEmailBody.html"))
                        {
                            body = reader.ReadToEnd();
                        };
                        //LogFile("Passing the parameters to Api call", item);
                        apiResp = await _commonService.ApprovedListScreeningCall(item.CustomerId, baseURL, c6BaseURL, schedulerRunId, "API", body );
                        //LogFile("ends the api call", apiResp);
                        log.Info("Ends Api calling", item.CustomerId, DateTime.Now);
                        if (apiResp.IsMatched == 1)
                        {
                            count++;
                            html_table += "<tr><td>" + count + "</td><td>" + item.CustomerId + "</td><td>" + item.LastName + "</td><td>" + item.CreatedOn + "</td></tr>";
                        }
                    }
                    html_table += "</tbody></table>";
                    potentialhits += html_table;
                    if (count == 0)
                    {

                        potentialhits = "";
                        html_table = "";
                    }
                    var loginfo = count.ToString() + "/" + result.Count().ToString() + "cases added to approved list on " + DateTime.Now;
                    
                    //LogFile1("Inserting into the sql database",result.Count());
                    _customerCaseService.InsertDigiSchedulerLogs(count, result.Count(), client.ClientId,schedulerRunId);
                    using (StreamReader reader = new StreamReader(@"Views/Risk/ApprovedScreenLogEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    }

                    string emailBody = body;
                    emailBody = emailBody.Replace("{Date}", DateTime.Now.ToString());
                    emailBody = emailBody.Replace("{count}", count.ToString());
                    emailBody = emailBody.Replace("{TotalCount}", result.Count().ToString());
                    string actionDisplay = (count == 0) ? "No Action Required" : "Action Required";
                    string alertDisplay = (count == 0) ? "No further action is required from your end." : "Please clear the alerts from the case management section.";
                    emailBody = emailBody.Replace("{alertDisplay}", alertDisplay);
                    emailBody = emailBody.Replace("{ActionDisplay}", actionDisplay);
                    emailBody = emailBody.Replace("{Html_Table}", potentialhits);
                    emailBody = emailBody.Replace("{CompanyName}", clientMasterDTO.ClientName);
                    _commonService.SendEmailLog(emailBody, client.ClientId, actionDisplay);
                }
                catch { }
                if (result != null)
                {
                    respData += count.ToString() + "/" + result.Count().ToString() + " hits";
                }
            }
            string resp = "null";
            resp = respData;
            return Ok(new
            {
                //matchScore = apiResp.MatchScore,
                SchedulerLog = resp
            });
        }


        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] ApiAuthRequest model)
        {
            model.Secret = secret;
            var response = _commonService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username, password or Company name is incorrect" });

            return Ok(response);
        }
        [HttpPost("ScreeningLogs")]
        public IActionResult DatabaseScreeningLogs([FromBody] ScreeinglogsModel model) 
        {

            var response = _customerCaseService.InsertScreeninglogs(model);

                return Ok(new
                {
                    status=200,
                    Message = "Record has been inserted successfully",
                    
                });

        }

        [HttpPost("DataSetsScreeningLogs")]
        public IActionResult DataSetsScreeningLogs([FromBody] DataSetsScreeinglogsModel model)
        {

            var response = _customerCaseService.InsertDatasetsScreeninglogs(model);

            return Ok(new
            {
                status = 200,
                Message = "Record has been inserted successfully",

            });

        }


        [HttpGet("PendingFromSchedulerScreening")]
        //    [JwtAuthorize]
        public async Task<IActionResult> PendingFromSchedulerScreening()
        {
            
                //ClientMasterDTO clientMasterDTO = clients.Find(val => val.ClientId == client.ClientId);

                

                var result = _customerCaseService.GetCasespendingScheduler();
                int clientid = 0;
                CustomerCaseDTO apiResp = new CustomerCaseDTO();
                var count = 0;
                string body = string.Empty;
                try
                {
                  
                    foreach (var item in result)
                    {
                    clientid = item.ClientId;
                    CustomerScreeningRQ screeningrq = new CustomerScreeningRQ();
                    screeningrq.CASEID = Convert.ToString(item.Id);
                    await AMLUtility.ScreeningAPICall(screeningrq, ScreeningService.GETPendingCasesScheduler, baseURL);
                    apiResp = null;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/ApprovedRiskEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    };

                    //LogFile("Passing the parameters to Api call", item);
                    //apiResp = await _commonService.ApprovedListScreeningCall(item.CustomerId, baseURL, c6BaseURL, "API", body);
                    //LogFile("ends the api call", apiResp);
                    //log.Info("Ends Api calling", item.CustomerId, DateTime.Now);

                    if (item.CustomerType == "I")
                        apiResp = await _commonService.CustomerScreeningCall(item, baseURL, c6BaseURL, "INDIVIDUAL", body, item.Threshold, item.CustomerMasterId);
                    else if (item.CustomerType == "C")
                        apiResp = await _commonService.CustomerScreeningCall(item, baseURL, c6BaseURL, "CORPORATE", body, item.Threshold, item.CustomerMasterId);

                    if (apiResp.IsMatched == 1)
                    {
                        count++;

                    }
                }








                //_commonService.SendEmailLog(emailBody, client.ClientId, actionDisplay);
            }
                catch { }
                
            
            
            return Ok(new
            {
                //matchScore = apiResp.MatchScore,
                Clientid= clientid,
                status = 200,
                Message = "Record has been delete from mango db and screened successfully",

            });
        }

        //[HttpPost("PendingRecordsbyclient")]
        ////    [JwtAuthorize]
        //public async Task<IActionResult> GetallPendingrecordsbyclient([FromBody]APImodel model)
        //{

        //    //ClientMasterDTO clientMasterDTO = clients.Find(val => val.ClientId == client.ClientId);
        //    string startDate = model.startDate;
        //    string endDate = model.endDate;
        //    string cust_type = model.cust_type;
        //    int userId = model.userId;


        //    var result = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAll(userId, startDate, endDate, cust_type));
        //    int clientid = 0;
        //    CustomerCaseDTO apiResp = new CustomerCaseDTO();
        //    var count = 0;
        //    string body = string.Empty;
        //    try
        //    {

        //        foreach (var item in result)
        //        {
        //            CaseCommentModel model1= new CaseCommentModel();
        //            clientid = item.ClientId;
        //            model1.CreatedOn = DateTime.Now;
        //            model1.CaseId = item.Id;
        //            model1.Comment = "All the potential matches against the subject case are false , hence the case is approved";
        //            model1.CreatedBy =userId;
        //            var result2 = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(model1));

                    
        //                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(item.Id);
        //            _CustomerCaseDTO.Status =  2;
        //            _CustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.Now);
        //            var result3 = _customerCaseService.Update(_CustomerCaseDTO);
        //            CaseCommentModel remarkModel = new CaseCommentModel();
        //            remarkModel.CaseId = item.Id;
        //            remarkModel.Comment = "Approved Case";
        //            remarkModel.CreatedBy = userId;
        //            remarkModel.CreatedOn = DateTime.Now;
        //            var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));


        //        }








        //        //_commonService.SendEmailLog(emailBody, client.ClientId, actionDisplay);
        //    }
        //    catch { }



        //    return Ok(new
        //    {
        //        //matchScore = apiResp.MatchScore,
        //        Clientid = clientid,
        //        status = 200,
        //        Message = "Instered case comment successfully and updated status as approved",

        //    });
        //}



        //private async void LogFile(string message2, CustomerCaseDTO data)
        //{
        //    string data2 = "";
        //    if (data.ApiResults != null)
        //    {
        //         data2 = data.ApiResults.Count().ToString().IsNotNullOrEmpty() ? data.ApiResults.Count().ToString() : "";
        //    }
        //    string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        //    message += Environment.NewLine;
        //    message += "-----------------------------------------------------------";
        //    message += Environment.NewLine;
        //    message += message2;
        //    message += Environment.NewLine;
        //    message += string.Format("Customerid:{0}", data.CustomerId.IsNotNullOrEmpty() ? data.CustomerId : "");
        //    message += Environment.NewLine;
        //    message += string.Format("customerName : {0}", data.FirstName + " " + data.MiddleName + " " + data.LastName);
        //    message += Environment.NewLine;
        //    message += string.Format("MatchCategory : {0}", data.MatchCategory.IsNotNullOrEmpty() ? data.MatchCategory : "");
        //    message += Environment.NewLine;
        //    message += string.Format("Message: {0} ", data2.IsNotNullOrEmpty() ? data2 : "");



        //    if (!System.IO.File.Exists(@"WebLogs.txt"))
        //    {
        //        using (System.IO.StreamWriter sw = System.IO.File.CreateText(@"WebLogs.txt"))
        //        {
        //            sw.WriteLine(message);
        //        }
        //    }
        //    else
        //    {
        //        using (System.IO.StreamWriter sw = System.IO.File.AppendText(@"WebLogs.txt"))
        //        {
        //            sw.WriteLine(message);
        //        }
        //    }



        //}
        //private async void LogFile1(string message2, int data)
        //{
        //    string data2 = "";
        //    if (data != null)
        //    {
        //         data2 = data.ToString();
        //    }
        //    string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        //    message += Environment.NewLine;
        //    message += "-----------------------------------------------------------";
        //    message += Environment.NewLine;
        //    message += message2;
        //    message += Environment.NewLine;
        //    message += string.Format("ResultCount", data2.IsNotNullOrEmpty() ? data2 : "");




        //    if (!System.IO.File.Exists(@"WebLogs.txt"))
        //    {
        //        using (System.IO.StreamWriter sw = System.IO.File.CreateText(@"WebLogs.txt"))
        //        {
        //            sw.WriteLine(message);
        //        }
        //    }
        //    else
        //    {
        //        using (System.IO.StreamWriter sw = System.IO.File.AppendText(@"WebLogs.txt"))
        //        {
        //            sw.WriteLine(message);
        //        }
        //    }



        //}

    }
}
