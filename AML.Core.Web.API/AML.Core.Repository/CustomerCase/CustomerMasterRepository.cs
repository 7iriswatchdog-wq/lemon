using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerCase;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Kyc;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.Kyc;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;

namespace AML.Core.Repository.CustomerCase
{
    public class CustomerMasterRepository : BaseRepository, ICustomerMasterRepository
    {
        private readonly IConfiguration _configuration;
        private string customerCodeprefix;
        public CustomerMasterRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        {
            _configuration = configuration;
            customerCodeprefix = _configuration.GetSection("CustomerCodePrefix").Value;
        }
        public ServiceResponse<string> Create(CustomerMasterDTO _CustomerMasterDTO)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_fname", _CustomerMasterDTO.FirstName);
                parameters.Add("@p_mname", _CustomerMasterDTO.MiddleName);
                parameters.Add("@p_lname", _CustomerMasterDTO.LastName);
                parameters.Add("@p_dob", _CustomerMasterDTO.DOB);
                parameters.Add("@p_cust_id", _CustomerMasterDTO.CustomerId == "0" || _CustomerMasterDTO.CustomerId.IsNullOrEmpty() ? null : _CustomerMasterDTO.CustomerId);///this is the id of the customermaster table which will be appended with prefix in the insert statement
                //parameters.Add("@p_cust_id", _CustomerMasterDTO.CustomerId);
                parameters.Add("@p_customer_type", _CustomerMasterDTO.CustomerType);
                parameters.Add("@p_nationality", _CustomerMasterDTO.Nationality);
                parameters.Add("@p_created_by", _CustomerMasterDTO.CreatedBy);
                parameters.Add("@p_mobile", _CustomerMasterDTO.Mobile);
                parameters.Add("@p_customer_id_type", _CustomerMasterDTO.CustomerIdType);
                parameters.Add("@p_customer_id_number", _CustomerMasterDTO.CustomerIdNumber);
                parameters.Add("@p_batch_id", _CustomerMasterDTO.Batch.IsNotNullOrEmpty() ? _CustomerMasterDTO.Batch : 0);
                //parameters.Add("@p_customerId", _CustomerMasterDTO.CustomerId);
                parameters.Add("@p_prefix", customerCodeprefix);
                parameters.Add("@p_clientId", _CustomerMasterDTO.ClientId);
                parameters.Add("@p_userName", _CustomerMasterDTO.UserId);
                parameters.Add("@p_clientName", _CustomerMasterDTO.CompanyName);
                parameters.Add("@p_companyCode", _CustomerMasterDTO.CompanyCode);
                parameters.Add("@p_createdon", _CustomerMasterDTO.CreatedOnDB);
                parameters.Add("@p_type", _CustomerMasterDTO.Type);
                parameters.Add("@p_deliverychannel", _CustomerMasterDTO.DeliveryChannelName);
                parameters.Add("@p_productname", _CustomerMasterDTO.ProductName);
                parameters.Add("@p_modeofpayment", _CustomerMasterDTO.Modeofpayment);
                parameters.Add("@p_entitytypetxt", _CustomerMasterDTO.EntityTypeTxt);
                parameters.Add("@p_businesstype", _CustomerMasterDTO.BusinessType);
                parameters.Add("@p_residencestatus", _CustomerMasterDTO.ResidenceStatus);
                parameters.Add("@p_profession", _CustomerMasterDTO.OccupatinTypeTxt);
                parameters.Add("@p_cifnumber", _CustomerMasterDTO.CIFNumber);
                parameters.Add("@p_parentid", _CustomerMasterDTO.ParentID);
                parameters.Add("@p_screeningoption", _CustomerMasterDTO.ScreeningOptions);
                parameters.Add("@p_idissuedate", _CustomerMasterDTO.IdIssueDate);
                parameters.Add("@p_idexpirydate", _CustomerMasterDTO.IdExpiryDate);
                parameters.Add("@p_residence", _CustomerMasterDTO.Residence);
                parameters.Add("@p_employer", _CustomerMasterDTO.Employer);
                parameters.Add("@p_goldenvisa", _CustomerMasterDTO.GoldenVisa);
                parameters.Add("@p_employerindustry", _CustomerMasterDTO.EmployerIndustry);
                parameters.Add("@p_employersector", _CustomerMasterDTO.EmployerSector);
                parameters.Add("@p_sowsofcountry", _CustomerMasterDTO.SOWSOFCountry);
                parameters.Add("@p_counterparty", _CustomerMasterDTO.CounterParty);
                parameters.Add("@p_productvalue", _CustomerMasterDTO.ProductValue);
                parameters.Add("@p_productrefno", _CustomerMasterDTO.ProductRefNo);
                parameters.Add("@p_tradelicenseauthority", _CustomerMasterDTO.TradeLicenseAuthority);
                parameters.Add("@p_tradelicensesector", _CustomerMasterDTO.TradeLicenseSector);
                parameters.Add("@p_share", _CustomerMasterDTO.Share);
                parameters.Add("@p_desgination", _CustomerMasterDTO.Designation);
                parameters.Add("@p_counterpartyname", _CustomerMasterDTO.CounterPartyName);
                parameters.Add("@p_relationship", _CustomerMasterDTO.Relationship);
                parameters.Add("@p_flag", _CustomerMasterDTO.FlagType);
                //parameters.Add("@p_address",_CustomerMasterDTO.Address);
                //parameters.Add("@p_establishmentdate", _CustomerMasterDTO.EstablishmentDate);
                var response = ExecuteScalar("ins_customer_master", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<string> CreateScreening(CustomerMasterDTO _CustomerMasterDTO)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {

                if (_CustomerMasterDTO.onb_created_on == null)
                {
                    _CustomerMasterDTO.onb_created_on = DateTime.Now;
                }

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_ref_id", _CustomerMasterDTO.onb_cust_ref_id);
                parameters.Add("@p_name", _CustomerMasterDTO.onb_name);
                parameters.Add("@p_dob", _CustomerMasterDTO.DOB);
                parameters.Add("@p_nationality", _CustomerMasterDTO.onb_nationality);
                parameters.Add("@p_cust_type", _CustomerMasterDTO.onb_cust_type);
                parameters.Add("@p_cust_id_type", _CustomerMasterDTO.onb_cust_id_type);
                parameters.Add("@p_cust_id", _CustomerMasterDTO.onb_cust_id);
                parameters.Add("@p_mobile", _CustomerMasterDTO.onb_mobile);
                parameters.Add("@p_created_by", _CustomerMasterDTO.onb_created_by);
                parameters.Add("@p_created_on", _CustomerMasterDTO.onb_created_on);
                parameters.Add("@p_updated_by", _CustomerMasterDTO.onb_updated_by);
                parameters.Add("@p_emirates_id", _CustomerMasterDTO.onb_emirates_id);
                parameters.Add("@p_emirates_id_expiry", _CustomerMasterDTO.onb_emirates_id_expiry);
                parameters.Add("@p_passport_expiry", _CustomerMasterDTO.onb_passport_expiry);
                parameters.Add("@p_profession", _CustomerMasterDTO.onb_profession);
                parameters.Add("@p_residence_status", _CustomerMasterDTO.onb_residence_status);
                parameters.Add("@p_insurance_product", _CustomerMasterDTO.onb_insurance_product);
                parameters.Add("@p_delv_channel", _CustomerMasterDTO.onb_delv_channel);
                parameters.Add("@p_src_funds", _CustomerMasterDTO.onb_src_funds);
                parameters.Add("@p_mode_of_pymt", _CustomerMasterDTO.onb_mode_of_pymt);
                parameters.Add("@p_dept", _CustomerMasterDTO.onb_dept);
                parameters.Add("@p_policy_no", _CustomerMasterDTO.onb_policy_no);
                parameters.Add("@p_endt_no", _CustomerMasterDTO.onb_endt_no);
                parameters.Add("@p_doc_no", _CustomerMasterDTO.onb_doc_no);
                parameters.Add("@p_party_type", _CustomerMasterDTO.onb_party_type);
                parameters.Add("@p_subclass", _CustomerMasterDTO.onb_subclass);
                parameters.Add("@p_app_id", _CustomerMasterDTO.ClientId);
                parameters.Add("@p_is_screened", _CustomerMasterDTO.onb_is_screened);
                parameters.Add("@p_threshold", _CustomerMasterDTO.onb_threshold);
                parameters.Add("@p_batch_id", _CustomerMasterDTO.Batch.IsNotNullOrEmpty() ? _CustomerMasterDTO.Batch : 0);
                parameters.Add("@p_prefix", customerCodeprefix);
                var response = ExecuteScalar("ins_customerscreen_master", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }




        public ServiceResponse<string> CreatePrefix(CustomerMasterDTO _CustomerMasterDTO)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_fname", _CustomerMasterDTO.FirstName);
                parameters.Add("@p_mname", _CustomerMasterDTO.MiddleName);
                parameters.Add("@p_lname", _CustomerMasterDTO.LastName);
                parameters.Add("@p_dob", _CustomerMasterDTO.DOB);
                parameters.Add("@p_cust_id", _CustomerMasterDTO.CustomerId);
                parameters.Add("@p_customer_type", _CustomerMasterDTO.CustomerType);
                parameters.Add("@p_nationality", _CustomerMasterDTO.Nationality);
                parameters.Add("@p_created_by", _CustomerMasterDTO.CreatedBy);
                parameters.Add("@p_mobile", _CustomerMasterDTO.Mobile);
                parameters.Add("@p_customer_id_type", _CustomerMasterDTO.CustomerIdType);
                parameters.Add("@p_customer_id_number", _CustomerMasterDTO.CustomerIdNumber);
                parameters.Add("@p_batch_id", _CustomerMasterDTO.Batch.IsNotNullOrEmpty() ? _CustomerMasterDTO.Batch : 0);
                parameters.Add("@p_customerId", _CustomerMasterDTO.CustomerId);
                parameters.Add("@p_prefix", _CustomerMasterDTO.customerCodeprefix);
                parameters.Add("@p_clientId", _CustomerMasterDTO.ClientId);
                parameters.Add("@p_userName", _CustomerMasterDTO.UserId);
                parameters.Add("@p_companycode", _CustomerMasterDTO.CompanyCode);
                parameters.Add("@p_tradelicense", _CustomerMasterDTO.Tradelicense);
                Console.WriteLine(string.Format("FirstName: {0}" +
                    "\nMiddleName: {1}" +
                    "\nLastName: {2}" +
                    "\nDOB: {3}" +
                    "\nCustomerId: {4}" +
                    "\nCustomerType: {5}" +
                    "\nNationality: {6}" +
                    "\nCreatedBy: {7}" +
                    "\nMobile: {8}" +
                    "\nCustomerIdType: {9}" +
                    "\nCustomerIdNumber: {10}" +
                    "\nBatch: {11}" +
                    "\nCustomerId: {12}" +
                    "\ncustomerCodeprefix: {13}" +
                    "\nClientId: {14}" +
                    "\nUserId: {15}",
                    _CustomerMasterDTO.FirstName,
                    _CustomerMasterDTO.MiddleName,
                    _CustomerMasterDTO.LastName,
                    _CustomerMasterDTO.DOB,
                    _CustomerMasterDTO.CustomerId,
                    _CustomerMasterDTO.CustomerType,
                    _CustomerMasterDTO.Nationality,
                    _CustomerMasterDTO.CreatedBy,
                    _CustomerMasterDTO.Mobile,
                    _CustomerMasterDTO.CustomerIdType,
                    _CustomerMasterDTO.CustomerIdNumber,
                    _CustomerMasterDTO.Batch.IsNotNullOrEmpty() ? _CustomerMasterDTO.Batch : 0,
                    _CustomerMasterDTO.CustomerId,
                    customerCodeprefix,
                    _CustomerMasterDTO.ClientId,
                    _CustomerMasterDTO.UserId));
                var response = ExecuteScalar("ins_customer_master_prefix", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<CustomerMasterDTO> GetDetailsById(int Id)
        {
            ServiceResponse<CustomerMasterDTO> serviceResponse = new ServiceResponse<CustomerMasterDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<CustomerMasterDTO>("get_customer_master_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CustomerMasterDTO>> GetDetailsBySearch(string searchterm, string cust_type, int clientId)
        {
            ServiceResponse<List<CustomerMasterDTO>> serviceResponse = new ServiceResponse<List<CustomerMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_searchitem", searchterm);
                parameters.Add("@p_cust_type", cust_type);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<CustomerMasterDTO>("get_customer_master_by_searchterm", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CustomerMasterDTO>> GetCustomerMasterByCodePrefix(string prefix)
        {
            ServiceResponse<List<CustomerMasterDTO>> serviceResponse = new ServiceResponse<List<CustomerMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_prefix", prefix);
                serviceResponse.Result = Get<CustomerMasterDTO>("get_customer_by_code_from_customermasters", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CustomerMasterDTO>> GetCustomerMasterByCode(string customerType,int clientId)
        {
            ServiceResponse<List<CustomerMasterDTO>> serviceResponse = new ServiceResponse<List<CustomerMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_type", customerType);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<CustomerMasterDTO>("get_customer_by_type", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CustomerMasterDTO>> GetCustomerMasterByCodePrefixAndCode(string prefix, string code)
        {
            ServiceResponse<List<CustomerMasterDTO>> serviceResponse = new ServiceResponse<List<CustomerMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_prefix", prefix);
                parameters.Add("@p_cust_type", code);
                serviceResponse.Result = Get<CustomerMasterDTO>("get_customer_by_code_from_customermasters_by_type", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Insert(CustomerMasterDTO _CustomerMasterDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_ref_id", _CustomerMasterDTO.CustomerReferenceID);
                parameters.Add("@p_fname", _CustomerMasterDTO.FirstName);
                parameters.Add("@p_mname", _CustomerMasterDTO.MiddleName);
                parameters.Add("@p_lname", _CustomerMasterDTO.LastName);
                parameters.Add("@p_dob", _CustomerMasterDTO.DOB);
                parameters.Add("@p_nationality", _CustomerMasterDTO.Nationality);
                parameters.Add("@p_cust_type", _CustomerMasterDTO.CustomerType);
                parameters.Add("@p_cust_id_type", _CustomerMasterDTO.CustomerIdType);
                parameters.Add("@p_cust_id_number", _CustomerMasterDTO.CustomerIdNumber);
                parameters.Add("@p_mobile", _CustomerMasterDTO.Mobile);
                parameters.Add("@p_created_by", _CustomerMasterDTO.CreatedBy);
                parameters.Add("@p_clientId", _CustomerMasterDTO.ClientId);
                var response = ExecuteScalar("ins_customermaster", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(CustomerMasterDTO _CustomerMasterDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _CustomerMasterDTO.Id);
                parameters.Add("@p_cust_ref_id", _CustomerMasterDTO.CustomerReferenceID);
                parameters.Add("@p_fname", _CustomerMasterDTO.FirstName);
                parameters.Add("@p_mname", _CustomerMasterDTO.MiddleName);
                parameters.Add("@p_lname", _CustomerMasterDTO.LastName);
                parameters.Add("@p_dob", _CustomerMasterDTO.DOB);
                parameters.Add("@p_nationality", _CustomerMasterDTO.Nationality);
                parameters.Add("@p_cust_type", _CustomerMasterDTO.CustomerType);
                parameters.Add("@p_cust_id_type", _CustomerMasterDTO.CustomerIdType);
                parameters.Add("@p_cust_id_number", _CustomerMasterDTO.CustomerIdNumber);
                parameters.Add("@p_mobile", _CustomerMasterDTO.Mobile);
                parameters.Add("@p_batch_id", _CustomerMasterDTO.Batch.IsNotNullOrEmpty() ? _CustomerMasterDTO.Batch : 0);
                parameters.Add("@p_is_deleted", _CustomerMasterDTO.IsDeleted);
                parameters.Add("@p_status", _CustomerMasterDTO.Status);
                parameters.Add("@p_updated_by", _CustomerMasterDTO.UpdatedBy);
                parameters.Add("@p_updated_on", DateTime.UtcNow, DbType.DateTime);
                parameters.Add("@p_customer_final_risk_score", _CustomerMasterDTO.CustomerFinalRiskScore);
                parameters.Add("@p_customer_screen_match_score", _CustomerMasterDTO.CustomerScreenMatchScore);
                var response = ExecuteScalar("mod_customermaster", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Get Paginated Customer Master List
        /// </summary>
        /// <param name="inActive"></param>
        /// <param name="isDue"></param>
        /// <param name="pgIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="companyId"></param>
        /// <param name="sortCol"></param>
        /// <param name="s_dir"></param>
        /// <param name="filterTxt"></param>
        /// <param name="siteIds"></param>
        /// <returns></returns>
        public IEnumerable<CustomerMasterDTO> GetPaginatedCustomerMaster(int pgIndex, int pageSize, string sortCol, string s_dir, string filterTxt,int clientId)
        {
            try
            {
                var query = "sel_customermaster_paginated";
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@_PageIndex", pgIndex);
                parameter.Add("@_PageSize", pageSize);
                parameter.Add("@sortCol", sortCol);
                parameter.Add("@sortOrder", s_dir);
                parameter.Add("@filterTxt", filterTxt);
                parameter.Add("@c_clientId", clientId);
                return Get<CustomerMasterDTO>(query, parameter, commandType: CommandType.StoredProcedure);
            }
            catch
            {
                return new List<CustomerMasterDTO>();
            }
        }

        public ServiceResponse<int> UpdateWhiteList(int id, string isWhiteListed)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                parameters.Add("@p_isWhiteList", isWhiteListed);
                var response = ExecuteScalar("mod_whitelist_status", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> InsertCustomerWhiteListLogs(int customerMasterID, string customerID, int createdBy, string isWhiteListed)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_master_id", customerMasterID);
                parameters.Add("@p_cust_id", customerID);
                parameters.Add("@p_created_by", createdBy);
                parameters.Add("@p_whitelisted_for_screening", isWhiteListed);
                var response = ExecuteScalar("ins_customer_whitelist_log", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer Whitelist Logs Inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<string> CreateCustomerMasterShareholder(CustomerMasterDTO _CustomerMasterDTO, string corporateID, int sharePercentage)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_ref_id", _CustomerMasterDTO.CustomerReferenceID);
                parameters.Add("@p_fname", _CustomerMasterDTO.FirstName);
                parameters.Add("@p_mname", _CustomerMasterDTO.MiddleName);
                parameters.Add("@p_lname", _CustomerMasterDTO.LastName);
                parameters.Add("@p_dob", _CustomerMasterDTO.DOB);
                parameters.Add("@p_nationality", _CustomerMasterDTO.Nationality);
                parameters.Add("@p_cust_type", _CustomerMasterDTO.CustomerType);
                parameters.Add("@p_cust_id_type", _CustomerMasterDTO.CustomerIdType);
                parameters.Add("@p_cust_id_number", _CustomerMasterDTO.CustomerIdNumber);
                parameters.Add("@p_mobile", _CustomerMasterDTO.Mobile);
                parameters.Add("@p_created_by", _CustomerMasterDTO.CreatedBy);
                parameters.Add("@c_corporate_id", corporateID);
                parameters.Add("@c_share_percentage", sharePercentage);
                parameters.Add("@c_clientId", _CustomerMasterDTO.ClientId);
                parameters.Add("@p_c6threshold", _CustomerMasterDTO.C6Threshold);
                parameters.Add("@threshold", _CustomerMasterDTO.Threshold);
                var response = ExecuteScalar("ins_customermaster_and_shareholder_prefix", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> Delete(string Id)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                Console.WriteLine(string.Format("cmr Id : {0}", Id));
                parameters.Add("@p_id", Id);
                var response = Execute("del_customer", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer Deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> Undelete(string Id)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                Console.WriteLine(string.Format("cmr Id : {0}", Id));
                parameters.Add("@p_id", Id);
                var response = Execute("undel_customer", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer Undeleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<string> CreateGroupEntity(CustomerMasterDTO _CustomerMasterDTO, string CorporateType = null)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_fname", _CustomerMasterDTO.FirstName);
                parameters.Add("@p_mname", _CustomerMasterDTO.MiddleName);
                parameters.Add("@p_lname", _CustomerMasterDTO.LastName);
                parameters.Add("@p_dob", _CustomerMasterDTO.DOB);
                parameters.Add("@p_cust_id", _CustomerMasterDTO.CustomerId);
                parameters.Add("@p_customer_type", _CustomerMasterDTO.CustomerType);
                parameters.Add("@p_nationality", _CustomerMasterDTO.Nationality);
                parameters.Add("@p_created_by", _CustomerMasterDTO.CreatedBy);
                parameters.Add("@p_mobile", _CustomerMasterDTO.Mobile);
                parameters.Add("@p_customer_id_type", _CustomerMasterDTO.CustomerIdType);
                parameters.Add("@p_customer_id_number", _CustomerMasterDTO.CustomerIdNumber);
                parameters.Add("@p_batch_id", _CustomerMasterDTO.Batch.IsNotNullOrEmpty() ? _CustomerMasterDTO.Batch : 0);
                parameters.Add("@p_customerId", _CustomerMasterDTO.CustomerId);
                parameters.Add("@p_prefix", _CustomerMasterDTO.customerCodeprefix);
                parameters.Add("@p_corporate_type", CorporateType);
                parameters.Add("p_group_entity_of", _CustomerMasterDTO.GroupEntityof);
                parameters.Add("p_Client_Id", _CustomerMasterDTO.ClientId);
                var response = ExecuteScalar("ins_customer_master_group_entity", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'ins_customer_master_group_entity'\n{ex.Message}");
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<string> CreateCustomerMasterShareholder(CustomerMasterDTO _CustomerMasterDTO, string corporateID, int sharePercentage, string Designation = null, string EmiratesId = null, string EmiratesIdExpiry = null, string PassportExpiry = null, string customerType = null)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_ref_id", _CustomerMasterDTO.CustomerReferenceID);
                parameters.Add("@p_fname", _CustomerMasterDTO.FirstName);
                parameters.Add("@p_mname", _CustomerMasterDTO.MiddleName);
                parameters.Add("@p_lname", _CustomerMasterDTO.LastName);
                parameters.Add("@p_dob", _CustomerMasterDTO.DOB);
                parameters.Add("@p_nationality", _CustomerMasterDTO.Nationality);
                parameters.Add("@p_cust_type", _CustomerMasterDTO.CustomerType);
                parameters.Add("@p_cust_id_type", _CustomerMasterDTO.CustomerIdType);
                parameters.Add("@p_cust_id_type", _CustomerMasterDTO.CustomerIdType);
                parameters.Add("@p_cust_id_number", _CustomerMasterDTO.CustomerIdNumber);
                parameters.Add("@p_mobile", _CustomerMasterDTO.Mobile);
                parameters.Add("@p_created_by", _CustomerMasterDTO.CreatedBy);
                parameters.Add("@c_corporate_id", corporateID);
                parameters.Add("@c_share_percentage", sharePercentage);
                parameters.Add("p_designation", Designation);
                parameters.Add("p_emirates_id", EmiratesId);
                parameters.Add("p_emirates_id_expiry", EmiratesIdExpiry);
                parameters.Add("p_passport_expiry", PassportExpiry);
                parameters.Add("p_customer_type", customerType);
                parameters.Add("c_clientId", _CustomerMasterDTO.ClientId);
                parameters.Add("p_c6threshold", _CustomerMasterDTO.C6Threshold);
                parameters.Add("threshold", _CustomerMasterDTO.Threshold);
                var response = ExecuteScalar("ins_customermaster_and_shareholder_prefix", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<KycIndividualDTO> GetAll(string CustomerId)
        {
            ServiceResponse<KycIndividualDTO> serviceResponse = new ServiceResponse<KycIndividualDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_customer_id", CustomerId);
                serviceResponse.Result = GetFirstOrDefault<KycIndividualDTO>("get_all_kyc_individual", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Kyc individual successfully fetched";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_all_kyc_individual'\n{ex.Message}");

                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }

            return serviceResponse;
        }
        public ServiceResponse<CorporateKycDTO> GetAllCorporate(string CustomerId)
        {
            ServiceResponse<CorporateKycDTO> serviceResponse = new ServiceResponse<CorporateKycDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_customer_id", CustomerId);
                serviceResponse.Result = GetFirstOrDefault<CorporateKycDTO>("get_all_kyc_corporate", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result.EntityAddress = new AddressDTO();
                serviceResponse.Result.EntityAddress.City = serviceResponse.Result.City;
                serviceResponse.Result.EntityAddress.Emirate = serviceResponse.Result.Emirate;
                serviceResponse.Result.EntityAddress.Country = serviceResponse.Result.Country;
                serviceResponse.Result.EntityAddress.POBox = serviceResponse.Result.POBox;
                serviceResponse.Result.CommercialLicense = new LicenseDTO();
                serviceResponse.Result.CommercialLicense.LicenseNumber = serviceResponse.Result.LicenseNumber;
                serviceResponse.Result.CommercialLicense.LicenseIssueDate = serviceResponse.Result.LicenseIssueDate;
                serviceResponse.Result.CommercialLicense.LicenseIssuingAuthority = serviceResponse.Result.LicenseIssuingAuthority;
                serviceResponse.Result.CommercialLicense.LicenseExpiryDate = serviceResponse.Result.LicenseExpiryDate;
                serviceResponse.Result.CommercialLicense.PlaceofIssue = serviceResponse.Result.PlaceofIssue;
                serviceResponse.Result.CommercialLicense.BusinessActivity = serviceResponse.Result.BusinessActivity;
                serviceResponse.Result.CommercialLicense.LicenseTypeTxt = serviceResponse.Result.LicenseTypeTxt;

                serviceResponse.Message = "Kyc Corporate successfully fetched";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_all_kyc_corporate'\n{ex.Message}");

                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<GroupEntityDTO>> GetAllGroupEntities(string CustomerId)
        {
            ServiceResponse<List<GroupEntityDTO>> serviceResponse = new ServiceResponse<List<GroupEntityDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_cust_ref_id", CustomerId);
                serviceResponse.Result = Get<GroupEntityDTO>("get_group_entity_by_cust_ref_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Group entity details successfullly fetched";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<PersonDetailsDTO>> GetAllPartners(string CustomerId)
        {
            ServiceResponse<List<PersonDetailsDTO>> serviceResponse = new ServiceResponse<List<PersonDetailsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_cust_ref_id", CustomerId);
                serviceResponse.Result = Get<PersonDetailsDTO>("get_shareholder_by_cust_ref_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Shareholder details successfullly fetched";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<PersonDetailsDTO>> GetAllSeniorManagement(string CustomerId)
        {
            ServiceResponse<List<PersonDetailsDTO>> serviceResponse = new ServiceResponse<List<PersonDetailsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_cust_ref_id", CustomerId);
                serviceResponse.Result = Get<PersonDetailsDTO>("get_senior_management_by_cust_ref_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Shareholder details successfullly fetched";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<PersonDetailsDTO>> GetAllSignatories(string CustomerId)
        {
            ServiceResponse<List<PersonDetailsDTO>> serviceResponse = new ServiceResponse<List<PersonDetailsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_cust_ref_id", CustomerId);
                serviceResponse.Result = Get<PersonDetailsDTO>("get_signatories_by_cust_ref_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Shareholder details successfullly fetched";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        //Risk Bulk Upload


        public ServiceResponse<string> GetRiskTypeId(RiskBulkDTO _RiskBulkDTO)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_customertype", _RiskBulkDTO.CustomerType);
                parameters.Add("p_clientId", _RiskBulkDTO.ClientId);
                if (_RiskBulkDTO.CustomerType == "Individual")
                {

                    parameters.Add("p_nationality", _RiskBulkDTO.Nationality != null && _RiskBulkDTO.Nationality != "0" ? _RiskBulkDTO.Nationality : "");
                }
                else
                {
                    parameters.Add("p_nationality", _RiskBulkDTO.CountryOfIncorporationTxt != null && _RiskBulkDTO.CountryOfIncorporationTxt != "0" ? _RiskBulkDTO.CountryOfIncorporationTxt : "");
                }
                parameters.Add("p_profession", _RiskBulkDTO.Profession != null && _RiskBulkDTO.Profession != "0" ? _RiskBulkDTO.Profession : "");
                parameters.Add("p_residence", _RiskBulkDTO.ResidenceType != null ? _RiskBulkDTO.ResidenceType : "");
                parameters.Add("p_product", _RiskBulkDTO.Product != null ? _RiskBulkDTO.Product : "");
                parameters.Add("p_delivery", _RiskBulkDTO.DeliveryChannel != null ? _RiskBulkDTO.DeliveryChannel : "");
                parameters.Add("p_sourceoffund", _RiskBulkDTO.SourceOfFund != null ? _RiskBulkDTO.SourceOfFund : "");
                parameters.Add("p_modeofpayment", _RiskBulkDTO.ModeOfPayment != null ? _RiskBulkDTO.ModeOfPayment : "");
                parameters.Add("p_screened", _RiskBulkDTO.Screened != null ? _RiskBulkDTO.Screened : "");
                parameters.Add("p_moreproduct", _RiskBulkDTO.MoreProduct != null ? _RiskBulkDTO.MoreProduct : "");
                parameters.Add("p_isadveres", _RiskBulkDTO.IsAdverse != null ? _RiskBulkDTO.IsAdverse : "");
                parameters.Add("p_FATF", _RiskBulkDTO.FATF != null && _RiskBulkDTO.FATF != "0" ? _RiskBulkDTO.FATF : "");
                parameters.Add("p_nat1", _RiskBulkDTO.Nat1 != null && _RiskBulkDTO.Nat1 != "0" ? _RiskBulkDTO.Nat1 : "");
                parameters.Add("p_nat2", _RiskBulkDTO.Nat2 != null && _RiskBulkDTO.Nat2 != "0" ? _RiskBulkDTO.Nat2 : "");
                parameters.Add("p_nat3", _RiskBulkDTO.Nat3 != null && _RiskBulkDTO.Nat3 != "0" ? _RiskBulkDTO.Nat3 : "");

                var response = ExecuteScalar("get_bulkrisk_type_id", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }



        public ServiceResponse<string> GetRisklovId(RiskBulkDTO _RiskBulkDTO)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_customertype", _RiskBulkDTO.CustomerType);
                parameters.Add("p_clientId", _RiskBulkDTO.ClientId);
                if(_RiskBulkDTO.CustomerType =="Individual")
                {

                    parameters.Add("p_nationality", _RiskBulkDTO.Nationality != null && _RiskBulkDTO.Nationality != "0" ? _RiskBulkDTO.Nationality : "");
                }
                else
                {
                    parameters.Add("p_nationality", _RiskBulkDTO.CountryOfIncorporationTxt != null && _RiskBulkDTO.CountryOfIncorporationTxt != "0" ? _RiskBulkDTO.CountryOfIncorporationTxt : "");
                }

                parameters.Add("p_profession", _RiskBulkDTO.Profession != null && _RiskBulkDTO.Profession != "0" ? _RiskBulkDTO.Profession : "");
                parameters.Add("p_residence", _RiskBulkDTO.ResidenceType != null ? _RiskBulkDTO.ResidenceType : "");
                parameters.Add("p_product", _RiskBulkDTO.Product != null ? _RiskBulkDTO.Product : "");
                parameters.Add("p_delivery", _RiskBulkDTO.DeliveryChannel != null ? _RiskBulkDTO.DeliveryChannel : "");
                parameters.Add("p_sourceoffund", _RiskBulkDTO.SourceOfFund != null ? _RiskBulkDTO.SourceOfFund : "");
                parameters.Add("p_modeofpayment", _RiskBulkDTO.ModeOfPayment != null ? _RiskBulkDTO.ModeOfPayment : "");
                parameters.Add("p_screened", _RiskBulkDTO.Screened != null ? _RiskBulkDTO.Screened : "");
                parameters.Add("p_moreproduct", _RiskBulkDTO.MoreProduct != null ? _RiskBulkDTO.MoreProduct : "");
                parameters.Add("p_isadveres", _RiskBulkDTO.IsAdverse != null ? _RiskBulkDTO.IsAdverse : "");
                parameters.Add("p_FATF", _RiskBulkDTO.FATF != null && _RiskBulkDTO.FATF != "0" ? _RiskBulkDTO.FATF : "");
                parameters.Add("p_nat1", _RiskBulkDTO.Nat1 != null && _RiskBulkDTO.Nat1 != "0" ? _RiskBulkDTO.Nat1 : "");
                parameters.Add("p_nat2", _RiskBulkDTO.Nat2 != null && _RiskBulkDTO.Nat2 != "0" ? _RiskBulkDTO.Nat2 : "");
                parameters.Add("p_nat3", _RiskBulkDTO.Nat3 != null && _RiskBulkDTO.Nat3 != "0" ? _RiskBulkDTO.Nat3 : "");
                var response = ExecuteScalar("get_bulkrisk_lovtype_id", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        //Risk Bulk Upload

        public ServiceResponse<int> CreateBulkrisk(RiskDTO _riskDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode", _riskDTO.CustomerCode);
                parameters.Add("@p_customername", _riskDTO.CustomerName);
                parameters.Add("@p_dateofassessment", _riskDTO.DateofAssessment);
                parameters.Add("@p_address", _riskDTO.Address);
                parameters.Add("@p_Customer_nationality", _riskDTO.MainNationalityTxt);
                parameters.Add("@p_final_risk_score", _riskDTO.FinalRiskScore);
                parameters.Add("@p_risk_score_sum", _riskDTO.RiskScoreSum);
                parameters.Add("@p_risk_score_count", _riskDTO.RiskScoreCount);
                parameters.Add("@p_risk_score_before_override", _riskDTO.RiskScoreBeforeOverride);
                string riskItemlist = "";
                int index2 = 0;
                for (int i = 0; i < _riskDTO.RiskTypeCategoryDTO.Count; i++)
                {

                    for (int j = 0; j < _riskDTO.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (_riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId != 0)
                        {
                            riskItemlist += _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].Id + "Ø" + _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId + "¥";
                            index2++;
                        }

                    }
                }
                parameters.Add("@p_risk_data", riskItemlist);
                parameters.Add("@p_risk_category_count", index2);
                parameters.Add("@p_clientId", _riskDTO.ClientId);
                parameters.Add("@p_created_by", _riskDTO.CreatedBy);
                parameters.Add("@p_version", _riskDTO.version);
                var response = ExecuteScalar("ins_transaction_risk_individual", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Risk Assesment for Corporate Customer added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

        }
        public ServiceResponse<int> CreateCorpCustomerBulkRisk(RiskCorpCustomerDTO _riskDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode", _riskDTO.UniqueID);
                parameters.Add("@p_customername", _riskDTO.LegalNameOfEntity);
                parameters.Add("@p_dateofassessment", _riskDTO.DateofAssessment);
                parameters.Add("@p_created_date", DateTime.Now);
                parameters.Add("@p_address", string.Empty);
                parameters.Add("@p_Customer_nationality", _riskDTO.CountryOfIncorporationTxt);
                parameters.Add("@p_final_risk_score", _riskDTO.RiskAssessmentRating);
                parameters.Add("@p_risk_score_before_override", _riskDTO.RiskAssessmentRatingWithoutOverride);
                parameters.Add("@p_risk_sum", _riskDTO.RiskScoreSum);
                parameters.Add("@p_risk_count", _riskDTO.RiskScoreCount);
                var rowData = "";
                var index = 0;
                for (var i = 0; i < _riskDTO.RiskTypeCategoryDTO.Count; i++)
                {
                    for (var j = 0; j < _riskDTO.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (_riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId != 0)
                        {
                            rowData += _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].Id + "Ø" + _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId + "¥";
                            index++;
                        }
                    }
                }
                parameters.Add("@p_risk_data", rowData);
                parameters.Add("@p_risk_category_count", index);
                parameters.Add("@p_clientId", _riskDTO.ClientId);
                parameters.Add("@p_created_by", _riskDTO.CreatedBy);
                parameters.Add("@p_version", _riskDTO.version);
                var response = ExecuteScalar("ins_transaction_risk_Corporate", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Risk Assesment for Corporate Customer added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

             }
        public ServiceResponse<string> GetCustomerId(string  CustomerId)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_ref_id", CustomerId);
                var response = ExecuteScalar("get_customerdetails", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<string> CreateShareholdersData(ShareholderDTO _shareholderDTO)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_fname", _shareholderDTO.Name);
                parameters.Add("@p_nationality", _shareholderDTO.Nationality);
                parameters.Add("@p_companycode", _shareholderDTO.CompanyCode);
                parameters.Add("@p_share", _shareholderDTO.Share);
                parameters.Add("@p_issuedate", _shareholderDTO.IssueDate);
                parameters.Add("@p_expirydate", _shareholderDTO.IdExpiry);
                parameters.Add("@p_idnumber", _shareholderDTO.IdNumber);
                parameters.Add("@p_idtype", _shareholderDTO.IdType);
                parameters.Add("@p_cif", _shareholderDTO.Cif);
                parameters.Add("@p_designation", _shareholderDTO.Designation);
                parameters.Add("@p_type", _shareholderDTO.Type);
                parameters.Add("@p_registrationdate", _shareholderDTO.RegistrationDate);
                parameters.Add("@p_tradelicense", _shareholderDTO.TradeLicence);
                parameters.Add("@p_thershold", _shareholderDTO.Thershold);
                parameters.Add("@p_clientid", _shareholderDTO.ClientId);
                parameters.Add("@p_userid", _shareholderDTO.UserId);
                parameters.Add("@p_companycode", _shareholderDTO.CompanyCode);
                parameters.Add("@p_companyname", _shareholderDTO.CompanyName);
                parameters.Add("@p_employer", _shareholderDTO.Employer);
                parameters.Add("@p_GoldenVisa", _shareholderDTO.GoldenVisa);
                parameters.Add("@p_residence", _shareholderDTO.Residence);
                parameters.Add("@p_docfilename", _shareholderDTO.DocumentFileName);
                parameters.Add("@p_custtype", _shareholderDTO.CustType);
                parameters.Add("@p_docfullpath", _shareholderDTO.DocFullPath);
                parameters.Add("@p_employerindustry", _shareholderDTO.EmployerIndustry);
                parameters.Add("@p_employersector", _shareholderDTO.EmployerSector);
                parameters.Add("@p_sowsofcountry", _shareholderDTO.SOWSOFCountry);
                parameters.Add("@p_tradelicenseauthority", _shareholderDTO.TradeLicenseAuthority);
                parameters.Add("@p_tradelicensesector", _shareholderDTO.TradeLicenseSector);
                parameters.Add("@p_gender", _shareholderDTO.Gender);
                parameters.Add("@p_relationship", _shareholderDTO.Relationship);
                parameters.Add("@p_flag", _shareholderDTO.FlagType);
                parameters.Add("@p_parentid", _shareholderDTO.ParentId);
                parameters.Add("@p_displayid", _shareholderDTO.DisplayId);

                var response = ExecuteScalar("ins_shareholders_data", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer master added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<string> DeleteShareholders(int id)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                var response = ExecuteScalar("Del_shareholders", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Shareholder deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<string> DeletePendingShareholders(string companyCode)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_companyCode", companyCode);
                var response = ExecuteScalar("Del_shareholders_companycode", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Pending shareholders deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<string> UpdateParentId(int id, int? parentId)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                parameters.Add("@p_parentid", parentId);
                string query = "UPDATE tempshareholdersdata SET parentId = @p_parentid WHERE id = @p_id";
                Execute(query, parameters, commandType: CommandType.Text);
                serviceResponse.Message = "Parent ID updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
}
}