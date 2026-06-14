using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract;
using AML.Core.RepositoryContract.CodeMaster;
using AML.DTO.DTO.Branch;
using AML.DTO.DTO.CodesMaster;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AML.Core.Repository.CodesMaster
{
    public class CodesMasterRepository : BaseRepository, ICodesMasterRepository
    {
        public CodesMasterRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        {

        }

        public ServiceResponse<List<CodesTableDTO>> GetAllCodes(int CcClientId)
        {
            ServiceResponse<List<CodesTableDTO>> serviceResponse = new ServiceResponse<List<CodesTableDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", CcClientId);
                serviceResponse.Result = Get<CodesTableDTO>("get_all_codes", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "codes details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<CodesTableDTO> GetDetails(int Id, string Code, string Type)
        {
            ServiceResponse<CodesTableDTO> serviceResponse = new ServiceResponse<CodesTableDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@clientId", Id);
                parameters.Add("@CodCode", Code);
                parameters.Add("@CodType", Type);
                serviceResponse.Result = GetFirstOrDefault<CodesTableDTO>("get_code_for_edit", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Codes are fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(CodesTableDTO _CodesTableDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CodCode", _CodesTableDTO.ccCode);
                parameters.Add("@CodName", _CodesTableDTO.ccName);
                parameters.Add("@CodDetails", _CodesTableDTO.ccDetails);
                parameters.Add("@CodType", _CodesTableDTO.ccType);
                parameters.Add("@CodClientId", _CodesTableDTO.ccClientId);
                parameters.Add("@CodFlexi1", _CodesTableDTO.ccFlexi1);
                parameters.Add("@CodFlexi2", _CodesTableDTO.ccFlexi2);
                if (_CodesTableDTO.ccActiveYn == true)
                {
                    parameters.Add("@CodActive", 1);
                }
                else
                {
                    parameters.Add("@CodActive", 0);
                }

                var response = ExecuteScalar("ins_codes", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                if (serviceResponse.Result == 1)
                {
                    serviceResponse.Message = "Codes added successfully.";
                    serviceResponse.Status = StaticResource.SuccessStatusCode;
                }
                    
                else
                    serviceResponse.Message = "Codes are not added";
                
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(CodesTableDTO _CodesTableDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CodCode", _CodesTableDTO.ccCode);
                parameters.Add("@CodName", _CodesTableDTO.ccName);
                parameters.Add("@CodDetails", _CodesTableDTO.ccDetails);
                parameters.Add("@CodType", _CodesTableDTO.ccType);
                parameters.Add("@CodFlexi1", _CodesTableDTO.ccFlexi1);
                parameters.Add("@CodFlexi2", _CodesTableDTO.ccFlexi2);
                parameters.Add("@CodFlexi2", _CodesTableDTO.ccFlexi2);
                if(_CodesTableDTO.ccActiveYn==true)
                {
                    parameters.Add("@CodActive", 1);
                }
                else
                {
                    parameters.Add("@CodActive", 0);
                }
                var response = ExecuteScalar("mod_codes", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Codes updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Delete(int id,string code, string type)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CodCode", code);
                parameters.Add("@clientId", id);
                parameters.Add("@CodType", type);
                var response = ExecuteScalar("del_codes", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Codes deleted successfully.";
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
