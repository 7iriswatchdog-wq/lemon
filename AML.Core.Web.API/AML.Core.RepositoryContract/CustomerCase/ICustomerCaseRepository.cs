using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.EtlBatch;
using System;
using System.Collections.Generic;
using System.Text;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CodesMaster;
using AML.ViewModel.ViewModels.ApiAuthentication;
using System.Threading.Tasks;
using AML.ViewModel.ViewModels.Kyc;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace AML.Core.RepositoryContract.CustomerCase
{
    public interface ICustomerCaseRepository : IBaseRepository
    {
        ServiceResponse<int> Create(CustomerCaseDTO _CustomerCaseDTO);
        //DNIRC
        ServiceResponse<int> CreateScreening(CustomerCaseDTO _CustomerCaseDTO);

        ServiceResponse<string> IndividualRisk(CustomerCaseDTO _CustomerCaseDTO);

        ServiceResponse<string> CorporateRisk(CustomerCaseDTO _CustomerCaseDTO);



        ServiceResponse<CustomerCaseDTO> GetDetails(int Id);

        ServiceResponse<List<CustomerCaseDTO>> GetShareHoldersByCompanyCode(string Id);
        int GetCaseId(string CustId);


        ServiceResponse<int> Update(CustomerCaseDTO _CustomerCaseDTO);
        ServiceResponse<int> UpdateCase(int caseid,int userid);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<List<CustomerMasterDTO>> GetUnscreenedCustomers();
        ServiceResponse<List<CustomerCaseDTO>> GetAll(int userId, string startDate, string endDate, string cust_type, string matchscore,int createdBy,int caseStatus,string riskLevel, string caseStatusChange, string usergroupName);

        ServiceResponse<List<CustomerCaseDTO>> GetAllCompletedCases(int userId, string startDate, string endDate, string cust_type);

        ServiceResponse<List<CustomerCaseDTO>> GetAllBySearchValue(int userId, string startDate, string endDate, string cust_type, string searchValue,string usergroupName);

        ServiceResponse<List<CustomerCaseDTO>> GetAllCompletedBySearchValue(int userId, string startDate, string endDate, string cust_type, string searchValue);
        ServiceResponse<List<CustomerCaseDTO>> GetAllSanctionDashboard(int clientid, string ctype);
        ServiceResponse<List<CustomerExcelDTO>> LoadCustomerCaseExcelData(string fileName,string CustomerType, int sheetNo = 1);
        ServiceResponse<List<CorporateExcelDTO>> LoadCorporateCaseExcelData(string fileName, int sheetNo = 1);
        ServiceResponse<List<IndividualExcel>> LoadIndividualRiskExcelData(string fileName, int sheetNo = 1);
        ServiceResponse<List<CorporateExcel>> LoadCorporateRiskExcelData(string fileName, int sheetNo = 1);


        ServiceResponse<List<EtlBatchDTO>> DataLoadReport(ETLDataLoadReportDTO _ETLDataLoadReportDTO);
        ServiceResponse<List<CustomerCaseDTO>> DataLoadReportByBatch(int BatchId, int clientId);
        ServiceResponse<CustomerCaseDTO> GetCaseFullDetailsByCustomerId(string _custId);
        ServiceResponse<CustomerCaseDTO> GetCaseFullDetailsByCustomerId(int _custId);
        ServiceResponse<CustomerCaseDTO> GetCaseFullDetailsByCaseId(int _caseId);
        ServiceResponse<string> GetUserIdForAPI(string UserId);
        ServiceResponse<CustomerCaseDTO> GetCaseStatusByCustomerId(string newCustMasterId);
        ServiceResponse<CustomerCaseDTO> GetRiskStatusByCustomerId(string newCustMasterId,string customertype);
        ServiceResponse<CreatedAndUpdatedByNames> GetCreatedByAndUpdateByNameFromId(int createdById, int updatedById);
        ServiceResponse<List<CustomerCaseDTO>> GetApprovedList(int clientId);

        ServiceResponse<List<CustomerCaseDTO>> GetCasespendingScheduler();
        int InsertDigiSchedulerLogs(int totalHits, int totalRecords, int clientId);
        ServiceResponse<List<ClientMasterDTO>> GetAllClients();
        ServiceResponse<ClientMasterDTO> GetClientDetailsByID(int clientId);

 
        ServiceResponse<int> UpdateClient(ClientMasterDTO _ClientDTO);
        ServiceResponse<int> CreateClient(ClientMasterDTO _ClientDTO);
        ServiceResponse<int> UploadLogo(ClientMasterDTO _ClientDTO);
        ServiceResponse<List<ClientRightsDTO>> GetClientRightsByClientId(int clientId);
        ServiceResponse<List<MenuModelDTO>> GetAllMenus();
        ServiceResponse<int> DeleteRightsByClientId(int ClientId);
        ServiceResponse<int> CreateClientRight(ClientMenuRightsModelDTO _clientRightDTO);
        ServiceResponse<int> DeleteClient(ClientMasterDTO _clientDTO);

        ServiceResponse<List<CodesTableDTO>> GetCodesByClientID(int ClientId);

        ServiceResponse<ClientMasterDTO> GetCustomerCodeprefixByclient(int ClientId);


        ServiceResponse<int> InsertScreeninglogs(ScreeinglogsModel model);

        Task<ServiceResponse<List<ClientMasterDTO>>> GetAllClientsAsync();

        Task<ServiceResponse<List<CustomerCaseDTO>>> GetApprovedListAsync(int clientId);

        //ServiceResponse<string> CreatePassportDetails(PassportDetails passport);

        //ServiceResponse<List<PassportDetailsDTO>> GetPassportdetails(int caseid);

        ServiceResponse<List<CustomerCaseDTO>> GetDuplicateNames(string FullName, string type, int clientId);

        ServiceResponse<List<CustomerCaseDTO>> GetCompanyCode(string CompanyCode, int clientId);

        ServiceResponse<List<ShareholderDTO>> GetAllShareHolders(int clientId,string companyCode);


    }
}
