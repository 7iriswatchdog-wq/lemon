using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerCase;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.CaseDocument;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.CustomerCase
{
    public class CaseDocumentRepository : BaseRepository, ICaseDocumentRepository
    {
        public CaseDocumentRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<DocumentCategoryDTO>> GetAllCategories()
        {
            ServiceResponse<List<DocumentCategoryDTO>> serviceResponse = new ServiceResponse<List<DocumentCategoryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<DocumentCategoryDTO>("get_all_documentcategory", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Case Document category fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<DocumentTypeDTO>> GetAllTypes()
        {
            ServiceResponse<List<DocumentTypeDTO>> serviceResponse = new ServiceResponse<List<DocumentTypeDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<DocumentTypeDTO>("get_all_documenttype", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Case Document types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CaseDocumentDTO>> GetAll()
        {
            ServiceResponse<List<CaseDocumentDTO>> serviceResponse = new ServiceResponse<List<CaseDocumentDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<CaseDocumentDTO>("get_all_CaseDocument", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Case Document details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<CaseDocumentDTO> GetDetails(int Id)
        {
            ServiceResponse<CaseDocumentDTO> serviceResponse = new ServiceResponse<CaseDocumentDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<CaseDocumentDTO>("get_CaseDocument_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Case Document details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CaseDocumentDTO>> GetDetailsByCaseId(int caseId)
        {
            ServiceResponse<List<CaseDocumentDTO>> serviceResponse = new ServiceResponse<List<CaseDocumentDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_case_id", caseId);
                serviceResponse.Result = Get<CaseDocumentDTO>("get_casedocuments_by_case_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Case Document details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(CaseDocumentDTO _CaseDocumentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {


                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_case_id", _CaseDocumentDTO.CaseId);
                parameters.Add("@p_document_name", _CaseDocumentDTO.DocumentName);
                parameters.Add("@p_document_category", _CaseDocumentDTO.DocumentCategory);
                parameters.Add("@p_document_type", _CaseDocumentDTO.DocumentType);
                parameters.Add("@p_document_file_name", _CaseDocumentDTO.DocumentFileName);
                parameters.Add("@p_document_full_path", _CaseDocumentDTO.DocumentFullPath);
                parameters.Add("@p_documents_details", _CaseDocumentDTO.DocumentDetails);
                parameters.Add("@p_remarks", _CaseDocumentDTO.Remarks);
                parameters.Add("@p_issued_date", _CaseDocumentDTO.IssuedDateOnDB);
                parameters.Add("@p_expiry_date", _CaseDocumentDTO.ExpiryDateOnDB);
                parameters.Add("@p_created_on", _CaseDocumentDTO.CreatedOn);
                parameters.Add("@p_created_by", _CaseDocumentDTO.CreatedBy);
                parameters.Add("@p_customerid", _CaseDocumentDTO.CustomerId);
                var response = ExecuteScalar("ins_case_document", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Document added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(CaseDocumentDTO _CaseDocumentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _CaseDocumentDTO.Id);
                parameters.Add("@p_document_name", _CaseDocumentDTO.DocumentName);
                parameters.Add("@p_document_category", _CaseDocumentDTO.DocumentCategory);
                parameters.Add("@p_document_type", _CaseDocumentDTO.DocumentType);
                parameters.Add("@p_document_file_name", _CaseDocumentDTO.DocumentFileName);
                parameters.Add("@p_document_full_path", _CaseDocumentDTO.DocumentFullPath);
                parameters.Add("@p_documents_details", _CaseDocumentDTO.DocumentDetails);
                parameters.Add("@p_remarks", _CaseDocumentDTO.Remarks);
                parameters.Add("@p_issued_date", _CaseDocumentDTO.IssuedDate);
                parameters.Add("@p_expiry_date", _CaseDocumentDTO.ExpiryDate);
                parameters.Add("@p_created_on", _CaseDocumentDTO.CreatedOn);
                parameters.Add("@p_created_by", _CaseDocumentDTO.CreatedBy);
                var response = ExecuteScalar("mod_CaseDocument", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Document updated successfully.";
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
                var response = ExecuteScalar("del_CaseDocument", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Document deleted successfully.";
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
