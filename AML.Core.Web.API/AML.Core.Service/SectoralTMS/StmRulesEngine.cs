using AML.DTO.DTO.SectoralTMS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AML.Core.Service.SectoralTMS
{
    /// <summary>
    /// Context the rules engine needs beyond the single transaction being evaluated:
    /// the customer's recent transaction history (which INCLUDES the current
    /// transaction) for Count/Sum aggregation and sequence rules, plus the "as of"
    /// date used to size time windows. When null, history-based conditions degrade
    /// gracefully to "current transaction only" (used by the standalone rule tester).
    /// </summary>
    public class StmRuleEvalContext
    {
        public List<StmTransactionDTO> CustomerHistory { get; set; } = new List<StmTransactionDTO>();
        public DateTime AsOf { get; set; }
    }

    /// <summary>
    /// Dynamic rules engine for STM. Evaluates a rule's conditions against a transaction.
    /// Conditions are joined using their conjunction (AND/OR) processed left-to-right with the
    /// rule-level logical_operator as a fallback for the overall combination.
    ///
    /// Field values are resolved honestly: direct transaction columns by reflection,
    /// derived fields (PolicyAgeMonths, PreviousOwnershipDays, PriceDeviationPct) computed
    /// from real inputs, and party-derived fields read from the parties list. When the
    /// underlying data is absent the field resolves to null and the condition MISSes -
    /// the engine never substitutes an unrelated field (e.g. Amount) for a missing one.
    /// </summary>
    public static class StmRulesEngine
    {
        private static readonly HashSet<string> HighRiskCountries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "IR","KP","SY","CU","MM","AF","YE","SO","SD","IQ","LY","VE","NI","BY"
            // ISO-2 codes; client-overridable via config or stm_high_risk_country table in future
        };

        /// <summary>
        /// Evaluates a rule without history context. Used by the standalone rule tester.
        /// Aggregation/sequence conditions fall back to "current transaction only".
        /// </summary>
        public static (bool IsHit, List<string> Trace) Evaluate(StmRuleDTO rule, StmTransactionDTO tran)
            => Evaluate(rule, tran, null);

        /// <summary>
        /// Evaluates the rule against the transaction. Returns hit/no-hit + per-condition trace.
        /// </summary>
        public static (bool IsHit, List<string> Trace) Evaluate(StmRuleDTO rule, StmTransactionDTO tran, StmRuleEvalContext ctx)
        {
            var trace = new List<string>();
            if (rule == null || rule.Conditions == null || rule.Conditions.Count == 0)
            {
                trace.Add("No conditions defined - rule skipped");
                return (false, trace);
            }

            bool? overall = null;
            string previousConjunction = "AND";

            foreach (var cond in rule.Conditions.OrderBy(c => c.SequenceNo))
            {
                bool result = EvaluateCondition(cond, tran, ctx, out string reason);
                var aggLabel = string.IsNullOrWhiteSpace(cond.FieldAggregation) ? "" : cond.FieldAggregation + " ";
                trace.Add($"[Seq {cond.SequenceNo}] {aggLabel}{cond.FieldName} {cond.Operator} {cond.CompareValue} -> {(result ? "HIT" : "MISS")} ({reason})");

                if (!overall.HasValue)
                {
                    overall = result;
                }
                else
                {
                    if (string.Equals(previousConjunction, "OR", StringComparison.OrdinalIgnoreCase))
                        overall = overall.Value || result;
                    else
                        overall = overall.Value && result;
                }

                previousConjunction = string.IsNullOrEmpty(cond.Conjunction) ? rule.LogicalOperator ?? "AND" : cond.Conjunction;
            }

            return (overall ?? false, trace);
        }

        private static bool EvaluateCondition(StmRuleConditionDTO cond, StmTransactionDTO tran, StmRuleEvalContext ctx, out string reason)
        {
            reason = string.Empty;
            string op = (cond.Operator ?? "").ToLowerInvariant();

            // ---- Aggregation conditions (Count / Sum over the customer's history within a timeframe) ----
            if (!string.IsNullOrWhiteSpace(cond.FieldAggregation) &&
                !string.Equals(cond.FieldAggregation, "None", StringComparison.OrdinalIgnoreCase))
            {
                decimal aggregate = ComputeAggregate(cond, tran, ctx);
                bool aggHit = CompareDecimal(aggregate, cond.CompareValue, op);
                reason = $"{cond.FieldAggregation}={aggregate} {op} {cond.CompareValue}";
                return aggHit;
            }

            // ---- Sequence conditions (a related transaction exists before/after, within a timeframe) ----
            if (string.Equals(cond.FieldName, "SubsequentTranType", StringComparison.OrdinalIgnoreCase))
                return EvaluateSequence(cond, tran, ctx, forward: true, out reason);
            if (string.Equals(cond.FieldName, "PriorTranType", StringComparison.OrdinalIgnoreCase))
                return EvaluateSequence(cond, tran, ctx, forward: false, out reason);

            object actual = ResolveField(tran, cond.FieldName, ctx);

            try
            {
                switch (op)
                {
                    case "equals":
                    case "eq":
                        reason = $"actual='{actual}' expected='{cond.CompareValue}'";
                        return string.Equals(actual?.ToString(), cond.CompareValue, StringComparison.OrdinalIgnoreCase);

                    case "not_equals":
                    case "neq":
                        reason = $"actual='{actual}' expected_not='{cond.CompareValue}'";
                        return !string.Equals(actual?.ToString(), cond.CompareValue, StringComparison.OrdinalIgnoreCase);

                    case "not_equals_field":
                        var otherFieldVal = ResolveField(tran, cond.CompareField, ctx)?.ToString();
                        var leftVal = actual?.ToString();
                        reason = $"left='{leftVal}' right='{otherFieldVal}'";
                        // A meaningful "different" requires BOTH sides to be present. A blank
                        // remitter/nominee is "not supplied", not "a different party", so MISS.
                        if (string.IsNullOrWhiteSpace(leftVal) || string.IsNullOrWhiteSpace(otherFieldVal))
                            return false;
                        return !string.Equals(leftVal, otherFieldVal, StringComparison.OrdinalIgnoreCase);

                    case "equals_field":
                        var otherEq = ResolveField(tran, cond.CompareField, ctx)?.ToString();
                        var leftEq = actual?.ToString();
                        reason = $"left='{leftEq}' right='{otherEq}'";
                        if (string.IsNullOrWhiteSpace(leftEq) || string.IsNullOrWhiteSpace(otherEq))
                            return false;
                        return string.Equals(leftEq, otherEq, StringComparison.OrdinalIgnoreCase);

                    case "gt":
                    case "greater_than":
                    {
                        var c = CompareNumeric(actual, cond.CompareValue, out reason);
                        return c.HasValue && c.Value > 0;
                    }

                    case "gte":
                    case "greater_than_or_equal":
                    {
                        var c = CompareNumeric(actual, cond.CompareValue, out reason);
                        return c.HasValue && c.Value >= 0;
                    }

                    case "lt":
                    case "less_than":
                    {
                        var c = CompareNumeric(actual, cond.CompareValue, out reason);
                        return c.HasValue && c.Value < 0;
                    }

                    case "lte":
                    case "less_than_or_equal":
                    {
                        var c = CompareNumeric(actual, cond.CompareValue, out reason);
                        return c.HasValue && c.Value <= 0;
                    }

                    case "contains":
                        reason = $"actual='{actual}'";
                        return (actual?.ToString() ?? "").IndexOf(cond.CompareValue ?? "", StringComparison.OrdinalIgnoreCase) >= 0;

                    case "not_contains":
                    case "does_not_contain":
                        reason = $"actual='{actual}'";
                        return (actual?.ToString() ?? "").IndexOf(cond.CompareValue ?? "", StringComparison.OrdinalIgnoreCase) < 0;

                    case "in":
                    {
                        var parts = (cond.CompareValue ?? "").Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(p => p.Trim());
                        reason = $"actual='{actual}'";
                        return parts.Any(p => string.Equals(p, actual?.ToString(), StringComparison.OrdinalIgnoreCase));
                    }

                    case "not_in":
                    {
                        var parts = (cond.CompareValue ?? "").Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(p => p.Trim()).ToList();
                        reason = $"actual='{actual}'";
                        return !parts.Any(p => string.Equals(p, actual?.ToString(), StringComparison.OrdinalIgnoreCase));
                    }

                    case "in_high_risk_list":
                        reason = $"country='{actual}'";
                        return actual != null && !string.IsNullOrWhiteSpace(actual.ToString())
                               && HighRiskCountries.Contains(actual.ToString());

                    case "between":
                    {
                        var bounds = (cond.CompareValue ?? "").Split(new[] { ',', '-' }, StringSplitOptions.RemoveEmptyEntries);
                        if (bounds.Length == 2 && decimal.TryParse(bounds[0], out var lo) && decimal.TryParse(bounds[1], out var hi)
                            && decimal.TryParse(actual?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                        {
                            reason = $"value={v} bounds={lo}..{hi}";
                            return v >= lo && v <= hi;
                        }
                        reason = "between: bad format";
                        return false;
                    }

                    default:
                        reason = $"unsupported operator '{cond.Operator}'";
                        return false;
                }
            }
            catch (Exception ex)
            {
                reason = $"eval-error: {ex.Message}";
                return false;
            }
        }

        // ---- Aggregation over the customer's transaction history ----
        // Counts/sums the customer's transactions inside the timeframe window whose value of
        // the aggregated field matches the CURRENT transaction's value (so "Count TranType >= 3
        // in 90 days" counts transactions of the same type as the one being submitted).
        private static decimal ComputeAggregate(StmRuleConditionDTO cond, StmTransactionDTO tran, StmRuleEvalContext ctx)
        {
            var history = (ctx?.CustomerHistory != null && ctx.CustomerHistory.Count > 0)
                ? ctx.CustomerHistory
                : new List<StmTransactionDTO> { tran };

            DateTime asOf = (ctx != null && ctx.AsOf != default(DateTime)) ? ctx.AsOf : tran.TranDate;
            DateTime windowStart = WindowStart(asOf, cond.TimeframeValue, cond.TimeframeUnit);

            var inWindow = history.Where(h => h.TranDate >= windowStart && h.TranDate <= asOf);

            // Restrict to rows matching the current transaction's value of the field (if resolvable).
            var currentVal = ResolveField(tran, cond.FieldName, ctx)?.ToString();
            if (!string.IsNullOrWhiteSpace(currentVal))
            {
                inWindow = inWindow.Where(h =>
                    string.Equals(ResolveField(h, cond.FieldName, null)?.ToString(), currentVal, StringComparison.OrdinalIgnoreCase));
            }

            if (string.Equals(cond.FieldAggregation, "Sum", StringComparison.OrdinalIgnoreCase))
                return inWindow.Sum(h => h.Amount);

            // Default aggregation is Count.
            return inWindow.Count();
        }

        // ---- Sequence: does a related transaction (by TranType) exist within the timeframe? ----
        // forward=true  -> a SUBSEQUENT transaction after the current one (current + window)
        // forward=false -> a PRIOR transaction before the current one (current - window)
        private static bool EvaluateSequence(StmRuleConditionDTO cond, StmTransactionDTO tran, StmRuleEvalContext ctx,
                                             bool forward, out string reason)
        {
            var history = ctx?.CustomerHistory ?? new List<StmTransactionDTO>();
            DateTime anchor = tran.TranDate;
            int n = cond.TimeframeValue ?? 0;

            DateTime lo, hi;
            if (forward) { lo = anchor; hi = WindowEnd(anchor, n, cond.TimeframeUnit); }
            else { lo = WindowStart(anchor, n, cond.TimeframeUnit); hi = anchor; }

            bool exists = history.Any(h =>
                h.Id != tran.Id &&
                h.TranDate >= lo && h.TranDate <= hi &&
                string.Equals(h.TranType, cond.CompareValue, StringComparison.OrdinalIgnoreCase));

            reason = $"{(forward ? "subsequent" : "prior")} '{cond.CompareValue}' within {n} {cond.TimeframeUnit} -> {(exists ? "found" : "none")}";
            return exists;
        }

        private static DateTime WindowStart(DateTime asOf, int? value, string unit)
        {
            if (!value.HasValue || value.Value <= 0) return DateTime.MinValue;
            return Shift(asOf, -value.Value, unit);
        }

        private static DateTime WindowEnd(DateTime asOf, int value, string unit)
        {
            if (value <= 0) return DateTime.MaxValue;
            return Shift(asOf, value, unit);
        }

        private static DateTime Shift(DateTime from, int delta, string unit)
        {
            switch ((unit ?? "Day").Trim().ToLowerInvariant())
            {
                case "week": return from.AddDays(7 * delta);
                case "month": return from.AddMonths(delta);
                case "year": return from.AddYears(delta);
                case "day":
                default: return from.AddDays(delta);
            }
        }

        private static bool CompareDecimal(decimal actual, string compareValue, string op)
        {
            if (!decimal.TryParse(compareValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var b))
                return false;
            switch (op)
            {
                case "gt": case "greater_than": return actual > b;
                case "gte": case "greater_than_or_equal": return actual >= b;
                case "lt": case "less_than": return actual < b;
                case "lte": case "less_than_or_equal": return actual <= b;
                case "equals": case "eq": return actual == b;
                case "not_equals": case "neq": return actual != b;
                default: return false;
            }
        }

        // Returns the sign of (actual - compareValue), or null when either side is not a usable
        // number. Callers treat null as "condition not satisfied" so a missing/derived-null field
        // never satisfies a gt/gte/lt/lte check (e.g. a missing PolicyAgeMonths must NOT pass
        // "lte 12", and a non-numeric 'Surrender' must NOT pass "gte 3").
        private static int? CompareNumeric(object actual, string compareValue, out string reason)
        {
            if (decimal.TryParse(actual?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var a) &&
                decimal.TryParse(compareValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var b))
            {
                reason = $"actual={a} compare={b}";
                return a.CompareTo(b);
            }
            reason = $"non-numeric actual='{actual}' compare='{compareValue}'";
            return null;
        }

        /// <summary>
        /// Resolves a field name (case-insensitive) to a value on the transaction. Handles
        /// direct columns, derived numeric fields, and party-derived fields. Returns null
        /// when the source data is absent - callers treat null as a MISS.
        /// </summary>
        public static object ResolveField(StmTransactionDTO tran, string fieldName, StmRuleEvalContext ctx = null)
        {
            if (tran == null || string.IsNullOrWhiteSpace(fieldName)) return null;

            switch (fieldName.Trim())
            {
                // ---- Derived numeric fields (null when the underlying input is missing) ----
                case "PolicyAgeMonths":
                    return tran.PolicyInceptionDate.HasValue
                        ? (object)MonthsBetween(tran.PolicyInceptionDate.Value, tran.TranDate)
                        : null;

                case "PreviousOwnershipDays":
                    return tran.PropertyPurchaseDate.HasValue
                        ? (object)(decimal)Math.Floor((tran.TranDate - tran.PropertyPurchaseDate.Value).TotalDays)
                        : null;

                case "PriceDeviationPct":
                    if (tran.PropertyValue.HasValue && tran.PropertyValue.Value > 0 && tran.Amount > 0)
                        return (tran.Amount - tran.PropertyValue.Value) / tran.PropertyValue.Value * 100m;
                    return null;

                // ---- Party / customer derived fields ----
                case "CustomerNationality":
                    if (!string.IsNullOrWhiteSpace(tran.CustomerNationality)) return tran.CustomerNationality;
                    return PrimaryPartyNationality(tran);

                case "NomineeNationality":
                    return PartyNationality(tran, "Nominee");

                case "HasPEP":
                    return (tran.HasPep != 0) ? "true" : "false";
            }

            string normalized = AliasMap.TryGetValue(fieldName, out var mapped) ? mapped : fieldName;

            var prop = typeof(StmTransactionDTO).GetProperty(normalized,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(tran);
        }

        private static decimal MonthsBetween(DateTime from, DateTime to)
        {
            int months = ((to.Year - from.Year) * 12) + to.Month - from.Month;
            if (to.Day < from.Day) months--;
            return months < 0 ? 0 : months;
        }

        private static string PrimaryPartyNationality(StmTransactionDTO tran)
        {
            if (tran.Parties == null || tran.Parties.Count == 0) return null;
            // Prefer the party matching the primary customer; else the first party with a nationality.
            var match = tran.Parties.FirstOrDefault(p =>
                            !string.IsNullOrWhiteSpace(p.CustomerId) &&
                            string.Equals(p.CustomerId, tran.CustomerId, StringComparison.OrdinalIgnoreCase))
                        ?? tran.Parties.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Nationality));
            return match?.Nationality;
        }

        private static string PartyNationality(StmTransactionDTO tran, string role)
        {
            if (tran.Parties == null) return null;
            var match = tran.Parties.FirstOrDefault(p =>
                string.Equals(p.PartyRole, role, StringComparison.OrdinalIgnoreCase));
            return match?.Nationality;
        }

        // Aliases mapping rule-schema field names to real DTO property names. Only legitimate
        // 1:1 aliases live here - derived/party fields are handled in ResolveField above.
        private static readonly Dictionary<string, string> AliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"Amount","Amount"},
            {"TranType","TranType"},
            {"TranMode","TranMode"},
            {"Country","BeneficiaryCountry"},
            {"BeneficiaryCountry","BeneficiaryCountry"},
            {"RemitterCountry","RemitterCountry"},
            {"BeneficiaryName","BeneficiaryName"},
            {"RemitterId","RemitterId"},
            {"CustomerId","CustomerId"},
            {"CustomerType","CustomerType"},
            {"Currency","Currency"},
            {"PolicyNo","PolicyNo"},
            {"PropertyValue","PropertyValue"},
            {"DeliveryChannel","DeliveryChannel"},
            {"Product","Product"}
        };
    }
}
