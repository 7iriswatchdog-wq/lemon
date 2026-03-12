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
        private readonly IProliferationFinanceMongoRepository _mongoRepository;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public ProliferationFinanceService(IProliferationFinanceRepository repository, IProliferationFinanceMongoRepository mongoRepository, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _repository = repository;
            _mongoRepository = mongoRepository;
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


                // Absolute path as per user request
                //string filePath = @"d:\Omkar\lemon_new\قرار مجلس الوزراء رقم (156) لسنة 2025 .pdf";
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "_قرار مجلس الوزراء رقم (156) لسنة 2025 .pdf");

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

        public ServiceResponse<bool> SyncSearchResultsToMongo(int caseId, string productName, List<UAEControlListDTO> chemicalHits, string pdfHits)
        {
            try
            {
                var result = new PFSearchResultsMongoDTO.PF_SEARCHRESULT
                {
                    CaseId = caseId,
                    ProductName = productName,
                    Hits = new List<PFSearchResultsMongoDTO.PF_Hit>()
                };

                // Add chemical hits
                if (chemicalHits != null)
                {
                    foreach (var h in chemicalHits)
                    {
                        result.Hits.Add(new PFSearchResultsMongoDTO.PF_Hit
                        {
                            SearchType = "Chemical",
                            ChemicalId = h.Id,
                            MatchedName = h.ChemicalName,
                            HsCode = h.HsCode,
                            CasNumber = h.CasNumber,
                            Eccn = h.Eccn,
                            FoundOn = DateTime.Now
                        });
                    }
                }

                // Add PDF hits
                if (!string.IsNullOrEmpty(pdfHits))
                {
                    var paragraphs = pdfHits.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in paragraphs)
                    {
                        result.Hits.Add(new PFSearchResultsMongoDTO.PF_Hit
                        {
                            SearchType = "Non-Chemical",
                            Snippet = p,
                            FoundOn = DateTime.Now
                        });
                    }
                }

                var success = _mongoRepository.SaveSearchResults(result);
                return new ServiceResponse<bool> { Status = success ? 200 : 500, Result = success };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool> { Status = 500, Message = ex.Message };
            }
        }

        public ServiceResponse<bool> UpdateMongoHitDecision(int caseId, int hitIndex, string decision, string remarks)
        {
            var success = _mongoRepository.UpdateHitDecision(caseId, hitIndex, decision, remarks);
            return new ServiceResponse<bool> { Status = success ? 200 : 500, Result = success };
        }

        public PFSearchResultsMongoDTO.PF_SEARCHRESULT GetMongoSearchResults(int caseId)
        {
            return _mongoRepository.GetSearchResultsByCaseId(caseId);
        }
    }
}
