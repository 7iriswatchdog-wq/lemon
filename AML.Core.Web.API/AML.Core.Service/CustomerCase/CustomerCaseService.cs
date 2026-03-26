using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Enum;
using AML.Core.Repository.CustomerCase;
using AML.Core.RepositoryContract.CorporateShareholder;
using AML.Core.RepositoryContract.Country;
using AML.Core.RepositoryContract.CustomerCase;
using AML.Core.RepositoryContract.CustomerScreening;
using AML.Core.RepositoryContract.EtlBatch;
using AML.Core.Service.CaseComment;
using AML.Core.ServiceContract.Branch;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.LovMaster;
using AML.Core.ServiceContract.Risk;
using AML.DTO.DTO.Branch;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.CodesMaster;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CorporateShareholder;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.EtlBatch;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.ApiAuthentication;
using AML.ViewModel.ViewModels.CaseComment;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.Risk;
using AML.ViewModel.ViewModels.RiskAPI;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AML.Core.Service.CustomerCase
{
    public class CustomerCaseService : BaseService, ICustomerCaseService
    {
        ICustomerCaseRepository _CustomerCaseRepository;
        ICustomerScreeningRepository _CustomerScreeningRepository;
        IEtlLogRepository _etlLogRepository;
        ICustomerMasterRepository _customerMasterRepository;
        ICorporateShareholderRepository _corporateShareholderRepository;
        ICountryRepository _countryRepository;
        IRiskService _riskService;
        ILovMasterService _lovMasterService;
        private ICaseCommentService _caseCommentService;
        private IMapper _mapper;
  

        private string baseURL = string.Empty;
        private int clientId = 0;
        private object _customerCaseService;

        public CustomerCaseService(IEtlLogRepository etlLogRepository, ICustomerCaseRepository customerCaseRepository, IConfiguration configuration, ICorporateShareholderRepository corporateShareholderRepository,
            IHostingEnvironment environment, ICustomerScreeningRepository customerScreeningRepository, ICustomerMasterRepository customerMasterRepository, ICaseCommentService caseCommentService, IMapper mapper,
            ICountryRepository countryRepository, ILovMasterService lovMasterService, IRiskService riskService)
            : base(customerCaseRepository, configuration)
        {
            _CustomerCaseRepository = customerCaseRepository;
            _CustomerScreeningRepository = customerScreeningRepository;
            _etlLogRepository = etlLogRepository;
            _customerMasterRepository = customerMasterRepository;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            _corporateShareholderRepository = corporateShareholderRepository;
            _countryRepository = countryRepository;
            _lovMasterService = lovMasterService;
            _riskService = riskService;
            _caseCommentService = caseCommentService;
        }
        public string Create(CustomerCaseDTO _CustomerCaseDT, bool returnId = false)
        {
            CustomerMasterDTO custDetails = new CustomerMasterDTO()
            {
                CustomerId = _CustomerCaseDT.CustomerId,
                FirstName = _CustomerCaseDT.FirstName,
                MiddleName = _CustomerCaseDT.MiddleName,
                LastName = _CustomerCaseDT.LastName,
                Nationality = _CustomerCaseDT.Nationality,
                CustDOB = _CustomerCaseDT.CustDOB,
                CustomerIdType = _CustomerCaseDT.CustomerIdType,
                CustomerIdNumber = _CustomerCaseDT.CustomerIdNumber,
                Mobile = _CustomerCaseDT.Mobile,
                CreatedBy = _CustomerCaseDT.CreatedBy,
                CustomerType = _CustomerCaseDT.CustomerType,
                UserId = _CustomerCaseDT.UserId,
                ClientId = _CustomerCaseDT.ClientId,
                CompanyName = _CustomerCaseDT.CompanyName,
                Threshold = _CustomerCaseDT.Threshold,
                CompanyCode = _CustomerCaseDT.CompanyCode,
            };
            ServiceResponse<string> _res = _customerMasterRepository.Create(custDetails);
            //var resu = _res.Result.Split('Ø');
            //string CustId = resu[1];
            var resu = _res.Result;
            var resu1 = "";
            string CustId = "";
            if (_res.Result.ToString() != "0")
            {
                resu = _res.Result.Split('Ø')[0];
                resu1 = _res.Result.Split('Ø')[1];
                CustId = resu1;
            }
            else
            {
                resu = _res.Result;
                resu1 = _res.Result;
                CustId = resu;
            }
            if (CustId != null && CustId != "0")
            {
                _CustomerCaseDT.CustomerId = CustId;
                _CustomerCaseDT.Id = resu.ParseInt();
                //  _CustomerCaseDT.CustomerId = CustId.ParseString();
                _CustomerCaseDT.CustomerMasterId = Regex.Replace(CustId, "[^0-9]", "").ParseInt();
                _CustomerCaseRepository.Create(_CustomerCaseDT);
            }
            _res.Result = resu1;
            return returnId ? resu : _res.Result;
        }
        public List<CustomerMasterDTO> GetUnscreenedCustomers()
        {
            return _CustomerCaseRepository.GetUnscreenedCustomers().Result;
        }
        public string GetUserIdForAPI(string UserId)
        {
            return _CustomerCaseRepository.GetUserIdForAPI(UserId).Result;
        }
        public ServiceResponse<string> Create(CustomerCaseDTO _CustomerCaseDT)
        {
            CustomerMasterDTO custDetails = new CustomerMasterDTO()
            {
                CustomerId = _CustomerCaseDT.CustomerId,
                FirstName = _CustomerCaseDT.FirstName,
                MiddleName = _CustomerCaseDT.MiddleName,
                LastName = _CustomerCaseDT.LastName,
                Nationality = _CustomerCaseDT.Nationality,
                DOB = _CustomerCaseDT.DOB,
                CustomerIdType = _CustomerCaseDT.CustomerIdType,
                CustomerIdNumber = _CustomerCaseDT.CustomerIdNumber,
                Mobile = _CustomerCaseDT.Mobile,
                CreatedBy = _CustomerCaseDT.CreatedBy,
                CustomerType = _CustomerCaseDT.CustomerType,
                UserId = _CustomerCaseDT.UserId,
                ClientId = _CustomerCaseDT.ClientId,
                CompanyName = _CustomerCaseDT.CompanyName,
                Threshold = _CustomerCaseDT.Threshold,
                CompanyCode = _CustomerCaseDT.CompanyCode,
                CreatedOnDB = Convert.ToDateTime(_CustomerCaseDT.CreatedOn),
                Type= _CustomerCaseDT.Type,
                ProductName=_CustomerCaseDT.ProductName,
                EntityTypeTxt=_CustomerCaseDT.EntityTypeTxt,
                DeliveryChannelName=_CustomerCaseDT.DeliveryChannelName,
                Modeofpayment=_CustomerCaseDT.Modeofpayment,
                ResidenceStatus=_CustomerCaseDT.ResidenceStatus,
                OccupatinTypeTxt=_CustomerCaseDT.OccupatinTypeTxt,
                BusinessType=_CustomerCaseDT.BusinessType,
                CIFNumber=_CustomerCaseDT.CIFNumber,
                ParentID = _CustomerCaseDT.ParentID,
                ScreeningOptions=_CustomerCaseDT.ScreeningOptions,
                IdIssueDate=_CustomerCaseDT.IdIssueDate,
                IdExpiryDate=_CustomerCaseDT.IdExpiryDate,
                Residence=_CustomerCaseDT.Residence,
                Employer=_CustomerCaseDT.Employer,
                GoldenVisa=_CustomerCaseDT.GoldenVisa,
                EmployerIndustry=_CustomerCaseDT.EmployerIndustry,
                EmployerSector=_CustomerCaseDT.EmployerSector,
                SOWSOFCountry=_CustomerCaseDT.SOWSOFCountry,
                TradeLicenseAuthority=_CustomerCaseDT.TradeLicenseAuthority,
                TradeLicenseSector=_CustomerCaseDT.TradeLicenseSector,
                ProductRefNo=_CustomerCaseDT.ProductRefNo,
                ProductValue=_CustomerCaseDT.ProductValue,
                CounterParty=_CustomerCaseDT.CounterParty,
                Share=_CustomerCaseDT.Share,
                Designation=_CustomerCaseDT.Designation,
                CounterPartyName=_CustomerCaseDT.CounterPartyName,
                FlagType=_CustomerCaseDT.FlagType,
                Relationship=_CustomerCaseDT.Relationship
                
                //EstablishmentDate=_CustomerCaseDT.EstablishmentDate,
                //Address= _CustomerCaseDT.Address
            };
            ServiceResponse<string> _res = _customerMasterRepository.Create(custDetails);

            string resu;
            string resu1;

            if (_res.Result.ToString() != "0")
            {
                resu = _res.Result.Split('Ø')[0];
                resu1 = _res.Result.Split('Ø')[1];
            }

            else
            {
                resu = _res.Result;
                resu1 = _res.Result;
            }

            if (resu1 != null && resu1 != "0")
            {
                _CustomerCaseDT.CustomerId = resu1;
            }
            if (resu != null && resu != "0")
            {
                _CustomerCaseDT.CustomerMasterId = Regex.Replace(resu, "[^0-9]", "").ParseInt();
            }

            if (resu1 != null && resu1 != "0" && resu != null && resu != "0")
            {
                
                
                _CustomerCaseRepository.Create(_CustomerCaseDT);
            }

            //_res.Result = resu;

            return _res;
        }
       

        public ServiceResponse<string> customerIdcheck(string CustomerId)
        {
            ServiceResponse<string> _res = _customerMasterRepository.GetCustomerId(CustomerId);

            return _res;


        }


        //DNIRC
        public ServiceResponse<string> CreateScreening(CustomerCaseDTO _CustomerCaseDT)
        {
            CustomerMasterDTO custDetails = new CustomerMasterDTO()
            {
                //CustomerId = _CustomerCaseDT.CustomerId,
                //FirstName = _CustomerCaseDT.FirstName,
                //MiddleName = _CustomerCaseDT.MiddleName,
                //LastName = _CustomerCaseDT.LastName,
                //Nationality = _CustomerCaseDT.Nationality,
                //CustDOB = _CustomerCaseDT.CustDOB,
                //CustomerIdType = _CustomerCaseDT.CustomerIdType,
                //CustomerIdNumber = _CustomerCaseDT.CustomerIdNumber,
                //Mobile = _CustomerCaseDT.Mobile,
                //CreatedBy = _CustomerCaseDT.CreatedBy,
                //CustomerType = _CustomerCaseDT.CustomerType,
                //UserId = _CustomerCaseDT.UserId,
                ClientId = _CustomerCaseDT.ClientId,
                //CompanyName = _CustomerCaseDT.CompanyName,
                //Threshold = _CustomerCaseDT.Threshold

                onb_cust_ref_id=_CustomerCaseDT.onb_cust_ref_id,
                onb_name=_CustomerCaseDT.onb_name,
                DOB = _CustomerCaseDT.DOB,
                onb_nationality =_CustomerCaseDT.onb_nationality,
                onb_cust_type= _CustomerCaseDT.onb_cust_type,
                onb_cust_id_type = _CustomerCaseDT.onb_cust_id_type,
                onb_cust_id = _CustomerCaseDT.onb_cust_id,
                onb_mobile = _CustomerCaseDT.onb_mobile,
                onb_created_by = _CustomerCaseDT.CreatedBy,
                onb_created_on = _CustomerCaseDT.onb_created_on,
                onb_updated_by = _CustomerCaseDT.UpdatedBy,
                onb_emirates_id = _CustomerCaseDT.onb_emirates_id,
                onb_emirates_id_expiry = _CustomerCaseDT.onb_emirates_id_expiry,
                onb_passport_expiry = _CustomerCaseDT.onb_passport_expiry,
                onb_profession = _CustomerCaseDT.onb_profession,
                onb_residence_status = _CustomerCaseDT.onb_residence_status,
                onb_insurance_product = _CustomerCaseDT.onb_insurance_product,
                onb_delv_channel = _CustomerCaseDT.onb_delv_channel,
                onb_src_funds = _CustomerCaseDT.onb_src_funds,
                onb_mode_of_pymt = _CustomerCaseDT.onb_mode_of_pymt,
                onb_dept = _CustomerCaseDT.onb_dept,
                onb_policy_no = _CustomerCaseDT.onb_policy_no,
                onb_endt_no = _CustomerCaseDT.onb_endt_no,
                onb_doc_no = _CustomerCaseDT.onb_doc_no,
                onb_party_type = _CustomerCaseDT.onb_party_type,
                onb_subclass = _CustomerCaseDT.onb_subclass,
                onb_app_name = _CustomerCaseDT.CompanyName,
                onb_customerid = _CustomerCaseDT.onb_cust_ref_id,
                onb_is_screened = _CustomerCaseDT.onb_is_screened,
                onb_threshold = _CustomerCaseDT.onb_threshold

            };
            ServiceResponse<string> _res = _customerMasterRepository.CreateScreening(custDetails);

            string resu;
            string resu1;

            if (_res.Result.ToString() != "0" || _res.Result ==null)
            {
                resu = _res.Result.Split('Ø')[0];
                resu1 = _res.Result.Split('Ø')[1];
            }

            else
            {
                resu = _res.Result;
                resu1 = _res.Result;
            }

            if (resu1 != null && resu1 != "0")
            {
                _CustomerCaseDT.CustomerId = resu1;
            }
            if (resu != null && resu != "0")
            {
                _CustomerCaseDT.CustomerMasterId = Regex.Replace(resu, "[^0-9]", "").ParseInt();
            }

            if (resu1 != null && resu1 != "0" && resu != null && resu != "0")
            {
                _CustomerCaseRepository.CreateScreening(_CustomerCaseDT);
            }

            _res.Result = resu;

            return _res;
        }


        public ServiceResponse<string> IndividualRiskCreation(CustomerCaseDTO _CustomerCaseDT)
        {
            CustomerMasterDTO riskDetails = new CustomerMasterDTO()
            {
                onb_profession = _CustomerCaseDT.onb_profession,
                onb_nationality=_CustomerCaseDT.onb_nationality,
                onb_residence_status = _CustomerCaseDT.onb_residence_status,
                onb_insurance_product = _CustomerCaseDT.onb_insurance_product,
                onb_delv_channel = _CustomerCaseDT.onb_delv_channel,
                onb_src_funds = _CustomerCaseDT.onb_src_funds,
                onb_mode_of_pymt = _CustomerCaseDT.onb_mode_of_pymt,

            };

            ServiceResponse<string> _res = _CustomerCaseRepository.IndividualRisk(_CustomerCaseDT);
            return _res;
        }


        public ServiceResponse<string> CorporateRiskCreation(CustomerCaseDTO _CustomerCaseDT)
        {
            CustomerMasterDTO riskDetails = new CustomerMasterDTO()
            {
               
                onb_nationality = _CustomerCaseDT.onb_nationality,
                onb_insurance_product = _CustomerCaseDT.onb_insurance_product,
                onb_delv_channel = _CustomerCaseDT.onb_delv_channel,
                onb_src_funds = _CustomerCaseDT.onb_src_funds,
                onb_mode_of_pymt = _CustomerCaseDT.onb_mode_of_pymt,

            };

            ServiceResponse<string> _res = _CustomerCaseRepository.CorporateRisk(_CustomerCaseDT);
            return _res;
        }



        public ServiceResponse<string> CreatePrefix(CustomerCaseDTO _CustomerCaseDT)
        {
            CustomerMasterDTO custDetails = new CustomerMasterDTO()
            {
                CustomerId = _CustomerCaseDT.CustomerId,
                FirstName = _CustomerCaseDT.FirstName,
                MiddleName = _CustomerCaseDT.MiddleName,
                LastName = _CustomerCaseDT.LastName,
                Nationality = _CustomerCaseDT.Nationality,
                //CustDOB = _CustomerCaseDT.CustDOB,
                //DOB = _CustomerCaseDT.DOB,
                DOB = _CustomerCaseDT.DOB.ToString("MM-dd-yyyy").ParseDB().GetValueOrDefault(),
                CustomerIdType = _CustomerCaseDT.CustomerIdType,
                CustomerIdNumber = _CustomerCaseDT.CustomerIdNumber,
                Mobile = _CustomerCaseDT.Mobile,
                CreatedBy = _CustomerCaseDT.CreatedBy,
                CustomerType = _CustomerCaseDT.CustomerType,
                UserId = _CustomerCaseDT.UserId,
                ClientId = _CustomerCaseDT.ClientId,
                CompanyCode = _CustomerCaseDT.CompanyCode,
                Tradelicense = _CustomerCaseDT.Tradelicense,
                customerCodeprefix= _CustomerCaseDT.customerCodeprefix,

            };
            ServiceResponse<string> _res = _customerMasterRepository.CreatePrefix(custDetails);
            string CustId = _res.Result;
            if (CustId.IsNotNullOrEmpty())
            {
                _CustomerCaseDT.CustomerMasterId = Regex.Replace(CustId, "[^0-9]", "").ParseInt();
                //  _CustomerCaseDT.CustomerMasterId= CustId.Substring(3).ParseInt();

                _CustomerCaseDT.CustomerId = CustId;//this value will be appended with prefix to get the original customer ID
                _CustomerCaseRepository.Create(_CustomerCaseDT);
            }
            return _res;
        }
       
        public async Task<ServiceResponse<List<CustomerCaseDTO>>> GetCasebyApprovedStatusAsync(int clientId)
        {

            return await _CustomerCaseRepository.GetApprovedListAsync(clientId);
        }
        public List<CustomerCaseDTO> GetCasebyApprovedStatus(int clientId)
        {

            return _CustomerCaseRepository.GetApprovedList(clientId).Result;
        }

        public List<CustomerCaseDTO> GetCasespendingScheduler()
        {

            return _CustomerCaseRepository.GetCasespendingScheduler().Result;
        }

        public async Task<ServiceResponse<List<ClientMasterDTO>>> GetAllClientsAsync()
        {

            return await _CustomerCaseRepository.GetAllClientsAsync();
        }
        public List<ClientMasterDTO> GetAllClients()
        {
            var result = _CustomerCaseRepository.GetAllClients().Result;
            return result ?? new List<ClientMasterDTO>();
        }
        public List<ClientMasterDTO> GetAllAdminClients()
        {
            var response = _CustomerCaseRepository.GetAllAdminClients();
            if (response.Status == StaticResource.SuccessStatusCode)
                return response.Result;
            else
                return new List<ClientMasterDTO>();
        }
        //public ServiceResponse<string> CreatePassportDetails(PassportDetails passport)
        //{

        //    return _CustomerCaseRepository.CreatePassportDetails(passport);
        //}

        //public List<PassportDetailsDTO> GetPassportdetails(int caseid)
        //{

        //    return _CustomerCaseRepository.GetPassportdetails(caseid).Result;
        //}


        public ServiceResponse<int> Update(ClientMasterDTO _clientDTO)
        {
            //Perform business requirements here
            var _client = _CustomerCaseRepository.GetClientDetailsByID(_clientDTO.ClientId);
            if (_client.Result != null)
            {
                //_clientDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
                var _Response = _CustomerCaseRepository.UpdateClient(_clientDTO);
                if (_Response.Result > 0)
                {
                    var _clientDetails = _CustomerCaseRepository.GetClientDetailsByID(_clientDTO.ClientId);
                    if (_clientDetails.Result == null)
                    {
                        return _CustomerCaseRepository.CreateClient(_clientDTO);
                    }
                    else
                    {
                        return _CustomerCaseRepository.UpdateClient(_clientDTO);
                    }
                }
                return _Response;
            }
            return null;
        }
        public ClientMasterDTO GetClientDetailsByID(int clientId)
        {

            return _CustomerCaseRepository.GetClientDetailsByID(clientId).Result;
        }
       
        public ServiceResponse<int> UpdateClient(ClientMasterDTO _clientDTO)
        {
            return _CustomerCaseRepository.UpdateClient(_clientDTO);
        }
        public ServiceResponse<int> CreateClient(ClientMasterDTO _clientDTO)
        {
            return _CustomerCaseRepository.CreateClient(_clientDTO);
        }
        public ServiceResponse<int> UploadLogo(ClientMasterDTO _clientDTO)
        {
            return _CustomerCaseRepository.UploadLogo(_clientDTO);
        }
        public ServiceResponse<int> DeleteClient(ClientMasterDTO _clientDTO)
        {
            return _CustomerCaseRepository.DeleteClient(_clientDTO);
        }
        public List<ClientRightsDTO> GetClientRightsByClientId(int clientId)
        {

            return _CustomerCaseRepository.GetClientRightsByClientId(clientId).Result;
        }
        public List<MenuModelDTO> GetAllMenus()
        {

            return _CustomerCaseRepository.GetAllMenus().Result;
        }
        public CustomerCaseDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetDetails(Id).Result;
        }

        public string GetDualGoodsStatus(string customerId)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetDualGoodsStatus(customerId).Result;
        }
        public List<CustomerCaseDTO> GetShareHoldersByCompanyCode(string Id)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetShareHoldersByCompanyCode(Id).Result;
        }
        public int GetCaseId(string CustId)
        {
            return _CustomerCaseRepository.GetCaseId(CustId);
        }


        public CustomerCaseDTO GetCaseFullDetailsByCaseId(int _newCaseId)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetCaseFullDetailsByCaseId(_newCaseId).Result;
        }

        public CustomerCaseDTO GetCaseFullDetailsByCustId(string _newCustMasterId)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetCaseFullDetailsByCustomerId(_newCustMasterId).Result;
        }
        public CustomerCaseDTO GetCaseFullDetailsByCustId(int _newCustMasterId)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetCaseFullDetailsByCustomerId(_newCustMasterId).Result;
        }

        public CustomerCaseDTO GetCaseStatusByCustId(string _newCustMasterId)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetCaseStatusByCustomerId(_newCustMasterId).Result;

        }

        public CustomerCaseDTO GetRiskStatusByCustId(string _newCustMasterId,string customertype)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetRiskStatusByCustomerId(_newCustMasterId,customertype).Result;

        }

        public CreatedAndUpdatedByNames GetCreatedByAndUpdateByNameFromId(int createdById, int updatedById)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetCreatedByAndUpdateByNameFromId(createdById, updatedById).Result;

        }
        public List<CustomerCaseDTO> GetAll(int userId, string startDate, string endDate, string cust_type, string matchscore, int createdBy,int caseStatus,string riskLevel,  string usergroupName,int clientId)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetAll(userId, startDate, endDate, cust_type, matchscore, createdBy, caseStatus, riskLevel, usergroupName, clientId).Result;
        }
        public List<CustomerCaseDTO> GetAllCompletedCases(int userId, string startDate, string endDate, string cust_type, string matchscore, int createdBy, int caseStatus, string riskLevel, int clientId)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetAllCompletedCases(userId, startDate, endDate, cust_type, matchscore, createdBy, caseStatus, riskLevel,clientId).Result;
        }
        public List<CustomerCaseDTO> GetAllBySearchValue(int userId, string startDate, string endDate, string cust_type,string searchValue, string matchscore, int createdBy, int caseStatus, string riskLevel, string usergroupName, int clientId)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetAllBySearchValue(userId, startDate, endDate, cust_type,searchValue, matchscore, createdBy, caseStatus, riskLevel, usergroupName,clientId).Result;
        }
        public List<CustomerCaseDTO> GetAllCompletedBySearchValue(int userId, string startDate, string endDate, string cust_type, string searchValue, string matchscore, int createdBy, int caseStatus, string riskLevel, int clientId)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetAllCompletedBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchscore, createdBy, caseStatus, riskLevel,clientId).Result;
        }

        public List<CustomerCaseDTO> GetAllSanctionDashboard(int clientId , string ctype)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetAllSanctionDashboard(clientId, ctype).Result;
        }

        public ServiceResponse<int> InsertScreeninglogs(ScreeinglogsModel model)
        {
            return _CustomerCaseRepository.InsertScreeninglogs(model);
        }
        public ServiceResponse<int> Update(CustomerCaseDTO _CustomerCaseDT)
        {
            return _CustomerCaseRepository.Update(_CustomerCaseDT);
        }
        public ServiceResponse<int> UpdateCase(int caseid,int userid)
        {
            return _CustomerCaseRepository.UpdateCase(caseid, userid);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _CustomerCaseRepository.Delete(Id);
        }
        public ServiceResponse<int> DeleteRightsByClientId(int ClientId)
        {
            return _CustomerCaseRepository.DeleteRightsByClientId(ClientId);
        }
        public ServiceResponse<int> CreateClientRight(ClientMenuRightsModelDTO _clientRightDTO)
        {
            return _CustomerCaseRepository.CreateClientRight(_clientRightDTO);
        }
        public void ScreeningSearch(CustomerScreeningRQ _customerCase)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<List<CustomerExcelDTO>> SaveCustomerCaseExcelData(DocumentsDTO _documentsModel, string typeId, int C6Threshold, int Threshold,string screeningoption)
        {
            ServiceResponse<List<CustomerExcelDTO>> _custExcelDataResponse = _CustomerCaseRepository.LoadCustomerCaseExcelData(_documentsModel.DocFullPath, typeId, 1);
            if (_custExcelDataResponse.Result.Count > 50)
            {
                _custExcelDataResponse.Status = StaticResource.FailStatusCode;
                _custExcelDataResponse.Message = "The number of customer data per file should not exceed 50.";
            }
            else
            {
                if (_custExcelDataResponse.Status == StaticResource.SuccessStatusCode)
                {
                    List<CustomerExcelDTO> _custExcelDataList = _custExcelDataResponse.Result;
                    int totalRows = _custExcelDataList.Count;
                    int _processedCount = 0;
                    var countries = _countryRepository.GetAll(_documentsModel.ClientId).Result.Select(_ => _.Name).ToList();
                    string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                     "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                     "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                     "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                     "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm"};

                    if (totalRows > 0)
                    {
                        EtlBatchDTO _etlBatchDTO = new EtlBatchDTO()
                        {
                            FileName = _documentsModel.DocName,
                            FileFullPath = _documentsModel.DocFullPath,
                            TotalRows = totalRows,
                            RowsRecorded = 0,
                            AddedBy = _documentsModel.AddedBy,
                            Type = (int)LogModulle.Customer,
                            ClientId = _documentsModel.ClientId
                        };
                        int batchId = _etlLogRepository.Create(_etlBatchDTO).Result;
                        if (batchId > 0)
                        {
                            _etlBatchDTO.Id = batchId;
                            for (int _counter = 0; _counter < totalRows; _counter++)
                            {
                                //DateTime dateValue = DateTime.Now;
                                

								DateTime dateValue = new DateTime();
                                bool isValidDate = false;
                                try
                                {
                                    dateValue = Convert.ToDateTime(_custExcelDataList[_counter].DOB);
                                    isValidDate = true;
                                }
                                catch
                                {

                                }
                                //                       try
                                //                       {
                                //if(DateTime.TryParseExact(_custExcelDataList[_counter].DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                                //                           {


                                //}

                                //                           isValidDate = true;
                                //                       }
                                //                       catch
                                //                       {

                                //                       }


                                if (!_custExcelDataList[_counter].CustomerID.Any(ch => !Char.IsLetterOrDigit(ch)) && !String.IsNullOrEmpty(_custExcelDataList[_counter].LastName) && !String.IsNullOrWhiteSpace(_custExcelDataList[_counter].LastName)
                                    //&& countries.Contains(_custExcelDataList[_counter].Nationality) && isValidDate
                                    )
                                {
                                    CustomerMasterDTO custDetails = new CustomerMasterDTO()
                                    {
                                        CustomerId = _custExcelDataList[_counter].CustomerID,
                                        FirstName = _custExcelDataList[_counter].Firstname,
                                        MiddleName = _custExcelDataList[_counter].MiddleName,
                                        LastName = _custExcelDataList[_counter].LastName,
                                        Nationality = _custExcelDataList[_counter].Nationality,
                                        //CustDOB = dateValue.ToString(),
                                        CreatedBy = _documentsModel.AddedBy,
                                        Batch = batchId,
                                        DOB= dateValue,
                                        //CustomerType = "I"
                                        CustomerType = typeId,
                                        ClientId = _documentsModel.ClientId,
                                        Type="Individual",
                                        OccupatinTypeTxt= _custExcelDataList[_counter].OccupatinTypeTxt,
                                        ProductName= _custExcelDataList[_counter].ProductName,
                                        Modeofpayment= _custExcelDataList[_counter].Modeofpayment,
                                        ResidenceStatus= _custExcelDataList[_counter].ResidenceStatus,
                                        DeliveryChannelName= _custExcelDataList[_counter].DeliveryChannelName,
                                        ScreeningOptions=screeningoption
                                        //CustomerIdType = _custExcelDataList[_counter].CustomerIdType,
                                        //CustomerIdNumber = _custExcelDataList[_counter].CustomerIdNumber

                                    };

                                    ServiceResponse<string> _res = _customerMasterRepository.Create(custDetails);
                                    string Case_Id = "";
                                    string Cust_Id = "";
                                    if (_res.Result.ToString() != "0")
                                    {
                                        var resu = _res.Result.Split('Ø');
                                        Case_Id = resu[0];
                                        Cust_Id = resu[1];
                                    }
                                    else
                                    {
                                        var resu = _res.Result;
                                        Cust_Id = resu;
                                    }
                                    int CaseId = Regex.Replace(Case_Id, "[^0-9]", "").ParseInt();
                                    if (_res.Status == StaticResource.SuccessStatusCode)
                                    {
                                        CustomerCaseDTO _case = new CustomerCaseDTO()
                                        {

                                            CustomerMasterId = CaseId,
                                            CustomerId = Cust_Id,
                                            CreatedBy = _documentsModel.AddedBy,
                                            ClientId = _documentsModel.ClientId,
                                            Threshold = Threshold,
                                            C6Threshold = C6Threshold
                                        };
                                        var res = _CustomerCaseRepository.Create(_case);
                                    }
                                    if (_res.Status == StaticResource.SuccessStatusCode)
                                    {
                                        _custExcelDataList[_counter].Id = CaseId; //_res.Result;
                                        _custExcelDataList[_counter].CustomerID =  Cust_Id;
                                        _custExcelDataList[_counter].Status = 1;
                                        _processedCount++;
                                    }
                                    else
                                    {
                                        _custExcelDataList[_counter].Id = CaseId;
                                        _custExcelDataList[_counter].CustomerID = Cust_Id;
                                        _custExcelDataList[_counter].Status = 3;
                                    }

                                    int CustomerCaseId = GetCaseId(Cust_Id);
                                    CaseCommentModel remarkModel = new CaseCommentModel();
                                    remarkModel.CaseId = CustomerCaseId;
                                    remarkModel.Comment = "Case is Created"; // ✅ FIXED
                                    remarkModel.CommentType = "Individual Bulk Screening";
                                    remarkModel.CreatedBy = _documentsModel.AddedBy;

                                    var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
                                }

                                
                            }
                            
                            _etlBatchDTO.RowsRecorded = _processedCount;
                            _etlLogRepository.Update(_etlBatchDTO);
                            _custExcelDataResponse.Status = StaticResource.SuccessStatusCode;
                            _custExcelDataResponse.Message = "Excel data saved successfully. Saved : " + _processedCount.ToString() + " Out of : " + totalRows.ToString();
                            _custExcelDataResponse.Result = _custExcelDataList;
                        }
                        else
                        {
                            _custExcelDataResponse.Status = StaticResource.FailStatusCode;
                            _custExcelDataResponse.Message = "Unable to create the batch details";
                        }
                    }
                    else
                    {
                        _custExcelDataResponse.Status = StaticResource.FailStatusCode;
                        _custExcelDataResponse.Message = "No valid records found";
                    }
                }
            }
            return _custExcelDataResponse;
        }
        public ServiceResponse<List<CorporateExcelDTO>> SaveCorporateCaseExcelData(DocumentsDTO _documentsModel, string typeId, int C6Threshold, int Threshold,string CompanyCode,string ScreeningOption)
        {

            // ServiceResponse<List<CustomerExcelDTO>> _custExcelDataResponse = _CustomerCaseRepository.LoadCustomerCaseExcelData(_documentsModel.DocFullPath, 1);

            ServiceResponse<List<CorporateExcelDTO>> _corpExcelDataResponse = _CustomerCaseRepository.LoadCorporateCaseExcelData(_documentsModel.DocFullPath, 1);
            if (_corpExcelDataResponse.Result.Count > 50)
            {
                _corpExcelDataResponse.Status = StaticResource.FailStatusCode;
                _corpExcelDataResponse.Message = "The number of customer data per file should not exceed 50.";
            }
            else
            {
                if (_corpExcelDataResponse.Status == StaticResource.SuccessStatusCode)
                {
                    List<CorporateExcelDTO> _custExcelDataList = _corpExcelDataResponse.Result;
                    int totalRows = _custExcelDataList.Count;
                    int _processedCount = 0;
                    var countries = _countryRepository.GetAll(_documentsModel.ClientId).Result.Select(_ => _.Name).ToList();
                    string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                     "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                     "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                     "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                     "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm"};

                    if (totalRows > 0)
                    {
                        EtlBatchDTO _etlBatchDTO = new EtlBatchDTO()
                        {
                            FileName = _documentsModel.DocName,
                            FileFullPath = _documentsModel.DocFullPath,
                            TotalRows = totalRows,
                            RowsRecorded = 0,
                            AddedBy = _documentsModel.AddedBy,
                            Type = (int)LogModulle.Customer,
                            ClientId = _documentsModel.ClientId
                        };
                        int batchId = _etlLogRepository.Create(_etlBatchDTO).Result;
                        if (batchId > 0)
                        {
                            _etlBatchDTO.Id = batchId;
                            for (int _counter = 0; _counter < totalRows; _counter++)
                            {
                                //DateTime dateValue = DateTime.Now;
                                DateTime dateValue = new DateTime();
                                bool isValidDate = false;
                                try
                                {
                                    dateValue = Convert.ToDateTime(_custExcelDataList[_counter].DateofIncorporation);
                                    isValidDate = true;
                                }
                                catch
                                {

                                }


                                //if (!_custExcelDataList[_counter].CustomerID.Any(ch => !Char.IsLetterOrDigit(ch)) && !String.IsNullOrEmpty(_custExcelDataList[_counter].Firstname) && !String.IsNullOrWhiteSpace(_custExcelDataList[_counter].Firstname)
                                //    && countries.Contains(_custExcelDataList[_counter].Nationality) && isValidDate
                                //    )
                                if (!_custExcelDataList[_counter].CustomerID.Any(ch => !Char.IsLetterOrDigit(ch)) && !String.IsNullOrEmpty(_custExcelDataList[_counter].EntityName) && !String.IsNullOrWhiteSpace(_custExcelDataList[_counter].EntityName)

                                    )
                                {
                                    CustomerMasterDTO custDetails = new CustomerMasterDTO()
                                    {
                                        CustomerId = _custExcelDataList[_counter].CustomerID,
                                        FirstName = "",
                                        MiddleName = "",
                                        LastName = _custExcelDataList[_counter].EntityName,
                                        Nationality = _custExcelDataList[_counter].CountryofIncorporation,
                                        CustDOB = dateValue.ToString(),
                                        CreatedBy = _documentsModel.AddedBy,
                                        Batch = batchId,
                                        //CustomerType = "I"
                                        CustomerType = typeId,
                                        ClientId = _documentsModel.ClientId,
                                        CompanyCode = CompanyCode,
                                        Type = "Corporate",
                                        EntityTypeTxt = _custExcelDataList[_counter].EntityTypeTxt,
                                        ProductName = _custExcelDataList[_counter].ProductName,
                                        Modeofpayment = _custExcelDataList[_counter].Modeofpayment,
                                        BusinessType = _custExcelDataList[_counter].BusinessType,
                                        DeliveryChannelName = _custExcelDataList[_counter].DeliveryChannelName
                                    };

                                    ServiceResponse<string> _res = _customerMasterRepository.Create(custDetails);
                                    //var resu = _res.Result.Split('Ø');
                                    var resu = _res.Result;
                                    var resu1 = "";
                                    if (_res.Result.ToString() == "0")
                                    {
                                        resu = _res.Result;
                                        _custExcelDataList[_counter].Id = resu.ParseInt();
                                        _custExcelDataList[_counter].Status = 3;
                                    }
                                    else
                                    {
                                        resu = _res.Result.Split('Ø')[0];
                                        resu1 = _res.Result.Split('Ø')[1];
                                        if (_res.Status == StaticResource.SuccessStatusCode)
                                        {
                                            int CustId = resu.ParseInt();
                                            string customerid = resu1;
                                            CustomerCaseDTO _case = new CustomerCaseDTO()
                                            {
                                                CustomerMasterId = CustId,
                                                CustomerId = customerid,
                                                CreatedBy = _documentsModel.AddedBy,
                                                C6Threshold = C6Threshold,
                                                Threshold = Threshold,
                                                ClientId = _documentsModel.ClientId
                                            };
                                            var res = _CustomerCaseRepository.Create(_case);
                                        }
                                        if (_res.Status == StaticResource.SuccessStatusCode)
                                        {
                                            _custExcelDataList[_counter].Id = resu.ParseInt();
                                            _custExcelDataList[_counter].CustomerID = resu1;
                                            _custExcelDataList[_counter].Status = 1;
                                            _processedCount++;
                                        }
                                        else
                                        {
                                            _custExcelDataList[_counter].Id = resu.ParseInt();
                                            _custExcelDataList[_counter].CustomerID = resu1;
                                            _custExcelDataList[_counter].Status = 3;
                                        }

                                        int CustomerCaseId = GetCaseId(resu1);
                                        CaseCommentModel remarkModel = new CaseCommentModel();
                                        remarkModel.CaseId = CustomerCaseId;
                                        remarkModel.Comment = "Case is Created"; // ✅ FIXED
                                        remarkModel.CommentType = "Corporate Bulk Screening";
                                        remarkModel.CreatedBy = _documentsModel.AddedBy;

                                        var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));

                                    }
                                }
                            }
                            _etlBatchDTO.RowsRecorded = _processedCount;
                            _etlLogRepository.Update(_etlBatchDTO);
                            _corpExcelDataResponse.Status = StaticResource.SuccessStatusCode;
                            _corpExcelDataResponse.Message = "Excel data saved successfully. Saved : " + _processedCount.ToString() + " Out of : " + totalRows.ToString();
                            _corpExcelDataResponse.Result = _custExcelDataList;
                        }
                        else
                        {
                            _corpExcelDataResponse.Status = StaticResource.FailStatusCode;
                            _corpExcelDataResponse.Message = "Unable to create the batch details";
                        }
                    }
                    else
                    {
                        _corpExcelDataResponse.Status = StaticResource.FailStatusCode;
                        _corpExcelDataResponse.Message = "No valid records found";
                    }
                }
            }
            return _corpExcelDataResponse;
        }
        public ServiceResponse<List<EtlBatchDTO>> DataLoadReport(ETLDataLoadReportDTO _ETLDataLoadReportDTO)
        {
            return _CustomerCaseRepository.DataLoadReport(_ETLDataLoadReportDTO);
        }

        public ServiceResponse<List<CustomerCaseDTO>> DataLoadReportByBatch(int BatchId, int clientId)
        {
            return _CustomerCaseRepository.DataLoadReportByBatch(BatchId, clientId);

        }

        public (List<string>, CustomerCaseDTO) SaveCorporateScreeningDetails(CorporateScreeningDTO model)
        {
            var responseList = new List<string>();
            CustomerCaseDTO _CustomerCaseDT = new CustomerCaseDTO()
            {
                CustomerId = model.CustomerId,
                Nationality = model.Nationality,
                //CustDOB=model.AccomplishedDate.ToDateFormat(),
                DOB = model.DOB != null ? DateTime.Parse(model.DOB) : DateTime.MinValue,
                FirstName = model.FirstName,
                CustomerIdType = "License No",
                CustomerIdNumber = model.Id.ToString(),
                Mobile = model.Mobile,
                CustomerType = "C",
                CreatedBy = model.CreatedBy,
                MatchCategory = "CORPORATE",
                ClientId = model.ClientId,
                C6Threshold = model.C6Threshold,
                Threshold = model.Threshold,
                IsDd = model.IsDd,
                IsIns = model.IsIns,
                IsPep = model.IsPep,
                IsPoi = model.IsPoi,
                IsRel = model.IsRel,
                IsRre = model.IsRre,
                IsSan = model.IsSan,
                CompanyCode = model.CompanyCode,
                Tradelicense = model.Tradelicense,
                customerCodeprefix = model.customerCodeprefix,
                Type=model.Type,
                BusinessType=model.BusinessType,
                EntityTypeTxt=model.EntityTypeTxt,
                DeliveryChannelName=model.DeliveryChannelName,
                ProductName=model.ProductName,
                Modeofpayment=model.Modeofpayment,
                CIFNumber=model.CIFNumber,
                ScreeningOptions=model.ScreeningOptions,
                IdExpiryDate=model.IdExpiryDate,
                Residence=model.Residence,
                TradeLicenseAuthority=model.TradeLicenseAuthority,
                TradeLicenseSector=model.TradeLicenseSector,
                ProductRefNo=model.ProductRefNo,
                ProductValue=model.ProductValue,
                CounterParty=model.CounterParty,
                CounterPartyName=model.CounterPartyName
                
            };
            //ServiceResponse<string> _corpCustomer = this.CreatePrefix(_CustomerCaseDT);
            ServiceResponse<string> _corpCustomer = this.Create(_CustomerCaseDT);
            if (_corpCustomer.Status == StaticResource.SuccessStatusCode)
            {
                string _corpCust = _corpCustomer.Result.Split('Ø')[1];
                //string _corpCust = _corpCustomer.Result;
                model.CustomerId = _corpCust;
                _CustomerCaseDT.CustomerId = _corpCust;   
                responseList.Add(_corpCust);
                //_corporateShareholderRepository.DeleteByCorporateID(_corpCust);
                //int _counter = 1;
                //foreach (var customer in model.CustomerDetailList.Where(_ => _.isDeleted == false))
                //{
                //string _shCode = _counter.ToString().PadLeft(3, '0');
                //var result = this.Create(new CustomerCaseDTO
                //{
                //    CustomerId = model.CustomerCode + _shCode,
                //    MatchCategory = "INDIVIDUAL",
                //    FirstName = customer.FirstName,
                //    MiddleName = customer.MiddleName,
                //    LastName = customer.LastName,
                //    CustomerType = "I",
                //    //CustomerType = "S",
                //    Status = '0',
                //    CreatedBy = model.CreatedBy,
                //    /*-------------------------------HARDCODED--- PLEASE FIX IT------------------------------------------*/
                //    Nationality = "UNICEF",
                //    CustDOB = "1970-01-01"
                //    /*-----------------------------------------------------------------------------------------------------*/

                //}); ;
                //_counter++;
                //if (result.Status == StaticResource.SuccessStatusCode)
                //{
                //    int shId = result.Result;
                //    _corporateShareholderRepository.Create(
                //        new CorporateShareholderDTO()
                //        {
                //            CorporateID = _corpCust,
                //            ShareholderID = shId,
                //            SharePercentage= customer.SharePercent,
                //            CustIDGen= model.CustomerCode + "#" + _shCode,
                //            Status=1,
                //            CreatedBy = model.CreatedBy
                //        });
                //    responseList.Add(shId);
                //}

                //}

            }
            return (responseList, _CustomerCaseDT);
        }









        public (List<Tuple<string, string>>, CustomerCaseDTO) SaveCorporateScreening(CorporateScreeningDTO model)
        {
            var responseList = new List<string>();
            
            var responseLists = new List<Tuple<string, string>>();
            CustomerCaseDTO _CustomerCaseDT = new CustomerCaseDTO()
            {

                CustomerId = model.CustomerCode,
                Nationality = model.AccomplishedCountry,
                //CustDOB=model.AccomplishedDate.ToDateFormat(),
                DOB = model.AccomplishedDate,
                FirstName = model.CompanyName == null ? model.FirstName : model.CompanyName,
                //LastName =model.FirstName,
                CustomerIdType = "License No",
                CustomerIdNumber = model.LicenseNumber,
                Mobile = model.MobileNo,
                CustomerType = "C",
                CreatedBy = model.CreatedBy,
                MatchCategory = "CORPORATE",
                //CustomerId = model.CustomerId,
                //Nationality = model.Nationality,
                ////CustDOB=model.AccomplishedDate.ToDateFormat(),
                //DOB = model.DOB != null ? DateTime.Parse(model.DOB) : DateTime.MinValue,
                //FirstName = model.FirstName,
                //CustomerIdType = "License No",
                //CustomerIdNumber = model.Id.ToString(),
                //Mobile = model.Mobile,
                //CustomerType = "C",
                //CreatedBy = model.CreatedBy,
                //MatchCategory = "CORPORATE",
                ClientId = model.ClientId,
                C6Threshold = model.C6Threshold,
                Threshold = model.Threshold,
                //CorporateType = model.CorporateType,
                IsDd = model.IsDd,
                IsIns = model.IsIns,
                IsPep = model.IsPep,
                IsPoi = model.IsPoi,
                IsRel = model.IsRel,
                IsRre = model.IsRre,
                IsSan = model.IsSan,
                customerCodeprefix=model.customerCodeprefix


            };
            ServiceResponse<string> _corpCustomer = new ServiceResponse<string>();
            if (model.CorporateType == "Group Entity")
            {
                _corpCustomer = this.CreateGroupEntity(_CustomerCaseDT);
            }
            else
                _corpCustomer = this.CreatePrefix(_CustomerCaseDT);



            if (_corpCustomer.Status == StaticResource.SuccessStatusCode)
            {
                string _corpCust = _corpCustomer.Result;
                model.CustomerId = _corpCust;
                responseList.Add(_corpCust);
                responseLists.Add(Tuple.Create(_corpCust, " "));
                _corporateShareholderRepository.DeleteByCorporateID(_corpCust);


                foreach (var customer in model.CustomerDetailList.Where(_ => _.isDeleted == false))
                {
                    model.CustomerCode = _corpCust;
                    if (customer.FirstName == null && customer.LastName == null && customer.MiddleName == null)
                    {

                    }
                    else
                    {
                        var res = _customerMasterRepository.CreateCustomerMasterShareholder(
                             new CustomerMasterDTO
                             {
                                 CustomerReferenceID = model.CustomerCode,
                                 FirstName = customer.FirstName,
                                 MiddleName = customer.MiddleName,
                                 LastName = customer.LastName,
                                 //       DOB = DateTime.Now,
                                 Nationality = customer.Nationality,
                                 CustomerType = "I",
                                 CustomerIdType = customer.CustomerIdType,
                                 CustomerIdNumber = customer.CustomerIdNumber,
                                 Mobile = customer.Mobile,
                                 CreatedBy = model.CreatedBy,
                                 Threshold = customer.Threshold,
                                 C6Threshold = customer.C6Threshold,
                                 ClientId = model.ClientId
                             }, _corpCust, Convert.ToInt32(customer.SharePercent),
                             customer.Designation,
                             customer.EmiratesIdNumber,
                             customer.EmiratesIdExpiry,
                             customer.CustomerIdExpiry,
                             customer.CustomerType
                        );

                        string _cust = res.Result;
                        string customertype = customer.CustomerType;
                        responseList.Add(_cust);
                        responseLists.Add(Tuple.Create(_cust, customertype));
                        

                    }
                }


                //int _counter = 1;
                //foreach (var customer in model.CustomerDetailList.Where(_ => _.isDeleted == false))
                //{
                //string _shCode = _counter.ToString().PadLeft(3, '0');
                //var result = this.Create(new CustomerCaseDTO
                //{
                //    CustomerId = model.CustomerCode + _shCode,
                //    MatchCategory = "INDIVIDUAL",
                //    FirstName = customer.FirstName,
                //    MiddleName = customer.MiddleName,
                //    LastName = customer.LastName,
                //    CustomerType = "I",
                //    //CustomerType = "S",
                //    Status = '0',
                //    CreatedBy = model.CreatedBy,
                //    /*-------------------------------HARDCODED--- PLEASE FIX IT------------------------------------------*/
                //    Nationality = "UNICEF",
                //    CustDOB = "1970-01-01"
                //    /*-----------------------------------------------------------------------------------------------------*/

                //}); ;
                //_counter++;
                //if (result.Status == StaticResource.SuccessStatusCode)
                //{
                //    int shId = result.Result;
                //    _corporateShareholderRepository.Create(
                //        new CorporateShareholderDTO()
                //        {
                //            CorporateID = _corpCust,
                //            ShareholderID = shId,
                //            SharePercentage= customer.SharePercent,
                //            CustIDGen= model.CustomerCode + "#" + _shCode,
                //            Status=1,
                //            CreatedBy = model.CreatedBy
                //        });
                //    responseList.Add(shId);
                //}

                //}

            }
            return (responseLists, _CustomerCaseDT);
        }


        public List<CustomerMasterDTO> GetCustomerMasterByCodePrefix(string prefix)
        {
            return _customerMasterRepository.GetCustomerMasterByCodePrefix(prefix).Result;
        }

        public List<CustomerMasterDTO> GetCustomerMasterByCode(string customerType, int clientId)
        {
            return _customerMasterRepository.GetCustomerMasterByCode(customerType, clientId).Result;
        }

        public List<CustomerMasterDTO> GetCustomerMasterByCodePrefixAndCode(string prefix, string code)
        {
            return _customerMasterRepository.GetCustomerMasterByCodePrefixAndCode(prefix, code).Result;
        }

        public int InsertDigiSchedulerLogs(int totalHits, int totalRecords, int clientId)
        {
            return _CustomerCaseRepository.InsertDigiSchedulerLogs(totalHits, totalRecords, clientId);
        }

        public List<string> GroupEntityScreeningDetails(CorporateScreeningDTO model)
        {
            var responseList = new List<string>();

            if (model.FirstName != " " || model.FirstName != null)
            {
                responseList = new List<string>();
                CustomerCaseDTO _CustomerCaseDT = new CustomerCaseDTO()
                {
                    CustomerId = model.CustomerId,
                    Nationality = model.Nationality,
                    //CustDOB=model.AccomplishedDate.ToDateFormat(),
                    DOB = DateTime.Parse(model.DOB ?? DateTime.Now.ToString()),
                    FirstName = model.CompanyName,
                    CustomerIdType = "License No",
                    CustomerIdNumber = model.Id.ToString(),
                    Mobile = model.Mobile,
                    CustomerType = "C",
                    CreatedBy = model.CreatedBy,
                    MatchCategory = "CORPORATE",
                    C6Threshold = model.C6Threshold,
                    Threshold = model.Threshold,
                    CorporateType = model.CorporateType,
                    GroupEntityof = model.GroupEntityOf,
                    ClientId = model.ClientId,
                    customerCodeprefix=model.customerCodeprefix
                };

                ServiceResponse<string> _corpCustomer = new ServiceResponse<string>();

                if (model.CorporateType == "Group Entity")
                {
                    _corpCustomer = this.CreateGroupEntity(_CustomerCaseDT);
                }

                responseList.Add(_corpCustomer.Result);

            }
            return responseList;
        }

        public ServiceResponse<string> CreateGroupEntity(CustomerCaseDTO _CustomerCaseDT)
        {
            CustomerMasterDTO custDetails = new CustomerMasterDTO()
            {
                CustomerId = _CustomerCaseDT.CustomerId,
                FirstName = _CustomerCaseDT.FirstName,
                MiddleName = _CustomerCaseDT.MiddleName,
                LastName = _CustomerCaseDT.LastName,
                Nationality = _CustomerCaseDT.Nationality,
                //CustDOB = _CustomerCaseDT.CustDOB,
                DOB = _CustomerCaseDT.DOB.ToString("MM-dd-yyyy").ParseDB().GetValueOrDefault(),
                CustomerIdType = _CustomerCaseDT.CustomerIdType,
                CustomerIdNumber = _CustomerCaseDT.CustomerIdNumber,
                Mobile = _CustomerCaseDT.Mobile,
                CreatedBy = _CustomerCaseDT.CreatedBy,
                CustomerType = _CustomerCaseDT.CustomerType,
                GroupEntityof = _CustomerCaseDT.GroupEntityof,
                C6Threshold = _CustomerCaseDT.C6Threshold,
                Threshold = _CustomerCaseDT.Threshold,
                ClientId = _CustomerCaseDT.ClientId,
                customerCodeprefix=_CustomerCaseDT.customerCodeprefix
            };

            ServiceResponse<string> _res = _customerMasterRepository.CreateGroupEntity(custDetails, _CustomerCaseDT.CorporateType);

            string CustId = _res.Result;

            Console.WriteLine($"Created group entity with customer id: {CustId}");

            if (CustId.IsNotNullOrEmpty())
            {
                _CustomerCaseDT.CustomerMasterId = Regex.Replace(CustId, "[^0-9]", "").ParseInt();
                //  _CustomerCaseDT.CustomerMasterId= CustId.Substring(3).ParseInt();

                _CustomerCaseDT.CustomerId = CustId;//this value will be appended with prefix to get the original customer ID
                int caseNo = _CustomerCaseRepository.Create(_CustomerCaseDT).Result;

                Console.WriteLine($"Created case ({caseNo}) for group entity with customer id: {CustId}");
            }

            return _res;
        }
        //Individual Risk Bulk Upload

        public Tuple<ServiceResponse<List<IndividualExcel>>, RiskAPIRequestModel> SaveIndividualRiskExcelData(DocumentsDTO _documentsModel, string typeId, int C6Threshold, int Threshold)
        {
            RiskAPIRequestModel riskModel = new RiskAPIRequestModel();
           // List<RiskAPIRequestModel> riskModel = RiskAPIRequestModel.Result;
            ServiceResponse<List<IndividualExcel>> _custExcelDataResponse = _CustomerCaseRepository.LoadIndividualRiskExcelData(_documentsModel.DocFullPath, 1);
            if (_custExcelDataResponse.Result.Count > 50)
            {
                _custExcelDataResponse.Status = StaticResource.FailStatusCode;
                _custExcelDataResponse.Message = "The number of customer data per file should not exceed 50.";
            }
            else
            {
                if (_custExcelDataResponse.Status == StaticResource.SuccessStatusCode)
                {
                    List<IndividualExcel> _custExcelDataList = _custExcelDataResponse.Result;
                    int totalRows = _custExcelDataList.Count;
                    int _processedCount = 0;
                    var countries = _countryRepository.GetAll(_documentsModel.ClientId).Result.Select(_ => _.Name).ToList();
                    string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                     "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                     "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                     "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                     "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm"};

                    if (totalRows > 0)
                    {
                        EtlBatchDTO _etlBatchDTO = new EtlBatchDTO()
                        {
                            FileName = _documentsModel.DocName,
                            FileFullPath = _documentsModel.DocFullPath,
                            TotalRows = totalRows,
                            RowsRecorded = 0,
                            AddedBy = _documentsModel.AddedBy,
                            Type = (int)LogModulle.Customer,
                            ClientId = _documentsModel.ClientId
                        };
                        int batchId = _etlLogRepository.Create(_etlBatchDTO).Result;
                        if (batchId > 0)
                        {
                            _etlBatchDTO.Id = batchId;
                            for (int _counter = 0; _counter < totalRows; _counter++)
                            {
                                //DateTime dateValue = DateTime.Now;
                                DateTime dateValue = new DateTime();
                                bool isValidDate = false;
                                try
                                {
                                    dateValue = Convert.ToDateTime(_custExcelDataList[_counter].DOB);
                                    isValidDate = true;
                                }
                                catch
                                {

                                }

                                if (!_custExcelDataList[_counter].CustomerId.Any(ch => !Char.IsLetterOrDigit(ch)) && !String.IsNullOrEmpty(_custExcelDataList[_counter].CustomerName) && !String.IsNullOrWhiteSpace(_custExcelDataList[_counter].CustomerName))

                                {
                                    RiskBulkDTO custDetails = new RiskBulkDTO()
                                    {
                                        CustomerCode = _custExcelDataList[_counter].CustomerId,
                                        CustomerName = _custExcelDataList[_counter].CustomerName,
                                        Nationality = _custExcelDataList[_counter].Nationality,
                                        CustomerType = _custExcelDataList[_counter].CustomerType,
                                        Profession = _custExcelDataList[_counter].Profession,
                                        ResidenceType = _custExcelDataList[_counter].ResidenceStatus,
                                        Product = _custExcelDataList[_counter].Product,
                                        DeliveryChannel = _custExcelDataList[_counter].DeliveryChannel,
                                        ModeOfPayment = _custExcelDataList[_counter].ModeOfPayment,
                                        Screened = _custExcelDataList[_counter].Screened,
                                        MoreProduct=_custExcelDataList[_counter].MoreProduct,
                                        IsAdverse=_custExcelDataList[_counter].IsAdverse,
                                        CreatedBy = _documentsModel.AddedBy,
                                        ClientId = _documentsModel.ClientId
                                    };
                                    if (custDetails.CustomerName == "" || custDetails.CustomerName == null)
                                    {
                                        Console.WriteLine("Customer Name Is Empty.");
                                        continue;
                                    }
                                    ServiceResponse<string> _res = _customerMasterRepository.GetCustomerId(custDetails.CustomerCode);
                                    if (_res.Result == "")
                                    {
                                        Console.WriteLine("Customer Is Not Screened.");
                                        continue;
                                    }

                                    else
                                    {

                                    ServiceResponse<string> str1 = _customerMasterRepository.GetRisklovId(custDetails);
                                        if (str1.Result == null || str1.Result == "")
                                        {

                                            Console.WriteLine("Unable to calculate risk due to insufficient data.");
                                            _custExcelDataList[_counter].risk = "No";
                                            continue;

                                        }
                                        else
                                        {
                                            var _spStr = str1.Result.Split('Ø');
                                            var proflovId = _spStr[0];
                                            var natlovId = _spStr[1];
                                            var reslovId = _spStr[2];
                                            var prodlovId = _spStr[3];
                                            var dellovId = _spStr[4];
                                            var modlovId = _spStr[5];
                                            var moreprodlovId= _spStr[6];
                                            var screlovId= _spStr[7];
                                            var adverselovId = _spStr[8];
                                            ServiceResponse<string> str = _customerMasterRepository.GetRiskTypeId(custDetails);
                                            if (str.Result == null || str.Result == "")
                                            {

                                                Console.WriteLine("Unable to calculate risk due to insufficient data.");
                                                return new Tuple<ServiceResponse<List<IndividualExcel>>, RiskAPIRequestModel>(_custExcelDataResponse, riskModel);

                                            }
                                            else
                                            {
                                                var spStr = str.Result.Split('Ø');
                                                var profId = spStr[0];
                                                var natId = spStr[1];
                                                var resId = spStr[2];
                                                var prodId = spStr[3];
                                                var delId = spStr[4];
                                                var modId = spStr[5];
                                                var moreprodId = spStr[6];
                                                var screId = spStr[7];
                                                var adverseId = spStr[8];
                                                //RiskAPIRequestModel riskModel = new RiskAPIRequestModel();
                                                riskModel.CustomerId = custDetails.CustomerCode;
                                                riskModel.CustomerName = custDetails.CustomerName;
                                                riskModel.ClientId = custDetails.ClientId;
                                                riskModel.CreatedBy = custDetails.CreatedBy;
                                                riskModel.RiskCategory = "I";
                                                var riskTypeList = new List<RiskTypeListModel>();
                                                if (profId != "0")
                                                {
                                                    var riskType1 = new RiskTypeListModel();
                                                    riskType1.Id = Convert.ToString(proflovId);
                                                    var riskItem1 = new RiskItemListModel();
                                                    riskItem1.Id = profId.ToString();
                                                    var riskItemList1 = new List<RiskItemListModel>();
                                                    riskItemList1.Add(riskItem1);
                                                    riskType1.RiskItemList = riskItemList1;
                                                    riskTypeList.Add(riskType1);
                                                }
                                                if (resId != "0")
                                                {
                                                    var riskType2 = new RiskTypeListModel();
                                                    riskType2.Id = Convert.ToString(reslovId);
                                                    var riskItem2 = new RiskItemListModel();
                                                    riskItem2.Id = resId.ToString();
                                                    var riskItemList2 = new List<RiskItemListModel>();
                                                    riskItemList2.Add(riskItem2);
                                                    riskType2.RiskItemList = riskItemList2;
                                                    riskTypeList.Add(riskType2);
                                                }
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

                                                if (prodId != "0")
                                                {
                                                    var riskType4 = new RiskTypeListModel();
                                                    riskType4.Id = Convert.ToString(prodlovId);
                                                    var riskItem4 = new RiskItemListModel();
                                                    riskItem4.Id = prodId.ToString();
                                                    var riskItemList4 = new List<RiskItemListModel>();
                                                    riskItemList4.Add(riskItem4);
                                                    riskType4.RiskItemList = riskItemList4;
                                                    riskTypeList.Add(riskType4);
                                                }
                                                if (moreprodId != "0")
                                                {
                                                    var riskType5 = new RiskTypeListModel();
                                                    riskType5.Id = Convert.ToString(moreprodlovId);
                                                    var riskItem5 = new RiskItemListModel();
                                                    riskItem5.Id = moreprodId.ToString();
                                                    var riskItemList5 = new List<RiskItemListModel>();
                                                    riskItemList5.Add(riskItem5);
                                                    riskType5.RiskItemList = riskItemList5;
                                                    riskTypeList.Add(riskType5);
                                                }

                                                if (delId != "0")
                                                {
                                                    var riskType6 = new RiskTypeListModel();
                                                    riskType6.Id = Convert.ToString(dellovId);
                                                    var riskItem6 = new RiskItemListModel();
                                                    riskItem6.Id = delId.ToString();
                                                    var riskItemList6 = new List<RiskItemListModel>();
                                                    riskItemList6.Add(riskItem6);
                                                    riskType6.RiskItemList = riskItemList6;
                                                    riskTypeList.Add(riskType6);
                                                }
                                                if (screId != "0")
                                                {
                                                    var riskType7 = new RiskTypeListModel();
                                                    riskType7.Id = Convert.ToString(screlovId);
                                                    var riskItem7 = new RiskItemListModel();
                                                    riskItem7.Id = screId.ToString();
                                                    var riskItemList7 = new List<RiskItemListModel>();
                                                    riskItemList7.Add(riskItem7);
                                                    riskType7.RiskItemList = riskItemList7;
                                                    riskTypeList.Add(riskType7);
                                                }
                                                if (adverseId != "0")
                                                {
                                                    var riskType8 = new RiskTypeListModel();
                                                    riskType8.Id = Convert.ToString(adverselovId);
                                                    var riskItem8 = new RiskItemListModel();
                                                    riskItem8.Id = adverseId.ToString();
                                                    var riskItemList8 = new List<RiskItemListModel>();
                                                    riskItemList8.Add(riskItem8);
                                                    riskType8.RiskItemList = riskItemList8;
                                                    riskTypeList.Add(riskType8);
                                                }
                                                if (modId != "0")
                                                {
                                                    var riskType9 = new RiskTypeListModel();
                                                    riskType9.Id = Convert.ToString(modlovId);
                                                    var riskItem9 = new RiskItemListModel();
                                                    riskItem9.Id = modId.ToString();
                                                    var riskItemList9 = new List<RiskItemListModel>();
                                                    riskItemList9.Add(riskItem9);
                                                    riskType9.RiskItemList = riskItemList9;
                                                    riskTypeList.Add(riskType9);
                                                }
                                                riskModel.RiskTypeList = riskTypeList;
                                                var riskResult = Assessment(riskModel);
                                            }
                                        }
                                    }
                                }
                            }
                           
                        }
                       
                    }
                   
                }
            }
            return new Tuple<ServiceResponse<List<IndividualExcel>>, RiskAPIRequestModel>(_custExcelDataResponse, riskModel);
        }

        //Corporate Risk Assessment

        public Tuple<ServiceResponse<List<CorporateExcel>>, RiskAPIRequestModel> SaveCorporateRiskExcelData(DocumentsDTO _documentsModel, string typeId, int C6Threshold, int Threshold)
        {
            RiskAPIRequestModel riskModel = new RiskAPIRequestModel();
            // List<RiskAPIRequestModel> riskModel = RiskAPIRequestModel.Result;
            ServiceResponse<List<CorporateExcel>> _custExcelDataResponse = _CustomerCaseRepository.LoadCorporateRiskExcelData(_documentsModel.DocFullPath, 1);
            if (_custExcelDataResponse.Result.Count > 50)
            {
                _custExcelDataResponse.Status = StaticResource.FailStatusCode;
                _custExcelDataResponse.Message = "The number of customer data per file should not exceed 50.";
            }
            else
            {
                if (_custExcelDataResponse.Status == StaticResource.SuccessStatusCode)
                {
                    List<CorporateExcel> _custExcelDataList = _custExcelDataResponse.Result;
                    int totalRows = _custExcelDataList.Count;
                    int _processedCount = 0;
                    var countries = _countryRepository.GetAll(_documentsModel.ClientId).Result.Select(_ => _.Name).ToList();
                    string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                     "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                     "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                     "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                     "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm"};

                    if (totalRows > 0)
                    {
                        EtlBatchDTO _etlBatchDTO = new EtlBatchDTO()
                        {
                            FileName = _documentsModel.DocName,
                            FileFullPath = _documentsModel.DocFullPath,
                            TotalRows = totalRows,
                            RowsRecorded = 0,
                            AddedBy = _documentsModel.AddedBy,
                            Type = (int)LogModulle.Customer,
                            ClientId = _documentsModel.ClientId
                        };
                        int batchId = _etlLogRepository.Create(_etlBatchDTO).Result;
                       
                            _etlBatchDTO.Id = batchId;
                            int _counter = 0;
                            for (_counter = 0; _counter < totalRows; _counter++)
                            {
                                
                                DateTime dateValue = new DateTime();
                                bool isValidDate = false;
                                
                                    RiskBulkDTO custDetails = new RiskBulkDTO()
                                    {
                                        UniqueID = _custExcelDataList[_counter].CustomerId,
                                        LegalNameOfEntity = _custExcelDataList[_counter].LegalNameOfEntity,
                                        CountryOfIncorporationTxt = _custExcelDataList[_counter].CountryOfIncorporationTxt,
                                        CustomerType = _custExcelDataList[_counter].CustomerType,
                                        Profession = _custExcelDataList[_counter].Profession,
                                        ResidenceType = _custExcelDataList[_counter].ResidenceStatus,
                                        Product = _custExcelDataList[_counter].Product,
                                        DeliveryChannel = _custExcelDataList[_counter].DeliveryChannel,
                                        SourceOfFund = _custExcelDataList[_counter].SourceOfFund,
                                        ModeOfPayment = _custExcelDataList[_counter].ModeOfPayment,
                                        Screened = _custExcelDataList[_counter].Screened,
                                        MoreProduct = _custExcelDataList[_counter].MoreProduct,
                                        IsAdverse = _custExcelDataList[_counter].IsAdverse,
                                        CreatedBy = _documentsModel.AddedBy,
                                        ClientId = _documentsModel.ClientId,
                                        FATF = _custExcelDataList[_counter].FATF,
                                        Nat1 = _custExcelDataList[_counter].Nat1,
                                        Nat2 = _custExcelDataList[_counter].Nat2,
                                        Nat3 = _custExcelDataList[_counter].Nat3

                                    };
                                    if (custDetails.LegalNameOfEntity=="" || custDetails.LegalNameOfEntity == null)
                                    {
                                        Console.WriteLine("Customer Name Is Empty.");
                                        continue;
                               
                                     }

                                    ServiceResponse<string> _res = _customerMasterRepository.GetCustomerId(custDetails.UniqueID);
                                    if(_res.Result=="")
                                    {
                                        Console.WriteLine("Customer Is Not Screened.");
                                       continue;
                               
                                    }
                                    else
                                    {
                                        ServiceResponse<string> str1 = _customerMasterRepository.GetRisklovId(custDetails);
                                        if (str1.Result == null || str1.Result=="")
                                        {
                                            
                                            Console.WriteLine("Unable to calculate risk due to insufficient data.");
                                            _custExcelDataList[_counter].risk = "No";
                                            continue;

                                        }
                                        else
                                        {
                                            var _spStr = str1.Result.Split('Ø');
                                            var proflovId = _spStr[0];
                                            var natlovId = _spStr[1];
                                            var reslovId = _spStr[2];
                                            var prodlovId = _spStr[3];
                                            var dellovId = _spStr[4];
                                            var modlovId = _spStr[5];
                                            var moreprodlovId = _spStr[6];
                                            var screlovId = _spStr[7];
                                            var adverselovId = _spStr[8];
                                            var FATFlovId = _spStr[9];
                                            var Nat1lovId = _spStr[10];
                                            var Nat2lovId = _spStr[11];
                                            var Nat3lovId = _spStr[12];

                                    ServiceResponse<string> str = _customerMasterRepository.GetRiskTypeId(custDetails);
                                            if (str.Result == null || str1.Result == "")
                                            {

                                                Console.WriteLine("Unable to calculate risk due to insufficient data.");
                                               _custExcelDataList[_counter].risk = "No";
                                                continue;

                                             }
                                            else
                                            {
                                                var spStr = str.Result.Split('Ø');
                                                var profId = spStr[0];
                                                var natId = spStr[1];
                                                var resId = spStr[2];
                                                var prodId = spStr[3];
                                                var delId = spStr[4];
                                                var modId = spStr[5];
                                                var moreprodId = spStr[6];
                                                var screId = spStr[7];
                                                var adverseId = spStr[8];
                                                var FATFId = spStr[9];
                                                var Nat1Id = spStr[10];
                                                var Nat2Id = spStr[11];
                                                var Nat3Id = spStr[12];

                                        //RiskAPIRequestModel riskModel = new RiskAPIRequestModel();
                                                riskModel.CustomerId = custDetails.UniqueID;
                                                riskModel.CustomerName = custDetails.LegalNameOfEntity;
                                                riskModel.CreatedBy = custDetails.CreatedBy;
                                                riskModel.ClientId = custDetails.ClientId;
                                                riskModel.RiskCategory = "C";
                                                var riskTypeList = new List<RiskTypeListModel>();
                                                if (profId != "0")
                                                {
                                                    var riskType1 = new RiskTypeListModel();
                                                    riskType1.Id = Convert.ToString(proflovId);
                                                    var riskItem1 = new RiskItemListModel();
                                                    riskItem1.Id = profId.ToString();
                                                    var riskItemList1 = new List<RiskItemListModel>();
                                                    riskItemList1.Add(riskItem1);
                                                    riskType1.RiskItemList = riskItemList1;
                                                    riskTypeList.Add(riskType1);
                                                }
                                                if (resId != "0")
                                                {
                                                    var riskType2 = new RiskTypeListModel();
                                                    riskType2.Id = Convert.ToString(reslovId);
                                                    var riskItem2 = new RiskItemListModel();
                                                    riskItem2.Id = resId.ToString();
                                                    var riskItemList2 = new List<RiskItemListModel>();
                                                    riskItemList2.Add(riskItem2);
                                                    riskType2.RiskItemList = riskItemList2;
                                                    riskTypeList.Add(riskType2);
                                                }
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
                                        if (Nat1Id != "0")
                                        {
                                            var riskType13 = new RiskTypeListModel();
                                            riskType13.Id = Convert.ToString(Nat1lovId);
                                            var riskItem13 = new RiskItemListModel();
                                            riskItem13.Id = Nat1Id.ToString();
                                            var riskItemList13 = new List<RiskItemListModel>();
                                            riskItemList13.Add(riskItem13);
                                            riskType13.RiskItemList = riskItemList13;
                                            riskTypeList.Add(riskType13);
                                        }
                                        if (Nat2Id != "0")
                                        {
                                            var riskType14 = new RiskTypeListModel();
                                            riskType14.Id = Convert.ToString(Nat2lovId);
                                            var riskItem14 = new RiskItemListModel();
                                            riskItem14.Id = Nat2Id.ToString();
                                            var riskItemList14 = new List<RiskItemListModel>();
                                            riskItemList14.Add(riskItem14);
                                            riskType14.RiskItemList = riskItemList14;
                                            riskTypeList.Add(riskType14);
                                        }
                                        if (Nat3Id != "0")
                                        {
                                            var riskType15 = new RiskTypeListModel();
                                            riskType15.Id = Convert.ToString(Nat3lovId);
                                            var riskItem15 = new RiskItemListModel();
                                            riskItem15.Id = Nat3Id.ToString();
                                            var riskItemList15 = new List<RiskItemListModel>();
                                            riskItemList15.Add(riskItem15);
                                            riskType15.RiskItemList = riskItemList15;
                                            riskTypeList.Add(riskType15);
                                        }

                                        if (FATFId != "0")
                                        {
                                            var riskType12 = new RiskTypeListModel();
                                            riskType12.Id = Convert.ToString(FATFlovId);
                                            var riskItem12 = new RiskItemListModel();
                                            riskItem12.Id = FATFId.ToString();
                                            var riskItemList12 = new List<RiskItemListModel>();
                                            riskItemList12.Add(riskItem12);
                                            riskType12.RiskItemList = riskItemList12;
                                            riskTypeList.Add(riskType12);
                                        }

                                        if (prodId != "0")
                                                {
                                                    var riskType4 = new RiskTypeListModel();
                                                    riskType4.Id = Convert.ToString(prodlovId);
                                                    var riskItem4 = new RiskItemListModel();
                                                    riskItem4.Id = prodId.ToString();
                                                    var riskItemList4 = new List<RiskItemListModel>();
                                                    riskItemList4.Add(riskItem4);
                                                    riskType4.RiskItemList = riskItemList4;
                                                    riskTypeList.Add(riskType4);
                                                }
                                                if (moreprodId != "0")
                                                {
                                                   var riskType5 = new RiskTypeListModel();
                                                   riskType5.Id = Convert.ToString(moreprodlovId);
                                                   var riskItem5 = new RiskItemListModel();
                                                   riskItem5.Id = moreprodId.ToString();
                                                   var riskItemList5 = new List<RiskItemListModel>();
                                                   riskItemList5.Add(riskItem5);
                                                   riskType5.RiskItemList = riskItemList5;
                                                   riskTypeList.Add(riskType5);
                                                }
                                                if (delId != "0")
                                                {
                                                    var riskType6 = new RiskTypeListModel();
                                                    riskType6.Id = Convert.ToString(dellovId);
                                                    var riskItem6 = new RiskItemListModel();
                                                    riskItem6.Id = delId.ToString();
                                                    var riskItemList6 = new List<RiskItemListModel>();
                                                    riskItemList6.Add(riskItem6);
                                                    riskType6.RiskItemList = riskItemList6;
                                                    riskTypeList.Add(riskType6);
                                                }
                                                if (screId != "0")
                                                {
                                                  var riskType7 = new RiskTypeListModel();
                                                  riskType7.Id = Convert.ToString(screlovId);
                                                  var riskItem7 = new RiskItemListModel();
                                                  riskItem7.Id = screId.ToString();
                                                  var riskItemList7 = new List<RiskItemListModel>();
                                                  riskItemList7.Add(riskItem7);
                                                  riskType7.RiskItemList = riskItemList7;
                                                  riskTypeList.Add(riskType7);
                                                }
                                                if (adverseId != "0")
                                                {
                                                  var riskType8 = new RiskTypeListModel();
                                                  riskType8.Id = Convert.ToString(adverselovId);
                                                  var riskItem8 = new RiskItemListModel();
                                                  riskItem8.Id = adverseId.ToString();
                                                  var riskItemList8 = new List<RiskItemListModel>();
                                                  riskItemList8.Add(riskItem8);
                                                  riskType8.RiskItemList = riskItemList8;
                                                  riskTypeList.Add(riskType8);
                                                }
                                                if (modId != "0")
                                                {
                                                    var riskType9 = new RiskTypeListModel();
                                                    riskType9.Id = Convert.ToString(modlovId);
                                                    var riskItem9 = new RiskItemListModel();
                                                    riskItem9.Id = modId.ToString();
                                                    var riskItemList9 = new List<RiskItemListModel>();
                                                    riskItemList9.Add(riskItem9);
                                                    riskType9.RiskItemList = riskItemList9;
                                                    riskTypeList.Add(riskType9);
                                                }
                                                riskModel.RiskTypeList = riskTypeList;
                                                var riskResult = Assessment(riskModel);
                                            }
                                        }

                                    }

                                
                             }
                        
                        

                    }

                }
            }
            return new Tuple<ServiceResponse<List<CorporateExcel>>, RiskAPIRequestModel>(_custExcelDataResponse, riskModel);
        }


        public IActionResult Assessment([FromBody] RiskAPIRequestModel model)
        {
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
                        if (model.RiskTypeList[a].Id == riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id.ToString())
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
                RiskDTO _RiskDTO = new RiskDTO()
                {
                    CustomerCode= riskmodel.CustomerCode,
                    CustomerName=riskmodel.CustomerName,
                    DateofAssessment=DateTime.Now,
                    Address=riskmodel.Address,
                    MainNationalityTxt=riskmodel.MainNationalityTxt,
                    FinalRiskScore =riskmodel.FinalRiskScore,
                    RiskScoreSum=riskmodel.RiskScoreSum,
                    RiskScoreCount=riskmodel.RiskScoreCount,
                    ClientId=riskmodel.ClientId,
                    CreatedBy=riskmodel.CreatedBy,
                    RiskScoreBeforeOverride= riskmodel.RiskScoreBeforeOverride,
                    RiskTypeCategoryDTO=riskmodel.RiskTypeCategoryDTO

                };
              
                _customerMasterRepository.CreateBulkrisk(_RiskDTO);
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
                        if (model.RiskTypeList[a].Id == riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id.ToString())
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
                riskmodel.version = 2;

                RiskCorpCustomerDTO _RiskDTO = new RiskCorpCustomerDTO()
                {
                    UniqueID = riskmodel.UniqueID,
                    LegalNameOfEntity = riskmodel.LegalNameOfEntity,
                    DateofAssessment = DateTime.Now,
                    CountryOfIncorporationTxt=riskmodel.CountryOfIncorporationTxt,
                    RiskAssessmentRating = riskmodel.RiskAssessmentRating,
                    RiskAssessmentRatingWithoutOverride = riskmodel.RiskAssessmentRatingWithoutOverride,
                    RiskScoreSum = riskmodel.RiskScoreSum,
                    RiskScoreCount=riskmodel.RiskScoreCount,
                    ClientId = riskmodel.ClientId,
                    CreatedBy = riskmodel.CreatedBy,
                    RiskTypeCategoryDTO = riskmodel.RiskTypeCategoryDTO

                };


                _customerMasterRepository.CreateCorpCustomerBulkRisk(_RiskDTO);

            }
            return null;
        }

        public List<CodesTableDTO> GetCodesByClientID(int clientId)
        {

            return _CustomerCaseRepository.GetCodesByClientID(clientId).Result;
        }
        public bool GetRiskCategoryStatus(string culture, int ClientId, string CustomerType)
        {

            return _CustomerCaseRepository.GetRiskCategoryStatus(culture,ClientId,CustomerType).Result;
        }


        public ClientMasterDTO GetCustomerCodeprefixByclient(int clientId)
        {

            return _CustomerCaseRepository.GetCustomerCodeprefixByclient(clientId).Result;
        }

        public List<CustomerCaseDTO> GetDuplicateNames(string FullName,string type,int clientid)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetDuplicateNames(FullName,type, clientid).Result;
        }
        public List<CustomerCaseDTO> GetCompanyCode(string CompanyCode, int clientid,string CustomerType)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetCompanyCode(CompanyCode, clientid,CustomerType).Result;
        }
        public ServiceResponse<string> CreateShareholdersData(ShareholderDTO _shareholderDTO)
        {
            //Perform business requirements here
            return _customerMasterRepository.CreateShareholdersData(_shareholderDTO);
        }
        public List<ShareholderDTO> GetAllShareHolders(int clientid,string companyCode,int userId,string customerType)
        {
            //Perform business requirements here
            return _CustomerCaseRepository.GetAllShareHolders(clientid, companyCode, userId, customerType).Result;
        }

        public ServiceResponse<string> DeleteShareholders(int id)
        {
            //Perform business requirements here
            return _customerMasterRepository.DeleteShareholders(id);
        }
        public ServiceResponse<string> DeletePendingShareholders(string companyCode)
        {
            //Perform business requirements here
            return _customerMasterRepository.DeletePendingShareholders(companyCode);
        }
    }
}

