using AML.Core.Common.StaticResource;
using AML.DTO.DTO.RiskV2;
using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Report;
using System.Collections.Generic;
using System;

namespace AML.Core.RepositoryContract.RiskV2
{
    public interface IRiskV2Repository
    {
        ServiceResponse<int> Create(RiskDTOV2 _countryDTO);
        ServiceResponse<int> CreateCorpCustomerRisk(RiskCorpCustomerDTOV2 _riskDTO);
        ServiceResponse<int> CreateBankRisk(RiskAssessmentBankDTOV2 _riskDTO);
        ServiceResponse<int> CreateVendorRisk(RiskAssessmentVendorDTOV2 _riskDTO);
        ServiceResponse<List<RiskSummaryDTOV2>> GetRiskSummary(DateTime date,int clientId);
        ServiceResponse<List<RiskExcelReportDTOV2>> GetRiskReportForExcel(DateTime fromDate, DateTime toDate, string createdByUserID, string searchvalue, string riskType, int riskLevel, int clientId);
        ServiceResponse<List<RiskReportDTOV2>> GetRiskReportBetweenDateAndType(int clientId,string createdByUserID,DateTime fromDate, DateTime toDate, string riskType, int riskLevel);
        ServiceResponse<List<RiskReportDTOV2>> GetRiskReportBetweenDateAndTypeWithDuplicate(int clientId, string createdByUserID,DateTime fromDate, DateTime toDate, string riskType, int riskLevel);

        ServiceResponse<List<RiskReportDTOV2>> GetAllVersionRiskReportByid(string customerCode, string riskType, int clientId);
        ServiceResponse<RiskDTOV2> GetRiskDetailsOfIndividual(int id);
        ServiceResponse<RiskCorpCustomerDTOV2> GetRiskDetailsOfCorporate(int id);
        ServiceResponse<RiskAssessmentBankDTOV2> GetRiskDetailsOfBank(int id);
        ServiceResponse<RiskAssessmentVendorDTOV2> GetRiskDetailsOfVendor(int id);
        //soft delete
        ServiceResponse<int> DeleteRiskIndiviual(int id);
        ServiceResponse<int> DeleteRiskCorporate(int id);
        ServiceResponse<int> DeleteRiskBank(int id);
        ServiceResponse<int> DeleteRiskVendor(int id);

        //DNIRC

        ServiceResponse<RiskDTOV2> GetRiskDetailsOfIndividualByCID(string id);
        ServiceResponse<RiskCorpCustomerDTOV2> GetRiskDetailsOfCorporateByCID(string id);
        ServiceResponse<RiskAssessmentVendorDTOV2> GetRiskDetailsOfVendorByCID(string id);

        ServiceResponse<int> GetRiskId(int riskTypeId, string riskData);
    }
}
