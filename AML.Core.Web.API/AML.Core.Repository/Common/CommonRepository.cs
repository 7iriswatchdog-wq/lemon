using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Common;
using AML.DTO.DTO.Common;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AML.Core.Repository.Common
{
    public class CommonRepository : BaseRepository, ICommonRepository
    {
        //public IConfiguration Configuration;
        //private string _ohsbaseurl = string.Empty;
        public CommonRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        {
            
        }
        public string EncryptionString(string strValue)
        {
            return Core.Common.CommonClasses.EncryptionHelper.Encrypt(strValue);

        }
        /// <summary>
        /// decrypt data
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public string DecryptionString(string strValue)
        {
            return Core.Common.CommonClasses.EncryptionHelper.Decrypt(strValue);

        }

        public List<ApplicationSetupDTO> GetConfigurationDetailsByGroup(string setupGroup,int ClientId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_setup_group", setupGroup);
                parameters.Add("@p_clientId", ClientId);
                List<ApplicationSetupDTO> listValues = Get<ApplicationSetupDTO>("get_application_setup_by_group", parameters, commandType: CommandType.StoredProcedure).ToList(); ;

                return listValues;
            }
            catch(Exception ex)
            {
                return new List<ApplicationSetupDTO>();
            }
        }
        public ServiceResponse<string> createErrorlog(ErrorLogDTO errorlog)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_module", errorlog.module);
                parameters.Add("@p_description", errorlog.description);
                parameters.Add("@p_created_by", errorlog.createdBy);
                parameters.Add("@p_created_on", errorlog.created_on);
                parameters.Add("@p_comments", errorlog.comments);
                parameters.Add("@p_status_code", errorlog.status_code);
                var response = ExecuteScalar("ins_error_log", parameters, commandType: CommandType.StoredProcedure).ParseString();
                serviceResponse.Result = response;
                serviceResponse.Message = "Error log table inserted successfully";
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
