using System;
using Microsoft.Extensions.Configuration;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Department;
using Dapper;
using Microsoft.AspNetCore.Http;
using AML.DTO.DTO.Department;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace AML.Core.Repository.Department
{
    public class DepartmentRepository : BaseRepository, IDepartmentRepository
    {
        public DepartmentRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<DepartmentDTO>> GetAll(int clientId)
        {
            ServiceResponse<List<DepartmentDTO>> serviceResponse = new ServiceResponse<List<DepartmentDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<DepartmentDTO>("get_all_department", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Department details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<DepartmentDTO> GetDetails(int Id)
        {
            ServiceResponse<DepartmentDTO> serviceResponse = new ServiceResponse<DepartmentDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<DepartmentDTO>("get_department_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Department details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(DepartmentDTO _departmentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_code", _departmentDTO.Code);
                parameters.Add("@p_name", _departmentDTO.Name);
                parameters.Add("@p_description", _departmentDTO.Description);
                parameters.Add("@p_created_by", _departmentDTO.CreatedBy);
                parameters.Add("@p_clientId", _departmentDTO.ClientId);
                var response = ExecuteScalar("ins_department", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Department added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch(Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(DepartmentDTO _departmentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _departmentDTO.Id);
                parameters.Add("@p_code", _departmentDTO.Code);
                parameters.Add("@p_name", _departmentDTO.Name);
                parameters.Add("@p_description", _departmentDTO.Description);
                parameters.Add("@p_updated_by", _departmentDTO.UpdatedBy);
                parameters.Add("@p_is_active", _departmentDTO.IsActive);
                var response = ExecuteScalar("mod_department", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Department updated successfully.";
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
                var response = ExecuteScalar("del_department", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Department deleted successfully.";
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
