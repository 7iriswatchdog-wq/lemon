using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.LovMaster;
using AML.Core.ServiceContract.LovMaster;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.Risk;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AML.Core.Service.LovMaster
{
    public class LovMasterService : BaseService, ILovMasterService
    {
        ILovMasterRepository _lovmasterRepository;
        public LovMasterService(ILovMasterRepository countryRepository, IConfiguration configuration, IHostingEnvironment environment) : base(countryRepository, configuration)
        {
            _lovmasterRepository = countryRepository;
        }

        public List<LovMasterDTO> GetAll()
        {
            //Perform business requirements here
            return _lovmasterRepository.GetAllLovMaster().Result;
        }

        public List<LovMasterDTO> GetAllLovMasterCategories()
        {
            return _lovmasterRepository.GetAllLovMasterCategories().Result;
        }

        public List<LovMasterDTO> GetRiskTypes(string riskCategoryType, int clientId)
        {
            return _lovmasterRepository.GetRiskTypes(riskCategoryType, clientId).Result;
        }
        public List<LovMasterDTO> GetRiskType(string riskCategoryType,int clientId)
        {
            return _lovmasterRepository.GetRiskType(riskCategoryType, clientId).Result;
        }
        public List<LovTypeCategoryDTO> GetRiskCategoryType(string riskCategoryID,int clientId)
        {
            return _lovmasterRepository.GetRiskCategoryType(riskCategoryID,clientId).Result;
        }

        public List<LovTypeCategoryDTO> GetRiskCategoryTypes(string riskCategoryID,int clientId)
        {
            return _lovmasterRepository.GetRiskCategoryTypes(riskCategoryID,clientId).Result;
        }
        public List<LovMasterDTO> GetRiskItems(int riskTypeID,int clientId)
        {
            return _lovmasterRepository.GetRiskItems(riskTypeID, clientId).Result;
        }
        public List<ProdtypecategoryDTO> GetProductRiskCategoryType(int clientId)
        {
            return _lovmasterRepository.GetProductRiskCategoryType(clientId).Result;
        }
        public int UpdateRiskItemStatus(int id, int status)
        {
            return _lovmasterRepository.UpdateRiskItemStatus(id, status).Result;
        }

        public bool AddRiskTypes(RIskConfigurationMasterDTO model, out string returnMsg, out bool isSuccess)
        {
            var updateMsg = string.Empty;
            var insertMsg = string.Empty;
            bool status = true;
            //Update existing Items
            foreach (var item in model.AvailableItems)
            {
                var UpdateRisks = _lovmasterRepository.UpdateLovMaster(new LovMasterDTO
                {
                    Id = item.Id,
                    LovRiskCategory = model.RiskCategoryID,
                    LovRiskCategoryCode = model.RiskCategory,
                    LovTypeId = model.RiskTypeID,
                    LovTypeName = model.RiskTypeName,
                    LovRiskData = item.RiskItem,
                    LovRiskScore = item.RiskScore,
                    OverrideScore = item.OverrideScore,
                    IsActive = item.isActive,
                    ClientId = model.ClientId

                });

                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Risk Item Failed.";
                    status = false;
                }
            }

            //Insert new risk items
            foreach (var item in model.AddedItems)
            {
                if (!item.isDeletedInUI && !String.IsNullOrEmpty(item.RiskItem))
                {
                    var insertedID = _lovmasterRepository.InsertLovMaster(new LovMasterDTO
                    {
                        LovRiskCategory = model.RiskCategoryID,
                        LovRiskCategoryCode = model.RiskCategory,
                        LovTypeId = model.RiskTypeID,
                        LovTypeName = model.RiskTypeName,
                        LovRiskData = item.RiskItem,
                        LovRiskScore = item.RiskScore,
                        OverrideScore = item.OverrideScore,
                        IsActive = true,
                        ClientId = model.ClientId
                    });

                    if (insertedID.Status == StaticResource.FailStatusCode)
                    {
                        insertMsg = "Insert Risk Item Failed.";
                        status = false;
                    }
                }
            }

            returnMsg = String.Concat(updateMsg, insertMsg);
            isSuccess = status;

            return true;
        }
        public bool AddRiskTypeCategory(RIskConfigurationMasterDTO model, out string returnMsg, out bool isSuccess)
        {
            var updateMsg = string.Empty;
            var insertMsg = string.Empty;
            bool status = true;
            //Update existing Items
            foreach (var item in model.AvailableRiskTypeCategory)
            {
                var UpdateRisks = _lovmasterRepository.UpdateLovTypeCategory(new LovTypeCategoryDTO
                {
                    LovTypeCategoryId = item.Id,
                    LovRiskCategory = model.RiskCategory,
                    LovRiskCategoryCode = model.RiskCategoryID,
                    LovCategoryType = item.RiskTypeCategory,
                    IsActive = item.isActive,
                    ClientId = model.ClientId,
                    CreatedBy=model.CreatedBy,
                });

                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Risk Item Failed.";
                    status = false;
                }
            }

            //Insert new risk items
            foreach (var item in model.AddedRiskTypeCategory)
            {
                if (!item.isDeletedInUI && !String.IsNullOrEmpty(item.RiskTypeCategory))
                {
                    var insertedID = _lovmasterRepository.InsertLovTypeCategory(new LovTypeCategoryDTO
                    {
                        LovRiskCategory = model.RiskCategory,
                        LovRiskCategoryCode = model.RiskCategoryID,
                        LovCategoryType = item.RiskTypeCategory,
                        IsActive = true,
                        ClientId = model.ClientId,
                        CreatedBy = model.CreatedBy,
                    });

                    if (insertedID.Status == StaticResource.FailStatusCode)
                    {
                        insertMsg = "Insert Risk Item Failed.";
                        status = false;
                    }
                }
            }

            returnMsg = String.Concat(updateMsg, insertMsg);
            isSuccess = status;

            return true;
        }

        public bool AddRiskType(RIskConfigurationMasterDTO model, out string returnMsg, out bool isSuccess)
        {
            var updateMsg = string.Empty;
            var insertMsg = string.Empty;
            bool status = true;
            //Update existing Items
            foreach (var item in model.AvailableRiskType)
            {
                int lov_duplicate_country;
                if (item.RiskType.StartsWith("Nationali"))
                {
                    lov_duplicate_country = 1;
                }
                else
                {
                    lov_duplicate_country = 0;
                }
                var UpdateRisks = _lovmasterRepository.UpdateLovType(new LovTypeMasterDTO
                {
                    Id = item.Id,
                    LovRiskCategory = model.RiskCategory,
                    LovRiskCategoryCode = model.RiskCategoryID,
                    LovTypeId = item.Id.ToString(),
                    LovTypeCategoryId = model.RiskTypeCategoryID,
                    LovTypeName = item.RiskType,
                    IsActive = item.isActive,
                    ClientId = model.ClientId,
                    lov_country_duplicate = lov_duplicate_country
                });
                 
                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Risk Item Failed.";
                    status = false;
                }
            }

            //Insert new risk items
            foreach (var item in model.AddedRiskType)
            {
                if (!item.isDeletedInUI && !String.IsNullOrEmpty(item.RiskType))
                {
                    int lov_duplicate_country;
                    if (item.RiskType.StartsWith("Nationali"))
                    {
                         lov_duplicate_country = 1;
                    }
                    else
                    {
                        lov_duplicate_country = 0;
                    }
                    var insertedID = _lovmasterRepository.InsertLovType(new LovTypeMasterDTO
                    {
                        LovRiskCategory = model.RiskCategory,
                        LovRiskCategoryCode = model.RiskCategoryID,
                        LovTypeId = item.Id.ToString(),
                        LovTypeCategoryId = model.RiskTypeCategoryID,
                        LovTypeName = item.RiskType,
                        IsActive = true,
                        ClientId = model.ClientId,
                        lov_country_duplicate= lov_duplicate_country
                    });

                    if (insertedID.Status == StaticResource.FailStatusCode)
                    {
                        insertMsg = "Insert Risk Item Failed.";
                        status = false;
                    }
                }
            }

            returnMsg = String.Concat(updateMsg, insertMsg);
            isSuccess = status;

            return true;
        }
        public List<RiskTypeCategoryDTO> GetAllRiskConfig(string riskCategoryID,int SelectID, int CategoryID, int TypeID,int clientId)
        {
            return _lovmasterRepository.GetAllRiskConfig(riskCategoryID, SelectID, CategoryID, TypeID,clientId).Result;
        }
        //Risk Configuration
        public List<RiskTypeCategoryDTO> GetAllRiskConfigReport(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId,int dt)
        {
            return _lovmasterRepository.GetAllRiskConfigReport(riskCategoryID, SelectID, CategoryID, TypeID, clientId,dt).Result;
        }
        //kyc risk configuration

        public List<RiskTypeCategoryDTO> GetAllKycRiskConfig(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId)
        {
            return _lovmasterRepository.GetAllKycRiskConfig(riskCategoryID, SelectID, CategoryID, TypeID, clientId).Result;
        } 
    }
}
