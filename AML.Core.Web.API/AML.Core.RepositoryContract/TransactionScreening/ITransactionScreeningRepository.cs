using System;
using System.Collections.Generic;
using AML.DTO.DTO.TransactionScreening;
using AML.Core.Common.StaticResource;

namespace AML.Core.RepositoryContract.TransactionScreening
{
    public interface ITransactionScreeningRepository : IBaseRepository
    {
        ServiceResponse<int> InsertTransactionScreening(TranScreenDTO model);

        ServiceResponse<int> UpdateTransactionScreening(TranScreenDTO model);

        ServiceResponse<List<PendingTransactionsDTO>> GetPendingTransactions(PendingTransactionRequestDTO model);

        ServiceResponse<List<PendingCasesDTO>> GetIndividualPendingCases(PendingCasesRequestDTO model);

        ServiceResponse<List<TransactionCaseDocumentDTO>> GetCaseDocumentByCaseId(int caseId);

        ServiceResponse<List<TransactionCaseDocumentDTO>> GetCaseDocumentByTranRefNo(string tranrefno);

        ServiceResponse<TranScreenDTO> GetTranCaseByID(int Id);

        ServiceResponse<List<TranScreenDTO>> GetTranCaseByRefNo(string tranrefno);

        ServiceResponse<TranstatusCheckDTO> GetTranStatusByTranRefNo(string _tranRefNo);
        ServiceResponse<int> CreateCaseDocumentByCaseID(TransactionCaseDocumentDTO _CaseDocumentDTO);

        ServiceResponse<int> CreateCaseCommentByCaseID(TransactionCaseCommentDTO _CaseCommentDTO);

        ServiceResponse<List<TransactionCaseCommentDTO>> GetAllCaseCommentByCase(int CaseId);

        ServiceResponse<int> CreateTransactionCaseTransfer(TransactionCaseAssignmentDTO _CaseAssignmentDTO);

        ServiceResponse<IsWhiteListedCheckDTO> checkIfCusWhiteListed(string custref);

        ServiceResponse<List<PendingTransactionsDTO>> GetAllTransactions(PendingTransactionRequestDTO model);
        ServiceResponse<List<TranScreenDTO>> GetAllUnScreenedTransactions();

    }



}
