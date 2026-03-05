using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.EWRA;
using AML.Core.ServiceContract.EWRA;
using AML.DTO.DTO.EWRA;
using System;
using System.Collections.Generic;

namespace AML.Core.Service.EWRA
{
    public class EWRAService : IEWRAService
    {
        IEWRARepository _ewraRepository;
        public EWRAService(IEWRARepository ewraRepository)
        {
            _ewraRepository = ewraRepository;
        }
        public List<EWRACustomerTypeDTO> GetAllCustomerTypes(int type,int clientId)
        {
            return _ewraRepository.GetAllCustomerTypes(type, clientId).Result;
        }
       

        
        public ServiceResponse<string> SaveCustomerProfile(EWRAModelDTO model)
        {
            return _ewraRepository.SaveCustomerProfile(model);
        }
        public List<EWRAProductCategoryDTO> GetAllProductCategories(int clientId)
        {
            return _ewraRepository.GetAllProductCategories(clientId).Result;
        }
        public List<EWRACounterPartyTypeDTO> GetAllCounterParty(int type,int clientId)
        {
            return _ewraRepository.GetAllCounterParty(type,clientId).Result;
        }
        public List<EWRADeliveryTypeDTO> GetAllDeliveryType(int clientId)
        {
            return _ewraRepository.GetAllDeliveryType(clientId).Result;
        }
        public List<EWRAJurisdictionDTO> GetAllJurisdictionCountry(int Id)
        {
            return _ewraRepository.GetAllJurisdictionCountry(Id).Result;
        }
        //public bool AddCustomerType(EWRAConfigModelDTO model, out string returnMsg, out bool isSuccess)
        public bool AddCustomerType(EWRAConfigModelDTO model, out string returnMsg, out bool isSuccess)
        {
            var ClientId = model.ClientId;
            var updateMsg = string.Empty;
            var insertMsg = string.Empty;
            bool status = true;
            //Update existing Items
            foreach (var item in model.EWRAQuantitativeConfigModel[0].AvailableCustomerType)
            {
                var UpdateRisks = _ewraRepository.UpdateCustomerType(new EWRACustomerTypeDTO
                {
                    CustTypeId = item.CustTypeId,
                    CustType = item.CustType,
                    isActive = item.isActive,
                    ClientId= ClientId
                });
                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Customer Type Failed.";
                    status = false;
                }
            }
            //Insert new risk items
            foreach (var item in model.EWRAQuantitativeConfigModel[0].AddedCustomerType)
            {
                if (!item.isDeletedInUI && !String.IsNullOrEmpty(item.CustType))
                {
                    var insertedID = _ewraRepository.InsertCustomerType(new EWRACustomerTypeDTO
                    {
                        CustTypeId = item.CustTypeId,
                        CustType = item.CustType,
                        isActive = 1,
                        ClientId = ClientId
                    });
                    if (insertedID.Status == StaticResource.FailStatusCode)
                    {
                        insertMsg = "Insert Customer Type Failed.";
                        status = false;
                    }
                }
            }
            returnMsg = String.Concat(updateMsg, insertMsg);
            isSuccess = status;
            return true;
        }
        //Counterparty
        public bool AddCounterparty(EWRAConfigModelDTO model, out string returnMsg, out bool isSuccess)
        {
            var ClientId = model.ClientId;
            var updateMsg = string.Empty;
            var insertMsg = string.Empty;
            bool status = true;
            //Update existing Items
            foreach (var item in model.EWRAQuantitativeConfigModel[0].EWRACounterPartyType[0].EWRACounterPartyList)
            {
                var UpdateRisks = _ewraRepository.UpdateCounterparty(new EWRACounterPartyListDTO
                {
                    Id=item.Id,
                    CounterParty = item.CounterParty,
                    Country = item.Country,
                    isActive = item.isActive
                });
                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Counterparty Failed.";
                    status = false;
                }
            }
            //Insert new risk items
            foreach (var item in model.EWRAQuantitativeConfigModel[0].AddedCounterparty)
            {
                if (!String.IsNullOrEmpty(item.CounterParty))
                {
                    var insertedID = _ewraRepository.InsertCounterparty(new EWRACounterPartyListDTO
                    {
                        CounterParty = item.CounterParty,
                        Country = item.Country,
                        isActive = 1,
                        ClientId = ClientId
                    });
                    if (insertedID.Status == StaticResource.FailStatusCode)
                    {
                        insertMsg = "Insert Counterparty Failed.";
                        status = false;
                    }
                }
            }
            returnMsg = String.Concat(updateMsg, insertMsg);
            isSuccess = status;
            return true;
        }

        public bool AddEWRAConfig(EWRAConfigModelDTO model, out string returnMsg, out bool isSuccess)
        {
            var updateMsg = string.Empty;
            var insertMsg = string.Empty;
            bool status = true;
            var ClientId = model.ClientId;
            //Update existing Items in ewra risk description
            foreach (var item in model.EWRAQualitativeConfigModel[0].AvailableRiskDescription)
            {
                var UpdateRisks = _ewraRepository.UpdateRiskDescription(new RiskDescriptionDataDTO
                {
                    DescriptionId = item.DescriptionId,
                    RiskDescription = item.RiskDescription,
                    IsActive = item.IsActive
                });
                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Risk Description Failed.";
                    status = false;
                }
            }
            //Insert new risk items in ewra risk description
            foreach (var item in model.EWRAQualitativeConfigModel[0].AddedRiskDescription)
            {
                item.TypeId = model.EWRAQualitativeConfigModel[0].RiskTypeId;
                if (!item.isDeletedInUI && !String.IsNullOrEmpty(item.RiskDescription))
                {
                    var insertedID = _ewraRepository.InsertRiskDescription(new RiskDescriptionDataDTO
                    {
                        TypeId = item.TypeId,
                        DescriptionId = item.DescriptionId,
                        RiskDescription = item.RiskDescription,
                        IsActive = item.IsActive,
                        ClientId = ClientId
                    });
                    if (insertedID.Status == StaticResource.FailStatusCode)
                    {
                        insertMsg = "Insert Risk Description Failed.";
                        status = false;
                    }
                }
            }
            //Update existing Items in inherent risk
            foreach (var item in model.EWRAQualitativeConfigModel[0].AvailableInherentRisk)
            {
                var UpdateRisks = _ewraRepository.UpdateInherentRisk(new InherentRiskModelDTO
                {
                    InherentRiskId = item.InherentRiskId,
                    InherentRiskDescription = item.InherentRiskDescription,
                    IsActive = item.IsActive
                });
                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Inherent Risk Failed.";
                    status = false;
                }
            }
            //Insert new risk items in inherent risk
            foreach (var item in model.EWRAQualitativeConfigModel[0].AddedInherentRisk)
            {
                item.TypeId = model.EWRAQualitativeConfigModel[0].RiskTypeId;
                if (!item.isDeletedInUI && !String.IsNullOrEmpty(item.InherentRiskDescription))
                {
                    var insertedID = _ewraRepository.InsertInherentRisk(new InherentRiskModelDTO
                    {
                        TypeId = item.TypeId,
                        InherentRiskId = item.InherentRiskId,
                        InherentRiskDescription = item.InherentRiskDescription,
                        IsActive = item.IsActive,
                        ClientId = ClientId
                    });
                    if (insertedID.Status == StaticResource.FailStatusCode)
                    {
                        insertMsg = "Insert Inherent Risk Failed.";
                        status = false;
                    }
                }
            }
            //Update existing Items in key control
            foreach (var item in model.EWRAQualitativeConfigModel[0].AvailableKey)
            {
                var UpdateRisks = _ewraRepository.UpdateKeyControl(new KeyControlModelDTO
                {
                    KeyId = item.KeyId,
                    KeyControl = item.KeyControl,
                    IsActive = item.IsActive
                });
                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Key Control Failed.";
                    status = false;
                }
            }
            //Insert new risk items in key control
            foreach (var item in model.EWRAQualitativeConfigModel[0].AddedKey)
            {
                item.TypeId = model.EWRAQualitativeConfigModel[0].RiskTypeId;
                if (!item.isDeletedInUI && !String.IsNullOrEmpty(item.KeyControl))
                {
                    var insertedID = _ewraRepository.InsertKeyControl(new KeyControlModelDTO
                    {
                        TypeId = item.TypeId,
                        KeyId = item.KeyId,
                        KeyControl = item.KeyControl,
                        IsActive = item.IsActive,
                        ClientId = ClientId
                    });
                    if (insertedID.Status == StaticResource.FailStatusCode)
                    {
                        insertMsg = "Insert Key Control Failed.";
                        status = false;
                    }
                }
            }
            //Update existing Items in residual risk
            foreach (var item in model.EWRAQualitativeConfigModel[0].AvailableResidualRisk)
            {
                var UpdateRisks = _ewraRepository.UpdateResidualRisk(new ResidualRiskModelDTO
                {
                    ResidualRiskId = item.ResidualRiskId,
                    ResidualRiskDescription = item.ResidualRiskDescription,
                    IsActive = item.IsActive
                });
                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Residual Risk Failed.";
                    status = false;
                }
            }
            //Insert new risk items in inherent risk
            foreach (var item in model.EWRAQualitativeConfigModel[0].AddedResidualRisk)
            {
                item.TypeId = model.EWRAQualitativeConfigModel[0].RiskTypeId;
                if (!item.isDeletedInUI && !String.IsNullOrEmpty(item.ResidualRiskDescription))
                {
                    var insertedID = _ewraRepository.InsertResidualRisk(new ResidualRiskModelDTO
                    {
                        TypeId = item.TypeId,
                        ResidualRiskId = item.ResidualRiskId,
                        ResidualRiskDescription = item.ResidualRiskDescription,
                        IsActive = item.IsActive,
                        ClientId = ClientId
                    });
                    if (insertedID.Status == StaticResource.FailStatusCode)
                    {
                        insertMsg = "Insert Residual Risk Failed.";
                        status = false;
                    }
                }
            }

            returnMsg = String.Concat(updateMsg, insertMsg);
            isSuccess = status;
            return true;
        }
        public ServiceResponse<List<EWRAModelDTO>> GetEwraBetweenDates(DateTime fromDate, DateTime toDate,int clientId)
        {
            return _ewraRepository.GetEwraBetweenDates(fromDate, toDate, clientId);
        }
        public List<EWRAModelDTO> GetConsolidatedEwra(int Id, int type, int Qid)
        {
            return _ewraRepository.GetConsolidatedEwra(Id, type, Qid).Result;
        }
        public List<EWRAModelDTO> GetEwraCustomerProfileData(int Id)
        {
            return _ewraRepository.GetEwraCustomerProfileData(Id).Result;
        }
        public List<EWRAModelDTO> GetEwraQuantitativePendingData(int Id)
        {
            return _ewraRepository.GetEwraQuantitativePendingData(Id).Result;
        }
        public List<EWRAModelDTO> GetEwraCounterpartyData(int Id)
        {
            return _ewraRepository.GetEwraCounterpartyData(Id).Result;
        }
        public List<EWRAModelDTO> GetEwraProductsData(int Id)
        {
            return _ewraRepository.GetEwraProductsData(Id).Result;
        }
        public List<EWRAModelDTO> GetEwraJurisdictionData(int Id)
        {
            return _ewraRepository.GetEwraJurisdictionData(Id).Result;
        }
        public List<EWRAModelDTO> GetEwraDeliveryData(int Id)
        {
            return _ewraRepository.GetEwraDeliveryData(Id).Result;
        }
        public List<EWRAQualitativeModelDTO> GetAllRiskTypes(int clientId)
        {
            return _ewraRepository.GetAllRiskTypes(clientId).Result;
        }
        public ServiceResponse<string> SaveCategorywiseAssessmentData(EWRAModelDTO model)
        {
            return _ewraRepository.SaveCategorywiseAssessmentData(model);
        }
        public List<EWRAQualitativeModelDTO> GetEwraQualitativeSavedData(int Id, int type)
        {
            return _ewraRepository.GetEwraQualitativeSavedData(Id, type).Result;
        }
        public List<EWRAQualitativeConfigModelDTO> GetAllRiskConfigTypes()
        {
            return _ewraRepository.GetAllRiskConfigTypes().Result;
        }
        public List<RiskDescriptionDataDTO> getAllRiskDescription(int RiskTypeId,int clientId)
        {
            return _ewraRepository.getAllRiskDescription(RiskTypeId, clientId).Result;
        }
        public List<InherentRiskModelDTO> getAllInherentRisk(int riskTypeId)
        {
            return _ewraRepository.getAllInherentRisk(riskTypeId).Result;
        }
        public List<KeyControlModelDTO> getAllKeyControl(int riskTypeId)
        {
            return _ewraRepository.getAllKeyControl(riskTypeId).Result;
        }
        public List<ResidualRiskModelDTO> getAllResidual(int riskTypeId)
        {
            return _ewraRepository.getAllResidual(riskTypeId).Result;
        }
        public ServiceResponse<string> GetPendingEWRA(int type, int userId,int clientId)
        {
            return _ewraRepository.GetPendingEWRA(type, userId,clientId);
        }
        public ServiceResponse<string> GetEWRAQualitativeId(int qId)
        {
            return _ewraRepository.GetEWRAQualitativeId(qId);
        }
    }
}
