using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.RiskV2;

namespace AML.Core.RepositoryContract.LovMasterV2
{
    public interface ILovMasterV2Repository : IBaseRepository
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
        ServiceResponse<List<RiskTypeCategoryDTOV2>> GetAllRiskConfig(string riskCategoryID,int SelectID, int CategoryID, int TypeID,int clientId);
        ServiceResponse<List<RiskTypeCategoryDTOV2>> GetAllRiskConfigReport(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId,int dt);
        ServiceResponse<List<RiskTypeCategoryDTOV2>> GetAllKycRiskConfig(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId);
    }
}
