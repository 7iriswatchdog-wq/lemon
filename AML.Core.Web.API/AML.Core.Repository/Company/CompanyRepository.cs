using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Company;
using AML.DTO.DTO.Company;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AML.Core.Repository.Company
{
    public class CompanyRepository : BaseRepository, ICompanyRepository
    {
        public CompanyRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<CompanyDTO>> GetAll()
        {
            ServiceResponse<List<CompanyDTO>> serviceResponse = new ServiceResponse<List<CompanyDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<CompanyDTO>("get_all_company", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Company details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<CompanyDTO> GetDetails(int Id)
        {
            ServiceResponse<CompanyDTO> serviceResponse = new ServiceResponse<CompanyDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<CompanyDTO>("get_company_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Company details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(CompanyDTO _CompanyDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_legalname", _CompanyDTO.LegalName);
                parameters.Add("@p_legaltype", _CompanyDTO.LegalType);
                parameters.Add("@p_licencenumber", _CompanyDTO.LicenseNumber);
                parameters.Add("@p_created_by", _CompanyDTO.CreatedBy);
                parameters.Add("@p_mobile", _CompanyDTO.Mobile);
                var response = ExecuteScalar("ins_company", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Company added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(CompanyDTO _CompanyDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _CompanyDTO.Id);
                parameters.Add("@p_legalname", _CompanyDTO.LegalName);
                parameters.Add("@p_legaltype", _CompanyDTO.LegalType);
                parameters.Add("@p_licencenumber", _CompanyDTO.LicenseNumber);
                parameters.Add("@p_created_by", _CompanyDTO.CreatedBy);
                parameters.Add("@p_mobile", _CompanyDTO.Mobile);
                var response = ExecuteScalar("mod_company", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Company updated successfully.";
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
                var response = ExecuteScalar("del_company", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Company deleted successfully.";
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
