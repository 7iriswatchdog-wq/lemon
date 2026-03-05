using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.LegalType;
using AML.DTO.DTO.LegalType;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AML.Core.Repository.LegalType
{
    public class LegalTypeRepository : BaseRepository, ILegalTypeRepository
    {
        public LegalTypeRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<LegalTypeDTO>> GetAll()
        {
            ServiceResponse<List<LegalTypeDTO>> serviceResponse = new ServiceResponse<List<LegalTypeDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<LegalTypeDTO>("get_all_legaltype", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Identity Type details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<LegalTypeDTO> GetDetails(int Id)
        {
            ServiceResponse<LegalTypeDTO> serviceResponse = new ServiceResponse<LegalTypeDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<LegalTypeDTO>("get_legaltype_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Identity Type details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(LegalTypeDTO _LegalTypeDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_code", _LegalTypeDTO.Code);
                parameters.Add("@p_name", _LegalTypeDTO.Name);
                parameters.Add("@p_description", _LegalTypeDTO.Description);
                parameters.Add("@p_created_by", _LegalTypeDTO.CreatedBy);
                var response = ExecuteScalar("ins_legaltype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Legal Type added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(LegalTypeDTO _LegalTypeDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _LegalTypeDTO.Id);
                parameters.Add("@p_code", _LegalTypeDTO.Code);
                parameters.Add("@p_name", _LegalTypeDTO.Name);
                parameters.Add("@p_description", _LegalTypeDTO.Description);
                parameters.Add("@p_updated_by", _LegalTypeDTO.UpdatedBy);
                parameters.Add("@p_is_deleted", _LegalTypeDTO.IsDeleted);
                var response = ExecuteScalar("mod_legaltype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Legal Type updated successfully.";
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
                var response = ExecuteScalar("del_legaltype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Legal Type deleted successfully.";
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
