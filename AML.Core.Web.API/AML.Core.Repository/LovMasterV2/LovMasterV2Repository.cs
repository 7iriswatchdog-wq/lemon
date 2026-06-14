using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.LovMaster;
using AML.Core.RepositoryContract.LovMasterV2;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.RiskV2;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace AML.Core.Repository.LovMasterV2
{
    public class LovMasterV2Repository : BaseRepository,  ILovMasterV2Repository
    {
        public LovMasterV2Repository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }
        public ServiceResponse<List<LovMasterDTO>> GetAllLovMaster()
        {
            ServiceResponse<List<LovMasterDTO>> serviceResponse = new ServiceResponse<List<LovMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<LovMasterDTO>("get_all_lovmaster_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Country details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<LovMasterDTO>> GetAllLovMasterCategories()
        {
            ServiceResponse<List<LovMasterDTO>> serviceResponse = new ServiceResponse<List<LovMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<LovMasterDTO>("get_lovmaster_risk_category_all_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Category fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<LovMasterDTO>> GetRiskTypes(string riskCategory,int clientId)
        {
            ServiceResponse<List<LovMasterDTO>> serviceResponse = new ServiceResponse<List<LovMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_lov_catogory", riskCategory);
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<LovMasterDTO>("get_lovtype_by_catogorytype_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        public ServiceResponse<List<LovMasterDTO>> GetRiskType(string riskCategory,int clientId)
        {
            ServiceResponse<List<LovMasterDTO>> serviceResponse = new ServiceResponse<List<LovMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_lov_catogory", riskCategory);
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<LovMasterDTO>("get_lov_risktype_by_categorytype_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        public ServiceResponse<List<LovTypeCategoryDTO>> GetRiskCategoryTypes(string riskCategoryID,int clientId)
        {
            ServiceResponse<List<LovTypeCategoryDTO>> serviceResponse = new ServiceResponse<List<LovTypeCategoryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_lov_category", riskCategoryID);
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<LovTypeCategoryDTO>("get_lovtypeCategory_by_riskcategory_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        public ServiceResponse<List<LovTypeCategoryDTO>> GetRiskCategoryType(string riskCategoryID,int clientId)
        {
            ServiceResponse<List<LovTypeCategoryDTO>> serviceResponse = new ServiceResponse<List<LovTypeCategoryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_lov_risk_category_code", riskCategoryID);
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<LovTypeCategoryDTO>("get_lov_riskcategorytype_by_riskcategory_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        public ServiceResponse<List<LovMasterDTO>> GetRiskItems(int riskTypeID, int clientId)
        {
            ServiceResponse<List<LovMasterDTO>> serviceResponse = new ServiceResponse<List<LovMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_lov_type_id", riskTypeID);
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<LovMasterDTO>("get_lov_riskitems_by_lovtype_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk items fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<ProdtypecategoryDTO>> GetProductRiskCategoryType(int clientId)
        {
            ServiceResponse<List<ProdtypecategoryDTO>> serviceResponse = new ServiceResponse<List<ProdtypecategoryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<ProdtypecategoryDTO>("get_prod_by_riskcategory", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk items fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> UpdateRiskItemStatus(int id, int status)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                parameters.Add("@p_status", status);
                var response = ExecuteScalar("mod_lovmaster_active_status", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "status updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> InsertLovMaster(LovMasterDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                var active = model.IsActive ? "1" : "0";
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_lov_risk_catogory", model.LovRiskCategoryCode);
                parameters.Add("@p_lov_risk_catogory_code", model.LovRiskCategory);
                parameters.Add("@p_lov_type_id", model.LovTypeId);
                parameters.Add("@p_lov_type_name", model.LovTypeName);
                parameters.Add("@p_lov_risk_data", model.LovRiskData);
                parameters.Add("@p_lov_risk_score", model.LovRiskScore);
                parameters.Add("@p_Over_ride_Score", Convert.ToInt32(model.OverrideScore));
                parameters.Add("@p_clientId", model.ClientId);
                //parameters.Add("@p_active_yn", active); TODO - pass correct value type
                parameters.Add("@p_date_created", DateTime.Now);
                parameters.Add("@p_createdBy", model.CreatedBy);
                parameters.Add("@p_clientId", model.ClientId);
                var response = ExecuteScalar("ins_lovmaster_v2", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "risk inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> InsertLovTypeCategory(LovTypeCategoryDTO model)
        {

            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_lov_type_category", model.LovCategoryType);
                parameters.Add("@p_lov_risk_category_code", model.LovRiskCategoryCode);
                parameters.Add("@p_lov_risk_category", model.LovRiskCategory);
                parameters.Add("@p_clientId", model.ClientId);
                parameters.Add("@p_createdBy", model.CreatedBy);
                var response = ExecuteScalar("ins_lovtypecategory_v2", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "risk type category inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {

            }

            return serviceResponse;
        }

        public ServiceResponse<int> InsertLovType(LovTypeMasterDTO model)
        {

            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_lov_risk_category", model.LovRiskCategory);
                parameters.Add("@p_lov_risk_category_code", model.LovRiskCategoryCode);
                parameters.Add("@p_lov_type_id", model.LovTypeId);
                parameters.Add("@p_lovtype_category_id", model.LovTypeCategoryId);
                parameters.Add("@p_lov_type_name", model.LovTypeName);
                parameters.Add("@p_clientId", model.ClientId);
                parameters.Add("@p_lovcountryduplicate", model.lov_country_duplicate);
                parameters.Add("@p_percentage", model.Percentage);
                var response = ExecuteScalar("ins_lovtype_v2", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "risk type category inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {

            }

            return serviceResponse;
        }

        public ServiceResponse<int> UpdateLovTypeCategory(LovTypeCategoryDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", model.LovTypeCategoryId);
                parameters.Add("@p_lov_type_category", model.LovCategoryType);
                parameters.Add("@p_lov_risk_category_code", model.LovRiskCategoryCode);
                parameters.Add("@p_lov_risk_category", model.LovRiskCategory);
                parameters.Add("@p_active_yn", model.IsActive ? 1 : 0);
                parameters.Add("@p_clientId", model.ClientId);
                parameters.Add("@p_createdBy", model.CreatedBy);
                var response = ExecuteScalar("mod_lovtypecategory_v2", parameters, commandType: CommandType.StoredProcedure).ParseInt();
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
        public ServiceResponse<int> UpdateLovType(LovTypeMasterDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", model.Id);
                parameters.Add("@p_lov_risk_category", model.LovRiskCategory);
                parameters.Add("@p_lov_risk_category_code", model.LovRiskCategoryCode);
                parameters.Add("@p_lov_type_id", model.LovTypeId);
                parameters.Add("@p_lovtype_category_id", model.LovTypeCategoryId);
                parameters.Add("@p_lov_type_name", model.LovTypeName);
                parameters.Add("@p_active_yn", model.IsActive ? 1 : 0);
                parameters.Add("@p_clientId", model.ClientId);
                parameters.Add("@p_lovcountryduplicate", model.lov_country_duplicate);
                parameters.Add("@p_percentage", model.Percentage);
                var response = ExecuteScalar("mod_lovtype_v2", parameters, commandType: CommandType.StoredProcedure).ParseInt();
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

        public ServiceResponse<int> UpdateLovMaster(LovMasterDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", model.Id);
                parameters.Add("@p_lov_risk_catogory", model.LovRiskCategoryCode);
                parameters.Add("@p_lov_risk_catogory_code", model.LovRiskCategory);
                parameters.Add("@p_lov_type_id", model.LovTypeId);
                parameters.Add("@p_lov_type_name", model.LovTypeName);
                parameters.Add("@p_lov_risk_data", model.LovRiskData);
                parameters.Add("@p_lov_risk_score", model.LovRiskScore);
                parameters.Add("@p_Over_ride_Score", Convert.ToInt32(model.OverrideScore));
                parameters.Add("@p_active_yn", model.IsActive ? 1 : 0);
                parameters.Add("@p_date_updated", DateTime.Now);
                parameters.Add("@p_clientId", model.ClientId);
                var response = ExecuteScalar("mod_lovmaster_v2", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "status updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<RiskTypeCategoryDTOV2>> GetAllRiskConfig(string riskCategoryID, int SelectID, int CategoryID, int TypeID,int clientId)
        {
            ServiceResponse<List<RiskTypeCategoryDTOV2>> serviceResponse = new ServiceResponse<List<RiskTypeCategoryDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_risk_code", riskCategoryID);
                parameters.Add("@p_lov_select", SelectID);
                parameters.Add("@p_lov_type_category_id", CategoryID);
                parameters.Add("@p_lov_type_id", TypeID);
                parameters.Add("@p_clientId", clientId);
                  serviceResponse.Result = Get<RiskTypeCategoryDTOV2>("get_riskconfiguration_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
                    RIskConfigurationMasterDTOV2 _riskConfig = new RIskConfigurationMasterDTOV2();
                    _riskConfig.RiskTypeCategories = serviceResponse.Result;
                    serviceResponse.Message = "Category details fetched successfully.";
                    serviceResponse.Status = StaticResource.SuccessStatusCode;
                    //foreach (var category in _riskConfig.RiskTypeCategories)
                    for(var i=0;i< _riskConfig.RiskTypeCategories.Count;i++)
                    {
                        ServiceResponse<List<RiskTypeDTOV2>> serviceResponse1 = new ServiceResponse<List<RiskTypeDTOV2>>();
                        parameters.Add("@p_lov_type_category_id", _riskConfig.RiskTypeCategories[i].Id);
                        parameters.Add("@p_lov_select", 2);
                        serviceResponse1.Result = Get<RiskTypeDTOV2>("get_riskconfiguration_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
                        serviceResponse1.Message = "Sub Category details fetched successfully.";
                        serviceResponse1.Status = StaticResource.SuccessStatusCode;
                        _riskConfig.RiskTypeCategories[i].RiskTypes = serviceResponse1.Result;
                        for(var j=0;j< serviceResponse1.Result.Count; j++)
                        {
                            ServiceResponse<List<RiskItemsDTOV2>> serviceResponse2 = new ServiceResponse<List<RiskItemsDTOV2>>();
                        if (_riskConfig.RiskTypeCategories[i].RiskTypes[j].lov_country_duplicate == 0)
                        {
                            parameters.Add("@p_lov_type_id", _riskConfig.RiskTypeCategories[i].RiskTypes[j].Id);
                            parameters.Add("@p_lov_select", 3);
                            serviceResponse2.Result = Get<RiskItemsDTOV2>("get_riskconfiguration_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
                            serviceResponse2.Message = "Sub Category details fetched successfully.";
                            serviceResponse2.Status = StaticResource.SuccessStatusCode;
                            _riskConfig.RiskTypeCategories[i].RiskTypes[j].RiskItems = serviceResponse2.Result;
                        }
                        else if (_riskConfig.RiskTypeCategories[i].RiskTypes[j].lov_country_duplicate == 1)
                        {
                            parameters.Add("@p_clientId", clientId);
                            serviceResponse2.Result = Get<RiskItemsDTOV2>("get_all_countrymaster", parameters, commandType: CommandType.StoredProcedure).ToList();
                            serviceResponse2.Message = "Sub Category details fetched successfully.";
                            serviceResponse2.Status = StaticResource.SuccessStatusCode;
                            _riskConfig.RiskTypeCategories[i].RiskTypes[j].RiskItems = serviceResponse2.Result;
                        }
                        }
                    }
                
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<RiskTypeCategoryDTOV2>> GetAllRiskConfigReport(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId,int dt)
        {
            ServiceResponse<List<RiskTypeCategoryDTOV2>> serviceResponse = new ServiceResponse<List<RiskTypeCategoryDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_risk_code", riskCategoryID);
                parameters.Add("@p_lov_select", SelectID);
                parameters.Add("@p_lov_type_category_id", CategoryID);
                parameters.Add("@p_lov_type_id", TypeID);
                parameters.Add("@p_clientId", clientId);
                parameters.Add("@p_version",dt);
                serviceResponse.Result = Get<RiskTypeCategoryDTOV2>("get_riskconfiguration_report_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
                RIskConfigurationMasterDTOV2 _riskConfig = new RIskConfigurationMasterDTOV2();
                _riskConfig.RiskTypeCategories = serviceResponse.Result;
                serviceResponse.Message = "Category details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                //foreach (var category in _riskConfig.RiskTypeCategories)
                for (var i = 0; i < _riskConfig.RiskTypeCategories.Count; i++)
                {
                    ServiceResponse<List<RiskTypeDTOV2>> serviceResponse1 = new ServiceResponse<List<RiskTypeDTOV2>>();
                    parameters.Add("@p_lov_type_category_id", _riskConfig.RiskTypeCategories[i].Id);
                    parameters.Add("@p_lov_select", 2);
                    serviceResponse1.Result = Get<RiskTypeDTOV2>("get_riskconfiguration_report_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "Sub Category details fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    _riskConfig.RiskTypeCategories[i].RiskTypes = serviceResponse1.Result;
                    for (var j = 0; j < serviceResponse1.Result.Count; j++)
                    {
                        ServiceResponse<List<RiskItemsDTOV2>> serviceResponse2 = new ServiceResponse<List<RiskItemsDTOV2>>();
                        if (_riskConfig.RiskTypeCategories[i].RiskTypes[j].lov_country_duplicate == 0)
                        {
                            parameters.Add("@p_lov_type_id", _riskConfig.RiskTypeCategories[i].RiskTypes[j].Id);
                            parameters.Add("@p_lov_select", 3);
                            serviceResponse2.Result = Get<RiskItemsDTOV2>("get_riskconfiguration_report_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
                            serviceResponse2.Message = "Sub Category details fetched successfully.";
                            serviceResponse2.Status = StaticResource.SuccessStatusCode;
                            _riskConfig.RiskTypeCategories[i].RiskTypes[j].RiskItems = serviceResponse2.Result;
                        }
                        else if (_riskConfig.RiskTypeCategories[i].RiskTypes[j].lov_country_duplicate == 1)
                        {
                            parameters.Add("@p_clientId", clientId);
                            serviceResponse2.Result = Get<RiskItemsDTOV2>("get_all_countrymaster", parameters, commandType: CommandType.StoredProcedure).ToList();
                            serviceResponse2.Message = "Sub Category details fetched successfully.";
                            serviceResponse2.Status = StaticResource.SuccessStatusCode;
                            _riskConfig.RiskTypeCategories[i].RiskTypes[j].RiskItems = serviceResponse2.Result;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        //kyc risk configuration

        public ServiceResponse<List<RiskTypeCategoryDTOV2>> GetAllKycRiskConfig(string riskCategoryID, int SelectID, int CategoryID, int TypeID, int clientId)
        {
            ServiceResponse<List<RiskTypeCategoryDTOV2>> serviceResponse = new ServiceResponse<List<RiskTypeCategoryDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_risk_code", riskCategoryID);
                parameters.Add("@p_lov_select", SelectID);
                parameters.Add("@p_lov_type_category_id", CategoryID);
                parameters.Add("@p_lov_type_id", TypeID);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<RiskTypeCategoryDTOV2>("get_kyc_riskconfiguration", parameters, commandType: CommandType.StoredProcedure).ToList();
                RIskConfigurationMasterDTOV2 _riskConfig = new RIskConfigurationMasterDTOV2();
                _riskConfig.RiskTypeCategories = serviceResponse.Result;
                serviceResponse.Message = "Category details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                //foreach (var category in _riskConfig.RiskTypeCategories)
                for (var i = 0; i < _riskConfig.RiskTypeCategories.Count; i++)
                {
                    ServiceResponse<List<RiskTypeDTOV2>> serviceResponse1 = new ServiceResponse<List<RiskTypeDTOV2>>();
                    parameters.Add("@p_lov_type_category_id", _riskConfig.RiskTypeCategories[i].Id);
                    parameters.Add("@p_lov_select", 2);
                    serviceResponse1.Result = Get<RiskTypeDTOV2>("get_kyc_riskconfiguration", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "Sub Category details fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    _riskConfig.RiskTypeCategories[i].RiskTypes = serviceResponse1.Result;
                    for (var j = 0; j < serviceResponse1.Result.Count; j++)
                    {
                        ServiceResponse<List<RiskItemsDTOV2>> serviceResponse2 = new ServiceResponse<List<RiskItemsDTOV2>>();
                        if (_riskConfig.RiskTypeCategories[i].RiskTypes[j].lov_country_duplicate == 0)
                        {

                            parameters.Add("@p_lov_type_id", _riskConfig.RiskTypeCategories[i].RiskTypes[j].Id);
                            parameters.Add("@p_lov_select", 3);
                            serviceResponse2.Result = Get<RiskItemsDTOV2>("get_kyc_riskconfiguration", parameters, commandType: CommandType.StoredProcedure).ToList();
                            serviceResponse2.Message = "Sub Category details fetched successfully.";
                            serviceResponse2.Status = StaticResource.SuccessStatusCode;
                            _riskConfig.RiskTypeCategories[i].RiskTypes[j].RiskItems = serviceResponse2.Result;
                        }
                        else if (_riskConfig.RiskTypeCategories[i].RiskTypes[j].lov_country_duplicate == 1)
                        {
                            parameters.Add("@p_clientId", clientId);
                            serviceResponse2.Result = Get<RiskItemsDTOV2>("get_all_countrymaster", parameters, commandType: CommandType.StoredProcedure).ToList();
                            serviceResponse2.Message = "Sub Category details fetched successfully.";
                            serviceResponse2.Status = StaticResource.SuccessStatusCode;
                            _riskConfig.RiskTypeCategories[i].RiskTypes[j].RiskItems = serviceResponse2.Result;
                        }
                    }
                }

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
