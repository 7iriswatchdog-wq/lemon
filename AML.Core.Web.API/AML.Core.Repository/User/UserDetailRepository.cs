using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.User;
using AML.DTO.DTO.User;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AML.Core.Repository.User
{
    public class UserDetailRepository : BaseRepository, IUserDetailRepository
    {
        public UserDetailRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        {
        }
        public ServiceResponse<int> Create(UserDetailDTO _userDetailDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_id", _userDetailDTO.UserId);
                parameters.Add("@p_date_of_join", _userDetailDTO.DateOfJoin);
                parameters.Add("@p_city", _userDetailDTO.City);
                parameters.Add("@p_phone", _userDetailDTO.Phone);
                parameters.Add("@p_fax", _userDetailDTO.Fax);
                parameters.Add("@p_email", _userDetailDTO.Email);
                parameters.Add("@p_address_1", _userDetailDTO.Address1);
                parameters.Add("@p_address_2", _userDetailDTO.Address2);
                parameters.Add("@p_identity_type", _userDetailDTO.IdentityType);
                parameters.Add("@p_approval_limit", _userDetailDTO.ApprovalLimit);
                parameters.Add("@p_identification_number", _userDetailDTO.IdentificationNumber);
                parameters.Add("@p_id_num_date_of_issue", _userDetailDTO.IdNumDateOfIssue);
                parameters.Add("@p_id_num_expry_date", _userDetailDTO.IdNumExpryDate);
                parameters.Add("@p_visa_type", _userDetailDTO.VisaType);
                parameters.Add("@p_current_add", _userDetailDTO.CurrentAdd);
                parameters.Add("@p_visa_number", _userDetailDTO.VisaNumber);
                parameters.Add("@p_visa_num_date_of_issue", _userDetailDTO.VisaNumDateOfIssue);
                parameters.Add("@p_visa_num_expry_date", _userDetailDTO.VisaNumExpryDate);
                parameters.Add("@p_cpr_number", _userDetailDTO.CprNumber);
                parameters.Add("@p_cpr_num_date_of_issue", _userDetailDTO.CprNumDateOfIssue);
                parameters.Add("@p_cpr_num_expry_date", _userDetailDTO.CprNumExpryDate);
                parameters.Add("@p_created_by", _userDetailDTO.CreatedBy);
                var response = ExecuteScalar("ins_userdetail", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<UserDetailDTO> GetDetails(int Id)
        {
            ServiceResponse<UserDetailDTO> serviceResponse = new ServiceResponse<UserDetailDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<UserDetailDTO>("get_userdetail_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "User details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> Update(UserDetailDTO _userDetailDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _userDetailDTO.Id);
                parameters.Add("@p_date_of_join", _userDetailDTO.DateOfJoin);
                parameters.Add("@p_city", _userDetailDTO.City);
                parameters.Add("@p_phone", _userDetailDTO.Phone);
                parameters.Add("@p_fax", _userDetailDTO.Fax);
                parameters.Add("@p_email", _userDetailDTO.Email);
                parameters.Add("@p_address_1", _userDetailDTO.Address1);
                parameters.Add("@p_address_2", _userDetailDTO.Address2);
                parameters.Add("@p_identity_type", _userDetailDTO.IdentityType);
                parameters.Add("@p_approval_limit", _userDetailDTO.ApprovalLimit);
                parameters.Add("@p_identification_number", _userDetailDTO.IdentificationNumber);
                parameters.Add("@p_id_num_date_of_issue", _userDetailDTO.IdNumDateOfIssue);
                parameters.Add("@p_id_num_expry_date", _userDetailDTO.IdNumExpryDate);
                parameters.Add("@p_visa_type", _userDetailDTO.VisaType);
                parameters.Add("@p_current_add", _userDetailDTO.CurrentAdd);
                parameters.Add("@p_visa_number", _userDetailDTO.VisaNumber);
                parameters.Add("@p_visa_num_date_of_issue", _userDetailDTO.VisaNumDateOfIssue);
                parameters.Add("@p_visa_num_expry_date", _userDetailDTO.VisaNumExpryDate);
                parameters.Add("@p_cpr_number", _userDetailDTO.CprNumber);
                parameters.Add("@p_cpr_num_date_of_issue", _userDetailDTO.CprNumDateOfIssue);
                parameters.Add("@p_cpr_num_expry_date", _userDetailDTO.CprNumExpryDate);
                parameters.Add("@p_updated_by", _userDetailDTO.UpdatedBy);
                var response = ExecuteScalar("mod_userdetail", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User Details updated successfully.";
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
