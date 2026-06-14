using AML.DTO.DTO.SectoralTMS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AML.Core.Service.SectoralTMS
{
    /// <summary>
    /// Evaluates a transaction against a sector's transaction-risk factors and
    /// produces an overall Low/Medium/High rating plus a weighted score.
    ///
    /// For each factor:
    ///   1. Read the transaction field referenced by <c>FieldName</c>.
    ///   2. Walk the factor's bands in order and pick the first that matches.
    ///   3. Add (band.score * factor.weight) to the running total.
    ///
    /// Rating rule: any band hit at rating="High" => overall High.
    /// Otherwise compute average band rating ranked Low(1) Medium(2) High(3),
    /// rounded up, against thresholds (avg >= 2 => Medium, >= 2.5 => High).
    /// </summary>
    public static class StmTxnRiskEngine
    {
        public static StmTxnRiskEvaluation Evaluate(StmTransactionDTO tran,
                                                    List<StmTxnRiskFactorDTO> factors)
        {
            var result = new StmTxnRiskEvaluation
            {
                TransactionId = tran?.Id ?? 0,
                TranRefNo = tran?.TranRefNo
            };
            if (tran == null || factors == null || factors.Count == 0) return result;

            int totalWeighted = 0;
            int maxPossible = 0;
            int hitCount = 0;
            bool anyHigh = false;
            decimal sumRatingRank = 0;

            foreach (var f in factors)
            {
                object value = ReadField(tran, f.FieldName);
                string valueStr = value?.ToString() ?? string.Empty;
                var band = PickBand(f, value, valueStr);
                if (band == null) continue;  // No matching band -> factor doesn't apply

                hitCount++;
                int weighted = band.Score * Math.Max(1, f.Weight);
                totalWeighted += weighted;
                maxPossible += 3 * Math.Max(1, f.Weight);
                if (string.Equals(band.Rating, "High", StringComparison.OrdinalIgnoreCase)) anyHigh = true;
                sumRatingRank += RankRating(band.Rating);

                result.Items.Add(new StmTxnRiskItemResult
                {
                    FactorCode = f.FactorCode,
                    FactorName = f.FactorName,
                    FieldName = f.FieldName,
                    FieldValue = valueStr,
                    BandLabel = band.BandLabel,
                    Score = band.Score,
                    Rating = band.Rating,
                    Weight = f.Weight,
                    WeightedScore = weighted
                });
            }

            result.TotalScore = totalWeighted;
            result.MaxPossibleScore = maxPossible;
            result.FactorCount = hitCount;

            // Rating decision
            if (anyHigh)
            {
                result.RiskRating = "High";
            }
            else if (hitCount == 0)
            {
                result.RiskRating = "Low";
            }
            else
            {
                decimal avg = sumRatingRank / hitCount;
                if (avg >= 2.5m) result.RiskRating = "High";
                else if (avg >= 1.5m) result.RiskRating = "Medium";
                else result.RiskRating = "Low";
            }

            return result;
        }

        private static StmTxnRiskBandDTO PickBand(StmTxnRiskFactorDTO f, object value, string valueStr)
        {
            if (f.Bands == null) return null;
            string type = (f.FactorType ?? "").ToUpperInvariant();

            foreach (var b in f.Bands.OrderBy(x => x.SequenceNo).ThenBy(x => x.Id))
            {
                switch (type)
                {
                    case "NUMERIC":
                    {
                        if (!TryParseDecimal(value, out var v)) return null;
                        bool minOk = !b.NumericMin.HasValue || v >= b.NumericMin.Value;
                        bool maxOk = !b.NumericMax.HasValue || v <  b.NumericMax.Value;
                        if (minOk && maxOk) return b;
                        break;
                    }
                    case "ENUM":
                    {
                        if (!string.IsNullOrEmpty(b.MatchValue) &&
                            string.Equals(b.MatchValue, valueStr, StringComparison.OrdinalIgnoreCase))
                            return b;
                        break;
                    }
                    case "FLAG":
                    {
                        // value could be bool or 0/1
                        var truthy = IsTruthy(valueStr);
                        var bandWantsTrue = IsTruthy(b.MatchValue);
                        if (truthy == bandWantsTrue) return b;
                        break;
                    }
                    case "LIST":
                    {
                        if (string.Equals(b.MatchInList, "*ELSE*", StringComparison.OrdinalIgnoreCase))
                        {
                            // Fallthrough band - return only if no earlier band matched
                            return b;
                        }
                        if (!string.IsNullOrEmpty(b.MatchInList) && !string.IsNullOrEmpty(valueStr))
                        {
                            var parts = b.MatchInList.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(p => p.Trim());
                            if (parts.Any(p => string.Equals(p, valueStr, StringComparison.OrdinalIgnoreCase)))
                                return b;
                        }
                        break;
                    }
                    default:
                        if (!string.IsNullOrEmpty(b.MatchValue) &&
                            string.Equals(b.MatchValue, valueStr, StringComparison.OrdinalIgnoreCase))
                            return b;
                        break;
                }
            }
            return null;
        }

        private static bool TryParseDecimal(object v, out decimal d)
        {
            d = 0;
            if (v == null) return false;
            return decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
        }

        private static bool IsTruthy(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            s = s.Trim().ToLowerInvariant();
            return s == "1" || s == "true" || s == "yes" || s == "y" || s == "t";
        }

        private static int RankRating(string r)
        {
            if (string.Equals(r, "High",   StringComparison.OrdinalIgnoreCase)) return 3;
            if (string.Equals(r, "Medium", StringComparison.OrdinalIgnoreCase)) return 2;
            return 1;
        }

        /// <summary>
        /// Resolve the transaction field. Supports a few synthetic fields
        /// (PayerIsThirdParty, HoldingDays, PriceDeviationPct) that are derived
        /// rather than stored directly.
        /// </summary>
        public static object ReadField(StmTransactionDTO t, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) return null;
            switch (fieldName)
            {
                case "PayerIsThirdParty":
                    // Third-party payer if RemitterId is non-empty AND different from CustomerId
                    var rid = t.RemitterId?.Trim();
                    var cid = t.CustomerId?.Trim();
                    return (!string.IsNullOrEmpty(rid) && !string.Equals(rid, cid, StringComparison.OrdinalIgnoreCase));
                case "HoldingDays":
                    // Not stored on stm_transaction yet - return null so the factor is skipped.
                    return null;
                case "PriceDeviationPct":
                    // PropertyValue - Amount comparison (% deviation)
                    if (t.PropertyValue.HasValue && t.PropertyValue.Value > 0 && t.Amount > 0)
                    {
                        var deviation = (t.Amount - t.PropertyValue.Value) / t.PropertyValue.Value * 100m;
                        return deviation;
                    }
                    return null;
                default:
                    var prop = typeof(StmTransactionDTO).GetProperty(fieldName,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    return prop?.GetValue(t);
            }
        }
    }
}
