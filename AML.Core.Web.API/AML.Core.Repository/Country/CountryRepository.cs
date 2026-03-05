using System;
using Microsoft.Extensions.Configuration;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Country;
using Dapper;
using Microsoft.AspNetCore.Http;
using AML.DTO.DTO.Country;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace AML.Core.Repository.Country
{
    public class CountryRepository : BaseRepository, ICountryRepository
    {
        public CountryRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }

        public ServiceResponse<List<CountryDTO>> GetAll(int ClientId)
        {
            ServiceResponse<List<CountryDTO>> serviceResponse = new ServiceResponse<List<CountryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", ClientId);
                serviceResponse.Result = Get<CountryDTO>("get_all_country", parameters, commandType: CommandType.StoredProcedure).ToList();
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
        public ServiceResponse<List<CountryDTO>> GetListAll()
        {
            ServiceResponse<List<CountryDTO>> serviceResponse = new ServiceResponse<List<CountryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<CountryDTO>("get_all_countrylist", parameters, commandType: CommandType.StoredProcedure).ToList();
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
        public ServiceResponse<List<CountryDTO>> GetListFATFAll()
        {
            ServiceResponse<List<CountryDTO>> serviceResponse = new ServiceResponse<List<CountryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<CountryDTO>("get_all_country_FATF", parameters, commandType: CommandType.StoredProcedure).ToList();
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
        public ServiceResponse<CountryDTO> GetDetails(int Id)
        {
            ServiceResponse<CountryDTO> serviceResponse = new ServiceResponse<CountryDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<CountryDTO>("get_country_by_id", parameters, commandType: CommandType.StoredProcedure);
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

        public ServiceResponse<int> Create(CountryDTO _countryDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Code", _countryDTO.Code);
                parameters.Add("@p_Name", _countryDTO.Name);
                parameters.Add("@p_Description", _countryDTO.Description);
                parameters.Add("@p_RiskRating", _countryDTO.RiskRating);
                parameters.Add("@p_FATFRISKRating", _countryDTO.FATFRiskRating);
                parameters.Add("@p_isocode3digit", _countryDTO.ISOCode3digit);
                parameters.Add("@p_Riskscore", _countryDTO.Riskscore);
                parameters.Add("@p_UNCode", _countryDTO.UNCode);
                parameters.Add("@p_Is_active", _countryDTO.IsActive ? 1 : 0);
                parameters.Add("@p_created_by", 1); // Update by user id
                parameters.Add("@p_clientId", _countryDTO.ClientId);
                var response = ExecuteScalar("ins_country", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Country added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(CountryDTO _countryDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_Id", _countryDTO.Id);
                parameters.Add("@p_Code", _countryDTO.Code);
                parameters.Add("@p_Name", _countryDTO.Name);
                parameters.Add("@p_Description", _countryDTO.Description);
                parameters.Add("@p_RiskRating", _countryDTO.RiskRating);
                parameters.Add("@p_FATFRISKRating", _countryDTO.FATFRiskRating);
                parameters.Add("@p_isocode3digit", _countryDTO.ISOCode3digit);
                parameters.Add("@p_Riskscore", _countryDTO.Riskscore);
                parameters.Add("@p_UNCode", _countryDTO.UNCode);
                parameters.Add("@p_Is_active", _countryDTO.IsActive ? 1 : 0);
                parameters.Add("@p_updated_by", 1);//Change based on the login user;
                // parameters.Add("@p_updated_by", _countryDTO.UpdatedBy);
                //  parameters.Add("@p_is_active", _countryDTO.IsActive);
                var response = ExecuteScalar("mod_country", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Country updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> Delete(int id)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                var response = ExecuteScalar("del_country", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Counry deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CountryDTO>> GetAllPartnerNationality(int ClientId)
        {
            ServiceResponse<List<CountryDTO>> serviceResponse = new ServiceResponse<List<CountryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_clientId", ClientId);
                serviceResponse.Result = Get<CountryDTO>("get_all_partner_nationality", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Country details fetched successfully.";
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
        public ServiceResponse<List<CountryDTO>> GetAllCountryRiskConfig(string culture)
        {
            ServiceResponse<List<CountryDTO>> serviceResponse = new ServiceResponse<List<CountryDTO>>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
            
                serviceResponse.Result = Get<CountryDTO>("get_all_country_risk_config", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Country details fetched successfully.";
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
