using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.TransactionScreening;
using AML.Core.ServiceContract.TransactionScreening;
using AML.DTO.DTO.TransactionScreening;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace AML.Core.Service.TransactionScreening
{
    public class TransactionScreeningService : BaseService, ITransactionScreeningService
    {


        ITransactionScreeningRepository _transactionscreeningRepository;
        public TransactionScreeningService(ITransactionScreeningRepository transactionscreeningRepository, IConfiguration configuration, IHostingEnvironment environment) : base(transactionscreeningRepository, configuration)
        {
            _transactionscreeningRepository = transactionscreeningRepository;
        }

        public ServiceResponse<int> InsertTransactionScreening(TranScreenDTO model)
        {
            return _transactionscreeningRepository.InsertTransactionScreening(model);
        }

        public ServiceResponse<int> UpdateTransactionScreening(TranScreenDTO model)
        {
            return _transactionscreeningRepository.UpdateTransactionScreening(model);
        }

        public List<PendingTransactionsDTO> GetPendingTransactions(PendingTransactionRequestDTO model)
        {
            return _transactionscreeningRepository.GetPendingTransactions(model).Result;
        }
        public List<PendingCasesDTO> GetIndividualPendingCases(PendingCasesRequestDTO model)
        {
            return _transactionscreeningRepository.GetIndividualPendingCases(model).Result;
        }

        public TranScreenDTO GetTranCaseByID(int Id)
        {
            //Perform business requirements here
            return _transactionscreeningRepository.GetTranCaseByID(Id).Result;
        }

        public List<TranScreenDTO> GetTranCaseByRefNo(string tranrefno)
        {
            //Perform business requirements here
            return _transactionscreeningRepository.GetTranCaseByRefNo(tranrefno).Result;
        }

        public List<TransactionCaseDocumentDTO> GetCaseDocumentByCaseId(int caseId)
        {
            //Perform business requirements here
            return _transactionscreeningRepository.GetCaseDocumentByCaseId(caseId).Result;
        }

        public List<TransactionCaseDocumentDTO> GetCaseDocumentByTranRefNo(string tranrefno)
        {
            //Perform business requirements here
            return _transactionscreeningRepository.GetCaseDocumentByTranRefNo(tranrefno).Result;
        }



        public TranstatusCheckDTO GetTranStatusByTranRefNo(string _tranRefNo)
        {
            //Perform business requirements here
            return _transactionscreeningRepository.GetTranStatusByTranRefNo(_tranRefNo).Result;

        }

        public ServiceResponse<int> CreateCaseDocumentByCaseID(TransactionCaseDocumentDTO _CaseDocumentDTO)
        {
            return _transactionscreeningRepository.CreateCaseDocumentByCaseID(_CaseDocumentDTO);
        }

        public ServiceResponse<int> CreateCaseCommentByCaseID(TransactionCaseCommentDTO _CaseCommentDTO)
        {
            return _transactionscreeningRepository.CreateCaseCommentByCaseID(_CaseCommentDTO);
        }


        public List<TransactionCaseCommentDTO> GetAllCaseCommentByCase(int Id)
        {
            //Perform business requirements here
            return _transactionscreeningRepository.GetAllCaseCommentByCase(Id).Result;
        }



        public ServiceResponse<int> CreateTransactionCaseTransfer(TransactionCaseAssignmentDTO _CaseAssignmentDTO)
        {
            return _transactionscreeningRepository.CreateTransactionCaseTransfer(_CaseAssignmentDTO);
        }


        public IsWhiteListedCheckDTO checkIfCusWhiteListed(string custref)
        {
            return _transactionscreeningRepository.checkIfCusWhiteListed(custref).Result;
        }

        public List<PendingTransactionsDTO> GetAllTransactions(PendingTransactionRequestDTO model)
        {
            return _transactionscreeningRepository.GetAllTransactions(model).Result;
        }

        public List<TranScreenDTO> GetAllUnScreenedTransactions()
        {
            return _transactionscreeningRepository.GetAllUnScreenedTransactions().Result;
        }

    }

}
