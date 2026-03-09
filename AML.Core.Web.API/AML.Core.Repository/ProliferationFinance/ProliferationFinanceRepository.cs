using AML.Core.RepositoryContract.ProliferationFinance;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.ProliferationFinance;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.ProliferationFinance
{
    public class ProliferationFinanceRepository : BaseRepository, IProliferationFinanceRepository
    {
        public ProliferationFinanceRepository(Microsoft.Extensions.Configuration.IConfiguration configuration, Microsoft.AspNetCore.Http.IHttpContextAccessor context) : base(configuration, context)
        {
        }

        public ServiceResponse<List<UAEControlListDTO>> SearchChemicals(ProliferationFinanceCaseDTO searchCriteria)
        {
            ServiceResponse<List<UAEControlListDTO>> serviceResponse = new ServiceResponse<List<UAEControlListDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_hs_code", searchCriteria.HsCode);
                parameters.Add("@p_cas_number", searchCriteria.CasNumber);
                parameters.Add("@p_eccn", searchCriteria.Eccn);
                parameters.Add("@p_chemical_name", searchCriteria.ChemicalName);
                parameters.Add("@p_synonym_name", searchCriteria.SynonymName);

                // Using a slightly more explicit approach to ensure property mapping works with Dapper
                var results = Get<UAEControlListDTO>("sp_SearchUAEControlList", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Result = results;
                serviceResponse.Status = 200;
                serviceResponse.Message = "Success";
            }
            catch (Exception ex)
            {
                serviceResponse.Status = 500;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> CreateCase(ProliferationFinanceCaseDTO caseDto)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_CustomerType", caseDto.CustomerType);
                parameters.Add("@p_CorporateId", caseDto.CorporateId);
                parameters.Add("@p_CompanyName", caseDto.CompanyName);
                parameters.Add("@p_HsCode", caseDto.HsCode);
                parameters.Add("@p_CasNumber", caseDto.CasNumber);
                parameters.Add("@p_Eccn", caseDto.Eccn);
                parameters.Add("@p_ChemicalName", caseDto.ChemicalName);
                parameters.Add("@p_SynonymName", caseDto.SynonymName);
                parameters.Add("@p_MatchedChemicalName", caseDto.MatchedChemicalName);
                parameters.Add("@p_Score", caseDto.Score);
                parameters.Add("@p_StatusReason", caseDto.StatusReason);
                parameters.Add("@p_Type", caseDto.Type ?? "Corporate");
                parameters.Add("@p_CreatedBy", caseDto.CreatedBy);
                parameters.Add("@p_SearchHitDetails", caseDto.SearchHitDetails);
                parameters.Add("@p_CaseId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var result = GetFirstOrDefault<int>("sp_CreateProliferationFinanceCase", parameters, commandType: CommandType.StoredProcedure);
                
                serviceResponse.Result = result > 0 ? result : parameters.Get<int>("@p_CaseId");
                serviceResponse.Status = 200;
                serviceResponse.Message = "Success";
            }
            catch (Exception ex)
            {
                serviceResponse.Status = 500;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<ProliferationFinanceCaseDTO>> GetAllCases()
        {
            ServiceResponse<List<ProliferationFinanceCaseDTO>> serviceResponse = new ServiceResponse<List<ProliferationFinanceCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<ProliferationFinanceCaseDTO>("sp_GetAllProliferationFinanceCases", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Status = 200;
                serviceResponse.Message = "Success";
            }
            catch (Exception ex)
            {
                serviceResponse.Status = 500;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public ProliferationFinanceCaseDTO GetCaseById(int id)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                return GetFirstOrDefault<ProliferationFinanceCaseDTO>("sp_GetProliferationFinanceCaseById", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ServiceResponse<bool> UpdateCaseStatus(int caseId, string status)
        {
            ServiceResponse<bool> serviceResponse = new ServiceResponse<bool>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_CaseId", caseId);
                parameters.Add("@p_Status", status);
                
                var result = Execute("sp_UpdateProliferationFinanceCaseStatus", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result = result > 0;
                serviceResponse.Status = 200;
                serviceResponse.Message = "Success";
            }
            catch (Exception ex)
            {
                serviceResponse.Status = 500;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public ServiceResponse<bool> UpdateCaseRemarks(int caseId, string remarks)
        {
            ServiceResponse<bool> serviceResponse = new ServiceResponse<bool>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_CaseId", caseId);
                parameters.Add("@p_Remarks", remarks);

                var result = Execute("sp_UpdateProliferationFinanceCaseRemarks", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result = result > 0;
                serviceResponse.Status = 200;
                serviceResponse.Message = "Success";
            }
            catch (Exception ex)
            {
                serviceResponse.Status = 500;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public ServiceResponse<bool> UpdateSearchHits(int caseId, string hitDetails)
        {
            ServiceResponse<bool> serviceResponse = new ServiceResponse<bool>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_CaseId", caseId);
                parameters.Add("@p_HitDetails", hitDetails);

                var result = Execute("sp_UpdateProliferationFinanceSearchHits", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result = result > 0;
                serviceResponse.Status = 200;
                serviceResponse.Message = "Success";
            }
            catch (Exception ex)
            {
                serviceResponse.Status = 500;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
    }
}
