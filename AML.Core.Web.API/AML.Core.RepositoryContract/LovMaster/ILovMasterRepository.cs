using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.Risk;

namespace AML.Core.RepositoryContract.LovMaster
{
    public interface ILovMasterRepository : IBaseRepository
    {
        ServiceResponse<List<LovMasterDTO>> GetAllLovMaster();
        ServiceResponse<List<LovMasterDTO>> GetAllLovMasterCategories();
        ServiceResponse<List<LovMasterDTO>> GetRiskTypes(string riskCategory, int clientId);
        ServiceResponse<List<LovMasterDTO>> GetRiskItems(int riskTypeID, int clientId);

        ServiceResponse<List<ProdtypecategoryDTO>> GetProductRiskCategoryType(int clientId);
        ServiceResponse<int> UpdateRiskItemStatus(int id, int status);
        ServiceResponse<int> InsertLovMaster(LovMasterDTO model);
        ServiceResponse<int> UpdateLovMaster(LovMasterDTO model);
        ServiceResponse<List<LovTypeCategoryDTO>> GetRiskCategoryTypes(string riskCategoryID,int clientId);
        ServiceResponse<List<LovTypeCategoryDTO>> GetRiskCategoryType(string riskCategoryID,int clientId);
        ServiceResponse<List<LovMasterDTO>> GetRiskType(string riskCategoryType, int clientId);
        ServiceResponse<int> UpdateLovTypeCategory(LovTypeCategoryDTO lovTypeCategoryDTO);
        ServiceResponse<int> InsertLovTypeCategory(LovTypeCategoryDTO model);
        ServiceResponse<int> UpdateLovType(LovTypeMasterDTO lovTypeMasterDTO);
        ServiceResponse<int> InsertLovType(LovTypeMasterDTO model);
        ServiceResponse<List<RiskTypeCategoryDTO>> GetAllRiskConfig(string riskCategoryID,int SelectID, int CategoryID, int TypeID,int clientId);
        ServiceResponse<List<RiskTypeCategoryDTO>> GetAllRiskConfigReport(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId,int dt);
        ServiceResponse<List<RiskTypeCategoryDTO>> GetAllKycRiskConfig(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId);
    }
}
