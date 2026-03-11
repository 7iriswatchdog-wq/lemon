using AML.Core.RepositoryContract.ProliferationFinance;
using AML.Core.ServiceContract.ProliferationFinance;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.ProliferationFinance;
using AML.Core.Common.CommonClasses;
using System.Collections.Generic;
using System;
using System.IO;

namespace AML.Core.Service.ProliferationFinance
{
    public class ProliferationFinanceService : IProliferationFinanceService
    {
        private readonly IProliferationFinanceRepository _repository;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public ProliferationFinanceService(IProliferationFinanceRepository repository, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public ServiceResponse<List<UAEControlListDTO>> SearchChemicals(ProliferationFinanceCaseDTO searchCriteria)
        {
            return _repository.SearchChemicals(searchCriteria);
        }

        public ServiceResponse<string> SearchNonChemical(string keyword)
        {
            try
            {
                // Dynamic path from appsettings.json
                string relativePath = _configuration["ProliferationFinance:ControlListPdfPath"] ?? @"wwwroot/Documents/UAE_Control_List.pdf";
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
                string matchedParagraph = PdfHelper.SearchKeywordInPdf(filePath, keyword);

                return new ServiceResponse<string>
                {
                    Status = 200,
                    Result = matchedParagraph,
                    Message = matchedParagraph != null ? "Match found" : "No match found"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>
                {
                    Status = 500,
                    Message = "Error searching PDF: " + ex.Message
                };
            }
        }

        public ServiceResponse<int> CreateCase(ProliferationFinanceCaseDTO caseDto)
        {
            return _repository.CreateCase(caseDto);
        }

        public ServiceResponse<List<ProliferationFinanceCaseDTO>> GetAllCases()
        {
            return _repository.GetAllCases();
        }

        public ProliferationFinanceCaseDTO GetCaseById(int id)
        {
            return _repository.GetCaseById(id);
        }

        public UAEControlListDTO GetChemicalById(int id)
        {
            return _repository.GetChemicalById(id);
        }

        public ServiceResponse<bool> UpdateCaseStatus(int caseId, string status)
        {
            return _repository.UpdateCaseStatus(caseId, status);
        }

        public ServiceResponse<bool> UpdateCaseRemarks(int caseId, string remarks)
        {
            return _repository.UpdateCaseRemarks(caseId, remarks);
        }

        public ServiceResponse<bool> UpdateSearchHits(int caseId, string hitDetails)
        {
            return _repository.UpdateSearchHits(caseId, hitDetails);
        }
    }
}
