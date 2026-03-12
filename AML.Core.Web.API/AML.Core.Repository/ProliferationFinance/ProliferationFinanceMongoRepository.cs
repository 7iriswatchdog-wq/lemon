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
                        PF_Hit match = null;

                        if (newHit.SearchType == "Chemical")
                        {
                            // Match by ChemicalId OR by the combination of key fields
                            match = existing.Hits.FirstOrDefault(h =>
                                h.SearchType == "Chemical" &&
                                (
                                    (h.ChemicalId.HasValue && newHit.ChemicalId.HasValue && h.ChemicalId == newHit.ChemicalId) ||
                                    (
                                        string.Equals(h.MatchedName?.Trim(), newHit.MatchedName?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(h.HsCode?.Trim(), newHit.HsCode?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(h.CasNumber?.Trim(), newHit.CasNumber?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(h.Eccn?.Trim(), newHit.Eccn?.Trim(), StringComparison.OrdinalIgnoreCase)
                                    )
                                )
                            );
                        }
                        else
                        {
                            // Non-Chemical: match by snippet content
                            match = existing.Hits.FirstOrDefault(h =>
                                h.SearchType == "Non-Chemical" &&
                                !string.IsNullOrEmpty(h.Snippet) &&
                                string.Equals(h.Snippet?.Trim(), newHit.Snippet?.Trim(), StringComparison.OrdinalIgnoreCase)
                            );
                        }

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
                var result = _collection.Find(filter).FirstOrDefault();

                if (result != null && result.Hits != null)
                {
                    // Deduplicate hits at read time
                    var deduped = new List<PF_Hit>();
                    foreach (var hit in result.Hits)
                    {
                        bool isDupe = false;
                        if (hit.SearchType == "Chemical")
                        {
                            isDupe = deduped.Any(d =>
                                d.SearchType == "Chemical" &&
                                (
                                    (d.ChemicalId.HasValue && hit.ChemicalId.HasValue && d.ChemicalId == hit.ChemicalId) ||
                                    (
                                        string.Equals(d.MatchedName?.Trim(), hit.MatchedName?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(d.HsCode?.Trim(), hit.HsCode?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(d.CasNumber?.Trim(), hit.CasNumber?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                        string.Equals(d.Eccn?.Trim(), hit.Eccn?.Trim(), StringComparison.OrdinalIgnoreCase)
                                    )
                                )
                            );
                        }
                        else
                        {
                            isDupe = deduped.Any(d =>
                                d.SearchType == "Non-Chemical" &&
                                string.Equals(d.Snippet?.Trim(), hit.Snippet?.Trim(), StringComparison.OrdinalIgnoreCase)
                            );
                        }

                        if (!isDupe) deduped.Add(hit);
                    }
                    result.Hits = deduped;
                }

                return result;
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
