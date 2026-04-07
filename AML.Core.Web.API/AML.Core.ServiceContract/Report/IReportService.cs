using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Report;
using AML.Web.Controllers.Reports;
using System.Collections.Generic;

namespace AML.Core.ServiceContract.Report
{
    public interface IReportService
    {
        List<CaseReportListDTO> GetCaseReportList(CaseReportRequestDTO model);

        List<CaseReportListDTO> GetCaseReportListBySearch(CaseReportRequestDTO model);

        List<CaseReportListDTO> GetCaseReportListBySchedulerTrackerId(CaseReportRequestDTO model);

        

        List<CaseReportListDTO> GetCaseManagementReportList(CaseReportRequestDTO model);

        List<CaseReportListDTO> GetCaseManagementSearchValueReportList(CaseReportRequestDTO model);
        List<CaseReportListDTO> GetCompletedCaseReportList(CaseReportRequestDTO model);
        List<CaseReportListDTO> GetCasePreviousWeekReportList(CaseReportRequestDTO model);
        int GetCustomerCaseCount(int status,int clientId);

        int GetCustomerTypeCount(string customertype, int clientId);

        int GetapprovedCaseCount(int status, int clientId);
        List<CaseReportListDTO> GetCustomerReportList(ReportLogSearchModel model);

        int GetClientCountReportList(CaseReportRequestDTO model);
        List<UploadLogsListDTO> GetUploadLogstList();
        List<DigiSchedulerLogsDTO> GetDigiSchedulerList(int clientId); 
        List<DigiSchedulerLogsDTO> GetDigiSchedulerList(int clientId, string startDate, string endDate);
        List<RiskDashboardDTO> GetRiskCount(int status,int clientId);
        List<CaseReportListDTO> GetKycReportList(CaseReportRequestDTO model);
        List<CaseReportListDTO> GetLatestDate(int clientid);

        List<ScreeningDatabaseLogDTO> GetScreeningDatabaseLogs(CaseReportRequestDTO model);
        List<DatasetUpdateLogDTO> GetDatasetUpdateLogs(CaseReportRequestDTO model);
    }
}
