using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using Aml.Screening.MongoModal.UnitOfWork;
using Aml.Screening.ServiceContracts.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Aml.Screening.Services.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Aml.Screening.ServiceContracts.ServiceContracts.ICaseLogServices" />
    public class CaseLogServices : ICaseLogServices
    {
        /// <summary>
        /// The unit of work case log
        /// </summary>
        private readonly UnitOfWork_CaseLogRepository _unitOfWorkCaseLog;
        /// <summary>
        /// The predicate
        /// </summary>
        private Expression<Func<CASELOG, bool>> _predicate = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseLogServices"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public CaseLogServices(IConfiguration configuration)
        {
            _unitOfWorkCaseLog = new UnitOfWork_CaseLogRepository(configuration);
        }
        /// <summary>
        /// Adds the specified case log.
        /// </summary>
        /// <param name="caseLog">The case log.</param>
        /// <returns></returns>
        public async Task<ServiceResponse> Add(CASELOG caseLog)
        {
            try
            {
                 _unitOfWorkCaseLog.DeleteAsync(caseLog._id);
                var result = await _unitOfWorkCaseLog.SaveOneAsync(caseLog);
                return new ServiceResponse()
                {
                    CaseId = caseLog.CASEID
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
        public async Task<CASELOG> GetCaseById(CaseRequestDto dto)
        {
            try
            {
                _predicate = x => x.CASEID.Equals(dto.CASEID);
                var result = (await _unitOfWorkCaseLog.FindAllAsync(_predicate)).ToList().LastOrDefault();
                if (result != null)
                    return result;
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


      
        }
    }

