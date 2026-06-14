using AML.Core.Common.StaticResource;
using AML.DTO.DTO.EWRA;
using System;
using System.Collections.Generic;

namespace AML.Core.RepositoryContract.EWRA
{
    public interface IEWRARepository
    {
        ServiceResponse<List<EWRACustomerTypeDTO>> GetAllCustomerTypes(int type,int clientId);
        ServiceResponse<string> SaveCustomerProfile(EWRAModelDTO ewraModelDTO);
        ServiceResponse<List<EWRAProductCategoryDTO>> GetAllProductCategories(int clientId);
        ServiceResponse<List<EWRACounterPartyTypeDTO>> GetAllCounterParty(int type,int clientId);
        ServiceResponse<List<EWRADeliveryTypeDTO>> GetAllDeliveryType(int clientId);
        ServiceResponse<List<EWRAJurisdictionDTO>> GetAllJurisdictionCountry(int Id);
        ServiceResponse<int> InsertCustomerType(EWRACustomerTypeDTO model);
        ServiceResponse<int> UpdateCustomerType(EWRACustomerTypeDTO model);
        ServiceResponse<int> InsertCounterparty(EWRACounterPartyListDTO model);
        ServiceResponse<int> UpdateCounterparty(EWRACounterPartyListDTO model);
        
        ServiceResponse<int> InsertRiskDescription(RiskDescriptionDataDTO model);
        ServiceResponse<int> UpdateRiskDescription(RiskDescriptionDataDTO model);
        ServiceResponse<int> InsertInherentRisk(InherentRiskModelDTO model);
        ServiceResponse<int> UpdateInherentRisk(InherentRiskModelDTO model);
        ServiceResponse<int> InsertKeyControl(KeyControlModelDTO model);
        ServiceResponse<int> UpdateKeyControl(KeyControlModelDTO model);
        ServiceResponse<int> InsertResidualRisk(ResidualRiskModelDTO model);
        ServiceResponse<int> UpdateResidualRisk(ResidualRiskModelDTO model);
        ServiceResponse<List<EWRAModelDTO>> GetEwraBetweenDates(DateTime fromDate, DateTime toDate, int clientId);
        ServiceResponse<List<EWRAModelDTO>> GetConsolidatedEwra(int Id, int type, int Qid);
        ServiceResponse<List<EWRAModelDTO>> GetEwraCustomerProfileData(int Id);
        ServiceResponse<List<EWRAModelDTO>> GetEwraCounterpartyData(int Id);
        ServiceResponse<List<EWRAModelDTO>> GetEwraProductsData(int Id);
        ServiceResponse<List<EWRAModelDTO>> GetEwraJurisdictionData(int Id);
        ServiceResponse<List<EWRAModelDTO>> GetEwraDeliveryData(int Id);
        ServiceResponse<List<EWRAQualitativeModelDTO>> GetAllRiskTypes(int clientId);
        ServiceResponse<string> SaveCategorywiseAssessmentData(EWRAModelDTO ewraModelDTO);
        ServiceResponse<List<EWRAQualitativeModelDTO>> GetEwraQualitativeSavedData(int Id, int type);
        ServiceResponse<List<EWRAQualitativeConfigModelDTO>> GetAllRiskConfigTypes();
        ServiceResponse<List<RiskDescriptionDataDTO>> getAllRiskDescription(int riskTypeId, int clientId);
        ServiceResponse<List<InherentRiskModelDTO>> getAllInherentRisk(int riskTypeId);
        ServiceResponse<List<KeyControlModelDTO>> getAllKeyControl(int riskTypeId);
        ServiceResponse<List<ResidualRiskModelDTO>> getAllResidual(int riskTypeId);
        ServiceResponse<string> GetPendingEWRA(int type, int userId,int clientId);
        ServiceResponse<List<EWRAModelDTO>> GetEwraQuantitativePendingData(int Id);
        ServiceResponse<string> GetEWRAQualitativeId(int qId);
    }
}
