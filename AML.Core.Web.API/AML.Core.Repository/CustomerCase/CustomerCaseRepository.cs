using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Branch;
using AML.Core.RepositoryContract.CustomerCase;
using AML.DTO.DTO.Branch;
using AML.DTO.DTO.CodesMaster;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.EtlBatch;
using AML.ViewModel.ViewModels.ApiAuthentication;
using AML.ViewModel.ViewModels.Kyc;
using Dapper;
using ExcelDataReader;
using iTextSharp.text.pdf.parser.clipper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AML.Core.Repository.CustomerCase
 {
    public class CustomerCaseRepository : BaseRepository, ICustomerCaseRepository
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        public CustomerCaseRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }
        public ServiceResponse<List<CustomerCaseDTO>> GetAll(int userId, string startDate, string endDate, string cust_type, string matchScore,int createdBy,int caseStatus,string riskLevel, string usergroupName, int clientId)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                int? matchFrom = null;
                int? matchTo = null;

                if (!string.IsNullOrWhiteSpace(matchScore))
                {
                    var parts = matchScore.Split('-');

                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int from) &&
                        int.TryParse(parts[1], out int to))
                    {
                        matchFrom = from;
                        matchTo = to;
                    }
                }
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_userid", userId);
                parameters.Add("c_from", Convert.ToDateTime(startDate));
                parameters.Add("c_to", Convert.ToDateTime(endDate));
                parameters.Add("cust_type", cust_type);
                parameters.Add("p_usergroup", usergroupName);
                parameters.Add("p_matchfrom", matchFrom, DbType.Int32);
                parameters.Add("p_matchto", matchTo, DbType.Int32);
                parameters.Add("p_createdBy",createdBy);
                parameters.Add("c_status", caseStatus);
                parameters.Add("p_riskLevel", riskLevel);
                parameters.Add("p_clientId", clientId);
                //parameters.Add("p_caseChangeStatus", caseStatusChange);
                if (usergroupName == "Senior Management")
                {
                    serviceResponse.Result = Get<CustomerCaseDTO>("get_all_customercase_seniormanagement", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
                else
                {
                    serviceResponse.Result = Get<CustomerCaseDTO>("get_all_customercase", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
                    
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CustomerCaseDTO>> GetAllCompletedCases(int userId, string startDate, string endDate, string cust_type, string matchScore, int createdBy, int caseStatus, string riskLevel, int clientId)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                int? matchFrom = null;
                int? matchTo = null;

                if (!string.IsNullOrWhiteSpace(matchScore))
                {
                    var parts = matchScore.Split('-');

                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int from) &&
                        int.TryParse(parts[1], out int to))
                    {
                        matchFrom = from;
                        matchTo = to;
                    }
                }
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_userid", userId);
                parameters.Add("c_from", Convert.ToDateTime(startDate));
                parameters.Add("c_to", Convert.ToDateTime(endDate));
                parameters.Add("cust_type", cust_type);
                parameters.Add("p_matchfrom", matchFrom, DbType.Int32);
                parameters.Add("p_matchto", matchTo, DbType.Int32);
                parameters.Add("p_createdBy", createdBy);
                parameters.Add("c_status", caseStatus);
                parameters.Add("p_riskLevel", riskLevel);
                parameters.Add("p_clientId", clientId);
                //parameters.Add("p_caseChangeStatus", caseStatusChange);
                serviceResponse.Result = Get<CustomerCaseDTO>("get_all_completed_customercase", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CustomerCaseDTO>> GetAllBySearchValue(int userId, string startDate, string endDate, string cust_type, string searchValue, string matchScore, int createdBy, int caseStatus, string riskLevel, string usergroupName,int clientId)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                int? matchFrom = null;
                int? matchTo = null;

                if (!string.IsNullOrWhiteSpace(matchScore))
                {
                    var parts = matchScore.Split('-');

                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int from) &&
                        int.TryParse(parts[1], out int to))
                    {
                        matchFrom = from;
                        matchTo = to;
                    }
                }
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_userid", userId);
                parameters.Add("c_from", Convert.ToDateTime(startDate));
                parameters.Add("c_to", Convert.ToDateTime(endDate));
                parameters.Add("cust_type", cust_type);
                parameters.Add("@p_searchvalue", searchValue);
                parameters.Add("p_matchfrom", matchFrom, DbType.Int32);
                parameters.Add("p_matchto", matchTo, DbType.Int32);
                parameters.Add("p_createdBy", createdBy);
                parameters.Add("c_status", caseStatus);
                parameters.Add("p_riskLevel", riskLevel);
                //parameters.Add("p_caseChangeStatus", caseStatusChange);
                parameters.Add("@p_usergroupname", usergroupName);
                parameters.Add("p_clientId", clientId);
                serviceResponse.Result = Get<CustomerCaseDTO>("get_all_customercase_by_searchvalue", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CustomerCaseDTO>> GetAllCompletedBySearchValue(int userId, string startDate, string endDate, string cust_type, string searchValue, string matchScore, int createdBy, int caseStatus, string riskLevel, int clientId)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                int? matchFrom = null;
                int? matchTo = null;

                if (!string.IsNullOrWhiteSpace(matchScore))
                {
                    var parts = matchScore.Split('-');

                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int from) &&
                        int.TryParse(parts[1], out int to))
                    {
                        matchFrom = from;
                        matchTo = to;
                    }
                }
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_userid", userId);
                parameters.Add("c_from", Convert.ToDateTime(startDate));
                parameters.Add("c_to", Convert.ToDateTime(endDate));
                parameters.Add("cust_type", cust_type);
                parameters.Add("@p_searchvalue", searchValue);
                parameters.Add("p_matchfrom", matchFrom, DbType.Int32);
                parameters.Add("p_matchto", matchTo, DbType.Int32);
                parameters.Add("p_createdBy", createdBy);
                parameters.Add("c_status", caseStatus);
                parameters.Add("p_riskLevel", riskLevel);
                parameters.Add("p_clientId", clientId);
                //parameters.Add("p_caseChangeStatus", caseStatusChange);
                serviceResponse.Result = Get<CustomerCaseDTO>("get_all_completed_by_searchvalue", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CustomerCaseDTO>> GetAllSanctionDashboard(int clientId ,string ctype)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@clientid", clientId);
                parameters.Add("@cType", ctype);
                serviceResponse.Result = Get<CustomerCaseDTO>("get_all_sanctionfordashboard", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
    
        public ServiceResponse<CustomerCaseDTO> GetDetails(int Id)
        {
            ServiceResponse<CustomerCaseDTO> serviceResponse = new ServiceResponse<CustomerCaseDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<CustomerCaseDTO>("get_customercase_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<string> GetDualGoodsStatus(string customerId)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", customerId);
                serviceResponse.Result = GetFirstOrDefault<string>("get_proliferation_status", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CustomerCaseDTO>> GetShareHoldersByCompanyCode(string Id)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = Get<CustomerCaseDTO>("get_all_customercase_by_companycode", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public int GetCaseId(string CustId)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", CustId);
                serviceResponse.Result = ExecuteScalar("get_id_by_cust_id", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse.Result;
        }
        public ServiceResponse<CustomerCaseDTO> GetCaseFullDetailsByCustomerId(string _custId)
        {
            ServiceResponse<CustomerCaseDTO> serviceResponse = new ServiceResponse<CustomerCaseDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_id", _custId);
                serviceResponse.Result = GetFirstOrDefault<CustomerCaseDTO>("get_customercase_full_details_by_cust_id_prefix", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<List<CustomerCaseDTO>> GetApprovedList(int clientId)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<CustomerCaseDTO>("get_all_customercase_approved", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;


        }

        public ServiceResponse<List<CustomerCaseDTO>> GetCasespendingScheduler()
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                
                serviceResponse.Result = Get<CustomerCaseDTO>("get_all_customercase_pending_scheduler", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;


        }
        public int InsertDigiSchedulerLogs(int totalHits, int totalRecords, int clientId, string SchedulerTrackerId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_total_hits", totalHits);
                parameters.Add("@p_total_records", totalRecords);
                parameters.Add("@p_clientId", clientId);
                parameters.Add("@p_schedulerTrackerId", SchedulerTrackerId);
                var response = ExecuteScalar("ins_digischeduler_logs", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                return response;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public ServiceResponse<List<ClientMasterDTO>> GetAllClients()
        {
            ServiceResponse<List<ClientMasterDTO>> serviceResponse = new ServiceResponse<List<ClientMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<ClientMasterDTO>("get_all_active_clients", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Client details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;


        }
        public ServiceResponse<List<ClientMasterDTO>> GetAllAdminClients()
        {
            ServiceResponse<List<ClientMasterDTO>> serviceResponse = new ServiceResponse<List<ClientMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<ClientMasterDTO>("get_all_clients_admin", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Admin client details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        // Start

        public async Task<ServiceResponse<List<CustomerCaseDTO>>> GetApprovedListAsync(int clientId)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                var procedureResult = await GetAsync<CustomerCaseDTO>("get_all_customercase_approved", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result = procedureResult.ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;


        }
        public async Task<ServiceResponse<List<ClientMasterDTO>>> GetAllClientsAsync()
        {
            ServiceResponse<List<ClientMasterDTO>> serviceResponse = new ServiceResponse<List<ClientMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                var clients = await GetAsync<ClientMasterDTO>("get_all_active_clients", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result = clients.ToList();
                serviceResponse.Message = "Client details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse; 
        }
// End 
        public ServiceResponse<List<ClientRightsDTO>> GetClientRightsByClientId(int clientId)
        {
            ServiceResponse<List<ClientRightsDTO>> serviceResponse = new ServiceResponse<List<ClientRightsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<ClientRightsDTO>("get_client_rights_by_clientId", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Client rights fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

        }
        public ServiceResponse<List<MenuModelDTO>> GetAllMenus()
        {
            ServiceResponse<List<MenuModelDTO>> serviceResponse = new ServiceResponse<List<MenuModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<MenuModelDTO>("get_all_menus", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Menus fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

        }
        public ServiceResponse<ClientMasterDTO> GetClientDetailsByID(int clientId)
        {
            ServiceResponse<ClientMasterDTO> serviceResponse = new ServiceResponse<ClientMasterDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = GetFirstOrDefault<ClientMasterDTO>("get_client_details_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Client details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;


        }
      
        public ServiceResponse<int> CreateClient(ClientMasterDTO _clientDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Client_Name", _clientDTO.ClientName);
                parameters.Add("@p_Prefix", _clientDTO.Prefix);
                parameters.Add("@p_C6Username", _clientDTO.C6Username);
                parameters.Add("@p_C6Threshold", _clientDTO.C6Threshold);
                parameters.Add("@p_Threshold", _clientDTO.Threshold);
                parameters.Add("@p_createdBy", _clientDTO.CreatedBy);
                parameters.Add("@p_description", _clientDTO.Description);
                parameters.Add("@p_complem", _clientDTO.Complem);
                parameters.Add("@p_C6BaseUrl", _clientDTO.C6BaseUrl);
                parameters.Add("@p_ApplicationStartDate", _clientDTO.ApplicationStartDate);
                parameters.Add("@p_ApplicationEndDate", _clientDTO.ApplicationEndDate);
                parameters.Add("@p_SearchCount", _clientDTO.SearchCount);
                var response = ExecuteScalar("ins_client", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Client added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> UploadLogo(ClientMasterDTO _clientDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_client_id", _clientDTO.ClientId);
                parameters.Add("@p_document_name", _clientDTO.DocumentName);
                parameters.Add("@p_document_file_name", _clientDTO.DocumentFileName);
                parameters.Add("@p_document_full_path", _clientDTO.DocumentFullPath);
                parameters.Add("@p_documents_details", _clientDTO.DocumentDetails);
                parameters.Add("@p_created_by", _clientDTO.CreatedBy);
                parameters.Add("@p_type", _clientDTO.type);
                var response = ExecuteScalar("ins_client_logo", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Logo added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> DeleteClient(ClientMasterDTO _clientDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", _clientDTO.ClientId);
                parameters.Add("@p_isActive", _clientDTO.isActive);
                parameters.Add("@p_updatedBy", _clientDTO.CreatedBy);
                var response = ExecuteScalar("del_client", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Client Deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> UpdateClient(ClientMasterDTO _clientDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _clientDTO.ClientId);
                parameters.Add("@p_Client_Name", _clientDTO.ClientName);
                parameters.Add("@p_Prefix", _clientDTO.Prefix);
                parameters.Add("@p_C6Username", _clientDTO.C6Username);
                parameters.Add("@p_C6Threshold", _clientDTO.C6Threshold);
                parameters.Add("@p_Threshold", _clientDTO.Threshold);
                parameters.Add("@p_description", _clientDTO.Description);
                parameters.Add("@p_updatedBy", _clientDTO.CreatedBy);
                parameters.Add("@p_complem", _clientDTO.Complem);
                parameters.Add("@p_C6BaseUrl", _clientDTO.C6BaseUrl);
                parameters.Add("@p_ApplicationStartDate", _clientDTO.ApplicationStartDate);
                parameters.Add("@p_ApplicationEndDate", _clientDTO.ApplicationEndDate);
                parameters.Add("@p_SearchCount", _clientDTO.SearchCount);
                var response = ExecuteScalar("mod_client", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Client Updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> DeleteRightsByClientId(int ClientId)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", ClientId);
                var response = ExecuteScalar("del_clientright_by_clientid", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> CreateClientRight(ClientMenuRightsModelDTO _clientRightDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", _clientRightDTO.Client_Id);
                parameters.Add("@p_menuId", _clientRightDTO.Menu_Id);
                parameters.Add("@p_isActive", _clientRightDTO.is_Active);
                parameters.Add("@p_createdBy", _clientRightDTO.Created_By);
                var response = ExecuteScalar("ins_clientRight", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<CustomerCaseDTO> GetCaseFullDetailsByCustomerId(int _custId)
        {
            ServiceResponse<CustomerCaseDTO> serviceResponse = new ServiceResponse<CustomerCaseDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_id", _custId);
                serviceResponse.Result = GetFirstOrDefault<CustomerCaseDTO>("get_customercase_full_details_by_cust_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<CustomerCaseDTO> GetCaseStatusByCustomerId(string _custId)
        {
            ServiceResponse<CustomerCaseDTO> serviceResponse = new ServiceResponse<CustomerCaseDTO>();

            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_id", _custId);
                serviceResponse.Result = GetFirstOrDefault<CustomerCaseDTO>("get_customercase_status_by_custid", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        

        public ServiceResponse<CustomerCaseDTO> GetRiskStatusByCustomerId(string _custId,string customertype)
        {
            ServiceResponse<CustomerCaseDTO> serviceResponse = new ServiceResponse<CustomerCaseDTO>();

            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_id", _custId);
                parameters.Add("@p_customer_type", customertype);
                serviceResponse.Result = GetFirstOrDefault<CustomerCaseDTO>("get_customerrisk_status_by_custid", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }



        public ServiceResponse<CreatedAndUpdatedByNames> GetCreatedByAndUpdateByNameFromId(int createdById, int updatedById)
        {
            ServiceResponse<CreatedAndUpdatedByNames> serviceResponse = new ServiceResponse<CreatedAndUpdatedByNames>();

            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_created_by_id", createdById);
                parameters.Add("@p_updated_by_id", updatedById);
                serviceResponse.Result = GetFirstOrDefault<CreatedAndUpdatedByNames>("get_created_by_and_update_by_name_from_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Names fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<CustomerCaseDTO> GetCaseFullDetailsByCaseId(int _caseId)
        {
            ServiceResponse<CustomerCaseDTO> serviceResponse = new ServiceResponse<CustomerCaseDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_case_id", _caseId);
                serviceResponse.Result = GetFirstOrDefault<CustomerCaseDTO>("get_customercase_full_details_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<string> GetUserIdForAPI(string UserId)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_id", UserId);
                serviceResponse.Result = GetFirstOrDefault<string>("get_id_by_user_id", parameters, commandType: CommandType.StoredProcedure) ?? "-1";
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> Create(CustomerCaseDTO _CustomerCaseDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_master_id", _CustomerCaseDTO.CustomerMasterId);
                parameters.Add("@p_cust_id", _CustomerCaseDTO.CustomerId);///this is the id of the customermaster table which will be appended with prefix in the insert statement
                parameters.Add("@p_created_by", _CustomerCaseDTO.CreatedBy);
                parameters.Add("@p_match_category", _CustomerCaseDTO.MatchCategory);
                parameters.Add("@p_match_type", _CustomerCaseDTO.MatchType);
                parameters.Add("@p_clientId", _CustomerCaseDTO.ClientId);
                parameters.Add("@p_c6threshold", _CustomerCaseDTO.C6Threshold);
                parameters.Add("@p_threshold", _CustomerCaseDTO.Threshold);
                parameters.Add("@p_birthyear", _CustomerCaseDTO.BirthYear);
                parameters.Add("@p_gender", _CustomerCaseDTO.Gender);
                parameters.Add("@p_source", _CustomerCaseDTO.Source);
                parameters.Add("@p_is_matched", _CustomerCaseDTO.IsMatched);
                parameters.Add("@p_match_score", _CustomerCaseDTO.MatchScore);
                parameters.Add("@p_source_unique_id", _CustomerCaseDTO.SourceUniqueId);
                parameters.Add("@p_caseChangeStatus", _CustomerCaseDTO.CaseChangeStatus);
                parameters.Add("@p_status", _CustomerCaseDTO.Status);
                parameters.Add("@p_createdon", Convert.ToDateTime(_CustomerCaseDTO.CreatedOn));
                var response = ExecuteScalar("ins_customercase_prefix", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer case added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        //DNIRC


        public ServiceResponse<int> CreateScreening(CustomerCaseDTO _CustomerCaseDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_master_id", _CustomerCaseDTO.CustomerMasterId);
                parameters.Add("@p_cust_id", _CustomerCaseDTO.CustomerId);///this is the id of the customermaster table which will be appended with prefix in the insert statement
                parameters.Add("@p_created_by", _CustomerCaseDTO.CreatedBy);
                parameters.Add("@p_match_category", _CustomerCaseDTO.MatchCategory);
                parameters.Add("@p_match_type", _CustomerCaseDTO.MatchType);
                parameters.Add("@p_clientId", _CustomerCaseDTO.ClientId);
                parameters.Add("@p_c6threshold", _CustomerCaseDTO.C6Threshold);
                parameters.Add("@p_threshold", _CustomerCaseDTO.Threshold);
                var response = ExecuteScalar("ins_customercase_prefix", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer case added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<string> IndividualRisk(CustomerCaseDTO _CustomerCaseDTO)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                
                parameters.Add("p_profession", _CustomerCaseDTO.onb_profession != null ? _CustomerCaseDTO.onb_profession : "");
                parameters.Add("p_nationality", _CustomerCaseDTO.onb_nationality != null && _CustomerCaseDTO.onb_nationality != "0" ? _CustomerCaseDTO.onb_nationality : "");
                parameters.Add("p_residence", _CustomerCaseDTO.onb_residence_status != null ? _CustomerCaseDTO.onb_residence_status : "");
                parameters.Add("p_deliverychannel", _CustomerCaseDTO.onb_delv_channel != null ? _CustomerCaseDTO.onb_delv_channel : "");
                parameters.Add("p_type", _CustomerCaseDTO.onb_cust_type);
                serviceResponse.Result = (string)ExecuteScalar("get_risk_type_id_screening", parameters, commandType: CommandType.StoredProcedure) ?? null;
                serviceResponse.Message = "Individual risk type id fetched successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_risk_type_id_kyc'\n{ex.Message}");
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
              return serviceResponse;
        }


        public ServiceResponse<string> CorporateRisk(CustomerCaseDTO _CustomerCaseDTO)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                var incorporationPlace = " ";
                if (_CustomerCaseDTO.onb_nationality == "UNITED ARAB EMIRATES")
                {
                     incorporationPlace = "Within UAE";
                }
                else
                {
                    incorporationPlace = "Outside UAE";
                }
                parameters.Add("p_incorporationPlace", incorporationPlace);
                parameters.Add("p_delivery", _CustomerCaseDTO.onb_delv_channel != null ? _CustomerCaseDTO.onb_delv_channel : "");
                parameters.Add("p_mode_of_pymt", _CustomerCaseDTO.onb_mode_of_pymt != null ? _CustomerCaseDTO.onb_mode_of_pymt : "");
                parameters.Add("p_type", _CustomerCaseDTO.onb_cust_type);
                serviceResponse.Result = (string)ExecuteScalar("get_risk_type_id_Corporate_screening", parameters, commandType: CommandType.StoredProcedure) ?? null;
                serviceResponse.Message = "Corporate risk type id fetched successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_risk_type_id_kyc'\n{ex.Message}");
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }



        public ServiceResponse<List<CustomerMasterDTO>> GetUnscreenedCustomers()
        {
            ServiceResponse<List<CustomerMasterDTO>> serviceResponse = new ServiceResponse<List<CustomerMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<CustomerMasterDTO>("get_unscreened_customers", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(CustomerCaseDTO _CustomerCaseDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@p_cust_id", _CustomerCaseDTO.CustomerId);
                parameters.Add("@p_created_by", _CustomerCaseDTO.CreatedBy);
                parameters.Add("@p_cust_master_id", _CustomerCaseDTO.CustomerMasterId);
                parameters.Add("@p_is_matched", _CustomerCaseDTO.IsMatched);
                parameters.Add("@p_match_categoy", _CustomerCaseDTO.MatchCategory);
                parameters.Add("@p_match_score", _CustomerCaseDTO.MatchScore);
                parameters.Add("@p_match_type", _CustomerCaseDTO.MatchType);
                parameters.Add("@p_risk_score", _CustomerCaseDTO.RiskScore);
                parameters.Add("@p_source", _CustomerCaseDTO.Source);
                parameters.Add("@p_source_unique_id", _CustomerCaseDTO.SourceUniqueId);
                parameters.Add("@p_updated_by", _CustomerCaseDTO.UpdatedBy);
                parameters.Add("@p_status", _CustomerCaseDTO.Status);
                parameters.Add("@p_id", _CustomerCaseDTO.Id);
                parameters.Add("@p_comment", _CustomerCaseDTO.Comments);
                parameters.Add("@p_rollback_count", _CustomerCaseDTO.RollbackCount);
                parameters.Add("@p_nomatch", _CustomerCaseDTO.NoMatch);
                parameters.Add("@p_truedomesticpep", _CustomerCaseDTO.TrueDomesticpep);
                parameters.Add("@p_trueforeignpep", _CustomerCaseDTO.TrueForeignpep);
                parameters.Add("@p_trueadversemedia", _CustomerCaseDTO.TrueAdverseMedia);
                parameters.Add("@p_partialdomesticpep", _CustomerCaseDTO.PartialDomesticpep);
                parameters.Add("@p_partialforeignpep", _CustomerCaseDTO.PartialForeignpep);
                parameters.Add("@p_partialadversemedia", _CustomerCaseDTO.Partialadversemedia);
                parameters.Add("@p_trueuaeunsanction", _CustomerCaseDTO.TrueUAEUNSanction);
                parameters.Add("@p_trueothersanction", _CustomerCaseDTO.TrueOtherSanction);
                parameters.Add("@p_changestatus", _CustomerCaseDTO.CaseChangeStatus);
                parameters.Add("@p_schedulerTrackerId", _CustomerCaseDTO.ScheduelerTrackerId);
                parameters.Add("@p_schedulerprocessedon", _CustomerCaseDTO.SchedulerProcessedOn);
                var response = ExecuteScalar("mod_customercase", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer case added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> UpdateCase(int caseid,int userid)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                
                parameters.Add("@p_updated_by", userid);
                
                parameters.Add("@p_id",caseid);
                

                var response = ExecuteScalar("mod_customercase_updatedate", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer case added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Delete(int Id)
        {
            throw new NotImplementedException();
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
        private DataTable ReadExcelAsDataTable(string fileName, int sheetNo)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream = System.IO.File.Open(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });

                    if (result.Tables.Count == 0)
                        throw new Exception("No worksheets found in the Excel file.");

                    if (result.Tables.Count < sheetNo)
                        throw new Exception($"Worksheet {sheetNo} not found. The file only contains {result.Tables.Count} worksheet(s).");

                    return result.Tables[sheetNo - 1];
                }
            }
        }

        public ServiceResponse<List<CustomerExcelDTO>> LoadCustomerCaseExcelData(string fileName, string customertype, int sheetNo = 1)
        {
            log.Debug($"LoadCustomerCaseExceldata customer type {fileName}");
            ServiceResponse<List<CustomerExcelDTO>> serviceResponse = new ServiceResponse<List<CustomerExcelDTO>>();
            try
            {
                List<CustomerExcelDTO> excelData = new List<CustomerExcelDTO>();
                DataTable table = ReadExcelAsDataTable(fileName, sheetNo);

                foreach (DataRow row in table.Rows)
                {
                    //if (row[0] != DBNull.Value)
                    //{
                        if (customertype == "I")
                        {
                            excelData.Add(new CustomerExcelDTO()
                            {
                                CustomerID = row[0] == DBNull.Value ? "" : Convert.ToString(row[0]),
                                LastName = Convert.ToString(row[1]),
                                Nationality = Convert.ToString(row[2]),
                                DOB = Convert.ToString(row[3]),
                                CustomerIdType = Convert.ToString(row[4]),
                                CustomerIdNumber = Convert.ToString(row[5]),
                                IDexpiry = Convert.ToString(row[6]),
                                Remarks = Convert.ToString(row[7]),
                                OccupatinTypeTxt= Convert.ToString(row[8]),
                                ResidenceStatus= Convert.ToString(row[9]),
                                ProductName = Convert.ToString(row[10]),
                                DeliveryChannelName= Convert.ToString(row[11]),
                                Modeofpayment= Convert.ToString(row[12]),
                                Status = 0
                            });
                        }
                        else
                        {
                            excelData.Add(new CustomerExcelDTO()
                            {
                                CustomerID = row[0] == DBNull.Value ? "" : Convert.ToString(row[0]),
                                LastName = Convert.ToString(row[1]),
                                Nationality = Convert.ToString(row[2]),
                                DOB = Convert.ToString(row[3]),
                                Status = 0
                            });
                        }
                    //}
                }
                serviceResponse.Message = "Excel Data Loaded.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                serviceResponse.Result = excelData;
                return serviceResponse;
            }
            catch (Exception ex)
            {
                log.Debug($"error message {ex.Message} ");
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CorporateExcelDTO>> LoadCorporateCaseExcelData(string fileName, int sheetNo = 1)
        {
            ServiceResponse<List<CorporateExcelDTO>> serviceResponse = new ServiceResponse<List<CorporateExcelDTO>>();
            try
            {
                List<CorporateExcelDTO> excelData = new List<CorporateExcelDTO>();
                DataTable table = ReadExcelAsDataTable(fileName, sheetNo);

                foreach (DataRow row in table.Rows)
                {
                    //if (row[0] != DBNull.Value)
                    //{
                        excelData.Add(new CorporateExcelDTO()
                        {
                            CustomerID = row[0] == DBNull.Value ? "" : Convert.ToString(row[0]),
                            EntityName = Convert.ToString(row[1]),
                            CountryofIncorporation = Convert.ToString(row[2]),
                            DateofIncorporation = Convert.ToString(row[3]),
                            EntityTypeTxt= Convert.ToString(row[4]),
                            BusinessType= Convert.ToString(row[5]),
                            ProductName = Convert.ToString(row[6]),
                            DeliveryChannelName= Convert.ToString(row[7]),
                            Modeofpayment= Convert.ToString(row[8]),

                            Status = 0
                        });

                    //}
                }
                serviceResponse.Message = "Excel Data Loaded.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                serviceResponse.Result = excelData;
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<IndividualExcel>> LoadIndividualRiskExcelData(string fileName, int sheetNo = 1)
        {
            ServiceResponse<List<IndividualExcel>> serviceResponse = new ServiceResponse<List<IndividualExcel>>();
            try
            {
                List<IndividualExcel> excelData = new List<IndividualExcel>();
                DataTable table = ReadExcelAsDataTable(fileName, sheetNo);

                if (table == null)
                {
                    serviceResponse.Message = "Failed to load worksheet data.";
                    serviceResponse.Status = StaticResource.FailStatusCode;
                    return serviceResponse;
                }

                foreach (DataRow row in table.Rows)
                {
                    if (row[0] != DBNull.Value)
                    {
                        excelData.Add(new IndividualExcel()
                        {
                            CustomerId = Convert.ToString(row[0]),
                            CustomerName = Convert.ToString(row[1]),
                            DOB = Convert.ToString(row[2]),
                            Nationality = Convert.ToString(row[3]),
                            CustomerType = Convert.ToString(row[4]),
                            Profession = Convert.ToString(row[5]),
                            ResidenceStatus = Convert.ToString(row[6]),
                            Product = Convert.ToString(row[7]),
                            DeliveryChannel = Convert.ToString(row[8]),
                            ModeOfPayment = Convert.ToString(row[9]),
                            MoreProduct = Convert.ToString(row[10]),
                            Screened = Convert.ToString(row[11]),
                            IsAdverse = Convert.ToString(row[12])
                        });
                    }
                }
                serviceResponse.Message = "Excel Data Loaded.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                serviceResponse.Result = excelData;
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CorporateExcel>> LoadCorporateRiskExcelData(string fileName, int sheetNo = 1)
        {
            ServiceResponse<List<CorporateExcel>> serviceResponse = new ServiceResponse<List<CorporateExcel>>();
            try
            {
                List<CorporateExcel> excelData = new List<CorporateExcel>();
                DataTable table = ReadExcelAsDataTable(fileName, sheetNo);

                if (table == null)
                {
                    serviceResponse.Message = "Failed to load worksheet data.";
                    serviceResponse.Status = StaticResource.FailStatusCode;
                    return serviceResponse;
                }

                foreach (DataRow row in table.Rows)
                {
                    if (row[0] != DBNull.Value)
                    {
                        excelData.Add(new CorporateExcel()
                        {
                            CustomerId = Convert.ToString(row[0]),
                            LegalNameOfEntity = Convert.ToString(row[1]),
                            CountryOfIncorporationTxt = Convert.ToString(row[3]),
                            CustomerType = Convert.ToString(row[4]),
                            Profession = Convert.ToString(row[5]),
                            ResidenceStatus = Convert.ToString(row[6]),
                            Product = Convert.ToString(row[7]),
                            DeliveryChannel = Convert.ToString(row[8]),
                            ModeOfPayment = Convert.ToString(row[9]),
                            MoreProduct = Convert.ToString(row[10]),
                            Screened = Convert.ToString(row[11]),
                            IsAdverse = Convert.ToString(row[12]),
                            FATF = Convert.ToString(row[13]),
                            Nat1 = Convert.ToString(row[14]),
                            Nat2 = Convert.ToString(row[15]),
                            Nat3 = Convert.ToString(row[16])
                        });
                    }
                }
                serviceResponse.Message = "Excel Data Loaded.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                serviceResponse.Result = excelData;
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<EtlBatchDTO>> DataLoadReport(ETLDataLoadReportDTO _ETLDataLoadReportDTO)
        {
            ServiceResponse<List<EtlBatchDTO>> serviceResponse = new ServiceResponse<List<EtlBatchDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", _ETLDataLoadReportDTO.ClientId);
                parameters.Add("@p_startDate", _ETLDataLoadReportDTO.FromDate);
                parameters.Add("@p_endDate", _ETLDataLoadReportDTO.ToDate);
                parameters.Add("@p_matchtype", _ETLDataLoadReportDTO.MatchType);
                serviceResponse.Result = Get<EtlBatchDTO>("get_etl_dataload_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Report fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CustomerCaseDTO>> DataLoadReportByBatch(int BatchId, int clientId)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_batchId", BatchId);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<CustomerCaseDTO>("get_etl_dataload_report_by_batch", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Report fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<List<CodesTableDTO>> GetCodesByClientID(int clientId)
        {
            ServiceResponse<List<CodesTableDTO>> serviceResponse = new ServiceResponse<List<CodesTableDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@clientId", clientId);
                serviceResponse.Result = Get<CodesTableDTO>("get_codes_by_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Codes fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
            //return null;

        }

        public ServiceResponse<ClientMasterDTO> GetCustomerCodeprefixByclient(int clientId)
        {
            ServiceResponse<ClientMasterDTO> serviceResponse = new ServiceResponse<ClientMasterDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@clientId", clientId);
                serviceResponse.Result = GetFirstOrDefault<ClientMasterDTO>("get_customerCode_Prefix_By_Client", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Codes fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
           

        }

        public ServiceResponse<int> InsertScreeninglogs(ScreeinglogsModel model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_individual", model.Individual);
                parameters.Add("@p_corporate", model.Corporate);
                parameters.Add("@p_createdon", model.CreatedOn);
                parameters.Add("@p_updatedon", model.UpdateOn);
                parameters.Add("@p_delete_ind", model.is_delete_ind);
                parameters.Add("@p_delete_corp", model.is_delete_corp);
                var response = ExecuteScalar("ins_screening_logs", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer case added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        //public ServiceResponse<string> CreatePassportDetails(PassportDetails passport)
        //{
        //    ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
        //    try
        //    {
        //        DynamicParameters parameters = new DynamicParameters();
        //        parameters.Add("p_caseid", passport.CaseId);
        //        parameters.Add("p_passportNo", passport.PassportNo);
        //        parameters.Add("p_passportIssueplace", passport.PassportIssusePlace);
        //        parameters.Add("p_passportIssuedate", passport.PassportIssuesDate);
        //        parameters.Add("p_passportExpirydate", passport.PassportExpiryDate);
        //        parameters.Add("p_createdon", passport.CreatedOn);
        //        parameters.Add("p_createdBy", passport.CreatedBy);
        //        parameters.Add("p_clientid", passport.ClientId);
        //        var response = ExecuteScalar("ins_passport_details", parameters, commandType: CommandType.StoredProcedure).ParseString();
        //        serviceResponse.Result = response;
        //        serviceResponse.Message = "Customer master added successfully.";
        //        serviceResponse.Status = StaticResource.SuccessStatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        serviceResponse.Message = ex.Message;
        //        serviceResponse.Status = StaticResource.FailStatusCode;
        //    }
        //    return serviceResponse;
        //}


        public ServiceResponse<List<PassportDetailsDTO>> GetPassportdetails(int CaseId)
        {
            ServiceResponse<List<PassportDetailsDTO>> serviceResponse = new ServiceResponse<List<PassportDetailsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_caseid", CaseId);
                serviceResponse.Result = Get<PassportDetailsDTO>("get_all_passportDetails_by_id", parameters, commandType: CommandType.StoredProcedure).ToList();
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
        public ServiceResponse<List<CustomerCaseDTO>> GetDuplicateNames(string fullname,string type,int clientid)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_fullname", fullname);
                parameters.Add("@p_type", type);
                parameters.Add("@p_clientid", clientid);
                serviceResponse.Result = Get<CustomerCaseDTO>("verify_duplicate_name", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CustomerCaseDTO>> GetCompanyCode(string CompanyCode, int clientid,string CustomerType)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_companycode", CompanyCode);
                parameters.Add("@p_clientid", clientid);
                parameters.Add("@p_customertype", CustomerType);
                serviceResponse.Result = Get<CustomerCaseDTO>("get_all_company_code", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<ShareholderDTO>> GetAllShareHolders(int clientid,string companyCode,int userId,string customerType)
        {
            ServiceResponse<List<ShareholderDTO>> serviceResponse = new ServiceResponse<List<ShareholderDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientid", clientid);
                parameters.Add("@p_companyCode", companyCode);
                parameters.Add("@p_userid", userId);
                parameters.Add("@p_customerType", customerType);
                serviceResponse.Result = Get<ShareholderDTO>("get_all_shareholders", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<bool> GetRiskCategoryStatus(string Customertype, int ClientId, string Categoryname)
        {
            ServiceResponse<bool> serviceResponse = new ServiceResponse<bool>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", ClientId);
                parameters.Add("@p_customertype", Customertype);
                parameters.Add("@p_categoryname", Categoryname);
                serviceResponse.Result = GetFirstOrDefault<bool>("get_all_riskcategory", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Codes fetched successfully.";
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
