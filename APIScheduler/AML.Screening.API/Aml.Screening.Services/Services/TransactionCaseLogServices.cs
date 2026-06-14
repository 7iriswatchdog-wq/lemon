using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using Aml.Screening.MongoModal.UnitOfWork;
using Aml.Screening.ServiceContracts.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aml.Screening.Services.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Aml.Screening.ServiceContracts.ServiceContracts.ICaseLogServices" />
    public class TransactionCaseLogServices : ITransactionCaseLogServices
    {
        /// <summary>
        /// The unit of work case log
        /// </summary>
        private readonly UnitOfWork_TranCaseLogRepository _unitOfWorkTranCaseLog;
        /// <summary>
        /// The predicate
        /// </summary>
        private Expression<Func<TRANSACTION_CASELOG, bool>> _predicate = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseLogServices"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public TransactionCaseLogServices(IConfiguration configuration)
        {
            _unitOfWorkTranCaseLog = new UnitOfWork_TranCaseLogRepository(configuration);
        }
        /// <summary>
        /// Adds the specified case log.
        /// </summary>
        /// <param name="caseLog">The case log.</param>
        /// <returns></returns>
        public async Task<ServiceResponse> Add(TRANSACTION_CASELOG tranCaseLog)
        {
            try
            {
                var result = await _unitOfWorkTranCaseLog.SaveOneAsync(tranCaseLog);
                return new ServiceResponse()
                {
                    CaseId = tranCaseLog.CASEID
                };
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Gets the case by identifier.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        public async Task<List<TRANCASELOGMATCH>> GetTranCaseById(CaseRequestDto dto)
        {
            try
            {
                _predicate = x => x.CASEID.Equals(dto.CASEID);
                var result = await _unitOfWorkTranCaseLog.FindOneAsync(_predicate);
                if (result != null)
                    return result.MATCHRECORDS;
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<TRANSACTION_CASELOG>> GetByTranRefNo(TranCaseRequestDto dto)
        {
            try
            {
                _predicate = x => x.TRANREFNO.Equals(dto.TRANREFNO);
                var result = (await _unitOfWorkTranCaseLog.FindAllAsync(_predicate)) ;
                List<TRANSACTION_CASELOG> searchRes = new List<TRANSACTION_CASELOG> ();
                if (result != null)
                {
                    searchRes = result.ToList();
                    return searchRes;
                }
                else { return null; }   
                
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}

