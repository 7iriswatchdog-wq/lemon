using Microsoft.AspNetCore.Http;
using AML.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using System.IO;
using System.Text;
using AutoMapper;
using AML.Web.Helper;
using Microsoft.Extensions.Configuration;

using AML.Core.ServiceContract.Kyc;
using AML.ViewModel.ViewModels.Kyc;
using AML.Core.ServiceContract.Common;
using AML.DTO.DTO.Common;
using AML.Core.ServiceContract.CustomerCase;
using AML.DTO.DTO.CustomerCase;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Kyc;
using System.Globalization;
using AML.ViewModel.ViewModels.RiskAPI;
using MySqlX.XDevAPI;


namespace AML.Web.Controllers
{
    [Route("api/Kyc")]
    [ApiController]
    public class KycScreeningAPIController : ControllerBase
    {
        private IMapper _mapper;
        private IHttpClientHandler _clientHandler;
        private ICustomerCaseService _customerCaseService;
        private IConfiguration _configuration;
        private string baseURL = string.Empty;
        private string c6BaseURL = string.Empty;
        private string secret = string.Empty;
        private ICommonService _commonService;
        private IKycService _kycService;
        private RiskAPIController _riskAPIController;
        private int checkThreshold = 0;
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private string culture = CultureInfo.CurrentCulture.Name;
        public KycScreeningAPIController(IMapper mapper, IHttpClientHandler clientHandler, ICommonService commonService,
            IConfiguration configuration, ICustomerCaseService customerCaseService, IKycService kycService, RiskAPIController riskAPIController
            )
        {
           
            _configuration = configuration;
            _clientHandler = clientHandler;
            _mapper = mapper;
            _commonService = commonService;
            _customerCaseService = customerCaseService;
            _kycService = kycService;
            _riskAPIController = riskAPIController;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            c6BaseURL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            secret = configuration.GetSection("JwtAppSettings").GetSection("Secret").Value;
        }
        // Get api/kyc
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("KYC API Working");
        }

        // POST api/Kyc/kycScreening
        [JwtAuthorize]
        [HttpPost("kycScreening")]
        public async Task<IActionResult> kycScreening([FromBody] kycapimodel model)
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

//#if DEBUG
//                    log.Debug("Debug mode, Generating random CID");
//                    model.CustomerId = RandomCID();
//#endif
                    model.ClientId = clientMasterDTO.ClientId;

                    CustomerCaseDTO _ccDTO = _mapper.Map<CustomerCaseDTO>(model);
                 

                    if (model.CustomerType == "C")
                    {
                        //_ccDTO.CustomerIdNumber = model.CommercialLicense.LicenseNumber;
                        _ccDTO.Nationality = model.PlaceofIncorporation;
                        
                    }
                    else
                    {
                        _ccDTO.Nationality = model.Nationality;
                    }
                    log.Debug("Storing initial details in db");
                 
                    var result = _customerCaseService.Create(_ccDTO);

                    log.Debug($"Got new case id {result.Result}");

                    if (result.Result == "0" && result.Result != "")
                    {
                        log.Debug("Record already exists");
                        return Ok(new
                        {
                          
                            CustomerId = model.CustomerId,
                            isMatched = "Record Exists"
                        });
                    }

                    //PassportDetails _passport = new PassportDetails();
                    //_passport.CaseId = _customerCaseService.GetCaseId(result.Result.Split('Ø')[1]).ToString();
                    //if (model.PassportDetails != null)
                    //    {
                    //        foreach (var item in model.PassportDetails)
                    //        {
                                
                    //            _passport.PassportNo = item.PassportNo;
                    //            _passport.PassportIssusePlace = item.PassportIssusePlace;
                    //            _passport.PassportIssuesDate = item.PassportIssuesDate;
                    //            _passport.PassportExpiryDate = item.PassportExpiryDate;
                    //            _passport.CreatedOn = DateTime.Now;
                    //            _passport.CreatedBy = createdById;
                    //            _passport.ClientId= model.ClientId;

                    //            var resp = _customerCaseService.CreatePassportDetails(_mapper.Map<PassportDetails>(_passport));


                    //        }
                    //    }

                    if (model.CustomerType == "I")
                    {
                        var kycresult = _kycService.CreateIndividual(_mapper.Map<KycIndividualDTO>(model));
                    }
                    else
                    {
                        var kycresult = _kycService.CreateCorporate(_mapper.Map<CorporateKycDTO>(model));
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

                 


                    if (model.CustomerType == "I")
                    {

                        //var isAdverse = 3345;

                        //if (apiResp.ApiResultsjson != null)
                        //{
                        //    if (apiResp.ApiResultsjson[0].isAdverse == true)
                        //    {
                        //        isAdverse = 3344;
                        //    }
                        //    else
                        //    {
                        //        isAdverse = 3345;
                        //    }
                        //}


                        //var isPep = 0;

                        //if (model.PEPStatus == 1)
                        //{
                        //    isPep = 3341;
                        //}
                        //else
                        //{
                        //    isPep = 3342;
                        //}

                        CorporateKycDTO corpModel = new CorporateKycDTO();
                        var str1 = _kycService.GetRiskLovId(_mapper.Map<KycIndividualDTO>(model), corpModel, "I", culture, model.ClientId);
                        var spStr1 = str1.Result.Split('Ø');
                        var proflovId = spStr1[0];
                        var natlovId = spStr1[1];
                        var reslovId = spStr1[5];
                        var productlovId = spStr1[13];
                        var deliverylovId = spStr1[14];
                        var modeofpaymentlovId = spStr1[15];
                        var domesticlovId = spStr1[17];
                        var foreginlovId = spStr1[19];
                        var redflagslovId = spStr1[21];
                        var sanctionlovId = spStr1[23];
                        var uaeorunsclovId = spStr1[25];
                        var highestriskprodlovId = spStr1[28];
                        var highnetworkindividuallovid = spStr1[30];

                        var str = _kycService.GetRiskTypeId(_mapper.Map<KycIndividualDTO>(model), corpModel, "I", culture, model.ClientId);
                        var spStr = str.Result.Split('Ø');
                        var profId = spStr[0];
                        var natId = spStr[1];
                        var resId = spStr[5];
                        var productId= spStr[13];
                        var deliveryId = spStr[14];
                        var modeofpaymentId = spStr[15];
                        var domesticId = spStr[17];
                        var foreginId = spStr[19];
                        var redflagsId= spStr[21];
                        var sanctionId= spStr[23];
                        var uaeorunscId= spStr[25];
                        var highestriskprodId= spStr[28];
                        var highnetworkindividualid = spStr[30];


                        RiskAPIRequestModel riskModel = new RiskAPIRequestModel();

                        riskModel.CustomerId = model.CustomerId;
                        riskModel.CustomerName = model.FirstName + model.MiddleName + model.LastName;
                       
                        if (apiResp.Nationality == "0")
                        {
                            riskModel.MainNationality = "";
                        }
                        else
                        {
                            riskModel.MainNationality = apiResp.Nationality;
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
                        //for Product start
                        if (productId != "0")
                        {
                            var riskType6 = new RiskTypeListModel();
                            riskType6.Id = Convert.ToString(productlovId);
                            var riskItem6 = new RiskItemListModel();
                            riskItem6.Id = productId.ToString();
                            var riskItemList6 = new List<RiskItemListModel>();
                            riskItemList6.Add(riskItem6);
                            riskType6.RiskItemList = riskItemList6;
                            riskTypeList.Add(riskType6);
                        }
                        //for product end
                        //for highestriskproduct start
                        if (highestriskprodId != "0")
                        {
                            var riskType7 = new RiskTypeListModel();
                            riskType7.Id = Convert.ToString(highestriskprodlovId);
                            var riskItem7 = new RiskItemListModel();
                            riskItem7.Id = highestriskprodId.ToString();
                            var riskItemList7 = new List<RiskItemListModel>();
                            riskItemList7.Add(riskItem7);
                            riskType7.RiskItemList = riskItemList7;
                            riskTypeList.Add(riskType7);
                        }
                        //for highestriskproduct end
                        //for devlivery start
                        if (deliveryId != "0")
                        {
                            var riskType8 = new RiskTypeListModel();
                            riskType8.Id = Convert.ToString(deliverylovId);
                            var riskItem8 = new RiskItemListModel();
                            riskItem8.Id = deliveryId.ToString();
                            var riskItemList8 = new List<RiskItemListModel>();
                            riskItemList8.Add(riskItem8);
                            riskType8.RiskItemList = riskItemList8;
                            riskTypeList.Add(riskType8);
                        }
                        //for delivery end

                        //for domesticpep start
                        if (domesticId != "0")
                        {
                            var riskType9 = new RiskTypeListModel();
                            riskType9.Id = Convert.ToString(domesticlovId);
                            var riskItem9 = new RiskItemListModel();
                            riskItem9.Id = domesticId.ToString();
                            var riskItemList9 = new List<RiskItemListModel>();
                            riskItemList9.Add(riskItem9);
                            riskType9.RiskItemList = riskItemList9;
                            riskTypeList.Add(riskType9);
                        }
                        //for domesticpep end
                        //for foreignpep start
                        if (foreginId != "0")
                        {
                            var riskType10 = new RiskTypeListModel();
                            riskType10.Id = Convert.ToString(foreginlovId);
                            var riskItem10 = new RiskItemListModel();
                            riskItem10.Id = foreginId.ToString();
                            var riskItemList10 = new List<RiskItemListModel>();
                            riskItemList10.Add(riskItem10);
                            riskType10.RiskItemList = riskItemList10;
                            riskTypeList.Add(riskType10);
                        }
                        //for foreignpep end
                        //for redflags start
                        if (redflagsId != "0")
                        {
                            var riskType11 = new RiskTypeListModel();
                            riskType11.Id = Convert.ToString(redflagslovId);
                            var riskItem11 = new RiskItemListModel();
                            riskItem11.Id = redflagsId.ToString();
                            var riskItemList11 = new List<RiskItemListModel>();
                            riskItemList11.Add(riskItem11);
                            riskType11.RiskItemList = riskItemList11;
                            riskTypeList.Add(riskType11);
                        }
                        //for redflags end
                        //for high nettwork individual start
                        if (highnetworkindividualid != "0")
                        {
                            var riskType19 = new RiskTypeListModel();
                            riskType19.Id = Convert.ToString(highnetworkindividuallovid);
                            var riskItem19 = new RiskItemListModel();
                            riskItem19.Id = highnetworkindividualid.ToString();
                            var riskItemList19 = new List<RiskItemListModel>();
                            riskItemList19.Add(riskItem19);
                            riskType19.RiskItemList = riskItemList19;
                            riskTypeList.Add(riskType19);
                        }
                        //for high nettwork individual end
                        //for sanctionmatch start
                        if (sanctionId != "0")
                        {
                            var riskType12 = new RiskTypeListModel();
                            riskType12.Id = Convert.ToString(sanctionlovId);
                            var riskItem12 = new RiskItemListModel();
                            riskItem12.Id = sanctionId.ToString();
                            var riskItemList12 = new List<RiskItemListModel>();
                            riskItemList12.Add(riskItem12);
                            riskType12.RiskItemList = riskItemList12;
                            riskTypeList.Add(riskType12);
                        }
                        //for sanctionmatch end
                        //for uaeorunsc start
                        if (uaeorunscId != "0")
                        {
                            var riskType13 = new RiskTypeListModel();
                            riskType13.Id = Convert.ToString(uaeorunsclovId);
                            var riskItem13 = new RiskItemListModel();
                            riskItem13.Id = uaeorunscId.ToString();
                            var riskItemList13 = new List<RiskItemListModel>();
                            riskItemList13.Add(riskItem13);
                            riskType13.RiskItemList = riskItemList13;
                            riskTypeList.Add(riskType13);
                        }
                        //for uaeorunsc end

                        //for Modeofpayment start
                        if (modeofpaymentId != "0")
                        {
                            var riskType14 = new RiskTypeListModel();
                            riskType14.Id = Convert.ToString(modeofpaymentlovId);
                            var riskItem14 = new RiskItemListModel();
                            riskItem14.Id = modeofpaymentId.ToString();
                            var riskItemList14 = new List<RiskItemListModel>();
                            riskItemList14.Add(riskItem14);
                            riskType14.RiskItemList = riskItemList14;
                            riskTypeList.Add(riskType14);
                        }
                        //for Modeofpayment end


                        //for pep start
                        //if (isPep != 0)
                        //{
                        //    var riskType1 = new RiskTypeListModel();
                        //    riskType1.Id = Convert.ToString(4);
                        //    var riskItem1 = new RiskItemListModel();
                        //    riskItem1.Id = isPep.ToString();
                        //    var riskItemList1 = new List<RiskItemListModel>();
                        //    riskItemList1.Add(riskItem1);
                        //    riskType1.RiskItemList = riskItemList1;
                        //    riskTypeList.Add(riskType1);
                        //}
                        //for pep end
                        //for adverse start
                        //if (isAdverse != 0)
                        //{
                        //    var riskType2 = new RiskTypeListModel();
                        //    riskType2.Id = Convert.ToString(5);
                        //    var riskItem2 = new RiskItemListModel();
                        //    riskItem2.Id = isAdverse.ToString();
                        //    var riskItemList2 = new List<RiskItemListModel>();
                        //    riskItemList2.Add(riskItem2);
                        //    riskType2.RiskItemList = riskItemList2;
                        //    riskTypeList.Add(riskType2);
                        //}
                        //for adverse end
                        riskModel.RiskTypeList = riskTypeList;
                        riskModel.ClientId = clientMasterDTO.ClientId;
                        var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                        var xyz = riskResult;
                    }
                   else if (model.CustomerType == "C")
                    {

                        //var isAdverse = 3345;

                        //if (apiResp.ApiResultsjson != null)
                        //{
                        //    if (apiResp.ApiResultsjson[0].isAdverse == true)
                        //    {
                        //        isAdverse = 3344;
                        //    }
                        //    else
                        //    {
                        //        isAdverse = 3345;
                        //    }
                        //}


                        //var isPep = 0;

                        //if (model.PEPStatus == 1)
                        //{
                        //    isPep = 3341;
                        //}
                        //else
                        //{
                        //    isPep = 3342;
                        //}

                        KycIndividualDTO imodel = new KycIndividualDTO();

                        var str1 = _kycService.GetRiskLovId(imodel, _mapper.Map<CorporateKycDTO>(model), "C", culture, model.ClientId);
                        
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
                        var fatflovId = spStr1[27];
                        var modeofpaymentlovId = spStr1[16];
                        var domesticlovId = spStr1[18];
                        var foreginlovId = spStr1[20];
                        var redflagslovId = spStr1[22];
                        var sanctionlovId = spStr1[24];
                        var uaeorunsclovId = spStr1[26];
                        var highestriskprodlovId = spStr1[29];
                        var highnetworkindividuallovid = spStr1[31];

                        var str = _kycService.GetRiskTypeId(imodel, _mapper.Map<CorporateKycDTO>(model), "C", culture, model.ClientId);
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
                        var fatfId = spStr[27];
                        var modeofpaymentId = spStr[16];
                        var domesticId = spStr[18];
                        var foreginId = spStr[20];
                        var redflagsId = spStr[22];
                        var sanctionId = spStr[24];
                        var uaeorunscId = spStr[26];
                        var highestriskprodId = spStr[29];
                        var highnetworkindividualid = spStr[31];



                        RiskAPIRequestModel riskModel = new RiskAPIRequestModel();
                        //riskModel.CustomerId = model.CustomerId;
                        riskModel.CustomerId = model.CustomerId;
                        riskModel.CustomerName = model.FirstName + model.MiddleName + model.LastName;
                        
                        //if (model.EntityAddress.Country == "0")
                        //{
                        //    riskModel.MainNationality = "";
                        //}
                        //else
                        //{
                        //    riskModel.MainNationality = model.EntityAddress.Country;
                        //}
                        //riskModel.MainNationality = x.Nationality;
                        riskModel.RiskCategory = "C";


                        var riskTypeList = new List<RiskTypeListModel>();
                        //for legal status of entity start
                        if (entId != "0")
                        {
                            var riskType1 = new RiskTypeListModel();
                            riskType1.Id = Convert.ToString(entlovId);
                            var riskItem1 = new RiskItemListModel();
                            riskItem1.Id = entId.ToString();
                            var riskItemList1 = new List<RiskItemListModel>();
                            riskItemList1.Add(riskItem1);
                            riskType1.RiskItemList = riskItemList1;
                            riskTypeList.Add(riskType1);
                        }
                        //for legal status of entity end
                        //for nature of business start
                        if (busId != "0")
                        {
                            var riskType2 = new RiskTypeListModel();
                            riskType2.Id = Convert.ToString(buslovId);
                            var riskItem2 = new RiskItemListModel();
                            riskItem2.Id = busId.ToString();
                            var riskItemList2 = new List<RiskItemListModel>();
                            riskItemList2.Add(riskItem2);
                            riskType2.RiskItemList = riskItemList2;
                            riskTypeList.Add(riskType2);
                        }
                        //for nature of business end
                        //for country of incorporation start
                        if (incorpId != "0")
                        {
                            var riskType3 = new RiskTypeListModel();
                            riskType3.Id = Convert.ToString(incorplovId);
                            var riskItem3 = new RiskItemListModel();
                            riskItem3.Id = incorpId.ToString();//Convert.ToString(1);
                            var riskItemList3 = new List<RiskItemListModel>();
                            riskItemList3.Add(riskItem3);
                            riskType3.RiskItemList = riskItemList3;
                            riskTypeList.Add(riskType3);
                        }
                        //for country of incorporation end
                        //nationality partner 1 start
                        if (nationality1Id != "0")
                        {
                            var riskType4 = new RiskTypeListModel();
                            riskType4.Id = Convert.ToString(nationality1lovId);
                            var riskItem4 = new RiskItemListModel();
                            riskItem4.Id = nationality1Id.ToString();//Convert.ToString(1);
                            var riskItemList4 = new List<RiskItemListModel>();
                            riskItemList4.Add(riskItem4);
                            riskType4.RiskItemList = riskItemList4;
                            riskTypeList.Add(riskType4);
                        }
                        //nationality partner 1 end
                        //nationality partner 2 start
                        if (nationality2Id != "0")
                        {
                            var riskType5 = new RiskTypeListModel();
                            riskType5.Id = Convert.ToString(nationality2lovId);
                            var riskItem5 = new RiskItemListModel();
                            riskItem5.Id = nationality2Id.ToString();//Convert.ToString(1);
                            var riskItemList5 = new List<RiskItemListModel>();
                            riskItemList5.Add(riskItem5);
                            riskType5.RiskItemList = riskItemList5;
                            riskTypeList.Add(riskType5);
                        }
                        //nationality partner 2 end
                        //nationality partner 3 start
                        if (nationality3Id != "0")
                        {
                            var riskType6 = new RiskTypeListModel();
                            riskType6.Id = Convert.ToString(nationality3Id);
                            var riskItem6 = new RiskItemListModel();
                            riskItem6.Id = nationality3Id.ToString();//Convert.ToString(1);
                            var riskItemList6 = new List<RiskItemListModel>();
                            riskItemList6.Add(riskItem6);
                            riskType6.RiskItemList = riskItemList6;
                            riskTypeList.Add(riskType6);
                        }
                        //nationality partner 3 end
                        //nationality partner 4 start
                        if (nationality4Id != "0")
                        {
                            var riskType7 = new RiskTypeListModel();
                            riskType7.Id = Convert.ToString(nationality4lovId);
                            var riskItem7 = new RiskItemListModel();
                            riskItem7.Id = nationality4Id.ToString();//Convert.ToString(1);
                            var riskItemList7 = new List<RiskItemListModel>();
                            riskItemList7.Add(riskItem7);
                            riskType7.RiskItemList = riskItemList7;
                            riskTypeList.Add(riskType7);
                        }
                        //nationality partner 4 end
                        //nationality partner 5 start
                        if (nationality5Id != "0")
                        {
                            var riskType8 = new RiskTypeListModel();
                            riskType8.Id = Convert.ToString(nationality5lovId);
                            var riskItem8 = new RiskItemListModel();
                            riskItem8.Id = nationality5Id.ToString();//Convert.ToString(1);
                            var riskItemList8 = new List<RiskItemListModel>();
                            riskItemList8.Add(riskItem8);
                            riskType8.RiskItemList = riskItemList8;
                            riskTypeList.Add(riskType8);
                        }
                        //nationality partner 5 end
                        //FATF  start
                        if (fatfId != "0")
                        {
                            var riskType18 = new RiskTypeListModel();
                            riskType18.Id = Convert.ToString(fatflovId);
                            var riskItem18 = new RiskItemListModel();
                            riskItem18.Id = fatfId.ToString();//Convert.ToString(1);
                            var riskItemList18 = new List<RiskItemListModel>();
                            riskItemList18.Add(riskItem18);
                            riskType18.RiskItemList = riskItemList18;
                            riskTypeList.Add(riskType18);
                        }
                        //FATF end
                        //product start
                        if (productId != "0")
                        {
                            var riskType9 = new RiskTypeListModel();
                            riskType9.Id = Convert.ToString(productlovId);
                            var riskItem9 = new RiskItemListModel();
                            riskItem9.Id = productId.ToString();//Convert.ToString(1);
                            var riskItemList9 = new List<RiskItemListModel>();
                            riskItemList9.Add(riskItem9);
                            riskType9.RiskItemList = riskItemList9;
                            riskTypeList.Add(riskType9);
                        }
                        //product end
                        //highest risk product start
                        if (highestriskprodId != "0")
                        {
                            var riskType10 = new RiskTypeListModel();
                            riskType10.Id = Convert.ToString(highestriskprodlovId);
                            var riskItem10 = new RiskItemListModel();
                            riskItem10.Id = highestriskprodId.ToString();//Convert.ToString(1);
                            var riskItemList10 = new List<RiskItemListModel>();
                            riskItemList10.Add(riskItem10);
                            riskType10.RiskItemList = riskItemList10;
                            riskTypeList.Add(riskType10);
                        }
                        //highest risk product  end

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

                        //for domesticpep start
                        if (domesticId != "0")
                        {
                            var riskType12 = new RiskTypeListModel();
                            riskType12.Id = Convert.ToString(domesticlovId);
                            var riskItem12 = new RiskItemListModel();
                            riskItem12.Id = domesticId.ToString();
                            var riskItemList12 = new List<RiskItemListModel>();
                            riskItemList12.Add(riskItem12);
                            riskType12.RiskItemList = riskItemList12;
                            riskTypeList.Add(riskType12);
                        }
                        //for domesticpep end
                        //for foreignpep start
                        if (foreginId != "0")
                        {
                            var riskType13 = new RiskTypeListModel();
                            riskType13.Id = Convert.ToString(foreginlovId);
                            var riskItem13 = new RiskItemListModel();
                            riskItem13.Id = foreginId.ToString();
                            var riskItemList13 = new List<RiskItemListModel>();
                            riskItemList13.Add(riskItem13);
                            riskType13.RiskItemList = riskItemList13;
                            riskTypeList.Add(riskType13);
                        }
                        //for foreignpep end
                        //for redflags start
                        if (redflagsId != "0")
                        {
                            var riskType14 = new RiskTypeListModel();
                            riskType14.Id = Convert.ToString(redflagslovId);
                            var riskItem14 = new RiskItemListModel();
                            riskItem14.Id = redflagsId.ToString();
                            var riskItemList14 = new List<RiskItemListModel>();
                            riskItemList14.Add(riskItem14);
                            riskType14.RiskItemList = riskItemList14;
                            riskTypeList.Add(riskType14);
                        }
                        //for redflags end
                        //for high nettwork individual start
                        if (highnetworkindividualid != "0")
                        {
                            var riskType19 = new RiskTypeListModel();
                            riskType19.Id = Convert.ToString(highnetworkindividuallovid);
                            var riskItem19 = new RiskItemListModel();
                            riskItem19.Id = highnetworkindividualid.ToString();
                            var riskItemList19 = new List<RiskItemListModel>();
                            riskItemList19.Add(riskItem19);
                            riskType19.RiskItemList = riskItemList19;
                            riskTypeList.Add(riskType19);
                        }
                        //for high nettwork individual end
                        //for sanctionmatch start
                        if (sanctionId != "0")
                        {
                            var riskType15 = new RiskTypeListModel();
                            riskType15.Id = Convert.ToString(sanctionlovId);
                            var riskItem15 = new RiskItemListModel();
                            riskItem15.Id = sanctionId.ToString();
                            var riskItemList15 = new List<RiskItemListModel>();
                            riskItemList15.Add(riskItem15);
                            riskType15.RiskItemList = riskItemList15;
                            riskTypeList.Add(riskType15);
                        }
                        //for sanctionmatch end
                        //for uaeorunsc start
                        if (uaeorunscId != "0")
                        {
                            var riskType16 = new RiskTypeListModel();
                            riskType16.Id = Convert.ToString(uaeorunsclovId);
                            var riskItem16 = new RiskItemListModel();
                            riskItem16.Id = uaeorunscId.ToString();
                            var riskItemList16 = new List<RiskItemListModel>();
                            riskItemList16.Add(riskItem16);
                            riskType16.RiskItemList = riskItemList16;
                            riskTypeList.Add(riskType16);
                        }
                        //for uaeorunsc end

                        //mode of payment start
                        if (modeofpaymentId != "0")
                        {
                            var riskType17 = new RiskTypeListModel();
                            riskType17.Id = Convert.ToString(modeofpaymentlovId);
                            var riskItem17 = new RiskItemListModel();
                            riskItem17.Id = modeofpaymentId.ToString();//Convert.ToString(1);
                            var riskItemList17 = new List<RiskItemListModel>();
                            riskItemList17.Add(riskItem17);
                            riskType17.RiskItemList = riskItemList17;
                            riskTypeList.Add(riskType17);
                        }
                        //mode of payment end
                        //for pep start
                        //if (isPep != 0)
                        //{
                        //    var riskType2 = new RiskTypeListModel();
                        //    riskType2.Id = Convert.ToString(21);
                        //    var riskItem2 = new RiskItemListModel();
                        //    riskItem2.Id = isPep.ToString();
                        //    var riskItemList2 = new List<RiskItemListModel>();
                        //    riskItemList2.Add(riskItem2);
                        //    riskType2.RiskItemList = riskItemList2;
                        //    riskTypeList.Add(riskType2);
                        //}
                        //for pep end

                        riskModel.RiskTypeList = riskTypeList;
                        riskModel.ClientId= clientMasterDTO.ClientId;
                        var riskResult = _riskAPIController.Assessment(riskModel);
                        var xyz = riskResult;

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
    }
}
