using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Risk;
using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Report;
using System.Collections.Generic;
using System;

namespace AML.Core.RepositoryContract.Risk
{
    public interface IRiskRepository
    {
        ServiceResponse<int> Create(RiskDTO _countryDTO);

        ServiceResponse<bool> UpdateRiskAssessmentVersion(RiskVersionUpdateDTO _countryDTO, int assessmentVersion);
        ServiceResponse<int> CreateCorpCustomerRisk(RiskCorpCustomerDTO _riskDTO);
        ServiceResponse<int> CreateBankRisk(RiskAssessmentBankDTO _riskDTO);
        ServiceResponse<int> CreateVendorRisk(RiskAssessmentVendorDTO _riskDTO);
        ServiceResponse<List<RiskSummaryDTO>> GetRiskSummary(DateTime date,int clientId);
        ServiceResponse<List<RiskExcelReportDTO>> GetRiskReportForExcel(DateTime fromDate, DateTime toDate, string createdByUserID, string riskType, int riskLevel, int clientId);
        ServiceResponse<List<RiskReportDTO>> GetRiskReportBetweenDateAndType(int clientId,string createdByUserID,DateTime fromDate, DateTime toDate, string riskType, int riskLevel);
        ServiceResponse<List<RiskReportDTO>> GetRiskReportBetweenDateAndTypeWithDuplicate(int clientId, string createdByUserID,DateTime fromDate, DateTime toDate, string riskType, int riskLevel);

        ServiceResponse<List<RiskReportDTO>> GetLastestRiskVersion(string customercode,string customertype);

        ServiceResponse<List<RiskReportDTO>> GetAllRiskVersion(string customercode, string customertype);
        ServiceResponse<List<RiskReportDTO>> GetAllVersionRiskReportByid(string customerCode, string riskType, int clientId);
        ServiceResponse<RiskDTO> GetRiskDetailsOfIndividual(int id);
        ServiceResponse<RiskCorpCustomerDTO> GetRiskDetailsOfCorporate(int id);
        ServiceResponse<RiskAssessmentBankDTO> GetRiskDetailsOfBank(int id);
        ServiceResponse<RiskAssessmentVendorDTO> GetRiskDetailsOfVendor(int id);
        //soft delete
        ServiceResponse<int> DeleteRiskIndiviual(int id);
        ServiceResponse<int> DeleteRiskCorporate(int id);
        ServiceResponse<int> DeleteRiskBank(int id);
        ServiceResponse<int> DeleteRiskVendor(int id);

        //DNIRC

        ServiceResponse<RiskDTO> GetRiskDetailsOfIndividualByCID(string id, int Caseversion);
        ServiceResponse<RiskCorpCustomerDTO> GetRiskDetailsOfCorporateByCID(string id,int Caseversion);
        ServiceResponse<RiskAssessmentVendorDTO> GetRiskDetailsOfVendorByCID(string id);

        ServiceResponse<int> GetRiskId(int riskTypeId, string riskData);

        ServiceResponse<int> GetRiskIdByCustomercode(string customercode, string type);
    }
}
