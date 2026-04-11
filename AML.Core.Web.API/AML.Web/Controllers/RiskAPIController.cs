using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.Common;
using AML.Core.RepositoryContract.Risk;
using AML.Core.ServiceContract.CustomerMaster;
using AML.Core.ServiceContract.LovMaster;
using AML.Core.ServiceContract.Risk;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.Risk;
using AML.ViewModel.ViewModels.RiskAPI;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using AML.Core.ServiceContract.CustomerCase;
using AML.DTO.DTO.CustomerCase;
using AML.ViewModel.ViewModels.CustomerCase;
using System.Linq;
using AML.DTO.DTO.Common;

namespace AML.Web.Controllers
{
    [Route("api/risk")]
    [ApiController]
    public class RiskAPIController : ControllerBase
    {

        private IMapper _mapper;
        private IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;
        private ICustomerCaseService _customerCaseService;
        private ICommonService _commonService;
        private ILovMasterService _lovMasterService;
        private IRiskService _riskService;
        private ICustomerMasterService _customerMasterService;
        private IRiskRepository _riskRepository;
        private string culture = CultureInfo.CurrentCulture.Name;
        private IRiskRepository riskRepository;

        public RiskAPIController(IMapper mapper, IHttpClientHandler clientHandler, ICommonService commonService,
            ILovMasterService lovMasterService, IRiskService riskService, ICustomerMasterService customerMasterService,
            IConfiguration configuration,ICustomerCaseService customerCaseService, IRiskRepository riskRepository)
        {
            _mapper = mapper;
            _customerCaseService = customerCaseService;
            _clientHandler = clientHandler;
            _configuration = configuration;
            _commonService = commonService;
            _lovMasterService = lovMasterService;
            _riskService = riskService;
            _riskRepository = riskRepository;
            _customerMasterService = customerMasterService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Risk API Working");
        }
        public int GetRiskId(int riskTypeId, string name)
        {
            int id = _riskRepository.GetRiskId(riskTypeId, name).Result;

            return id;
        }


        [HttpPost("Configuration")]
        public IActionResult Configuration([FromBody] RiskConfigRequestModel model)
        {
            var ClientId = _clientHandler.GetClientId();

            var list = _lovMasterService.GetAllRiskConfig(model.RiskCategory, 1, 0, 0, ClientId);

            return Ok(list);
        }
        [HttpPost("Assessment")]
        public IActionResult Assessment([FromBody] RiskAPIRequestModel model)
            
        {
            if (model.CompanyName != null && model.UserId != null)
            {


                ClientMasterDTO clientMasterDTO = _customerCaseService.GetAllClients().Find(val => val.ClientName == model.CompanyName);

                if (clientMasterDTO == null)
                {
                    //log.Error($"Coud not get customerid of {model.CompanyName}");
                    throw new Exception(string.Format("Could not get id of '{0}'", model.CompanyName));
                }
                model.ClientId = clientMasterDTO.ClientId;
                if (int.TryParse(_customerCaseService.GetUserIdForAPI(model.UserId), out int createdById))
                {
                    model.CreatedBy = createdById;
                }
                else
                {
                    model.CreatedBy = -1;
                }

                if (model.CreatedBy == -1)
                {
                    throw new Exception($"User '{model.UserId}' could not be foud.");
                }
            }
            
            RiskAPIResultModel res = new RiskAPIResultModel();

            if (model.RiskCategory == "I")
            {
                List<int> RiskScore = new List<int>();
                List<bool> OverrideScore = new List<bool>();
                int TotalScore = 0;
                dynamic items;
                bool Overrides = false;
                string OverrideRiskStatus = "Low Risk";
                string RiskStatus = "Low";
                string RiskAsPerScore = "";
                RiskModel riskmodel = new RiskModel();
                riskmodel.CustomerCode = model.CustomerId;
                riskmodel.RiskTypeCategoryDTO = new List<RiskTypeCategoryDTO>();
                var list = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0, model.ClientId);
                riskmodel.RiskTypeCategoryDTO = list;
                var a = 0;
                var b = 0;
                var TotalSelected = 0;
                for (var i = 0; i < riskmodel.RiskTypeCategoryDTO.Count; i++)
                {


                    for (var j = 0; j < riskmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (a < model.RiskTypeList.Count && model.RiskTypeList[a].Id == riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id.ToString())
                        {

                            for (int k = 0; k < riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems.Count; k++)
                            {

                                if (model.RiskTypeList[a].RiskItemList[b].Id == riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].Id.ToString())
                                {
                                    riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId = model.RiskTypeList[a].RiskItemList[b].Id.ParseInt();
                                    riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id = model.RiskTypeList[a].Id.ParseInt();
                                    RiskScore.Add(riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].RiskScore.ParseInt());
                                    OverrideScore.Add(riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].OverrideScore.ParseInt() == 3 ? true : false);
                                    TotalScore += riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].RiskScore.ParseInt();
                                    TotalSelected++;

                                    if (model.RiskTypeList[a].RiskItemList.Count > b)
                                    {
                                        a++;
                                        b = 0;
                                        break;
                                    }
                                    else
                                        b++;
                                }

                            }


                            if (model.RiskTypeList.Count == a)
                            {

                                break;
                            }
                        }
                        //    riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                    }
                    if (model.RiskTypeList.Count == a)
                    {

                        break;
                    }

                }

                var highScoreVal = (a * 3);
                var mediumScoreVal = (a * 2.24);
                var lowScoreVal = (a * 1.49);
                if (TotalScore <= highScoreVal)
                {
                    RiskStatus = "High Risk";
                    RiskAsPerScore = "As Per Risk Score - High";
                }
                if (TotalScore <= mediumScoreVal)
                {
                    RiskStatus = "Medium Risk";
                    RiskAsPerScore = "As Per Risk Score - Medium";
                }
                if (TotalScore <= lowScoreVal)
                {
                    RiskStatus = "Low Risk";
                    RiskAsPerScore = "As Per Risk Score - Low";
                }
                riskmodel.RiskScoreBeforeOverride = RiskStatus;

                for (int i = 0; i < a; i++)
                {
                    if (Overrides || OverrideScore[i])
                        RiskStatus = "High Risk";
                }
                riskmodel.CustomerCode = model.CustomerId;
                riskmodel.CustomerName = model.CustomerName;
                riskmodel.DateofAssessment = DateTime.Now;
                riskmodel.Address = null;
                riskmodel.MainNationalityTxt = model.MainNationality;
                riskmodel.FinalRiskScore = RiskStatus;
                riskmodel.RiskScoreSum = TotalScore;
                riskmodel.RiskScoreCount = a;
                riskmodel.ClientId = model.ClientId;
                riskmodel.CreatedBy = model.CreatedBy;
                var result = _riskService.Create(_mapper.Map<RiskDTO>(riskmodel));
                res = new RiskAPIResultModel
                {
                    User = new UserResultModel
                    {
                        TotalScore = TotalScore.ToString(),
                        TotalParameter = a.ToString(),
                        RiskAsPerScore = RiskAsPerScore,
                        FinalRiskScore = RiskStatus
                    },
                    Status = result.Status.ToString()
                };
            }
            if (model.RiskCategory == "C")
            {
                List<int> RiskScore = new List<int>();
                List<bool> OverrideScore = new List<bool>();
                int TotalScore = 0;
                dynamic items;
                bool Overrides = false;
                string OverrideRiskStatus = "Low Risk";
                string RiskStatus = "Low";
                string RiskAsPerScore = "";
                RiskCorpCustomerModel riskmodel = new RiskCorpCustomerModel();
                riskmodel.UniqueID = model.CustomerId;
                riskmodel.RiskTypeCategoryDTO = new List<RiskTypeCategoryDTO>();
                riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0, model.ClientId);
                var a = 0;
                var b = 0;
                var TotalSelected = 0;
                for (var i = 0; i < riskmodel.RiskTypeCategoryDTO.Count; i++)
                {


                    for (var j = 0; j < riskmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (a < model.RiskTypeList.Count && model.RiskTypeList[a].Id == riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id.ToString())
                        {

                            for (int k = 0; k < riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems.Count; k++)
                            {

                                if (model.RiskTypeList[a].RiskItemList[b].Id == riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].Id.ToString())
                                {
                                    riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId = model.RiskTypeList[a].RiskItemList[b].Id.ParseInt();
                                    riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id = model.RiskTypeList[a].Id.ParseInt();
                                    RiskScore.Add(riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].RiskScore.ParseInt());
                                    OverrideScore.Add(riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].OverrideScore.ParseInt() == 3 ? true : false);
                                    TotalScore += riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].RiskScore.ParseInt();
                                    TotalSelected++;

                                    if (model.RiskTypeList[a].RiskItemList.Count > b)
                                    {
                                        a++;
                                        b = 0;
                                        break;
                                    }
                                    else
                                        b++;
                                }

                            }


                            if (model.RiskTypeList.Count == a)
                            {

                                break;
                            }
                        }
                        //    riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                    }
                    if (model.RiskTypeList.Count == a)
                    {

                        break;
                    }

                }
                var highScoreVal = (a * 3);
                var mediumScoreVal = (a * 2.24);
                var lowScoreVal = (a * 1.49);
                if (TotalScore <= highScoreVal)
                {
                    RiskStatus = "High Risk";
                    RiskAsPerScore = "As Per Risk Score - High";
                }
                if (TotalScore <= mediumScoreVal)
                {
                    RiskStatus = "Medium Risk";
                    RiskAsPerScore = "As Per Risk Score - Medium";
                }
                if (TotalScore <= lowScoreVal)
                {
                    RiskStatus = "Low Risk";
                    RiskAsPerScore = "As Per Risk Score - Low";
                }
                riskmodel.RiskAssessmentRatingWithoutOverride = RiskStatus;

                for (int i = 0; i < a; i++)
                {
                    if (Overrides || OverrideScore[i])
                        RiskStatus = "High Risk";
                }
                riskmodel.UniqueID = model.CustomerId;
                riskmodel.LegalNameOfEntity = model.CustomerName;
                riskmodel.DateofAssessment = DateTime.Now;
                //  riskmodel.Address = null;
                riskmodel.CountryOfIncorporationTxt = model.MainNationality;
                riskmodel.RiskAssessmentRating = RiskStatus;
                riskmodel.RiskScoreSum = TotalScore;
                riskmodel.RiskScoreCount = a;
                riskmodel.ClientId = model.ClientId;
                riskmodel.CreatedBy = model.CreatedBy;
                var result = _riskService.CreateCorpCustomerRisk(_mapper.Map<RiskCorpCustomerDTO>(riskmodel));
                res = new RiskAPIResultModel
                {
                    User = new UserResultModel
                    {
                        TotalScore = TotalScore.ToString(),
                        TotalParameter = a.ToString(),
                        RiskAsPerScore = RiskAsPerScore,
                        FinalRiskScore = RiskStatus
                    },
                    Status = result.Status.ToString()
                };
            }

            return Ok(res);
        }


        public IActionResult KycRiskAssessment([FromBody] RiskAPIRequestModel model)

        {
            if (model.CompanyName != null && model.UserId != null)
            {


                ClientMasterDTO clientMasterDTO = _customerCaseService.GetAllClients().Find(val => val.ClientName == model.CompanyName);

                if (clientMasterDTO == null)
                {
                    //log.Error($"Coud not get customerid of {model.CompanyName}");
                    throw new Exception(string.Format("Could not get id of '{0}'", model.CompanyName));
                }
                model.ClientId = clientMasterDTO.ClientId;
                if (int.TryParse(_customerCaseService.GetUserIdForAPI(model.UserId), out int createdById))
                {
                    model.CreatedBy = createdById;
                }
                else
                {
                    model.CreatedBy = -1;
                }

                if (model.CreatedBy == -1)
                {
                    throw new Exception($"User '{model.UserId}' could not be foud.");
                }
            }

            RiskAPIResultModel res = new RiskAPIResultModel();

            if (model.RiskCategory == "I")
            {
                List<int> RiskScore = new List<int>();
                List<bool> OverrideScore = new List<bool>();
                int TotalScore = 0;
                dynamic items;
                bool Overrides = false;
                string OverrideRiskStatus = "Low Risk";
                string RiskStatus = "Low";
                string RiskAsPerScore = "";
                string RiskOverRide = "";
                RiskModel riskmodel = new RiskModel();
                riskmodel.CustomerCode = model.CustomerId;
                riskmodel.RiskTypeCategoryDTO = new List<RiskTypeCategoryDTO>();
                var list = _lovMasterService.GetAllKycRiskConfig("I", 1, 0, 0, model.ClientId);
                riskmodel.RiskTypeCategoryDTO = list;
                var a = 0;
                var b = 0;
                var TotalSelected = 0;
                for (var i = 0; i < riskmodel.RiskTypeCategoryDTO.Count; i++)
                {


                    for (var j = 0; j < riskmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (a < model.RiskTypeList.Count && model.RiskTypeList[a].Id == riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id.ToString())
                        {

                            for (int k = 0; k < riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems.Count; k++)
                            {
                                var isCountry = riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].lov_country_duplicate == 1;
                                var isNotCountryAndIdMatches = !isCountry && riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == Convert.ToInt32(model.RiskTypeList[a].Id);
                                if (isNotCountryAndIdMatches || isCountry)
                                {
                                    if (model.RiskTypeList[a].RiskItemList[b].Id == riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].Id.ToString())
                                    {
                                        riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId = model.RiskTypeList[a].RiskItemList[b].Id.ParseInt();
                                        riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id = model.RiskTypeList[a].Id.ParseInt();
                                        RiskScore.Add(riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].RiskScore.ParseInt());
                                        OverrideScore.Add(riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].OverrideScore.ParseInt() == 3 ? true : false);
                                        TotalScore += riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].RiskScore.ParseInt();
                                        TotalSelected++;

                                        if (model.RiskTypeList[a].RiskItemList.Count > b)
                                        {
                                            a++;
                                            b = 0;
                                            break;
                                        }
                                        else
                                            b++;
                                    }
                                }

                            }


                            if (model.RiskTypeList.Count == a)
                            {

                                break;
                            }
                        }
                        //    riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                    }
                    if (model.RiskTypeList.Count == a)
                    {

                        break;
                    }

                }

                var highScoreVal = (a * 3);
                var mediumScoreVal = (a * 2.24);
                var lowScoreVal = (a * 1.49);
                if (TotalScore <= highScoreVal)
                {
                    RiskStatus = "High Risk";
                    RiskAsPerScore = "As Per Risk Score - High";
                }
                if (TotalScore <= mediumScoreVal)
                {
                    RiskStatus = "Medium Risk";
                    RiskAsPerScore = "As Per Risk Score - Medium";
                }
                if (TotalScore <= lowScoreVal)
                {
                    RiskStatus = "Low Risk";
                    RiskAsPerScore = "As Per Risk Score - Low";
                }
                riskmodel.RiskScoreBeforeOverride = RiskStatus;

                for (int i = 0; i < a; i++)
                {
                    if (Overrides || OverrideScore[i])
                        RiskStatus = "High Risk";
                    RiskOverRide = "Override";


                }
                riskmodel.CustomerCode = model.CustomerId;
                riskmodel.CustomerName = model.CustomerName;
                riskmodel.DateofAssessment = DateTime.Now;
                riskmodel.Address = null;
                riskmodel.MainNationalityTxt = model.MainNationality;
                riskmodel.FinalRiskScore = RiskStatus;
                riskmodel.RiskOverRide = RiskOverRide;
                riskmodel.RiskScoreSum = TotalScore;
                riskmodel.RiskScoreCount = a;
                riskmodel.ClientId = model.ClientId;
                riskmodel.CreatedBy = model.CreatedBy;
                riskmodel.version = model.Version == 0 ? 1 : model.Version;
                var result = _riskService.Create(_mapper.Map<RiskDTO>(riskmodel));
                res = new RiskAPIResultModel
                {
                    User = new UserResultModel
                    {
                        TotalScore = TotalScore.ToString(),
                        TotalParameter = a.ToString(),
                        RiskAsPerScore = RiskAsPerScore,
                        FinalRiskScore = RiskStatus
                    },
                    Status = result.Status.ToString()
                };
            }
            if (model.RiskCategory == "C")
            {
                List<int> RiskScore = new List<int>();
                List<bool> OverrideScore = new List<bool>();
                int TotalScore = 0;
                dynamic items;
                bool Overrides = false;
                string OverrideRiskStatus = "Low Risk";
                string RiskStatus = "Low";
                string RiskAsPerScore = "";
                string RiskOverRide = "";
                RiskCorpCustomerModel riskmodel = new RiskCorpCustomerModel();
                riskmodel.UniqueID = model.CustomerId;
                riskmodel.RiskTypeCategoryDTO = new List<RiskTypeCategoryDTO>();
                riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllKycRiskConfig("C", 1, 0, 0, model.ClientId);
                var a = 0;
                var b = 0;
                var TotalSelected = 0;
                for (var i = 0; i < riskmodel.RiskTypeCategoryDTO.Count; i++)
                {


                    for (var j = 0; j < riskmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (a < model.RiskTypeList.Count && model.RiskTypeList[a].Id == riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id.ToString())
                        {

                            for (int k = 0; k < riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems.Count; k++)
                            {
                                var isCountry = riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].lov_country_duplicate == 1;
                                var isNotCountryAndIdMatches = !isCountry && riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == Convert.ToInt32(model.RiskTypeList[a].Id);
                                if (isNotCountryAndIdMatches || isCountry)
                                {

                                    if (model.RiskTypeList[a].RiskItemList[b].Id == riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].Id.ToString())
                                    {
                                        riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId = model.RiskTypeList[a].RiskItemList[b].Id.ParseInt();
                                        riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id = model.RiskTypeList[a].Id.ParseInt();
                                        RiskScore.Add(riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].RiskScore.ParseInt());
                                        OverrideScore.Add(riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].OverrideScore.ParseInt() == 3 ? true : false);
                                        TotalScore += riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems[k].RiskScore.ParseInt();
                                        TotalSelected++;

                                        if (model.RiskTypeList[a].RiskItemList.Count > b)
                                        {
                                            a++;
                                            b = 0;
                                            break;
                                        }
                                        else
                                            b++;
                                    }
                                }

                            }


                            if (model.RiskTypeList.Count == a)
                            {

                                break;
                            }
                        }
                        //    riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                    }
                    if (model.RiskTypeList.Count == a)
                    {

                        break;
                    }

                }
                var highScoreVal = (a * 3);
                var mediumScoreVal = (a * 2.24);
                var lowScoreVal = (a * 1.49);
                if (TotalScore <= highScoreVal)
                {
                    RiskStatus = "High Risk";
                    RiskAsPerScore = "As Per Risk Score - High";
                }
                if (TotalScore <= mediumScoreVal)
                {
                    RiskStatus = "Medium Risk";
                    RiskAsPerScore = "As Per Risk Score - Medium";
                }
                if (TotalScore <= lowScoreVal)
                {
                    RiskStatus = "Low Risk";
                    RiskAsPerScore = "As Per Risk Score - Low";
                }
                riskmodel.RiskAssessmentRatingWithoutOverride = RiskStatus;

                for (int i = 0; i < a; i++)
                {
                    if (Overrides || OverrideScore[i])
                        RiskStatus = "High Risk";
                    RiskOverRide = "Override";
                }
                riskmodel.UniqueID = model.CustomerId;
                riskmodel.LegalNameOfEntity = model.CustomerName;
                riskmodel.DateofAssessment = DateTime.Now;
                //  riskmodel.Address = null;
                riskmodel.CountryOfIncorporationTxt = model.MainNationality;
                riskmodel.RiskAssessmentRating = RiskStatus;
                riskmodel.RiskOverRide = RiskOverRide;
                riskmodel.RiskScoreSum = TotalScore;
                riskmodel.RiskScoreCount = a;
                riskmodel.ClientId = model.ClientId;
                riskmodel.CreatedBy = model.CreatedBy;
                riskmodel.version = model.Version == 0 ? 1 : model.Version;
                var result = _riskService.CreateCorpCustomerRisk(_mapper.Map<RiskCorpCustomerDTO>(riskmodel));
                res = new RiskAPIResultModel
                {
                    User = new UserResultModel
                    {
                        TotalScore = TotalScore.ToString(),
                        TotalParameter = a.ToString(),
                        RiskAsPerScore = RiskAsPerScore,
                        FinalRiskScore = RiskStatus
                    },
                    Status = result.Status.ToString()
                };
            }

            return Ok(res);
        }
    }
}
