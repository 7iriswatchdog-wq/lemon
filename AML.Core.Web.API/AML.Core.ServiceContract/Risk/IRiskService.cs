using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Risk;
using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Risk;
using System.Collections.Generic;
using System;

namespace AML.Core.ServiceContract.Risk
{
    public interface IRiskService
    {
        ServiceResponse<int> Create(RiskDTO model);
        ServiceResponse<int> CreateCorpCustomerRisk(RiskCorpCustomerDTO model);
        ServiceResponse<int> CreateBankRisk(RiskAssessmentBankDTO model);
        ServiceResponse<int> CreateVendorRisk(RiskAssessmentVendorDTO model);
        ServiceResponse<List<RiskSummaryDTO>> GetRiskSummary(DateTime date,int clientId);
        ServiceResponse<List<RiskReportDTO>> GetRiskReportBetweenDateAndType(int clientId, string createdByUserID, DateTime fromDate, DateTime toDate, string riskType, bool withDuplicate = false, int riskLevel = 0);

       List<RiskReportDTO> GetLastestRiskVersion(string customerId,string customertype);

        List<RiskReportDTO> GetAllRiskVersion(string customerId, string customertype);

        ServiceResponse<List<RiskReportDTO>> GetAllVersionRiskReportByid(string customerCode, string riskType,int clientId);
        ServiceResponse<List<RiskExcelReportDTO>> GetRiskReportForExcel(DateTime fromDate, DateTime toDate, string createdByUserID, string riskType, int riskLevel, int clientId);
        ServiceResponse<RiskDTO> GetRiskDetailsOfIndividual(int id);
        ServiceResponse<RiskCorpCustomerDTO> GetRiskDetailsOfCorporate(int id);
        ServiceResponse<RiskAssessmentBankDTO> GetRiskDetailsOfBank(int id);
        ServiceResponse<RiskAssessmentVendorDTO> GetRiskDetailsOfVendor(int id);
        //softdelete

        ServiceResponse<int> DeleteRiskIndiviual(int id);
        ServiceResponse<int> DeleteRiskCorporate(int id);
        ServiceResponse<int> DeleteRiskBank(int id);
        ServiceResponse<int> DeleteRiskVendor(int id);


        //DNIRC

        ServiceResponse<RiskDTO> GetRiskDetailsOfIndividualByCID(string id);
        ServiceResponse<RiskCorpCustomerDTO> GetRiskDetailsOfCorporateByCID(string id);
        ServiceResponse<RiskAssessmentVendorDTO> GetRiskDetailsOfVendorByCID(string id);

        ServiceResponse<int> GetRiskIdByCustomercode(string Customercode, string type);

    }
}
