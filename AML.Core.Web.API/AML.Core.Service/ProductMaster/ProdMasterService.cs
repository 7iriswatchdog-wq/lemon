using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.Core.Repository.LovMaster;
using AML.Core.RepositoryContract.LovMaster;
using AML.Core.RepositoryContract.ProductMaster;
using AML.Core.ServiceContract.LovMaster;
using AML.Core.ServiceContract.ProductMaster;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.ProductMaster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AML.Core.Service.ProductMaster
{
    public class ProdMasterService : BaseService, IProdMasterService
    {
        IProdMasterRepository _prodmasterRepository;
        public ProdMasterService(IProdMasterRepository countryRepository, IConfiguration configuration, IHostingEnvironment environment) : base(countryRepository, configuration)
        {
            _prodmasterRepository = countryRepository;
        }

        public List<ProdRiskTypeCategoryDTO> GetProdRiskCategoryTypes(int clientId)
        {
            return _prodmasterRepository.GetProdRiskCategoryTypes(clientId).Result;
        }

        public List<ProdRiskItemsDTO> GetProdRiskItems(int riskTypeID, int clientId)
        {
            return _prodmasterRepository.GetProdRiskItems(riskTypeID, clientId).Result;
        }

        public List<ProdRiskTypeCategoryDTO> GetAllProductRiskConfig( int SelectID, int CategoryID, int TypeID, int clientId)
        {
            return _prodmasterRepository.GetAllProductRiskConfig(SelectID, CategoryID, TypeID, clientId).Result;
        }

        public ServiceResponse<ProductRiskDTO> GetProdRiskDetails(int id)
        {
            return _prodmasterRepository.GetProdRiskDetails(id);
        }

        public ServiceResponse<int> Create(ProductRiskDTO model)
        {
            //Perform business requirements here
            return _prodmasterRepository.Create(model);
        }

        public ServiceResponse<List<ProductRiskReportDTO>> GetProductRiskReportBetweenDateAndType(int clientId, string createdByUserID, DateTime fromDate, DateTime toDate, int riskLevel = 0)
        {
           
                return _prodmasterRepository.GetProductRiskReportBetweenDateAndType(clientId, createdByUserID, fromDate, toDate, riskLevel);
            
        }

        public bool AddProductRiskTypeCategory(ProdRiskConfigurationMasterDTO model, out string returnMsg, out bool isSuccess)
        {
            var updateMsg = string.Empty;
            var insertMsg = string.Empty;
            bool status = true;
            //Update existing Items
            foreach (var item in model.AvailableProdRiskTypeCategory)
            {
                var UpdateRisks = _prodmasterRepository.UpdateProdTypeCategory(new ProdtypecategoryDTO
                {
                    ProdTypeCategoryId = item.Id,
                    ProdCategoryType = item.ProdRiskTypeCategory,
                    IsActive = item.isActive,
                    ClientId = model.ClientId,
                   
                });

                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Risk Item Failed.";
                    status = false;
                }
            }

            //Insert new risk items
            foreach (var item in model.AddedProdRiskTypeCategory)
            {
                if (!item.isDeletedInUI && !String.IsNullOrEmpty(item.ProdRiskTypeCategory))
                {
                    var insertedID = _prodmasterRepository.InsertProdTypeCategory(new ProdtypecategoryDTO
                    {
                        ProdCategoryType = item.ProdRiskTypeCategory,
                        IsActive = true,
                        ClientId = model.ClientId,
                        
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

        public bool AddProdRiskItems(ProdRiskConfigurationMasterDTO model, out string returnMsg, out bool isSuccess)
        {
            var updateMsg = string.Empty;
            var insertMsg = string.Empty;
            bool status = true;
            //Update existing Items
            foreach (var item in model.AvailableProdRiskItems)
            {
                var UpdateRisks = _prodmasterRepository.UpdateProdMaster(new ProdRiskItemsDTO
                {
                    Id = item.Id,
                    ProdRiskCategoreyId = model.ProdRiskCategoryID,
                    ProdRiskCategorey = model.ProdRiskCategory,
                    ProdRiskItem = item.ProdRiskItem,
                    ProdRiskScore = item.ProdRiskScore,
                    OverrideScore = item.OverrideScore,
                    isActive = item.isActive,
                    ClientId = model.ClientId

                });

                if (UpdateRisks.Status == StaticResource.FailStatusCode)
                {
                    updateMsg = "Update Risk Item Failed.";
                    status = false;
                }
            }

            //Insert new risk items
            foreach (var item in model.AddedProdRiskItems)
            {
                if (!item.isDeletedInUI && !String.IsNullOrEmpty(item.ProdRiskItem))
                {
                    var insertedID = _prodmasterRepository.InsertProdMaster(new ProdRiskItemsDTO
                    {
                        ProdRiskCategoreyId = model.ProdRiskCategoryID,
                        ProdRiskCategorey = model.ProdRiskCategory,
                        ProdRiskItem = item.ProdRiskItem,
                        ProdRiskScore = item.ProdRiskScore,
                        OverrideScore = item.OverrideScore,
                        isActive = true,
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


    }
}
