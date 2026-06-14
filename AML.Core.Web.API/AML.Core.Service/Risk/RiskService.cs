using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Report;
using AML.Core.RepositoryContract.Risk;
using AML.Core.ServiceContract.Report;
using AML.Core.ServiceContract.Risk;
using AML.DTO.DTO.Risk;
using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Report;
using System.Collections.Generic;
using System;

namespace AML.Core.Service.Risk 
{
    public class RiskService: IRiskService
    {
        private IRiskRepository _riskRepository;
        public RiskService(IRiskRepository riskRepository)
        {
            _riskRepository = riskRepository;
        }
        public ServiceResponse<int> Create(RiskDTO model)
        {
            //Perform business requirements here
            return _riskRepository.Create(model);
        }
        public ServiceResponse<bool> UpdateRiskAssessmentVersion(RiskVersionUpdateDTO model, int assessmentVersion)
        {
            //Perform business requirements here
            return _riskRepository.UpdateRiskAssessmentVersion(model,assessmentVersion);
        }
        public ServiceResponse<int> CreateCorpCustomerRisk(RiskCorpCustomerDTO model)
        {
            //Perform business requirements here
            return _riskRepository.CreateCorpCustomerRisk(model);
        }
        public ServiceResponse<List<RiskExcelReportDTO>> GetRiskReportForExcel(DateTime fromDate, DateTime toDate, string createdByUserID, string riskType, int riskLevel, int clientId)
        {
            return _riskRepository.GetRiskReportForExcel(fromDate, toDate, createdByUserID, riskType, riskLevel, clientId);
        }
        public ServiceResponse<int> CreateBankRisk(RiskAssessmentBankDTO model)
        {
            //Perform business requirements here
            return _riskRepository.CreateBankRisk(model);
        }
        public ServiceResponse<int> CreateVendorRisk(RiskAssessmentVendorDTO model)
        {
            //Perform business requirements here
            return _riskRepository.CreateVendorRisk(model);
        }
        public ServiceResponse<List<RiskSummaryDTO>> GetRiskSummary(DateTime date,int clientId)
        {
            return _riskRepository.GetRiskSummary(date,clientId);
        }
        public ServiceResponse<List<RiskReportDTO>> GetRiskReportBetweenDateAndType(int clientId,string createdByUserID,DateTime fromDate, DateTime toDate, string riskType, bool withDuplicate = false, int riskLevel = 0)
        {
            if (withDuplicate)
            {
                return _riskRepository.GetRiskReportBetweenDateAndTypeWithDuplicate(clientId, createdByUserID,fromDate, toDate, riskType, riskLevel);
            }
            else
            {
                return _riskRepository.GetRiskReportBetweenDateAndType(clientId, createdByUserID,fromDate, toDate, riskType, riskLevel);
            }
        }
        public List<RiskReportDTO> GetLastestRiskVersion(string customercode,string customertype)
        {

            return _riskRepository.GetLastestRiskVersion(customercode, customertype).Result;
            
        }
        public List<RiskReportDTO> GetAllRiskVersion(string customercode, string customertype)
        {

            return _riskRepository.GetAllRiskVersion(customercode, customertype).Result;

        }
        public ServiceResponse<List<RiskReportDTO>> GetAllVersionRiskReportByid(string customerCode, string riskType, int clientId)
        {
            
                return _riskRepository.GetAllVersionRiskReportByid(customerCode, riskType,clientId);
           
        }

        public ServiceResponse<RiskDTO> GetRiskDetailsOfIndividual(int id)
        {
            return _riskRepository.GetRiskDetailsOfIndividual(id);
        }

        public ServiceResponse<RiskCorpCustomerDTO> GetRiskDetailsOfCorporate(int id)
        {
            return _riskRepository.GetRiskDetailsOfCorporate(id);
        }

        public ServiceResponse<RiskAssessmentBankDTO> GetRiskDetailsOfBank(int id)
        {
            return _riskRepository.GetRiskDetailsOfBank(id);
        }

        public ServiceResponse<RiskAssessmentVendorDTO> GetRiskDetailsOfVendor(int id)
        {
            return _riskRepository.GetRiskDetailsOfVendor(id);
        }

        //soft delete
        public ServiceResponse<int> DeleteRiskIndiviual(int id)
        {
            return _riskRepository.DeleteRiskIndiviual(id);
        }
        public ServiceResponse<int> DeleteRiskCorporate(int id)
        {
            return _riskRepository.DeleteRiskCorporate(id);
        }
        public ServiceResponse<int> DeleteRiskBank(int id)
        {
            return _riskRepository.DeleteRiskBank(id);
        }
        public ServiceResponse<int> DeleteRiskVendor(int id)
        {
            return _riskRepository.DeleteRiskVendor(id);
        }
        //DNIRC

        public ServiceResponse<RiskDTO> GetRiskDetailsOfIndividualByCID(string id, int Caseversion)
        {
            return _riskRepository.GetRiskDetailsOfIndividualByCID(id, Caseversion);
        }

        public ServiceResponse<RiskCorpCustomerDTO> GetRiskDetailsOfCorporateByCID(string id,int Caseversion)
        {
            return _riskRepository.GetRiskDetailsOfCorporateByCID(id, Caseversion);
        }

        public ServiceResponse<RiskAssessmentVendorDTO> GetRiskDetailsOfVendorByCID(string id)
        {
            return _riskRepository.GetRiskDetailsOfVendorByCID(id);
        }
        public ServiceResponse<int> GetRiskIdByCustomercode(string customercode, string type)
        {

            return _riskRepository.GetRiskIdByCustomercode(customercode, type);
        }

    }
}
