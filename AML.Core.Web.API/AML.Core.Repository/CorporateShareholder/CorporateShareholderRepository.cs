using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CorporateShareholder;
using AML.DTO.DTO.CorporateShareholder;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace AML.Core.Repository.CorporateShareholder
{
    public class CorporateShareholderRepository: BaseRepository, ICorporateShareholderRepository
    {
        public CorporateShareholderRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }

        public ServiceResponse<CorporateShareholderDTO> GetDetails(int Id)
        {
            ServiceResponse<CorporateShareholderDTO> serviceResponse = new ServiceResponse<CorporateShareholderDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_shareholder_id", Id);
                serviceResponse.Result = GetFirstOrDefault<CorporateShareholderDTO>("get_shareholer_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Shareholder details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(CorporateShareholderDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_corporate_id", model.CorporateID);
                parameters.Add("@c_share_holder_id", model.ShareholderID);
                parameters.Add("@c_share_percentage", model.SharePercentage);
                parameters.Add("@c_cust_id_gen", model.CustIDGen);
                parameters.Add("@c_status", model.Status);
                parameters.Add("@c_created_by", model.CreatedBy);
                parameters.Add("@c_created_on", model.CreatedOn);
                var response = ExecuteScalar("ins_corporate_shareholder", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Corporate Shareholder added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> CreateShareHolder(CorporateShareholderDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_corporate_id", model.CorporateID);
                parameters.Add("@c_share_holder_id", model.ShareholderID);
                parameters.Add("@c_share_percentage", model.SharePercentage);
                parameters.Add("@c_cust_ref_id", model.CustomerRefId);
                parameters.Add("@c_status", model.Status);
                parameters.Add("@c_created_by", model.CreatedBy);
                var response = ExecuteScalar("ins_corporate_shareholder_new", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Corporate Shareholder added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> DeleteByCorporateID(string corporateID)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_corporate_id", corporateID);
                var response = ExecuteScalar("del_shareholer_by_corporate_id", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Shareholder details deleted successfully.";
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
