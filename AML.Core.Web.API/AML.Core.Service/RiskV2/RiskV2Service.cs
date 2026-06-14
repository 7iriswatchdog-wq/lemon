using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Report;
using AML.Core.RepositoryContract.Risk;
using AML.Core.ServiceContract.Report;
using AML.Core.ServiceContract.Risk;
using AML.DTO.DTO.RiskV2;
using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Report;
using System.Collections.Generic;
using System;
using AML.Core.ServiceContract.RiskV2;
using AML.Core.RepositoryContract.RiskV2;

namespace AML.Core.Service.RiskV2 
{
    public class RiskV2Service: IRiskV2Service
    {
        private IRiskV2Repository _riskRepository;
        public RiskV2Service(IRiskV2Repository riskRepository)
        {
            _riskRepository = riskRepository;
        }
        public ServiceResponse<int> Create(RiskDTOV2 model)
        {
            //Perform business requirements here
            return _riskRepository.Create(model);
        }
        public ServiceResponse<int> CreateCorpCustomerRisk(RiskCorpCustomerDTOV2 model)
        {
            //Perform business requirements here
            return _riskRepository.CreateCorpCustomerRisk(model);
        }
        public ServiceResponse<List<RiskExcelReportDTOV2>> GetRiskReportForExcel(DateTime fromDate, DateTime toDate, string createdByUserID, string searchvalue, string riskType, int riskLevel, int clientId)
        {
            return _riskRepository.GetRiskReportForExcel(fromDate, toDate, createdByUserID, searchvalue, riskType, riskLevel, clientId);
        }
        public ServiceResponse<int> CreateBankRisk(RiskAssessmentBankDTOV2 model)
        {
            //Perform business requirements here
            return _riskRepository.CreateBankRisk(model);
        }
        public ServiceResponse<int> CreateVendorRisk(RiskAssessmentVendorDTOV2 model)
        {
            //Perform business requirements here
            return _riskRepository.CreateVendorRisk(model);
        }
        public ServiceResponse<List<RiskSummaryDTOV2>> GetRiskSummary(DateTime date,int clientId)
        {
            return _riskRepository.GetRiskSummary(date,clientId);
        }
        public ServiceResponse<List<RiskReportDTOV2>> GetRiskReportBetweenDateAndType(int clientId,string createdByUserID,DateTime fromDate, DateTime toDate, string riskType, bool withDuplicate = false, int riskLevel = 0)
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
        public ServiceResponse<List<RiskReportDTOV2>> GetAllVersionRiskReportByid(string customerCode, string riskType, int clientId)
        {
            
                return _riskRepository.GetAllVersionRiskReportByid(customerCode, riskType,clientId);
           
        }

        public ServiceResponse<RiskDTOV2> GetRiskDetailsOfIndividual(int id)
        {
            return _riskRepository.GetRiskDetailsOfIndividual(id);
        }

        public ServiceResponse<RiskCorpCustomerDTOV2> GetRiskDetailsOfCorporate(int id)
        {
            return _riskRepository.GetRiskDetailsOfCorporate(id);
        }

        public ServiceResponse<RiskAssessmentBankDTOV2> GetRiskDetailsOfBank(int id)
        {
            return _riskRepository.GetRiskDetailsOfBank(id);
        }

        public ServiceResponse<RiskAssessmentVendorDTOV2> GetRiskDetailsOfVendor(int id)
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

        public ServiceResponse<RiskDTOV2> GetRiskDetailsOfIndividualByCID(string id)
        {
            return _riskRepository.GetRiskDetailsOfIndividualByCID(id);
        }

        public ServiceResponse<RiskCorpCustomerDTOV2> GetRiskDetailsOfCorporateByCID(string id)
        {
            return _riskRepository.GetRiskDetailsOfCorporateByCID(id);
        }

        public ServiceResponse<RiskAssessmentVendorDTOV2> GetRiskDetailsOfVendorByCID(string id)
        {
            return _riskRepository.GetRiskDetailsOfVendorByCID(id);
        }

    }
}
