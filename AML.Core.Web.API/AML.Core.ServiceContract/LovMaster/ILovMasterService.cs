using System;
using System.Collections.Generic;
using System.Text;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.Risk;

namespace AML.Core.ServiceContract.LovMaster
{
    public interface ILovMasterService : IBaseService
    {
        List<LovMasterDTO> GetAll();
        List<LovMasterDTO> GetAllLovMasterCategories();
        List<LovMasterDTO> GetRiskTypes(string riskCategory,int clientId);
        List<LovMasterDTO> GetRiskItems(int riskTypeID, int clientId );
        int UpdateRiskItemStatus(int id, int status);
        bool AddRiskTypes(RIskConfigurationMasterDTO model, out string returnMsg, out bool isSuccess);
        List<LovTypeCategoryDTO> GetRiskCategoryTypes(string riskCategoryID,int clientId);
        List<LovTypeCategoryDTO> GetRiskCategoryType(string riskCategoryID,int clientId);
        List<LovMasterDTO> GetRiskType(string riskTypeCategoryID,int clientId);

        List<ProdtypecategoryDTO> GetProductRiskCategoryType(int clientId);
        bool AddRiskTypeCategory(RIskConfigurationMasterDTO rIskConfigurationMasterDTO, out string returnMsg, out bool isSuccess);
        bool AddRiskType(RIskConfigurationMasterDTO rIskConfigurationMasterDTO, out string returnMsg, out bool isSuccess);
        List<RiskTypeCategoryDTO> GetAllRiskConfig(string riskCategoryID, int SelectID, int CategoryID, int TypeID,int clientId);
        List<RiskTypeCategoryDTO> GetAllRiskConfigReport(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId,int dt);
        List<RiskTypeCategoryDTO> GetAllKycRiskConfig(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId);
    }
}
