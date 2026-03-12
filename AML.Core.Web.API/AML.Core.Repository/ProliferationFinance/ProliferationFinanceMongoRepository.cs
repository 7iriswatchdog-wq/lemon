using AML.Core.RepositoryContract.ProliferationFinance;
using AML.DTO.DTO.ProliferationFinance;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using static AML.DTO.DTO.ProliferationFinance.PFSearchResultsMongoDTO;

namespace AML.Core.Repository.ProliferationFinance
{
    public class ProliferationFinanceMongoRepository : MongoRepository, IProliferationFinanceMongoRepository
    {
        private readonly IMongoDatabase _mongoDB;
        private readonly IMongoCollection<PF_SEARCHRESULT> _collection;

        public ProliferationFinanceMongoRepository(IConfiguration configuration) : base(configuration)
        {
            _mongoDB = GetMongoConnection();
            _collection = _mongoDB.GetCollection<PF_SEARCHRESULT>("PF_SEARCHRESULTS");
        }

        public bool SaveSearchResults(PF_SEARCHRESULT result)
        {
            try
            {
                var filter = Builders<PF_SEARCHRESULT>.Filter.Eq("CaseId", result.CaseId);
                var existing = _collection.Find(filter).FirstOrDefault();

                if (existing == null)
                {
                    result.UpdatedOn = DateTime.Now;
                    _collection.InsertOne(result);
                }
                else
                {
                    // Merge logic: only add new hits if they don't exist, and update decisions/remarks if provided
                    foreach (var newHit in result.Hits)
                    {
                        var match = existing.Hits.FirstOrDefault(h => 
                            (h.ChemicalId.HasValue && h.ChemicalId == newHit.ChemicalId) || 
                            (!string.IsNullOrEmpty(h.Snippet) && h.Snippet == newHit.Snippet)
                        );

                        if (match == null)
                        {
                            existing.Hits.Add(newHit);
                        }
                        else
                        {
                            // Update decision and remarks if new values are provided
                            if (!string.IsNullOrEmpty(newHit.Decision)) match.Decision = newHit.Decision;
                            if (!string.IsNullOrEmpty(newHit.Remarks)) match.Remarks = newHit.Remarks;
                        }
                    }
                    existing.UpdatedOn = DateTime.Now;
                    _collection.ReplaceOne(filter, existing);
                }
                return true;
            }
            catch (Exception ex)
            {
                // In a real app, log this
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }

        public PF_SEARCHRESULT GetSearchResultsByCaseId(int caseId)
        {
            try
            {
                var filter = Builders<PF_SEARCHRESULT>.Filter.Eq("CaseId", caseId);
                return _collection.Find(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return null;
            }
        }

        public bool UpdateHitDecision(int caseId, int hitIndex, string decision, string remarks)
        {
            try
            {
                var filter = Builders<PF_SEARCHRESULT>.Filter.Eq("CaseId", caseId);
                var existing = _collection.Find(filter).FirstOrDefault();
                if (existing != null && existing.Hits.Count > hitIndex)
                {
                    existing.Hits[hitIndex].Decision = decision;
                    existing.Hits[hitIndex].Remarks = remarks;
                    existing.UpdatedOn = DateTime.Now;
                    _collection.ReplaceOne(filter, existing);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
