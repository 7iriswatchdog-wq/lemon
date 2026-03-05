using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.EWRA;
using AML.DTO.DTO.EWRA;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.EWRA
{
    public class EWRARepository : BaseRepository, IEWRARepository
    {
        public EWRARepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }
        public ServiceResponse<List<EWRACustomerTypeDTO>> GetAllCustomerTypes(int type, int clientId)
        {
            ServiceResponse<List<EWRACustomerTypeDTO>> serviceResponse = new ServiceResponse<List<EWRACustomerTypeDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_type", type);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<EWRACustomerTypeDTO>("get_all_ewra_customer_types", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAProductCategoryDTO>> GetAllProductCategories(int clientId)
        {
            ServiceResponse<List<EWRAProductCategoryDTO>> serviceResponse = new ServiceResponse<List<EWRAProductCategoryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_categoryId", 0);
                parameters.Add("@p_type", 1);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<EWRAProductCategoryDTO>("get_all_ewra_product_category", parameters, commandType: CommandType.StoredProcedure).ToList();
                EWRAModelDTO _model = new EWRAModelDTO();
                _model.EWRAProductCategory = serviceResponse.Result;
                serviceResponse.Message = "Product Categories fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                for (var i = 0; i < _model.EWRAProductCategory.Count; i++)
                {
                    ServiceResponse<List<EWRAProductListDTO>> serviceResponse1 = new ServiceResponse<List<EWRAProductListDTO>>();
                    parameters.Add("@p_categoryId", 1);
                    parameters.Add("@p_type", 2);
                    serviceResponse1.Result = Get<EWRAProductListDTO>("get_all_ewra_product_category", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "Product list fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    _model.EWRAProductCategory[i].EWRAProductList = serviceResponse1.Result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRACounterPartyTypeDTO>> GetAllCounterParty(int type,int clientId)
        {
            ServiceResponse<List<EWRACounterPartyTypeDTO>> serviceResponse = new ServiceResponse<List<EWRACounterPartyTypeDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_typeId", 0);
                parameters.Add("@p_kind", 1);
                parameters.Add("@p_clientId", clientId);
                parameters.Add("@p_type", type);
                serviceResponse.Result = Get<EWRACounterPartyTypeDTO>("get_all_ewra_counterparty", parameters, commandType: CommandType.StoredProcedure).ToList();
                EWRAModelDTO _model = new EWRAModelDTO();
                _model.EWRACounterPartyType = serviceResponse.Result;
                serviceResponse.Message = "Product Categories fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                for (var i = 0; i < _model.EWRACounterPartyType.Count; i++)
                {
                    ServiceResponse<List<EWRACounterPartyListDTO>> serviceResponse1 = new ServiceResponse<List<EWRACounterPartyListDTO>>();
                    parameters.Add("@p_typeId", _model.EWRACounterPartyType[i].TypeId);
                    parameters.Add("@p_kind", 2);
                    parameters.Add("@p_clientId", clientId);
                    parameters.Add("@p_type", type);
                    serviceResponse1.Result = Get<EWRACounterPartyListDTO>("get_all_ewra_counterparty", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "Product list fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    _model.EWRACounterPartyType[i].EWRACounterPartyList = serviceResponse1.Result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRADeliveryTypeDTO>> GetAllDeliveryType(int clientId)
        {
            ServiceResponse<List<EWRADeliveryTypeDTO>> serviceResponse = new ServiceResponse<List<EWRADeliveryTypeDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<EWRADeliveryTypeDTO>("get_all_ewra_deliverytype", parameters, commandType: CommandType.StoredProcedure).ToList();
                EWRAModelDTO _model = new EWRAModelDTO();
                _model.EWRADeliveryType = serviceResponse.Result;
                serviceResponse.Message = "Delivery Types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAJurisdictionDTO>> GetAllJurisdictionCountry(int Id)
        {
            ServiceResponse<List<EWRAJurisdictionDTO>> serviceResponse = new ServiceResponse<List<EWRAJurisdictionDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_countryId", Id);
                serviceResponse.Result = Get<EWRAJurisdictionDTO>("get_ewra_jurisdiction_country_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                EWRAModelDTO _model = new EWRAModelDTO();
                _model.EWRAJurisdiction = serviceResponse.Result;
                serviceResponse.Message = "countries fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<string> SaveCustomerProfile(EWRAModelDTO model)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@p_dateOfAssessment", model.DateOfAssessment);
                parameters.Add("@p_custVolumeScore", model.CustVolumeScore);
                parameters.Add("@p_custCountScore", model.CustCountScore);
                parameters.Add("@p_custTransactionScore", model.CustTransactionScore);
                parameters.Add("@p_custOverallScore", model.CustOverallScore);
                parameters.Add("@p_custOverallRisk", model.CustOverallRisk);
                parameters.Add("@p_counterOverallScore", model.CounterOverallScore);
                parameters.Add("@p_counterOverallRisk", model.CounterOverallRisk);
                parameters.Add("@p_productsOverallScore", model.ProductOverallScore);
                parameters.Add("@p_productsOverallRisk", model.ProductOverallRisk);
                parameters.Add("@p_jurisdictionOverallScore", model.JurisdictionOverallScore);
                parameters.Add("@p_jurisdictionOverallRisk", model.JurisdictionOverallRisk);
                parameters.Add("@p_deliveryOverallScore", model.DeliveryOverallScore);
                parameters.Add("@p_deliveryOverallRisk", model.DeliveryOverallRisk);
                parameters.Add("@p_custStatus", 1);//1 for save, 2 for submit
                parameters.Add("@p_createdBy", model.CreatedBy);
                parameters.Add("@p_clientId", model.clientId);
                parameters.Add("@p_createdOn", DateTime.Now);
                parameters.Add("@p_QuantitativeName", model.QuantitativeName);
                string custProfData = "";
                int index = 0;
                for (int i = 0; i < model.EWRACustomerVolume.Count; i++)
                {
                    custProfData += model.EWRACustomerVolume[i].CategoryId + "и" + model.EWRACustomerVolume[i].TypeId + "и" + model.EWRACustomerVolume[i].High + "и" +
                        model.EWRACustomerVolume[i].Medium + "и" + model.EWRACustomerVolume[i].Low + "и" + model.EWRACustomerVolume[i].Total + "и" +
                        model.EWRACustomerVolume[i].RiskScore + "и" + model.EWRACustomerVolume[i].Status + "Ѕ";
                    index++;
                }
                for (int i = 0; i < model.EWRACustomerCount.Count; i++)
                {
                    custProfData += model.EWRACustomerCount[i].CategoryId + "и" + model.EWRACustomerCount[i].TypeId + "и" + model.EWRACustomerCount[i].High + "и" +
                        model.EWRACustomerCount[i].Medium + "и" + model.EWRACustomerCount[i].Low + "и" + model.EWRACustomerCount[i].Total + "и" +
                        model.EWRACustomerCount[i].RiskScore + "и" + model.EWRACustomerCount[i].Status + "Ѕ";
                    index++;
                }
                for (int i = 0; i < model.EWRACustomerTransaction.Count; i++)
                {
                    custProfData += model.EWRACustomerTransaction[i].CategoryId + "и" + model.EWRACustomerTransaction[i].TypeId + "и" + model.EWRACustomerTransaction[i].High + "и" +
                        model.EWRACustomerTransaction[i].Medium + "и" + model.EWRACustomerTransaction[i].Low + "и" + model.EWRACustomerTransaction[i].Total + "и" +
                        model.EWRACustomerTransaction[i].RiskScore + "и" + model.EWRACustomerTransaction[i].Status + "Ѕ";
                    index++;
                }
                Console.WriteLine($"p_cust_data length: {custProfData.Length}");
                parameters.Add("@p_cust_data", custProfData);
                parameters.Add("@p_index", index);
                string counterData = "";
                int counterIndex = 0;
                for (int i = 0; i < model.EWRACounterPartyType.Count; i++)
                {
                    for (int j = 0; j < model.EWRACounterPartyType[i].EWRACounterPartyList.Count; j++)
                    {
                        if (model.EWRACounterPartyType[i].EWRACounterPartyList[j].CounterParty == "") { continue; }
                        counterData +=
                        model.EWRACounterPartyType[i].EWRACounterPartyList[j].CounterParty + "и" +
                        model.EWRACounterPartyType[i].EWRACounterPartyList[j].selectedRiskId + "и" +
                        model.EWRACounterPartyType[i].EWRACounterPartyList[j].selectedRisk + "и" +
                        model.EWRACounterPartyType[i].EWRACounterPartyList[j].Country + "и" +
                        model.EWRACounterPartyType[i].EWRACounterPartyList[j].CountryId + "и" +
                        model.EWRACounterPartyType[i].EWRACounterPartyList[j].CountryRiskScore + "и" +
                        model.EWRACounterPartyType[i].EWRACounterPartyList[j].CountryRiskRating + "и" +
                        model.EWRACounterPartyType[i].EWRACounterPartyList[j].ExpenseAmount + "и" +
                        model.EWRACounterPartyType[i].EWRACounterPartyList[j].OverrideApplied + "Ѕ";
                        counterIndex++;
                    }
                }
                Console.WriteLine($"p_counter_data length: {counterData.Length}");
                parameters.Add("@p_counter_data", counterData);
                parameters.Add("@p_counter_index", counterIndex);
                string productData = "";
                int productIndex = 0;
                for (int i = 0; i < model.EWRAProductCategory.Count; i++)
                {
                    for (int j = 0; j < model.EWRAProductCategory[i].EWRAProductList.Count; j++)
                    {
                        productData += model.EWRAProductCategory[i].EWRAProductList[j].Product + "и" + model.EWRAProductCategory[i].EWRAProductList[j].ProductId + "и" + model.EWRAProductCategory[i].EWRAProductList[j].High + "и" + model.EWRAProductCategory[i].EWRAProductList[j].Medium + "и" + model.EWRAProductCategory[i].EWRAProductList[j].Low
                             + "и" + model.EWRAProductCategory[i].EWRAProductList[j].Total + "и" + model.EWRAProductCategory[i].EWRAProductList[j].RiskScore + "Ѕ";
                        productIndex++;
                    }
                }
                Console.WriteLine($"p_product_data length: {productData.Length}");

                parameters.Add("@p_product_data", productData);
                parameters.Add("@p_product_index", productIndex);
                string jurData = "";
                int jurIndex = 0;
                for (int i = 0; i < model.EWRAJurisdiction.Count; i++)
                {
                    jurData += model.EWRAJurisdiction[i].Country + "и" + (model.EWRAJurisdiction[i].CountryId == 0 ? 0 : model.EWRAJurisdiction[i].CountryId) + "и" + model.EWRAJurisdiction[i].CountryRiskScore + "и" + model.EWRAJurisdiction[i].CountryRiskRating + "и" + model.EWRAJurisdiction[i].CountryRiskLevel + "и" + model.EWRAJurisdiction[i].LedgerAmount + "Ѕ";
                    jurIndex++;
                }
                Console.WriteLine($"p_jur_data length: {jurData.Length}");

                parameters.Add("@p_jur_data", jurData);
                parameters.Add("@p_jur_index", jurIndex);
                string deliveryData = "";
                int deliveryIndex = 0;
                for (int i = 0; i < model.EWRADeliveryType.Count; i++)
                {
                    deliveryData += model.EWRADeliveryType[i].DeliveryTypeId + "и" + model.EWRADeliveryType[i].DeliveryType + "и" + model.EWRADeliveryType[i].Cash + "и" + model.EWRADeliveryType[i].Bank + "и" + model.EWRADeliveryType[i].RiskLevel + "и" + model.EWRADeliveryType[i].RiskScore + "Ѕ";
                    deliveryIndex++;
                }
                Console.WriteLine($"p_delivery_data length: {deliveryData.Length}");

                parameters.Add("@p_delivery_data", deliveryData);
                parameters.Add("@p_delivery_index", deliveryIndex);
                parameters.Add("@p_overallScore", model.QuantitativeOverallScore);
                parameters.Add("@p_overallRisk", model.QuantitativeOverallRisk);
                parameters.Add("@p_Id", model.Id);
                parameters.Add("@p_SaveType", model.SaveType);
                var response = ExecuteScalar("ins_ewraCustData", parameters, commandType: CommandType.StoredProcedure).ToString();
                serviceResponse.Result = response;
                serviceResponse.Message = "EWRA updated succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> UpdateCustomerType(EWRACustomerTypeDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_custTypeId", model.CustTypeId);
                parameters.Add("@p_custType", model.CustType);
                parameters.Add("@p_active_yn", model.isActive);
              
                var response = ExecuteScalar("mod_ewra_customertype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer Type updated succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> InsertCustomerType(EWRACustomerTypeDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_custTypeId", model.CustTypeId);
                parameters.Add("@p_custType", model.CustType);
                parameters.Add("@p_active_yn", model.isActive);
                parameters.Add("@p_clientId", model.ClientId);
                var response = ExecuteScalar("ins_ewra_customertype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "customer type inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

            }
            return serviceResponse;
        }
        //counterparty

        public ServiceResponse<int> UpdateCounterparty(EWRACounterPartyListDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Id", model.Id);
                parameters.Add("@p_CounterParty", model.CounterParty);
                parameters.Add("@p_CountryId", model.Country);
                parameters.Add("@p_active_yn", model.isActive);
                var response = ExecuteScalar("mod_ewra_counterparty_master", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer Type updated succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> InsertCounterparty(EWRACounterPartyListDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_CounterParty", model.CounterParty);
                parameters.Add("@p_CountryId", model.Country);
                parameters.Add("@p_active_yn", 1);
                parameters.Add("@p_type", 1);
                parameters.Add("@p_clientId", model.ClientId);
                var response = ExecuteScalar("ins_ewra_counterparty", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "customer type inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

            }
            return serviceResponse;
        }
        public ServiceResponse<int> InsertRiskDescription(RiskDescriptionDataDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Id", model.DescriptionId);
                parameters.Add("@p_data", model.RiskDescription);
                parameters.Add("@p_active_yn", model.IsActive);
                parameters.Add("@p_mode", 1);
                parameters.Add("@p_typeId", model.TypeId);
                parameters.Add("@p_clientId", model.ClientId);
                var response = ExecuteScalar("ins_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "customer type inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

            }
            return serviceResponse;
        }
        public ServiceResponse<int> UpdateRiskDescription(RiskDescriptionDataDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Id", model.DescriptionId);
                parameters.Add("@p_data", model.RiskDescription);
                parameters.Add("@p_active_yn", model.IsActive);
                parameters.Add("@p_mode", 1);
                parameters.Add("@p_typeId", model.TypeId);
                var response = ExecuteScalar("mod_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer Type updated succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> InsertInherentRisk(InherentRiskModelDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Id", model.InherentRiskId);
                parameters.Add("@p_data", model.InherentRiskDescription);
                parameters.Add("@p_active_yn", model.IsActive);
                parameters.Add("@p_mode", 2);
                parameters.Add("@p_typeId", model.TypeId);
                parameters.Add("@p_clientId", model.ClientId);
                var response = ExecuteScalar("ins_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "inherent Risk inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

            }
            return serviceResponse;
        }
        public ServiceResponse<int> UpdateInherentRisk(InherentRiskModelDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Id", model.InherentRiskId);
                parameters.Add("@p_data", model.InherentRiskDescription);
                parameters.Add("@p_active_yn", model.IsActive);
                parameters.Add("@p_mode", 2);
                parameters.Add("@p_typeId", model.TypeId);
                var response = ExecuteScalar("mod_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Inherent Risk updated succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> InsertKeyControl(KeyControlModelDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Id", model.KeyId);
                parameters.Add("@p_data", model.KeyControl);
                parameters.Add("@p_active_yn", model.IsActive);
                parameters.Add("@p_mode", 3);
                parameters.Add("@p_typeId", model.TypeId);
                parameters.Add("@p_clientId", model.ClientId);
                var response = ExecuteScalar("ins_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Key Control inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

            }
            return serviceResponse;
        }
        public ServiceResponse<int> UpdateKeyControl(KeyControlModelDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Id", model.KeyId);
                parameters.Add("@p_data", model.KeyControl);
                parameters.Add("@p_active_yn", model.IsActive);
                parameters.Add("@p_mode", 3);
                parameters.Add("@p_typeId", model.TypeId);
                var response = ExecuteScalar("mod_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Key Control updated succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> InsertResidualRisk(ResidualRiskModelDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Id", model.ResidualRiskId);
                parameters.Add("@p_data", model.ResidualRiskDescription);
                parameters.Add("@p_active_yn", model.IsActive);
                parameters.Add("@p_mode", 4);
                parameters.Add("@p_typeId", model.TypeId);
                parameters.Add("@p_clientId", model.ClientId);
                var response = ExecuteScalar("ins_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Residual Risk inserted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

            }
            return serviceResponse;
        }
        public ServiceResponse<int> UpdateResidualRisk(ResidualRiskModelDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Id", model.ResidualRiskId);
                parameters.Add("@p_data", model.ResidualRiskDescription);
                parameters.Add("@p_active_yn", model.IsActive);
                parameters.Add("@p_mode", 4);
                parameters.Add("@p_typeId", model.TypeId);
                var response = ExecuteScalar("mod_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Residual Risk updated succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAModelDTO>> GetEwraBetweenDates(DateTime fromDate, DateTime toDate, int clientId)
        {
            ServiceResponse<List<EWRAModelDTO>> serviceResponse = new ServiceResponse<List<EWRAModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                var dtfrom = fromDate.Year + "-" + fromDate.Month + "-" + fromDate.Day;
                var dtto = toDate.Year + "-" + toDate.Month + "-" + (toDate.Day + 1);
                parameters.Add("p_from_date", dtfrom);
                parameters.Add("p_to_date", dtto);
                parameters.Add("p_clientId", clientId);
                serviceResponse.Result = Get<EWRAModelDTO>("get_all_ewra_assessment", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "EWRA Assessment Details fetched successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAModelDTO>> GetConsolidatedEwra(int Id, int type, int Qid)
        {
            ServiceResponse<List<EWRAModelDTO>> serviceResponse = new ServiceResponse<List<EWRAModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_Id", Id);
                parameters.Add("p_Type", type);
                parameters.Add("p_qid", Qid);
                serviceResponse.Result = Get<EWRAModelDTO>("get_ewra_consolidated_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "ewra consolidated data fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAModelDTO>> GetEwraCustomerProfileData(int Id)
        {
            ServiceResponse<List<EWRAModelDTO>> serviceResponse = new ServiceResponse<List<EWRAModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_Id", Id);
                parameters.Add("p_Category", 1);
                parameters.Add("p_type", 1);
                parameters.Add("p_qid", 0);
                serviceResponse.Result = Get<EWRAModelDTO>("get_ewra_consolidated_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "ewra customer profile data fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                EWRAModelDTO _model = new EWRAModelDTO();
                ServiceResponse<List<EWRACustomerVolumeDTO>> serviceResponse1 = new ServiceResponse<List<EWRACustomerVolumeDTO>>();
                try
                {
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 1);
                    parameters.Add("p_Type", 1);
                    serviceResponse1.Result = Get<EWRACustomerVolumeDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Result[0].EWRACustomerVolume = serviceResponse1.Result;
                    _model.EWRACustomerVolume = serviceResponse1.Result;
                    serviceResponse1.Message = "ewra customer profile data fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    serviceResponse1.Message = ex.Message;
                    serviceResponse1.Status = StaticResource.FailStatusCode;
                }
                ServiceResponse<List<EWRACustomerCountDTO>> serviceResponse2 = new ServiceResponse<List<EWRACustomerCountDTO>>();
                try
                {
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 2);
                    parameters.Add("p_Type", 1);
                    serviceResponse2.Result = Get<EWRACustomerCountDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Result[0].EWRACustomerCount = serviceResponse2.Result;
                    serviceResponse2.Message = "ewra customer profile data fetched successfully.";
                    serviceResponse2.Status = StaticResource.SuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    serviceResponse2.Message = ex.Message;
                    serviceResponse2.Status = StaticResource.FailStatusCode;
                }
                ServiceResponse<List<EWRACustomerTransactionDTO>> serviceResponse3 = new ServiceResponse<List<EWRACustomerTransactionDTO>>();
                try
                {
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 3);
                    parameters.Add("p_Type", 1);
                    serviceResponse3.Result = Get<EWRACustomerTransactionDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Result[0].EWRACustomerTransaction = serviceResponse3.Result;
                    serviceResponse3.Message = "ewra customer profile data fetched successfully.";
                    serviceResponse3.Status = StaticResource.SuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    serviceResponse3.Message = ex.Message;
                    serviceResponse3.Status = StaticResource.FailStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAModelDTO>> GetEwraCounterpartyData(int Id)
        {
            ServiceResponse<List<EWRAModelDTO>> serviceResponse = new ServiceResponse<List<EWRAModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_Id", Id);
                parameters.Add("p_Category", 0);
                parameters.Add("p_Type", 1);
                parameters.Add("p_qid", 0);
                serviceResponse.Result = Get<EWRAModelDTO>("get_ewra_consolidated_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "ewra consolidated data fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                List<EWRACounterPartyTypeDTO> t = new List<EWRACounterPartyTypeDTO>();
                EWRACounterPartyTypeDTO td = new EWRACounterPartyTypeDTO();
                td.TypeId = 1;
                t.Add(td);
                serviceResponse.Result[0].EWRACounterPartyType = t;
                for (var i = 0; i < serviceResponse.Result.Count; i++)
                {
                    ServiceResponse<List<EWRACounterPartyListDTO>> serviceResponse1 = new ServiceResponse<List<EWRACounterPartyListDTO>>();
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 0);
                    parameters.Add("p_Type", 2);
                    serviceResponse1.Result = Get<EWRACounterPartyListDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "ewra consolidated data fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    serviceResponse.Result[0].EWRACounterPartyType[0].EWRACounterPartyList = serviceResponse1.Result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAModelDTO>> GetEwraProductsData(int Id)
        {
            ServiceResponse<List<EWRAModelDTO>> serviceResponse = new ServiceResponse<List<EWRAModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_Id", Id);
                parameters.Add("p_Category", 0);
                parameters.Add("p_Type", 1);
                parameters.Add("p_qid", 0);
                serviceResponse.Result = Get<EWRAModelDTO>("get_ewra_consolidated_data", parameters, commandType: CommandType.StoredProcedure).ToList(); ;
                serviceResponse.Message = "ewra consolidated data fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                for (var i = 0; i < 1; i++)
                {
                    ServiceResponse<List<EWRAProductListDTO>> serviceResponse1 = new ServiceResponse<List<EWRAProductListDTO>>();
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 0);
                    parameters.Add("p_Type", 3);
                    serviceResponse1.Result = Get<EWRAProductListDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "ewra consolidated data fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    serviceResponse.Result[0].EWRAProductList = serviceResponse1.Result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAModelDTO>> GetEwraJurisdictionData(int Id)
        {
            ServiceResponse<List<EWRAModelDTO>> serviceResponse = new ServiceResponse<List<EWRAModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_Id", Id);
                parameters.Add("p_Category", 0);
                parameters.Add("p_Type", 1);
                parameters.Add("p_qid", 0);
                serviceResponse.Result = Get<EWRAModelDTO>("get_ewra_consolidated_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "ewra customer profile data fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                EWRAModelDTO _model = new EWRAModelDTO();
                ServiceResponse<List<EWRAJurisdictionDTO>> serviceResponse1 = new ServiceResponse<List<EWRAJurisdictionDTO>>();
                try
                {
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 1);
                    parameters.Add("p_Type", 4);
                    serviceResponse1.Result = Get<EWRAJurisdictionDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Result[0].EWRAJurisdiction = serviceResponse1.Result;
                    _model.EWRAJurisdiction = serviceResponse1.Result;
                    serviceResponse1.Message = "ewra jurisdiction data fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    serviceResponse1.Message = ex.Message;
                    serviceResponse1.Status = StaticResource.FailStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAModelDTO>> GetEwraDeliveryData(int Id)
        {
            ServiceResponse<List<EWRAModelDTO>> serviceResponse = new ServiceResponse<List<EWRAModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_Id", Id);
                parameters.Add("p_Category", 0);
                parameters.Add("p_Type", 1);
                parameters.Add("p_qid", 0);
                serviceResponse.Result = Get<EWRAModelDTO>("get_ewra_consolidated_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "ewra customer profile data fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                EWRAModelDTO _model = new EWRAModelDTO();
                ServiceResponse<List<EWRADeliveryTypeDTO>> serviceResponse1 = new ServiceResponse<List<EWRADeliveryTypeDTO>>();
                try
                {
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 1);
                    parameters.Add("p_Type", 5);
                    serviceResponse1.Result = Get<EWRADeliveryTypeDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Result[0].EWRADeliveryType = serviceResponse1.Result;
                    _model.EWRADeliveryType = serviceResponse1.Result;
                    serviceResponse1.Message = "ewra delivery data fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    serviceResponse1.Message = ex.Message;
                    serviceResponse1.Status = StaticResource.FailStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAQualitativeModelDTO>> GetAllRiskTypes(int clientId)
        {
            ServiceResponse<List<EWRAQualitativeModelDTO>> serviceResponse = new ServiceResponse<List<EWRAQualitativeModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_index", 1);
                parameters.Add("@p_type", 0);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<EWRAQualitativeModelDTO>("get_all_ewra_qualitative_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                EWRAModelDTO _model = new EWRAModelDTO();
                _model.EWRAQualitativeModel = serviceResponse.Result;
                serviceResponse.Message = "Risk Types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                for (var i = 0; i < _model.EWRAQualitativeModel.Count; i++)
                {
                    ServiceResponse<List<EWRARiskModelDTO>> serviceResponse1 = new ServiceResponse<List<EWRARiskModelDTO>>();
                    parameters.Add("@p_index", 2);
                    parameters.Add("@p_type", _model.EWRAQualitativeModel[i].RiskTypeId);
                    parameters.Add("@p_clientId", clientId);
                    _model.EWRAQualitativeModel[i].EWRARiskModel = new List<EWRARiskModelDTO>();
                    serviceResponse1.Result = Get<EWRARiskModelDTO>("get_all_ewra_qualitative_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "Risk Description fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    _model.EWRAQualitativeModel[i].EWRARiskModel = serviceResponse1.Result;

                    for (var j = 0; j < _model.EWRAQualitativeModel[i].EWRARiskModel.Count; j++)
                    {
                        ServiceResponse<List<InherentRiskModelDTO>> serviceResponse2 = new ServiceResponse<List<InherentRiskModelDTO>>();
                        parameters.Add("@p_index", 3);
                        parameters.Add("@p_type", _model.EWRAQualitativeModel[i].RiskTypeId);
                        parameters.Add("@p_clientId", clientId);
                        _model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData = new List<InherentRiskModelDTO>();
                        serviceResponse2.Result = Get<InherentRiskModelDTO>("get_all_ewra_qualitative_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                        serviceResponse2.Message = "Risk Description fetched successfully.";
                        serviceResponse2.Status = StaticResource.SuccessStatusCode;
                        _model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData = serviceResponse2.Result;

                        ServiceResponse<List<KeyControlModelDTO>> serviceResponse3 = new ServiceResponse<List<KeyControlModelDTO>>();
                        parameters.Add("@p_index", 4);
                        parameters.Add("@p_type", _model.EWRAQualitativeModel[i].RiskTypeId);
                        parameters.Add("@p_clientId", clientId);
                        _model.EWRAQualitativeModel[i].EWRARiskModel[j].KeyControlModelData = new List<KeyControlModelDTO>();
                        serviceResponse3.Result = Get<KeyControlModelDTO>("get_all_ewra_qualitative_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                        serviceResponse3.Message = "Risk Description fetched successfully.";
                        serviceResponse3.Status = StaticResource.SuccessStatusCode;
                        _model.EWRAQualitativeModel[i].EWRARiskModel[j].KeyControlModelData = serviceResponse3.Result;

                        ServiceResponse<List<ResidualRiskModelDTO>> serviceResponse4 = new ServiceResponse<List<ResidualRiskModelDTO>>();
                        parameters.Add("@p_index", 5);
                        parameters.Add("@p_type", _model.EWRAQualitativeModel[i].RiskTypeId);
                        parameters.Add("@p_clientId", clientId);
                        _model.EWRAQualitativeModel[i].EWRARiskModel[j].ResidualRiskModelData = new List<ResidualRiskModelDTO>();
                        serviceResponse4.Result = Get<ResidualRiskModelDTO>("get_all_ewra_qualitative_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                        serviceResponse4.Message = "Risk Description fetched successfully.";
                        serviceResponse4.Status = StaticResource.SuccessStatusCode;
                        _model.EWRAQualitativeModel[i].EWRARiskModel[j].ResidualRiskModelData = serviceResponse4.Result;
                    }

                    //ServiceResponse<List<InherentRiskModelDTO>> serviceResponse2 = new ServiceResponse<List<InherentRiskModelDTO>>();
                    //parameters.Add("@p_index", 3);
                    //parameters.Add("@p_type", _model.EWRAQualitativeModel[i].RiskTypeId);
                    //serviceResponse2.Result = Get<InherentRiskModelDTO>("get_all_ewra_qualitative_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    //serviceResponse2.Message = "Risk Description fetched successfully.";
                    //serviceResponse2.Status = StaticResource.SuccessStatusCode;
                    //_model.EWRAQualitativeModel[i].InherentRiskModel = serviceResponse2.Result;

                    //ServiceResponse<List<KeyControlModelDTO>> serviceResponse3 = new ServiceResponse<List<KeyControlModelDTO>>();
                    //parameters.Add("@p_index", 4);
                    //parameters.Add("@p_type", _model.EWRAQualitativeModel[i].RiskTypeId);
                    //serviceResponse3.Result = Get<KeyControlModelDTO>("get_all_ewra_qualitative_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    //serviceResponse3.Message = "Risk Description fetched successfully.";
                    //serviceResponse3.Status = StaticResource.SuccessStatusCode;
                    //_model.EWRAQualitativeModel[i].KeyControlModel = serviceResponse3.Result;

                    //ServiceResponse<List<ResidualRiskModelDTO>> serviceResponse4 = new ServiceResponse<List<ResidualRiskModelDTO>>();
                    //parameters.Add("@p_index", 5);
                    //parameters.Add("@p_type", _model.EWRAQualitativeModel[i].RiskTypeId);
                    //serviceResponse4.Result = Get<ResidualRiskModelDTO>("get_all_ewra_qualitative_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    //serviceResponse4.Message = "Risk Description fetched successfully.";
                    //serviceResponse4.Status = StaticResource.SuccessStatusCode;
                    //_model.EWRAQualitativeModel[i].ResidualRiskModel = serviceResponse4.Result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<string> SaveCategorywiseAssessmentData(EWRAModelDTO model)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                string Data = "";
                int Index = 0;
                string inherentData = "";
                int inherentIndex = 0;
                string keyData = "";
                int keyIndex = 0;
                string residualData = "";
                int residualIndex = 0;
                for (int i = 0; i < model.EWRAQualitativeModel.Count; i++)
                {
                    for (int j = 0; j < model.EWRAQualitativeModel[i].EWRARiskModel.Count; j++)
                    {
                        for (var x = 0; x < model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData.Count; x++)
                        {
                            inherentData += model.EWRAQualitativeModel[i].RiskTypeId + "и" + model.EWRAQualitativeModel[i].RiskType + "и" +
                                model.EWRAQualitativeModel[i].EWRARiskModel[j].DescriptionId + "ии" + /*model.EWRAQualitativeModel[i].EWRARiskModel[j].RiskDescription*/
                                model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData[x].InherentRiskId + "ии" + /*model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData[x].InherentRiskDescription + "и" +*/
                                model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData[x].InherentImpactId + "и" + model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData[x].InherentImpact + "и" +
                                model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData[x].InherentLikelihoodId + "и" + model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData[x].InherentLikelihood + "и" +
                                model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData[x].InherentRiskRating + "и" + model.EWRAQualitativeModel[i].EWRARiskModel[j].InherentRiskModelData[x].InherentRiskScore + "Ѕ";
                            inherentIndex++;
                        }
                        for (var y = 0; y < model.EWRAQualitativeModel[i].EWRARiskModel[j].KeyControlModelData.Count; y++)
                        {
                            keyData += model.EWRAQualitativeModel[i].RiskTypeId + "ии" + /*model.EWRAQualitativeModel[i].RiskType + "и" +*/
                                model.EWRAQualitativeModel[i].EWRARiskModel[j].DescriptionId + "ии" + /*model.EWRAQualitativeModel[i].EWRARiskModel[j].RiskDescription + "и" +*/
                                model.EWRAQualitativeModel[i].EWRARiskModel[j].KeyControlModelData[y].KeyId + "ии" + /*model.EWRAQualitativeModel[i].EWRARiskModel[j].KeyControlModelData[y].KeyControl + "и" + */
                                model.EWRAQualitativeModel[i].EWRARiskModel[j].KeyControlModelData[y].SelectedKey + "ии" +
                                model.EWRAQualitativeModel[i].EWRARiskModel[j].ResidualRiskModelData[y].ResidualRiskScore + "Ѕ";
                            keyIndex++;

                        }
                       
                    }
                    Data += model.EWRAQualitativeModel[i].RiskTypeId + "и" + model.EWRAQualitativeModel[i].RiskType + "и" +
                        model.EWRAQualitativeModel[i].InherentImpactAvg + "и" + model.EWRAQualitativeModel[i].InherentLikelihoodAvg + "и" + model.EWRAQualitativeModel[i].InherentRiskRatingAvg + "и" + model.EWRAQualitativeModel[i].InherentRiskScoreAvg + "и" +
                        model.EWRAQualitativeModel[i].KeyControlRatingAvg + "и" + model.EWRAQualitativeModel[i].KeyControlScoreAvg + "и" +
                        model.EWRAQualitativeModel[i].ResidualRiskRatingAvg + "и" + model.EWRAQualitativeModel[i].ResidualRiskScoreAvg + "Ѕ";
                    Index++;
                }
                parameters.Add("@p_dateOfAssessment", model.DateOfAssessment);
                parameters.Add("@p_createdBy", model.QCreatedBy);
                parameters.Add("@p_createdOn", DateTime.Now);
                parameters.Add("@p_data", Data);
                parameters.Add("@p_index", Index);
                parameters.Add("@p_inherent_data", inherentData);
                parameters.Add("@p_inherent_index", inherentIndex);
                parameters.Add("@p_key_data", keyData);
                parameters.Add("@p_key_index", keyIndex);
                parameters.Add("@p_residual_data", residualData);
                parameters.Add("@p_residual_index", residualIndex);
                parameters.Add("@p_QualitativeOverallRating", model.QualitativeOverallRating);
                parameters.Add("@p_QualitativeOverallScore", model.QualitativeOverallScore);
                parameters.Add("@p_QCustOverallScore", model.QCustOverallScore);
                parameters.Add("@p_QCustOverallRisk", model.QCustOverallRisk);
                parameters.Add("@p_QCounterOverallScore", model.QCounterOverallScore);
                parameters.Add("@p_QCounterOverallRisk", model.QCounterOverallRisk);
                parameters.Add("@p_QProductOverallScore", model.QProductOverallScore);
                parameters.Add("@p_QProductOverallRisk", model.QProductOverallRisk);
                parameters.Add("@p_QJurisdictionOverallScore", model.QJurisdictionOverallScore);
                parameters.Add("@p_QJurisdictionOverallRisk", model.QJurisdictionOverallRisk);
                parameters.Add("@p_QDeliveryOverallScore", model.QDeliveryOverallScore);
                parameters.Add("@p_QDeliveryOverallRisk", model.QDeliveryOverallRisk);
                parameters.Add("@p_QID", model.QuantitativeId);
                parameters.Add("@p_SaveType", model.SaveType);
                parameters.Add("@p_Id", model.Id);
                parameters.Add("@p_clientId", model.clientId);
                var response = ExecuteScalar("ins_ewraQualitativeData", parameters, commandType: CommandType.StoredProcedure).ToString();
                serviceResponse.Result = response;
                serviceResponse.Message = "EWRA updated succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAQualitativeModelDTO>> GetEwraQualitativeSavedData(int Id, int Type)
        {
            ServiceResponse<List<EWRAQualitativeModelDTO>> serviceResponse = new ServiceResponse<List<EWRAQualitativeModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_index", 1);
                parameters.Add("@p_type", Type);
                parameters.Add("@p_Id", Id);
                parameters.Add("@p_typeId", Type);
                serviceResponse.Result = Get<EWRAQualitativeModelDTO>("get_ewra_detailed_data_qualitative", parameters, commandType: CommandType.StoredProcedure).ToList();
                //EWRAModelDTO _model = new EWRAModelDTO();
                List<EWRAQualitativeModelDTO> _model = new List<EWRAQualitativeModelDTO>();
                _model = serviceResponse.Result;
                serviceResponse.Message = "Risk Types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

                ServiceResponse<List<EWRARiskModelDTO>> serviceResponse1 = new ServiceResponse<List<EWRARiskModelDTO>>();
                parameters.Add("@p_index", 2);
                parameters.Add("@p_type", Type);
                parameters.Add("@p_Id", Id);
                parameters.Add("@p_typeId", Type);
                serviceResponse1.Result = Get<EWRARiskModelDTO>("get_ewra_detailed_data_qualitative", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse1.Message = "Risk Description fetched successfully.";
                serviceResponse1.Status = StaticResource.SuccessStatusCode;
                //_model.EWRAQualitativeModel[0].EWRARiskModel = serviceResponse1.Result;
                _model[0].EWRARiskModel = serviceResponse1.Result;

                //for (var j = 0; j < _model.EWRAQualitativeModel[0].EWRARiskModel.Count; j++)
                for (var j = 0; j < _model[0].EWRARiskModel.Count; j++)
                {
                    ServiceResponse<List<InherentRiskModelDTO>> serviceResponse2 = new ServiceResponse<List<InherentRiskModelDTO>>();
                    parameters.Add("@p_index", 3);
                    parameters.Add("@p_type", Type);
                    parameters.Add("@p_Id", Id);
                    parameters.Add("@p_typeId", _model[0].EWRARiskModel[j].DescriptionId);
                    serviceResponse2.Result = Get<InherentRiskModelDTO>("get_ewra_detailed_data_qualitative", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse2.Message = "Risk Description fetched successfully.";
                    serviceResponse2.Status = StaticResource.SuccessStatusCode;
                    _model[0].EWRARiskModel[j].InherentRiskModelData = serviceResponse2.Result;

                    ServiceResponse<List<KeyControlModelDTO>> serviceResponse3 = new ServiceResponse<List<KeyControlModelDTO>>();
                    parameters.Add("@p_index", 4);
                    parameters.Add("@p_type", Type);
                    parameters.Add("@p_Id", Id);
                    parameters.Add("@p_typeId", _model[0].EWRARiskModel[j].DescriptionId);
                    serviceResponse3.Result = Get<KeyControlModelDTO>("get_ewra_detailed_data_qualitative", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse3.Message = "Risk Description fetched successfully.";
                    serviceResponse3.Status = StaticResource.SuccessStatusCode;
                    _model[0].EWRARiskModel[j].KeyControlModelData = serviceResponse3.Result;

                    ServiceResponse<List<ResidualRiskModelDTO>> serviceResponse4 = new ServiceResponse<List<ResidualRiskModelDTO>>();
                    parameters.Add("@p_index", 5);
                    parameters.Add("@p_type", Type);
                    parameters.Add("@p_Id", Id);
                    parameters.Add("@p_typeId", _model[0].EWRARiskModel[j].DescriptionId);
                    serviceResponse4.Result = Get<ResidualRiskModelDTO>("get_ewra_detailed_data_qualitative", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse4.Message = "Risk Description fetched successfully.";
                    serviceResponse4.Status = StaticResource.SuccessStatusCode;
                    _model[0].EWRARiskModel[j].ResidualRiskModelData = serviceResponse4.Result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAQualitativeConfigModelDTO>> GetAllRiskConfigTypes()
        {
            ServiceResponse<List<EWRAQualitativeConfigModelDTO>> serviceResponse = new ServiceResponse<List<EWRAQualitativeConfigModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<EWRAQualitativeConfigModelDTO>("get_ewra_risk_types", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Category fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<RiskDescriptionDataDTO>> getAllRiskDescription(int riskTypeId, int clientId)
        {
            ServiceResponse<List<RiskDescriptionDataDTO>> serviceResponse = new ServiceResponse<List<RiskDescriptionDataDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_riskTypeId", riskTypeId);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<RiskDescriptionDataDTO>("get_ewra_risk_description", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Category fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRARiskModelDTO>> GetAllRiskDescription(int riskTypeId)
        {
            ServiceResponse<List<EWRARiskModelDTO>> serviceResponse = new ServiceResponse<List<EWRARiskModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_riskTypeId", riskTypeId);
                serviceResponse.Result = Get<EWRARiskModelDTO>("get_ewra_risk_description", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Category fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<InherentRiskModelDTO>> getAllInherentRisk(int riskTypeId)
        {
            ServiceResponse<List<InherentRiskModelDTO>> serviceResponse = new ServiceResponse<List<InherentRiskModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_riskTypeId", riskTypeId);
                parameters.Add("@p_Type", 1);
                serviceResponse.Result = Get<InherentRiskModelDTO>("get_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Category fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<KeyControlModelDTO>> getAllKeyControl(int riskTypeId)
        {
            ServiceResponse<List<KeyControlModelDTO>> serviceResponse = new ServiceResponse<List<KeyControlModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_riskTypeId", riskTypeId);
                parameters.Add("@p_Type", 2);
                serviceResponse.Result = Get<KeyControlModelDTO>("get_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Category fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<ResidualRiskModelDTO>> getAllResidual(int riskTypeId)
        {
            ServiceResponse<List<ResidualRiskModelDTO>> serviceResponse = new ServiceResponse<List<ResidualRiskModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_riskTypeId", riskTypeId);
                parameters.Add("@p_Type", 3);
                serviceResponse.Result = Get<ResidualRiskModelDTO>("get_ewra_config_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Category fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<string> GetPendingEWRA(int type, int userId,int clientId)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Type", type);
                parameters.Add("@p_userId", userId);
                parameters.Add("@p_clientId", clientId);
                var response = "";
                if (type == 1)
                {
                    var x = ExecuteScalar("get_pending_ewra", parameters, commandType: CommandType.StoredProcedure);
                    if (x != null)
                    {
                        response = x.ToString();
                    }
                    else
                    {
                        response = "";
                    }
                }
                else
                {
                    var x = Get<EWRAModelDTO>("get_pending_ewra", parameters, commandType: CommandType.StoredProcedure).ToList();
                    response = string.Empty;
                    foreach (var item in x)
                    {
                        response += item.QuantitativeId + "и" + item.QuantitativeName + "Ѕ";
                    }
                }
                if (response == null)
                {
                    response = string.Empty;
                }
                else
                {
                    response = response.ToString();
                }
                serviceResponse.Result = response.ToString();
                serviceResponse.Message = "pending EWRA fetched succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<EWRAModelDTO>> GetEwraQuantitativePendingData(int Id)
        {
            ServiceResponse<List<EWRAModelDTO>> serviceResponse = new ServiceResponse<List<EWRAModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_Id", Id);
                parameters.Add("p_Category", 1);
                parameters.Add("p_type", 1);
                parameters.Add("p_qid", 0);
                serviceResponse.Result = Get<EWRAModelDTO>("get_ewra_consolidated_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "ewra customer profile data fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                EWRAModelDTO _model = new EWRAModelDTO();
                ServiceResponse<List<EWRACustomerVolumeDTO>> serviceResponse1 = new ServiceResponse<List<EWRACustomerVolumeDTO>>();
                try
                {
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 1);
                    parameters.Add("p_Type", 1);
                    serviceResponse1.Result = Get<EWRACustomerVolumeDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Result[0].EWRACustomerVolume = serviceResponse1.Result;
                    _model.EWRACustomerVolume = serviceResponse1.Result;
                    serviceResponse1.Message = "ewra customer profile data fetched successfully.";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    serviceResponse1.Message = ex.Message;
                    serviceResponse1.Status = StaticResource.FailStatusCode;
                }
                ServiceResponse<List<EWRACustomerCountDTO>> serviceResponse2 = new ServiceResponse<List<EWRACustomerCountDTO>>();
                try
                {
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 2);
                    parameters.Add("p_Type", 1);
                    serviceResponse2.Result = Get<EWRACustomerCountDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Result[0].EWRACustomerCount = serviceResponse2.Result;
                    serviceResponse2.Message = "ewra customer profile data fetched successfully.";
                    serviceResponse2.Status = StaticResource.SuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    serviceResponse2.Message = ex.Message;
                    serviceResponse2.Status = StaticResource.FailStatusCode;
                }
                ServiceResponse<List<EWRACustomerTransactionDTO>> serviceResponse3 = new ServiceResponse<List<EWRACustomerTransactionDTO>>();
                try
                {
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 3);
                    parameters.Add("p_Type", 1);
                    serviceResponse3.Result = Get<EWRACustomerTransactionDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Result[0].EWRACustomerTransaction = serviceResponse3.Result;
                    serviceResponse3.Message = "ewra customer profile data fetched successfully.";
                    serviceResponse3.Status = StaticResource.SuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    serviceResponse3.Message = ex.Message;
                    serviceResponse3.Status = StaticResource.FailStatusCode;
                }
                List<EWRACounterPartyTypeDTO> t = new List<EWRACounterPartyTypeDTO>();
                EWRACounterPartyTypeDTO td = new EWRACounterPartyTypeDTO();
                td.TypeId = 1;
                t.Add(td);
                serviceResponse.Result[0].EWRACounterPartyType = t;
                for (var i = 0; i < serviceResponse.Result.Count; i++)
                {
                    ServiceResponse<List<EWRACounterPartyListDTO>> serviceResponse4 = new ServiceResponse<List<EWRACounterPartyListDTO>>();
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 0);
                    parameters.Add("p_Type", 2);
                    serviceResponse4.Result = Get<EWRACounterPartyListDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse4.Message = "ewra consolidated data fetched successfully.";
                    serviceResponse4.Status = StaticResource.SuccessStatusCode;
                    serviceResponse.Result[0].EWRACounterPartyType[0].EWRACounterPartyList = serviceResponse4.Result;
                }
                ServiceResponse<List<EWRAProductListDTO>> serviceResponse5 = new ServiceResponse<List<EWRAProductListDTO>>();
                parameters.Add("p_Id", Id);
                parameters.Add("p_Category", 0);
                parameters.Add("p_Type", 3);
                serviceResponse5.Result = Get<EWRAProductListDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse5.Message = "ewra consolidated data fetched successfully.";
                serviceResponse5.Status = StaticResource.SuccessStatusCode;
                _model.EWRAProductList = serviceResponse5.Result;
                serviceResponse.Result[0].EWRAProductList = serviceResponse5.Result;

                //serviceResponse.Result[0].EWRAProductCategory[0].EWRAProductList = serviceResponse5.Result;
                ServiceResponse<List<EWRAJurisdictionDTO>> serviceResponse6 = new ServiceResponse<List<EWRAJurisdictionDTO>>();
                try
                {
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 1);
                    parameters.Add("p_Type", 4);
                    serviceResponse6.Result = Get<EWRAJurisdictionDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Result[0].EWRAJurisdiction = serviceResponse6.Result;
                    _model.EWRAJurisdiction = serviceResponse6.Result;
                    serviceResponse6.Message = "ewra jurisdiction data fetched successfully.";
                    serviceResponse6.Status = StaticResource.SuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    serviceResponse1.Message = ex.Message;
                    serviceResponse1.Status = StaticResource.FailStatusCode;
                }
                ServiceResponse<List<EWRADeliveryTypeDTO>> serviceResponse7 = new ServiceResponse<List<EWRADeliveryTypeDTO>>();
                try
                {
                    parameters.Add("p_Id", Id);
                    parameters.Add("p_Category", 1);
                    parameters.Add("p_Type", 5);
                    serviceResponse7.Result = Get<EWRADeliveryTypeDTO>("get_ewra_detailed_data", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Result[0].EWRADeliveryType = serviceResponse7.Result;
                    _model.EWRADeliveryType = serviceResponse7.Result;
                    serviceResponse7.Message = "ewra delivery data fetched successfully.";
                    serviceResponse7.Status = StaticResource.SuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    serviceResponse1.Message = ex.Message;
                    serviceResponse1.Status = StaticResource.FailStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<string> GetEWRAQualitativeId(int qId)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_qId", qId);
                var response = "";
                var x = ExecuteScalar("get_pending_ewra_Qid", parameters, commandType: CommandType.StoredProcedure);
                if (x != null)
                {
                    response = x.ToString();
                }
                else
                {
                    response = "";
                }
                if (response == null)
                {
                    response = string.Empty;
                }
                else
                {
                    response = response.ToString();
                }
                serviceResponse.Result = response.ToString();
                serviceResponse.Message = "pending EWRA qualitative id successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
    }
}
