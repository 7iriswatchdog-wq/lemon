using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.SectoralTMS;
using AML.Core.ServiceContract.SectoralTMS;
using AML.DTO.DTO.SectoralTMS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AML.Core.Service.SectoralTMS
{
    public class StmService : IStmService
    {
        private readonly IStmRepository _repo;

        public StmService(IStmRepository repo)
        {
            _repo = repo;
        }

        // ==================================================================
        // SECTORS
        // ==================================================================
        public ServiceResponse<List<StmSectorDTO>> GetSectors(int clientId)
            => _repo.GetSectors(clientId);

        // ==================================================================
        // RULES
        // ==================================================================
        public ServiceResponse<int> CreateRule(StmRuleDTO rule)
        {
            var response = _repo.InsertRule(rule);
            if (response.Status != 200 || response.Result == 0) return response;

            int ruleId = response.Result;
            if (rule.Conditions != null)
            {
                int seq = 1;
                foreach (var c in rule.Conditions)
                {
                    c.RuleId = ruleId;
                    if (c.SequenceNo <= 0) c.SequenceNo = seq;
                    _repo.InsertRuleCondition(c);
                    seq++;
                }
            }
            return response;
        }

        public ServiceResponse<int> UpdateRule(StmRuleDTO rule)
        {
            var updateResponse = _repo.UpdateRule(rule);
            if (updateResponse.Status == 200)
            {
                _repo.DeleteRuleConditions(rule.Id);
                if (rule.Conditions != null)
                {
                    int seq = 1;
                    foreach (var c in rule.Conditions)
                    {
                        c.RuleId = rule.Id;
                        if (c.SequenceNo <= 0) c.SequenceNo = seq;
                        _repo.InsertRuleCondition(c);
                        seq++;
                    }
                }
            }
            return updateResponse;
        }

        public ServiceResponse<int> ToggleRuleStatus(int ruleId, int isActive, int userId)
            => _repo.ToggleRuleStatus(ruleId, isActive, userId);

        public ServiceResponse<List<StmRuleDTO>> GetRules(int clientId, string sectorCode = null)
            => _repo.GetRules(clientId, sectorCode);

        public ServiceResponse<StmRuleDTO> GetRule(int ruleId) => _repo.GetRuleById(ruleId);

        // ==================================================================
        // TRANSACTIONS + RULES EXECUTION
        // ==================================================================
        public ServiceResponse<StmTransactionResultDTO> SubmitTransaction(StmTransactionDTO tran, int userId)
        {
            var response = new ServiceResponse<StmTransactionResultDTO>();
            var result = new StmTransactionResultDTO();
            try
            {
                tran.CreatedBy = userId;
                tran.RuleHitStatus = "PENDING";
                var insertResponse = _repo.InsertTransaction(tran);
                if (insertResponse.Status != 200 || insertResponse.Result == 0)
                {
                    response.Status = 500;
                    response.Message = "Failed to insert transaction: " + insertResponse.Message;
                    return response;
                }

                int tranId = insertResponse.Result;
                tran.Id = tranId;
                result.TransactionId = tranId;
                result.TranRefNo = tran.TranRefNo;

                if (tran.Parties != null)
                {
                    foreach (var p in tran.Parties)
                    {
                        p.TransactionId = tranId;
                        _repo.InsertTransactionParty(p);
                    }
                }

                // ---- Run transaction risk scoring (industry-standard factors) ----
                // This runs BEFORE the rules engine so the rules can use the risk rating
                // if they want, and so the case (if any) has a risk score attached.
                string txnRiskRating = null;
                int txnRiskScore = 0;
                try
                {
                    var riskFactors = _repo.GetActiveRiskFactors(tran.SectorId, tran.ClientId).Result
                                       ?? new List<StmTxnRiskFactorDTO>();
                    if (riskFactors.Count > 0)
                    {
                        var riskEval = StmTxnRiskEngine.Evaluate(tran, riskFactors);
                        txnRiskRating = riskEval.RiskRating;
                        txnRiskScore = riskEval.TotalScore;
                        _repo.SaveRiskResult(new StmTxnRiskResultDTO
                        {
                            TransactionId = tran.Id,
                            SectorId = tran.SectorId,
                            TotalScore = riskEval.TotalScore,
                            RiskRating = riskEval.RiskRating,
                            FactorCount = riskEval.FactorCount,
                            FactorBreakdown = Newtonsoft.Json.JsonConvert.SerializeObject(riskEval.Items),
                            ClientId = tran.ClientId
                        });
                        // Mirror onto the transaction's flags so it surfaces in lists
                        if (string.Equals(riskEval.RiskRating, "High", StringComparison.OrdinalIgnoreCase))
                            _repo.UpdateTransactionRuleStatus(tran.Id, tran.RuleHitStatus, userId);
                    }
                }
                catch (Exception riskEx)
                {
                    // Risk scoring is best-effort - never block the transaction over it.
                    Console.Error.WriteLine($"Txn risk scoring failed for {tran.TranRefNo}: {riskEx.Message}");
                }

                // ---- Run rules engine ----
                var rules = _repo.GetRules(tran.ClientId).Result
                                ?.Where(r => r.IsActive == 1 && r.SectorId == tran.SectorId).ToList()
                            ?? new List<StmRuleDTO>();

                // Build the evaluation context: the customer's transaction history (which now
                // includes the just-inserted transaction) so Count/Sum aggregation and
                // sequence rules have real data to work with.
                var history = string.IsNullOrWhiteSpace(tran.CustomerId)
                    ? new List<StmTransactionDTO> { tran }
                    : (_repo.GetCustomerTransactions(tran.CustomerId, tran.ClientId).Result ?? new List<StmTransactionDTO> { tran });
                var evalContext = new StmRuleEvalContext { CustomerHistory = history, AsOf = tran.TranDate };

                var hitRules = new List<StmRuleDTO>();
                int totalScore = 0;
                string highestRisk = "Low";

                foreach (var rule in rules)
                {
                    rule.Conditions = _repo.GetRuleConditions(rule.Id).Result;
                    var (isHit, trace) = StmRulesEngine.Evaluate(rule, tran, evalContext);

                    var evalResult = new StmRuleEvaluationResultDTO
                    {
                        RuleId = rule.Id,
                        RuleCode = rule.RuleCode,
                        RuleName = rule.RuleName,
                        RiskRating = rule.RiskRating,
                        Score = rule.RuleScore,
                        IsHit = isHit,
                        HitReason = string.Join(" | ", trace),
                        ConditionResults = trace
                    };
                    result.RuleResults.Add(evalResult);

                    _repo.InsertRuleExecLog(new StmRuleExecLogDTO
                    {
                        TransactionId = tranId,
                        RuleId = rule.Id,
                        IsHit = isHit ? 1 : 0,
                        HitDetails = string.Join(" | ", trace)
                    });

                    if (isHit)
                    {
                        hitRules.Add(rule);
                        totalScore += rule.RuleScore;
                        if (RankRisk(rule.RiskRating) > RankRisk(highestRisk))
                            highestRisk = rule.RiskRating;
                    }
                }

                // ---- Create case if any rule hit, OR if no rule hit but txn risk is High ----
                if (hitRules.Any())
                {
                    result.AnyRuleHit = true;
                    _repo.UpdateTransactionRuleStatus(tranId, "HIT", userId);

                    var caseRefNo = $"STM-{DateTime.Now:yyyyMMdd}-{tranId:D6}";
                    var caseDto = new StmCaseDTO
                    {
                        CaseRefNo = caseRefNo,
                        TransactionId = tranId,
                        SectorId = tran.SectorId,
                        RulesViolated = JsonConvert.SerializeObject(hitRules.Select(r => r.Id).ToList()),
                        RuleNames = string.Join(", ", hitRules.Select(r => r.RuleName)),
                        TotalRiskScore = totalScore,
                        RiskRating = highestRisk,
                        Status = "OPEN",
                        ClientId = tran.ClientId,
                        CreatedBy = userId
                    };
                    var caseResponse = _repo.InsertCase(caseDto);
                    if (caseResponse.Status == 200)
                    {
                        result.CaseId = caseResponse.Result;
                        result.CaseRefNo = caseRefNo;
                        _repo.InsertCaseComment(new StmCaseCommentDTO
                        {
                            CaseId = caseResponse.Result,
                            CommentText = $"Case auto-created from rule hit. Rules: {string.Join(", ", hitRules.Select(r => r.RuleCode))}",
                            ActionType = "AUTO_CREATED",
                            CreatedBy = userId,
                            CreatedUser = "System"
                        });
                    }
                }
                else if (string.Equals(txnRiskRating, "High", StringComparison.OrdinalIgnoreCase))
                {
                    // No rule hit, but transaction risk scoring returned High - still open a case for review.
                    _repo.UpdateTransactionRuleStatus(tranId, "RISK_HIGH", userId);

                    var caseRefNo = $"STM-{DateTime.Now:yyyyMMdd}-{tranId:D6}";
                    var caseDto = new StmCaseDTO
                    {
                        CaseRefNo = caseRefNo,
                        TransactionId = tranId,
                        SectorId = tran.SectorId,
                        RulesViolated = JsonConvert.SerializeObject(new List<int>()),
                        RuleNames = "High Risk (no rule hit)",
                        TotalRiskScore = txnRiskScore,
                        RiskRating = "High",
                        Status = "OPEN",
                        ClientId = tran.ClientId,
                        CreatedBy = userId
                    };
                    var caseResponse = _repo.InsertCase(caseDto);
                    if (caseResponse.Status == 200)
                    {
                        result.CaseId = caseResponse.Result;
                        result.CaseRefNo = caseRefNo;
                        _repo.InsertCaseComment(new StmCaseCommentDTO
                        {
                            CaseId = caseResponse.Result,
                            CommentText = $"Case auto-created from high transaction risk score ({txnRiskScore}). No rule hit.",
                            ActionType = "AUTO_CREATED_RISK",
                            CreatedBy = userId,
                            CreatedUser = "System"
                        });
                    }
                }
                else
                {
                    _repo.UpdateTransactionRuleStatus(tranId, "NO_HIT", userId);
                }

                response.Result = result;
                response.Status = 200;
                if (result.AnyRuleHit)
                    response.Message = $"Transaction stored. {hitRules.Count} rule(s) hit. Case {result.CaseRefNo} created.";
                else if (!string.IsNullOrEmpty(result.CaseRefNo))
                    response.Message = $"Transaction stored. No rules hit, but risk is High. Case {result.CaseRefNo} created.";
                else
                    response.Message = "Transaction stored. No rules hit.";
            }
            catch (Exception ex)
            {
                response.Status = 500;
                response.Message = ex.Message;
            }
            return response;
        }

        private static int RankRisk(string r)
        {
            if (string.IsNullOrEmpty(r)) return 0;
            if (string.Equals(r, "High", StringComparison.OrdinalIgnoreCase)) return 3;
            if (string.Equals(r, "Medium", StringComparison.OrdinalIgnoreCase)) return 2;
            if (string.Equals(r, "Low", StringComparison.OrdinalIgnoreCase)) return 1;
            return 0;
        }

        public ServiceResponse<List<StmTransactionDTO>> SearchTransactions(StmTransactionSearchDTO search)
            => _repo.SearchTransactions(search);

        public ServiceResponse<StmTransactionDTO> GetTransaction(int id) => _repo.GetTransactionById(id);

        public ServiceResponse<List<StmTransactionDTO>> GetCustomerTransactions(string customerId, int clientId)
            => _repo.GetCustomerTransactions(customerId, clientId);

        // ==================================================================
        // CUSTOMER LOOKUP
        // ==================================================================
        public ServiceResponse<List<dynamic>> SearchCustomers(string query, int clientId)
            => _repo.SearchCustomers(query, clientId);

        public ServiceResponse<dynamic> GetCustomerDetails(string customerId, int clientId)
            => _repo.GetCustomerDetails(customerId, clientId);

        // ==================================================================
        // CASES
        // ==================================================================
        public ServiceResponse<List<StmCaseDTO>> GetOpenCases(StmCaseSearchDTO search)
        {
            if (string.IsNullOrEmpty(search.Status)) search.Status = "OPEN";
            return _repo.SearchCases(search);
        }

        public ServiceResponse<List<StmCaseDTO>> GetCompletedCases(StmCaseSearchDTO search)
        {
            // Completed = APPROVED / REJECTED / CLOSED / ESCALATED
            var resp = _repo.SearchCases(new StmCaseSearchDTO
            {
                SectorCode = search.SectorCode,
                CustomerId = search.CustomerId,
                CustomerName = search.CustomerName,
                FromDate = search.FromDate,
                ToDate = search.ToDate,
                RiskRating = search.RiskRating,
                ClientId = search.ClientId
            });
            if (resp.Status == 200 && resp.Result != null)
            {
                resp.Result = resp.Result.Where(c =>
                    c.Status == "APPROVED" || c.Status == "REJECTED" ||
                    c.Status == "CLOSED" || c.Status == "ESCALATED").ToList();
            }
            return resp;
        }

        public ServiceResponse<StmCaseDTO> GetCase(int caseId) => _repo.GetCaseById(caseId);

        public ServiceResponse<int> ReviewCase(int caseId, string decision, string remarks, string status, int userId, string userName)
        {
            var caseResponse = _repo.GetCaseById(caseId);
            if (caseResponse.Status != 200 || caseResponse.Result == null)
            {
                return new ServiceResponse<int> { Status = 404, Message = "Case not found" };
            }
            var c = caseResponse.Result;
            c.Status = status ?? "REJECTED";
            c.ReviewDecision = decision;
            c.ReviewRemarks = remarks;
            c.ReviewedBy = userId;
            c.ReviewedOn = DateTime.Now;
            c.UpdatedBy = userId;
            var updateResp = _repo.UpdateCase(c);
            if (updateResp.Status == 200)
            {
                _repo.InsertCaseComment(new StmCaseCommentDTO
                {
                    CaseId = caseId,
                    CommentText = $"Case reviewed. Decision: {decision}. Status: {status}. Remarks: {remarks}",
                    ActionType = "DECISION",
                    CreatedBy = userId,
                    CreatedUser = userName
                });
            }
            return updateResp;
        }

        public ServiceResponse<int> AddCaseComment(int caseId, string comment, int userId, string userName)
        {
            return _repo.InsertCaseComment(new StmCaseCommentDTO
            {
                CaseId = caseId,
                CommentText = comment,
                ActionType = "COMMENT",
                CreatedBy = userId,
                CreatedUser = userName
            });
        }

        // ==================================================================
        // TRANSACTION RISK
        // ==================================================================
        public ServiceResponse<List<StmTxnRiskFactorDTO>> GetTxnRiskFactors(int sectorId, int clientId)
            => _repo.GetActiveRiskFactors(sectorId, clientId);

        public ServiceResponse<List<StmTxnRiskFactorDTO>> GetAllTxnRiskFactors(int sectorId, int clientId)
            => _repo.GetAllRiskFactors(sectorId, clientId);

        public ServiceResponse<StmTxnRiskFactorDTO> GetTxnRiskFactor(int id)
            => _repo.GetRiskFactorById(id);

        public ServiceResponse<int> CreateTxnRiskFactor(StmTxnRiskFactorDTO factor)
        {
            // Insert factor row, then insert each band with the new factor_id.
            var resp = _repo.InsertRiskFactor(factor);
            if (resp.Status != 200 || resp.Result == 0) return resp;

            int factorId = resp.Result;
            if (factor.Bands != null)
            {
                int seq = 1;
                foreach (var b in factor.Bands)
                {
                    b.FactorId = factorId;
                    if (b.SequenceNo <= 0) b.SequenceNo = seq;
                    _repo.InsertRiskBand(b);
                    seq++;
                }
            }
            return resp;
        }

        public ServiceResponse<int> UpdateTxnRiskFactor(StmTxnRiskFactorDTO factor)
        {
            // Update factor then replace bands (simpler than diffing and matches the rule-editor pattern)
            var resp = _repo.UpdateRiskFactor(factor);
            if (resp.Status != 200) return resp;

            _repo.DeleteBandsForFactor(factor.Id);
            if (factor.Bands != null)
            {
                int seq = 1;
                foreach (var b in factor.Bands)
                {
                    b.FactorId = factor.Id;
                    if (b.SequenceNo <= 0) b.SequenceNo = seq;
                    _repo.InsertRiskBand(b);
                    seq++;
                }
            }
            return resp;
        }

        public ServiceResponse<int> DeleteTxnRiskFactor(int id)
            => _repo.DeleteRiskFactor(id);

        public ServiceResponse<StmTxnRiskEvaluation> EvaluateTxnRisk(int transactionId)
        {
            var response = new ServiceResponse<StmTxnRiskEvaluation>();
            try
            {
                var tranResp = _repo.GetTransactionById(transactionId);
                if (tranResp.Status != 200 || tranResp.Result == null)
                {
                    response.Status = 404;
                    response.Message = "Transaction not found";
                    return response;
                }
                var tran = tranResp.Result;
                var factors = _repo.GetActiveRiskFactors(tran.SectorId, tran.ClientId).Result
                              ?? new List<StmTxnRiskFactorDTO>();
                var eval = StmTxnRiskEngine.Evaluate(tran, factors);

                // Persist for audit / display on the case
                _repo.SaveRiskResult(new StmTxnRiskResultDTO
                {
                    TransactionId = tran.Id,
                    SectorId = tran.SectorId,
                    TotalScore = eval.TotalScore,
                    RiskRating = eval.RiskRating,
                    FactorCount = eval.FactorCount,
                    FactorBreakdown = Newtonsoft.Json.JsonConvert.SerializeObject(eval.Items),
                    ClientId = tran.ClientId
                });

                response.Result = eval;
                response.Status = 200;
                response.Message = $"{eval.FactorCount} factors evaluated. Rating: {eval.RiskRating}";
            }
            catch (Exception ex)
            {
                response.Status = 500;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<StmTxnRiskEvaluation> PreviewTxnRisk(StmTransactionDTO tran)
        {
            var response = new ServiceResponse<StmTxnRiskEvaluation>();
            try
            {
                if (tran == null)
                {
                    response.Status = 400;
                    response.Message = "Transaction is required";
                    return response;
                }
                var factors = _repo.GetActiveRiskFactors(tran.SectorId, tran.ClientId).Result
                              ?? new List<StmTxnRiskFactorDTO>();
                response.Result = StmTxnRiskEngine.Evaluate(tran, factors);
                response.Status = 200;
            }
            catch (Exception ex)
            {
                response.Status = 500;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<StmTxnRiskResultDTO> GetSavedRiskResult(int transactionId)
            => _repo.GetRiskResultForTransaction(transactionId);

        public ServiceResponse<bool> IsStmModuleEnabled(int clientId)
            => _repo.IsStmModuleEnabled(clientId);

        public ServiceResponse<List<string>> GetAllowedSectorCodes(int clientId)
            => _repo.GetAllowedSectorCodes(clientId);

        // ==================================================================
        // TEST RULE
        // ==================================================================
        public ServiceResponse<List<StmRuleEvaluationResultDTO>> TestRuleAgainstTransaction(StmRuleDTO rule, StmTransactionDTO tran)
        {
            var response = new ServiceResponse<List<StmRuleEvaluationResultDTO>>();
            try
            {
                var (isHit, trace) = StmRulesEngine.Evaluate(rule, tran);
                response.Result = new List<StmRuleEvaluationResultDTO>
                {
                    new StmRuleEvaluationResultDTO
                    {
                        RuleId = rule.Id,
                        RuleCode = rule.RuleCode,
                        RuleName = rule.RuleName,
                        RiskRating = rule.RiskRating,
                        Score = rule.RuleScore,
                        IsHit = isHit,
                        HitReason = string.Join(" | ", trace),
                        ConditionResults = trace
                    }
                };
                response.Status = 200;
                response.Message = isHit ? "Rule would HIT this transaction" : "Rule would NOT hit this transaction";
            }
            catch (Exception ex)
            {
                response.Status = 500;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
