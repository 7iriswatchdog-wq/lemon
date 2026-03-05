using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.LovMaster;
using AML.Core.RepositoryContract.ProductMaster;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.Risk;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace AML.Core.Repository.ProductMaster
{
    public class ProdMasterRepository : BaseRepository,  IProdMasterRepository
    {
        public ProdMasterRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }

        public ServiceResponse<int> UpdateProdTypeCategory(ProdtypecategoryDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id",model.ProdTypeCategoryId);
                parameters.Add("@p_prodCategorytype",model.ProdCategoryType);
                parameters.Add("@p_active_yn", model.IsActive ? 1 : 0);
                parameters.Add("@p_clientid", model.ClientId);
                var response = ExecuteScalar("mod_prodtypecategory", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Risk Type Category updated succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> InsertProdTypeCategory(ProdtypecategoryDTO model)
        {

            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_prodCategorytype",model.ProdCategoryType);
                parameters.Add("@p_clientid",model.ClientId);
                var response = ExecuteScalar("ins_prodtypecategory", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "risk type category inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {

            }

            return serviceResponse;
        }

        public ServiceResponse<int> InsertProdMaster(ProdRiskItemsDTO model)
        {

            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_prodCategoryId", model.ProdRiskCategoreyId);
                parameters.Add("@p_prodriskcategorey", model.ProdRiskCategorey);
                parameters.Add("@p_prodriskitem", model.ProdRiskItem);
                parameters.Add("@p_prodriskscore", model.ProdRiskScore);
                parameters.Add("@p_overridescore", model.OverrideScore);
                parameters.Add("@p_date_created", DateTime.Now);
                parameters.Add("@p_clientId", model.ClientId);
                var response = ExecuteScalar("ins_prodmaster", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "risk type category inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {

            }

            return serviceResponse;
        }

        public ServiceResponse<int> UpdateProdMaster(ProdRiskItemsDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", model.Id);
                parameters.Add("@p_prodCategoryId", model.ProdRiskCategoreyId);
                parameters.Add("@p_prodriskcategorey", model.ProdRiskCategorey);
                parameters.Add("@p_prodriskitem", model.ProdRiskItem);
                parameters.Add("@p_prodriskscore", model.ProdRiskScore);
                parameters.Add("@p_overridescore", model.OverrideScore);
                parameters.Add("@p_active_yn", model.isActive ? 1 : 0);
                parameters.Add("@p_date_updated", DateTime.Now);
                parameters.Add("@p_clientid", model.ClientId);
                var response = ExecuteScalar("mod_prodriskitem", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Risk Type Category updated succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<ProdRiskTypeCategoryDTO>> GetProdRiskCategoryTypes(int clientId)
        {
            ServiceResponse<List<ProdRiskTypeCategoryDTO>> serviceResponse = new ServiceResponse<List<ProdRiskTypeCategoryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<ProdRiskTypeCategoryDTO>("get_ProdtypeCategory", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

        }

        public ServiceResponse<List<ProdRiskItemsDTO>> GetProdRiskItems(int prodriskcategoryID, int clientId)
        {
            ServiceResponse<List<ProdRiskItemsDTO>> serviceResponse = new ServiceResponse<List<ProdRiskItemsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_prodriskcategory", prodriskcategoryID);
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<ProdRiskItemsDTO>("get_Prodriskitem_by_prodriskcategory", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

        }

        public ServiceResponse<int> Create(ProductRiskDTO _riskDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_productcode", _riskDTO.ProductCode);
                parameters.Add("@p_productname", _riskDTO.ProductName);
                parameters.Add("@p_dateofassessment", _riskDTO.DateofAssessment);
                parameters.Add("@p_final_risk_score", _riskDTO.FinalRiskScore);
                parameters.Add("@p_prod_risk_score_sum", _riskDTO.ProdRiskScoreSum);
                parameters.Add("@p_prod_risk_score_count", _riskDTO.ProdRiskScoreCount);
                parameters.Add("@p_prod_risk_score_before_override", _riskDTO.RiskScoreBeforeOverride);
                string riskItemlist = "";
                int index2 = 0;
                for (int i = 0; i < _riskDTO.ProdRiskTypeCategoryDTO.Count; i++)
                {
                    //for (int j = 0; j < _riskDTO.ProdRiskTypeCategoryDTO[i].ProdRiskItems.Count; j++)
                    //{

                        if (_riskDTO.ProdRiskTypeCategoryDTO[i].prodSelectedItemId != 0)
                        {
                            riskItemlist += _riskDTO.ProdRiskTypeCategoryDTO[i].Id + "Ø" + _riskDTO.ProdRiskTypeCategoryDTO[i].prodSelectedItemId + "¥";
                            index2++;
                        }

                    //}

                    
                }
                parameters.Add("@p_prod_risk_data", riskItemlist);
                parameters.Add("@p_prod_risk_category_count", index2);
                parameters.Add("@p_clientId", _riskDTO.ClientId);
                parameters.Add("@p_created_by", _riskDTO.CreatedBy);
                parameters.Add("@p_version", _riskDTO.version);

                var response = ExecuteScalar("ins_product_risk", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Risk Assessment for Individual added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<ProdRiskTypeCategoryDTO>> GetAllProductRiskConfig(int SelectID, int CategoryID, int TypeID, int clientId)
        {
            ServiceResponse<List<ProdRiskTypeCategoryDTO>> serviceResponse = new ServiceResponse<List<ProdRiskTypeCategoryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_lov_select", SelectID);
                parameters.Add("@p_lov_type_category_id", CategoryID);
                parameters.Add("@p_lov_type_id", TypeID);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<ProdRiskTypeCategoryDTO>("get_productriskconfiguration", parameters, commandType: CommandType.StoredProcedure).ToList();
                ProdRiskConfigurationMasterDTO _riskConfig = new ProdRiskConfigurationMasterDTO();
                _riskConfig.RiskTypeCategories = serviceResponse.Result;
                serviceResponse.Message = "Category details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                //foreach (var category in _riskConfig.RiskTypeCategories)
                for (var i = 0; i < _riskConfig.RiskTypeCategories.Count; i++)
                {
                    ServiceResponse<List<ProdRiskItemsDTO>> serviceResponse1 = new ServiceResponse<List<ProdRiskItemsDTO>>();
                    parameters.Add("@p_lov_type_category_id", _riskConfig.RiskTypeCategories[i].Id);
                    parameters.Add("@p_lov_select", 2);
                    serviceResponse1.Result = Get<ProdRiskItemsDTO>("get_productriskconfiguration", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "Sub Category details fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    _riskConfig.RiskTypeCategories[i].ProdRiskItems = serviceResponse1.Result;
                    
                    
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<ProductRiskReportDTO>> GetProductRiskReportBetweenDateAndType(int clientId, string createdByUserID, DateTime fromDate, DateTime toDate, int riskLevel)
        {
            ServiceResponse<List<ProductRiskReportDTO>> serviceResponse = new ServiceResponse<List<ProductRiskReportDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from_date", fromDate);
                parameters.Add("@c_to_date", toDate);
                parameters.Add("@c_clientId", clientId);
                parameters.Add("@c_risk_level", riskLevel);
                parameters.Add("@p_created_by", createdByUserID);
                serviceResponse.Result = Get<ProductRiskReportDTO>("get_product_risk_score", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<ProductRiskDTO> GetProdRiskDetails(int id)
        {
            ServiceResponse<ProductRiskDTO> serviceResponse = new ServiceResponse<ProductRiskDTO>();
            ServiceResponse<List<ProdReportDataDTO>> serviceResponse1 = new ServiceResponse<List<ProdReportDataDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
               serviceResponse.Result = GetFirstOrDefault<ProductRiskDTO>("get_product_risk_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ProdReportDataDTO>)Get<ProdReportDataDTO>("get_product_risk_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result.ProdReportDataDTO = serviceResponse1.Result;
                serviceResponse.Message = "Risk Details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


    }
}
