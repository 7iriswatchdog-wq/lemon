using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Report;
using AML.Web.Controllers.Reports;
using System.Collections.Generic;

namespace AML.Core.RepositoryContract.Report
{
    public interface IReportRepository
    {
        ServiceResponse<List<CaseReportListDTO>> GetCaseReportList(CaseReportRequestDTO requestModel);

        ServiceResponse<List<CaseReportListDTO>> GetCaseReportListBySearch(CaseReportRequestDTO requestModel);

        ServiceResponse<List<CaseReportListDTO>> GetCaseReportListBySchedulerTrackerId(CaseReportRequestDTO requestModel);

        
        ServiceResponse<List<CaseReportListDTO>> GetCaseManagementReportList(CaseReportRequestDTO requestModel);

        ServiceResponse<List<CaseReportListDTO>> GetCaseManagementSearchValueReportList(CaseReportRequestDTO requestModel);

        ServiceResponse<List<CaseReportListDTO>> GetCompletedCaseReportList(CaseReportRequestDTO requestModel);
        ServiceResponse<List<CaseReportListDTO>> GetCasePreviousWeekReportList(CaseReportRequestDTO requestModel);
        ServiceResponse<int> GetCustomerCaseCount(int status,int clientId);

        ServiceResponse<int> GetCustomerTypeCount(string customertype, int clientId);

        ServiceResponse<int> GetapprovedCaseCount(int status, int clientId);
        ServiceResponse<List<CaseReportListDTO>> GetCustomerReportList(ReportLogSearchModel requestModel);
        ServiceResponse<List<UploadLogsListDTO>> GetUploadLogstList();
        ServiceResponse<List<DigiSchedulerLogsDTO>> GetDigiSchedulerList(int clientId);
        ServiceResponse<List<DigiSchedulerLogsDTO>> GetDigiSchedulerList(int clientId, string startDate, string endDate);
        ServiceResponse<List<RiskDashboardDTO>> GetRiskCount(int category,int clientId);
        ServiceResponse<List<CaseReportListDTO>> GetKycReportList(CaseReportRequestDTO requestModel);

        ServiceResponse<List<CaseReportListDTO>> GetLatestDate(int clientId);

        ServiceResponse<int> GetClientCountReportList(CaseReportRequestDTO requestModel);

        ServiceResponse<List<ScreeningDatabaseLogDTO>> GetScreeningDatabaseLogs(CaseReportRequestDTO model);
        ServiceResponse<List<DatasetUpdateLogDTO>> GetDatasetUpdateLogs(CaseReportRequestDTO model);
    }
}
