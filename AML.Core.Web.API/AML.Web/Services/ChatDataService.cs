using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.Risk;
using AML.Core.ServiceContract.ProliferationFinance;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Risk;
using AML.DTO.DTO.ProliferationFinance;
using AML.DTO.DTO.CorporateShareholder;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace AML.Web.Services
{
    public class ChatDataService
    {
        private readonly ICustomerCaseService _customerCaseService;
        private readonly IRiskService _riskService;
        private readonly IProliferationFinanceService _proliferationFinanceService;
        private readonly IMemoryCache _memoryCache;

        public ChatDataService(
            ICustomerCaseService customerCaseService,
            IRiskService riskService,
            IProliferationFinanceService proliferationFinanceService,
            IMemoryCache memoryCache)
        {
            _customerCaseService = customerCaseService;
            _riskService = riskService;
            _proliferationFinanceService = proliferationFinanceService;
            _memoryCache = memoryCache;
        }

        public CustomerCaseDTO GetCaseDetails(string caseId)
        {
            if (int.TryParse(caseId, out int numericCaseId))
            {
                return _customerCaseService.GetCaseFullDetailsByCaseId(numericCaseId);
            }
            return _customerCaseService.GetCaseFullDetailsByCustId(caseId);
        }

        public List<CustomerCaseDTO> GetShareholders(string customerId)
        {
            return _customerCaseService.GetShareHoldersByCompanyCode(customerId);
        }

        public List<RiskReportDTO> GetRiskAssessment(string customerId, string customerType)
        {
            return _riskService.GetLastestRiskVersion(customerId, customerType);
        }

        public List<CustomerCaseDTO> GetAllSanctionDashboard(int clientId, string filter)
        {
            string cacheKey = $"SanctionDashboard_{clientId}_{filter}";
            
            if (!_memoryCache.TryGetValue(cacheKey, out List<CustomerCaseDTO> cachedResult))
            {
                cachedResult = _customerCaseService.GetAllSanctionDashboard(clientId, filter);
                
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                
                _memoryCache.Set(cacheKey, cachedResult, cacheEntryOptions);
            }
            
            return cachedResult;
        }

        public List<ProliferationFinanceCaseDTO> GetAllPFCases()
        {
            return _proliferationFinanceService.GetAllCases()?.Result ?? new List<ProliferationFinanceCaseDTO>();
        }

        public List<PFSearchResultsMongoDTO.PF_Hit> GetMongoSearchResults(int caseId)
        {
            var mongoData = _proliferationFinanceService.GetMongoSearchResults(caseId);
            return mongoData?.Hits ?? new List<PFSearchResultsMongoDTO.PF_Hit>();
        }
    }
}