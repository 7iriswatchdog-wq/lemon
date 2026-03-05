using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.TransactionScreening;
using AML.DTO.DTO.TransactionScreening;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.TransactionScreening
{
    public class TransactionScreeningRepository : BaseRepository, ITransactionScreeningRepository
    {
        public TransactionScreeningRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { } 
        public ServiceResponse<int> InsertTransactionScreening(TranScreenDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_tranrefno", model.TranRefNo);
                parameters.Add("@p_custrefno", model.CustRefNo);
                parameters.Add("@p_custtype", model.CustType);
                parameters.Add("@p_name", model.Name);
                parameters.Add("@p_country", model.Country);
                parameters.Add("@p_dob", model.DOB);
                parameters.Add("@p_iswhiteListed", model.IsWhiteListed);
                parameters.Add("@p_createdby", model.CreatedBy);
                parameters.Add("@p_threshold", model.Threshold);
                parameters.Add("@p_clientId", model.ClientId);
                serviceResponse.Result = ExecuteScalar("ins_transactionscreen", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Transaction Screening Details inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> UpdateTransactionScreening(TranScreenDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@p_tranrefno", model.TranRefNo);
                parameters.Add("@p_is_matched", model.IsMatched);
                parameters.Add("@p_match_category", model.MatchCategory);
                parameters.Add("@p_match_name", model.MatchName);
                parameters.Add("@p_match_score", model.MatchScore);
                parameters.Add("@p_match_type", model.MatchType);
                parameters.Add("@p_source", model.Source);
                parameters.Add("@p_source_unique_id", model.SourceUniqueId);
                parameters.Add("@p_updated_by", model.UpdatedBy);
                parameters.Add("@p_status", model.Status);
                parameters.Add("@p_id", model.Id);
                parameters.Add("@p_threshold", model.Threshold);
                parameters.Add("@p_comment", model.Comments);

                serviceResponse.Result = ExecuteScalar("mod_transactionscreen", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Transaction Screening Details updated successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public  ServiceResponse<List<PendingTransactionsDTO>> GetPendingTransactions(PendingTransactionRequestDTO model)
        {
            ServiceResponse<List<PendingTransactionsDTO>> serviceResponse = new ServiceResponse<List<PendingTransactionsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@c_from", Convert.ToDateTime(model.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(model.EndDate));
                parameters.Add("@p_clientId", model.ClientId);
                serviceResponse.Result = Get<PendingTransactionsDTO>("get_all_pending_transaction_screening_case", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Pending Transaction Cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<PendingCasesDTO>> GetIndividualPendingCases(PendingCasesRequestDTO model)
        {
            ServiceResponse<List<PendingCasesDTO>> serviceResponse = new ServiceResponse<List<PendingCasesDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@c_tranrefno", model.tranrefno);
             
                serviceResponse.Result = Get<PendingCasesDTO>("get_individual_pending_transaction_screeningcase", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Pending Individual Transaction Cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TransactionCaseDocumentDTO>> GetCaseDocumentByCaseId(int caseId)
        {
            ServiceResponse<List<TransactionCaseDocumentDTO>> serviceResponse = new ServiceResponse<List<TransactionCaseDocumentDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_case_id", caseId);
                serviceResponse.Result = Get<TransactionCaseDocumentDTO>("get_transactioncasedocuments_by_case_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Case Document details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TransactionCaseDocumentDTO>> GetCaseDocumentByTranRefNo(string tranrefno)
        {
            ServiceResponse<List<TransactionCaseDocumentDTO>> serviceResponse = new ServiceResponse<List<TransactionCaseDocumentDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_tranrefno", tranrefno);
                serviceResponse.Result = Get<TransactionCaseDocumentDTO>("get_transactioncasedocuments_by_tranrefno", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Case Document details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<TranScreenDTO> GetTranCaseByID(int Id)
        {
            ServiceResponse<TranScreenDTO> serviceResponse = new ServiceResponse<TranScreenDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<TranScreenDTO>("get_transactioncase_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Transaction cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<List<TranScreenDTO>> GetTranCaseByRefNo(string tranrefno)
        {
            ServiceResponse<List<TranScreenDTO>> serviceResponse = new ServiceResponse<List<TranScreenDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_tranrefno", tranrefno);
                serviceResponse.Result = Get<TranScreenDTO>("get_transactioncase_by_tranRefNo", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Transaction cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<TranstatusCheckDTO> GetTranStatusByTranRefNo(string tranRefNo)
        {
            ServiceResponse<TranstatusCheckDTO> serviceResponse = new ServiceResponse<TranstatusCheckDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_tranRefNo", tranRefNo);
                serviceResponse.Result = GetFirstOrDefault<TranstatusCheckDTO>("get_transaction_status_by_tranRefNo", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Transaction status fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> CreateCaseDocumentByCaseID(TransactionCaseDocumentDTO _CaseDocumentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_case_id", _CaseDocumentDTO.CaseId);
                parameters.Add("@p_tranref", _CaseDocumentDTO.TranRefNo);
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
                var response = ExecuteScalar("ins_trancase_document", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Document added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TransactionCaseCommentDTO>> GetAllCaseCommentByCase(int CaseId)
        {
            ServiceResponse<List<TransactionCaseCommentDTO>> serviceResponse = new ServiceResponse<List<TransactionCaseCommentDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_case_id", CaseId);
                serviceResponse.Result = Get<TransactionCaseCommentDTO>("get_trancase_comments_by_case_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Case Comment details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> CreateTransactionCaseTransfer(TransactionCaseAssignmentDTO _CaseAssignmentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_case_id", _CaseAssignmentDTO.CaseId);
                parameters.Add("@p_user_id", _CaseAssignmentDTO.UserId);
                parameters.Add("@p_comment", _CaseAssignmentDTO.Comment);
                parameters.Add("@p_created_on", _CaseAssignmentDTO.CreatedOn);
                parameters.Add("@p_created_by", _CaseAssignmentDTO.CreatedBy);
                var response = ExecuteScalar("ins_trancase_assignment", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Assignment added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> CreateCaseCommentByCaseID(TransactionCaseCommentDTO _CaseCommentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_tranref", _CaseCommentDTO.TranRefNo);
                parameters.Add("@p_case_id", _CaseCommentDTO.CaseId);
                parameters.Add("@p_comment", _CaseCommentDTO.Comment);
                parameters.Add("@p_created_on", _CaseCommentDTO.CreatedOn);
                parameters.Add("@p_created_by", _CaseCommentDTO.CreatedBy);
                var response = ExecuteScalar("ins_trancase_comment", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Comment added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<IsWhiteListedCheckDTO> checkIfCusWhiteListed(string custref)
        {
            ServiceResponse<IsWhiteListedCheckDTO> serviceResponse = new ServiceResponse<IsWhiteListedCheckDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_custref", custref);
                serviceResponse.Result = GetFirstOrDefault<IsWhiteListedCheckDTO>("get_cust_whitelist_status", parameters, commandType: CommandType.StoredProcedure);
               
                serviceResponse.Message = "WhiteList Status fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<PendingTransactionsDTO>> GetAllTransactions(PendingTransactionRequestDTO model)
        {
            ServiceResponse<List<PendingTransactionsDTO>> serviceResponse = new ServiceResponse<List<PendingTransactionsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@c_from", Convert.ToDateTime(model.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(model.EndDate));
                parameters.Add("@p_clientId", model.ClientId);
                serviceResponse.Result = Get<PendingTransactionsDTO>("get_all_transaction_screening_case", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Pending Transaction Cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TranScreenDTO>> GetAllUnScreenedTransactions()
        {
            ServiceResponse<List<TranScreenDTO>> serviceResponse = new ServiceResponse<List<TranScreenDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                serviceResponse.Result = Get<TranScreenDTO>("get_unscreened_transactions", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Pending Transaction Cases fetched successfully.";
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
