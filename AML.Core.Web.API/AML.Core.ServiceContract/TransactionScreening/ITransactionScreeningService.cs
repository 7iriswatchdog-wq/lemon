using AML.Core.Common.StaticResource;
using AML.DTO.DTO.TransactionScreening;
using System;
using System.Collections.Generic;


namespace AML.Core.ServiceContract.TransactionScreening
{
    public interface ITransactionScreeningService : IBaseService
    {
    ServiceResponse<int> InsertTransactionScreening(TranScreenDTO model);

    ServiceResponse<int> UpdateTransactionScreening(TranScreenDTO model);

    List<PendingTransactionsDTO> GetPendingTransactions(PendingTransactionRequestDTO model);
    List<PendingCasesDTO> GetIndividualPendingCases(PendingCasesRequestDTO model);
    TranScreenDTO GetTranCaseByID(int Id);

    List<TranScreenDTO> GetTranCaseByRefNo(string tranrefno);

    List<TransactionCaseDocumentDTO> GetCaseDocumentByCaseId(int caseId);

    List<TransactionCaseDocumentDTO> GetCaseDocumentByTranRefNo(string tranrefno);

     ServiceResponse<int> CreateCaseDocumentByCaseID(TransactionCaseDocumentDTO _CaseDocumentDTO);

    TranstatusCheckDTO GetTranStatusByTranRefNo(string _tranRefNo);

     ServiceResponse<int> CreateCaseCommentByCaseID(TransactionCaseCommentDTO _CaseCommentDTO);

     ServiceResponse<int> CreateTransactionCaseTransfer(TransactionCaseAssignmentDTO _CaseAssignmentDTO);

    List<TransactionCaseCommentDTO> GetAllCaseCommentByCase(int CaseId);

    IsWhiteListedCheckDTO checkIfCusWhiteListed(string custref);

    List<PendingTransactionsDTO> GetAllTransactions(PendingTransactionRequestDTO model);
        List<TranScreenDTO> GetAllUnScreenedTransactions();

    }
}
