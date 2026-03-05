using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.ProductMaster;

namespace AML.Core.ServiceContract.ProductMaster
{
    public interface IProdMasterService : IBaseService
    {

        bool AddProductRiskTypeCategory(ProdRiskConfigurationMasterDTO rIskConfigurationMasterDTO, out string returnMsg, out bool isSuccess);

        bool AddProdRiskItems(ProdRiskConfigurationMasterDTO rIskConfigurationMasterDTO, out string returnMsg, out bool isSuccess);

        List<ProdRiskTypeCategoryDTO> GetProdRiskCategoryTypes(int clientId);

        List<ProdRiskItemsDTO> GetProdRiskItems(int riskTypeID, int clientId);

        List<ProdRiskTypeCategoryDTO> GetAllProductRiskConfig(int SelectID, int CategoryID, int TypeID, int clientId);

        ServiceResponse<int> Create(ProductRiskDTO model);

        ServiceResponse<List<ProductRiskReportDTO>> GetProductRiskReportBetweenDateAndType(int clientId, string createdByUserID, DateTime fromDate, DateTime toDate, int riskLevel = 0);

        ServiceResponse<ProductRiskDTO> GetProdRiskDetails(int id);


    }
}
