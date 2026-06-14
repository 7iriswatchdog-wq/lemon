using System;
using Microsoft.Extensions.Configuration;
using AML.Core.Common.StaticResource;
using Dapper;
using Microsoft.AspNetCore.Http;
using AML.DTO.DTO.VisaType;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using AML.Core.RepositoryContract.VisaType;

namespace AML.Core.Repository.VisaType
{
    public class VisaTypeRepository : BaseRepository, IVisaTypeRepository
    {
        public VisaTypeRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<VisaTypeDTO>> GetAll(int clientId)
        {
            ServiceResponse<List<VisaTypeDTO>> serviceResponse = new ServiceResponse<List<VisaTypeDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<VisaTypeDTO>("get_all_visatype", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Visa Type details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<VisaTypeDTO> GetDetails(int Id)
        {
            ServiceResponse<VisaTypeDTO> serviceResponse = new ServiceResponse<VisaTypeDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<VisaTypeDTO>("get_visatype_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Visa Type details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(VisaTypeDTO _visaTypeDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_code", _visaTypeDTO.Code);
                parameters.Add("@p_name", _visaTypeDTO.Name);
                parameters.Add("@p_description", _visaTypeDTO.Description);
                parameters.Add("@p_created_by", _visaTypeDTO.CreatedBy);
                parameters.Add("@p_is_active", _visaTypeDTO.IsActive);
                parameters.Add("@p_clientId", _visaTypeDTO.ClientId);
                var response = ExecuteScalar("ins_visatype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Visa Type added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(VisaTypeDTO _visaTypeDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _visaTypeDTO.Id);
                parameters.Add("@p_code", _visaTypeDTO.Code);
                parameters.Add("@p_name", _visaTypeDTO.Name);
                parameters.Add("@p_description", _visaTypeDTO.Description);
                parameters.Add("@p_updated_by", _visaTypeDTO.UpdatedBy);
                parameters.Add("@p_clientId", _visaTypeDTO.ClientId);
                var response = ExecuteScalar("mod_visatype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Visa Type updated successfully.";
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
                var response = ExecuteScalar("del_visatype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Visa Type deleted successfully.";
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
