using System;
using Microsoft.Extensions.Configuration;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Designation;
using Dapper;
using Microsoft.AspNetCore.Http;
using AML.DTO.DTO.Designation;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace AML.Core.Repository.Designation
{
    public class DesignationRepository : BaseRepository, IDesignationRepository
    {
        public DesignationRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context){ }

        public ServiceResponse<List<DesignationDTO>> GetAll(int clientId)
        {
            ServiceResponse<List<DesignationDTO>> serviceResponse = new ServiceResponse<List<DesignationDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<DesignationDTO>("get_all_designation", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Designation details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<DesignationDTO> GetDetails(int Id)
        {
            ServiceResponse<DesignationDTO> serviceResponse = new ServiceResponse<DesignationDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<DesignationDTO>("get_designation_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Designation details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(DesignationDTO _designationDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_code", _designationDTO.Code);
                parameters.Add("@p_name", _designationDTO.Name);
                parameters.Add("@p_description", _designationDTO.Description);
                parameters.Add("@p_created_by", _designationDTO.CreatedBy);
                parameters.Add("@p_clientId", _designationDTO.ClientId);
                var response = ExecuteScalar("ins_designation", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Designation added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(DesignationDTO _designationDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _designationDTO.Id);
                parameters.Add("@p_code", _designationDTO.Code);
                parameters.Add("@p_name", _designationDTO.Name);
                parameters.Add("@p_description", _designationDTO.Description);
                parameters.Add("@p_updated_by", _designationDTO.UpdatedBy);
                parameters.Add("@p_is_active", _designationDTO.IsActive);
                var response = ExecuteScalar("mod_designation", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Designation updated successfully.";
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
                var response = ExecuteScalar("del_designation", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Designation deleted successfully.";
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
