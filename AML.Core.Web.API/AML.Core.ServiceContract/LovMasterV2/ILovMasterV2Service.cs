using System;
using System.Collections.Generic;
using System.Text;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.RiskV2;

namespace AML.Core.ServiceContract.LovMasterV2
{
    public interface ILovMasterV2Service : IBaseService
    {
        List<LovMasterDTO> GetAll();
        List<LovMasterDTO> GetAllLovMasterCategories();
        List<LovMasterDTO> GetRiskTypes(string riskCategory,int clientId);
        List<LovMasterDTO> GetRiskItems(int riskTypeID, int clientId );
        int UpdateRiskItemStatus(int id, int status);
        bool AddRiskTypes(RIskConfigurationMasterDTOV2 model, out string returnMsg, out bool isSuccess);
        List<LovTypeCategoryDTO> GetRiskCategoryTypes(string riskCategoryID,int clientId);
        List<LovTypeCategoryDTO> GetRiskCategoryType(string riskCategoryID,int clientId);
        List<LovMasterDTO> GetRiskType(string riskTypeCategoryID,int clientId);

        List<ProdtypecategoryDTO> GetProductRiskCategoryType(int clientId);
        bool AddRiskTypeCategory(RIskConfigurationMasterDTOV2 rIskConfigurationMasterDTO, out string returnMsg, out bool isSuccess);
        bool AddRiskType(RIskConfigurationMasterDTOV2 rIskConfigurationMasterDTO, out string returnMsg, out bool isSuccess);
        List<RiskTypeCategoryDTOV2> GetAllRiskConfig(string riskCategoryID, int SelectID, int CategoryID, int TypeID,int clientId);
        List<RiskTypeCategoryDTOV2> GetAllRiskConfigReport(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId,int dt);
        List<RiskTypeCategoryDTOV2> GetAllKycRiskConfig(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId);
    }
}
