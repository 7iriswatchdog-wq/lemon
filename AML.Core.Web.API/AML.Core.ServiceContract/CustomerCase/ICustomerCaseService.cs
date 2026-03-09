using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CodesMaster;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.EtlBatch;
using AML.ViewModel.ViewModels.ApiAuthentication;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.RiskAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Core.ServiceContract.CustomerCase
{
    public interface ICustomerCaseService : IBaseService
    {
        string Create(CustomerCaseDTO _customerCaseDTO, bool returnId = false);
        ServiceResponse<string> Create(CustomerCaseDTO _customerCaseDTO);

        ServiceResponse<string> customerIdcheck(string CustomerId);
        //DNIRC
        ServiceResponse<string> CreateScreening(CustomerCaseDTO _customerCaseDTO);
        ServiceResponse<string> IndividualRiskCreation(CustomerCaseDTO _customerCaseDTO);
        ServiceResponse<string> CorporateRiskCreation(CustomerCaseDTO _customerCaseDTO);

        string GetUserIdForAPI(string UserId);
        ServiceResponse<string> CreatePrefix(CustomerCaseDTO _customerCaseDTO);
        CustomerCaseDTO GetDetails(int Id);

        List<CustomerCaseDTO> GetShareHoldersByCompanyCode(string Id);
        List<CustomerMasterDTO> GetUnscreenedCustomers();
        int GetCaseId(string custId);
        ServiceResponse<int> Update(CustomerCaseDTO _customerCaseDTO);

        ServiceResponse<int> UpdateCase(int caseid,int userid);
        ServiceResponse<int> Delete(int Id);
        List<CustomerCaseDTO> GetAll(int userId, string startDate, string endDate, string cust_type,string matchscore,int createdBy,int caseStatus,string riskLevel, string usergroupName);

        List<CustomerCaseDTO> GetAllCompletedCases(int userId, string startDate, string endDate, string cust_type, string matchscore, int createdBy, int caseStatus, string riskLevel);

        List<CustomerCaseDTO> GetAllCompletedBySearchValue(int userId, string startDate, string endDate, string cust_type, string SearchValue, string matchscore, int createdBy, int caseStatus, string riskLevel);

        List<CustomerCaseDTO> GetAllBySearchValue(int userId, string startDate, string endDate, string cust_type,string SearchValue, string matchscore, int createdBy, int caseStatus, string riskLevel,string usergroupName);
        List<CustomerCaseDTO> GetAllSanctionDashboard(int clientid, string ctype);
        ServiceResponse<List<EtlBatchDTO>> DataLoadReport(ETLDataLoadReportDTO _ETLDataLoadReportDTO);
        ServiceResponse<List<CustomerCaseDTO>> DataLoadReportByBatch(int BatchId, int clientId);
        ServiceResponse<List<CustomerExcelDTO>> SaveCustomerCaseExcelData(DocumentsDTO _documentsModel, string typeId, int C6Threshold, int Threshold,string screeningoption);
        ServiceResponse<List<CorporateExcelDTO>> SaveCorporateCaseExcelData(DocumentsDTO _documentsModel, string typeId, int C6Threshold, int Threshold, string CompanyCode,string screeningoption);
        CustomerCaseDTO GetCaseFullDetailsByCaseId(int _newCaseId);
        CustomerCaseDTO GetCaseFullDetailsByCustId(string _newCustMasterId);
        CustomerCaseDTO GetCaseFullDetailsByCustId(int _newCustMasterId);
        (List<string>, CustomerCaseDTO) SaveCorporateScreeningDetails(CorporateScreeningDTO model);
        List<CustomerMasterDTO> GetCustomerMasterByCodePrefix(string prefix);
        List<CustomerMasterDTO> GetCustomerMasterByCode(string customerType, int clientId);
        List<CustomerMasterDTO> GetCustomerMasterByCodePrefixAndCode(string prefix, string code);
        CustomerCaseDTO GetCaseStatusByCustId(string newCustMasterId);

        CustomerCaseDTO GetRiskStatusByCustId(string newCustMasterId,string customertype);
        CreatedAndUpdatedByNames GetCreatedByAndUpdateByNameFromId(int createdById, int updatedById);
        List<CustomerCaseDTO> GetCasebyApprovedStatus(int clientId);

        List<CustomerCaseDTO> GetCasespendingScheduler();
        int InsertDigiSchedulerLogs(int totalHits, int totalRecords, int clientId);
        List<ClientMasterDTO> GetAllClients();
        List<ClientMasterDTO> GetAllAdminClients();

        Task<ServiceResponse<List<ClientMasterDTO>>> GetAllClientsAsync();

        Task<ServiceResponse<List<CustomerCaseDTO>>> GetCasebyApprovedStatusAsync(int clientId);

        ClientMasterDTO GetClientDetailsByID(int clientId);

        
        ServiceResponse<int> UpdateClient(ClientMasterDTO _clientDTO);
        ServiceResponse<int> CreateClient(ClientMasterDTO _clientDTO);
        ServiceResponse<int> UploadLogo(ClientMasterDTO _clientDTO);
        List<ClientRightsDTO> GetClientRightsByClientId(int clientId);
        List<MenuModelDTO> GetAllMenus();
        ServiceResponse<int> DeleteRightsByClientId(int clientId);
        ServiceResponse<int> CreateClientRight(ClientMenuRightsModelDTO _clientRightDTO);
        ServiceResponse<int> DeleteClient(ClientMasterDTO _clientDTO);
        List<string> GroupEntityScreeningDetails(CorporateScreeningDTO model);

        List<CodesTableDTO> GetCodesByClientID(int clientId);

        ClientMasterDTO GetCustomerCodeprefixByclient(int clientId);

        ServiceResponse<int> InsertScreeninglogs(ScreeinglogsModel model);

        //test 


        (List<Tuple<string, string>>, CustomerCaseDTO) SaveCorporateScreening(CorporateScreeningDTO model);
        //risk Bulkupload
        Tuple<ServiceResponse<List<IndividualExcel>>, RiskAPIRequestModel> SaveIndividualRiskExcelData(DocumentsDTO _documentsModel, string typeId, int C6Threshold, int Threshold);
        Tuple<ServiceResponse<List<CorporateExcel>>, RiskAPIRequestModel> SaveCorporateRiskExcelData(DocumentsDTO _documentsModel, string typeId, int C6Threshold, int Threshold);


        //ServiceResponse<string> CreatePassportDetails(PassportDetails passport);

        //List<PassportDetailsDTO> GetPassportdetails(int Id);

        List<CustomerCaseDTO> GetDuplicateNames(string FullName,string type,int clientid);

        List<CustomerCaseDTO> GetCompanyCode(string CompanyCode, int clientid,string CustomerType);

        ServiceResponse<string> CreateShareholdersData(ShareholderDTO _customerCaseDTO);

        List<ShareholderDTO> GetAllShareHolders(int clientid,string shareholders,int userId,string CustomerType);

        ServiceResponse<string> DeleteShareholders(int id);


        ServiceResponse<string> DeletePendingShareholders(string companyCode);


    }
}
