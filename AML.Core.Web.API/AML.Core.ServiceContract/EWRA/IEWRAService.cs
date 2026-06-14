using AML.Core.Common.StaticResource;
using AML.DTO.DTO.EWRA;
using System;
using System.Collections.Generic;

namespace AML.Core.ServiceContract.EWRA
{
    public interface IEWRAService
    {
        List<EWRACustomerTypeDTO> GetAllCustomerTypes(int type,int clientId);
       
        ServiceResponse<string> SaveCustomerProfile(EWRAModelDTO ewraModelDTO);
        List<EWRAProductCategoryDTO> GetAllProductCategories(int clientId);
        List<EWRACounterPartyTypeDTO> GetAllCounterParty(int type,int clientId);
        List<EWRADeliveryTypeDTO> GetAllDeliveryType(int clientId);
        List<EWRAJurisdictionDTO> GetAllJurisdictionCountry(int Id);
        bool AddCustomerType(EWRAConfigModelDTO ewraConfigModelDTO, out string returnMsg, out bool isSuccess);
        bool AddCounterparty(EWRAConfigModelDTO ewraConfigModelDTO, out string returnMsg, out bool isSuccess);
        bool AddEWRAConfig(EWRAConfigModelDTO ewraConfigModelDTO, out string returnMsg, out bool isSuccess);
        ServiceResponse<List<EWRAModelDTO>> GetEwraBetweenDates(DateTime fromDate, DateTime toDate,int clientId);
        List<EWRAModelDTO> GetConsolidatedEwra(int Id, int type, int Qid);
        List<EWRAModelDTO> GetEwraCustomerProfileData(int Id);
        List<EWRAModelDTO> GetEwraCounterpartyData(int Id);
        List<EWRAModelDTO> GetEwraProductsData(int Id);
        List<EWRAModelDTO> GetEwraJurisdictionData(int Id);
        List<EWRAModelDTO> GetEwraDeliveryData(int Id);
        List<EWRAQualitativeModelDTO> GetAllRiskTypes(int clientId);
        ServiceResponse<string> SaveCategorywiseAssessmentData(EWRAModelDTO ewraModelDTO);
        List<EWRAQualitativeModelDTO> GetEwraQualitativeSavedData(int Id, int type);
        List<EWRAQualitativeConfigModelDTO> GetAllRiskConfigTypes();
        List<RiskDescriptionDataDTO> getAllRiskDescription(int RiskTypeId, int clientId);
        List<InherentRiskModelDTO> getAllInherentRisk(int riskTypeId);
        List<KeyControlModelDTO> getAllKeyControl(int riskTypeId);
        List<ResidualRiskModelDTO> getAllResidual(int riskTypeId);
        ServiceResponse<string> GetPendingEWRA(int type, int userId,int clientId);
        List<EWRAModelDTO> GetEwraQuantitativePendingData(int Id);
        ServiceResponse<string> GetEWRAQualitativeId(int qId);

    }
}
