using AML.Core.RepositoryContract.Report;
using AML.Core.ServiceContract.Report;
using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Report;
using AML.Web.Controllers.Reports;
using System.Collections.Generic;

namespace AML.Core.Service.Report
{
    public class ReportService: IReportService
    {
        private IReportRepository _reportRepository;
        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }
        public List<CaseReportListDTO> GetCaseReportList(CaseReportRequestDTO model)
        {
            //Perform business requirements here
            return _reportRepository.GetCaseReportList(model).Result;
        }
        public List<CaseReportListDTO> GetCaseReportListBySearch(CaseReportRequestDTO model)
        {
            //Perform business requirements here
            return _reportRepository.GetCaseReportListBySearch(model).Result;
        }
        public List<CaseReportListDTO> GetCaseReportListBySchedulerTrackerId(CaseReportRequestDTO model)
        {
            //Perform business requirements here
            return _reportRepository.GetCaseReportListBySchedulerTrackerId(model).Result;
        }
        
        public List<CaseReportListDTO> GetCaseManagementReportList(CaseReportRequestDTO model)
        {
            //Perform business requirements here
            return _reportRepository.GetCaseManagementReportList(model).Result;
        }
        public List<CaseReportListDTO> GetCaseManagementSearchValueReportList(CaseReportRequestDTO model)
        {
            //Perform business requirements here
            return _reportRepository.GetCaseManagementSearchValueReportList(model).Result;
        }
        public List<CaseReportListDTO> GetCompletedCaseReportList(CaseReportRequestDTO model)
        {
            //Perform business requirements here
            return _reportRepository.GetCompletedCaseReportList(model).Result;
        }
        public List<CaseReportListDTO> GetCasePreviousWeekReportList(CaseReportRequestDTO model)
        {
            //Perform business requirements here
            return _reportRepository.GetCasePreviousWeekReportList(model).Result;
        }
        public List<CaseReportListDTO> GetLatestDate(int clientid)
        {
            //Perform business requirements here
            return _reportRepository.GetLatestDate(clientid).Result;
        }
        public int GetCustomerCaseCount(int status,int clientId)
        {
            return _reportRepository.GetCustomerCaseCount(status,clientId).Result;
        }

        public int GetCustomerTypeCount(string customertype, int clientId)
        {
            return _reportRepository.GetCustomerTypeCount(customertype,clientId).Result;
        }

        public int GetapprovedCaseCount(int status, int clientId)
        {
            return _reportRepository.GetapprovedCaseCount(status, clientId).Result;
        }
        public List<RiskDashboardDTO> GetRiskCount(int category,int clientId)
        {
            return _reportRepository.GetRiskCount(category,clientId).Result;
        }
        public List<CaseReportListDTO> GetCustomerReportList(ReportLogSearchModel model)
        {
            //Perform business requirements here
            return _reportRepository.GetCustomerReportList(model).Result;
        }
        public List<UploadLogsListDTO> GetUploadLogstList()
        {
            return _reportRepository.GetUploadLogstList().Result;
        }

        public List<DigiSchedulerLogsDTO> GetDigiSchedulerList(int clientId)
        {
            return _reportRepository.GetDigiSchedulerList(clientId).Result;
        }

        public List<DigiSchedulerLogsDTO> GetDigiSchedulerList(int clientId, string startDate, string endDate)
        {
            return _reportRepository.GetDigiSchedulerList(clientId, startDate, endDate).Result;
        }
        public List<CaseReportListDTO> GetKycReportList(CaseReportRequestDTO model)
        {
            //Perform business requirements here
            return _reportRepository.GetKycReportList(model).Result;
        }
        public int GetClientCountReportList(CaseReportRequestDTO model)
        {
            //Perform business requirements here
            return _reportRepository.GetClientCountReportList(model).Result;
        }
        public List<ScreeningDatabaseLogDTO> GetScreeningDatabaseLogs(CaseReportRequestDTO model)
        {
            return _reportRepository.GetScreeningDatabaseLogs(model).Result;
        }
        public List<DatasetUpdateLogDTO> GetDatasetUpdateLogs(CaseReportRequestDTO model)
        {
            return _reportRepository.GetDatasetUpdateLogs(model).Result;
        }
    }
}
