using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.Risk;

namespace AML.Core.RepositoryContract.ProductMaster
{
    public interface IProdMasterRepository : IBaseRepository
    {
        ServiceResponse<int> UpdateProdTypeCategory(ProdtypecategoryDTO prodTypeCategoryDTO);

        ServiceResponse<int> InsertProdTypeCategory(ProdtypecategoryDTO model);

        ServiceResponse<int> UpdateProdMaster(ProdRiskItemsDTO proditemDTO);

        ServiceResponse<int> InsertProdMaster(ProdRiskItemsDTO model);

        ServiceResponse<List<ProdRiskTypeCategoryDTO>> GetProdRiskCategoryTypes(int clientId);

        ServiceResponse<List<ProdRiskItemsDTO>> GetProdRiskItems(int riskTypeID, int clientId);

        ServiceResponse<List<ProdRiskTypeCategoryDTO>> GetAllProductRiskConfig(int SelectID, int CategoryID, int TypeID, int clientId);

        ServiceResponse<int> Create(ProductRiskDTO model);

        ServiceResponse<List<ProductRiskReportDTO>> GetProductRiskReportBetweenDateAndType(int clientId, string createdByUserID, DateTime fromDate, DateTime toDate,  int riskLevel = 0);


        ServiceResponse<ProductRiskDTO> GetProdRiskDetails(int id);

    }
}
