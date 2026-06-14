using System;
using System.Collections.Generic;
using System.Linq;

namespace AML.ViewModel.ViewModels.ComplianceHub
{
    // ── Hub landing ──
    public class ComplianceHubVM
    {
        public List<ComplianceHubTile> Tiles { get; set; } = new List<ComplianceHubTile>();
    }
    public class ComplianceHubTile
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Theme { get; set; }   // Lifecycle / Screening / Geography / Governance
        public string Url { get; set; }
        public string Icon { get; set; }    // lucide icon name or inline svg ref
        public string LiveStat { get; set; } // optional small stat shown on the tile
        public string LiveStatLabel { get; set; }
        public string Accent { get; set; }   // tailwind colour key (blue, rose, emerald…)
    }

    // ── 1. KYC Document Expiry Calendar ──
    public class KycExpiryVM
    {
        public List<KycExpiryRow> Rows { get; set; } = new List<KycExpiryRow>();
        public int Expired => Rows.Count(r => r.DaysUntil < 0);
        public int Within30 => Rows.Count(r => r.DaysUntil >= 0 && r.DaysUntil <= 30);
        public int Within60 => Rows.Count(r => r.DaysUntil > 30 && r.DaysUntil <= 60);
        public int Within90 => Rows.Count(r => r.DaysUntil > 60 && r.DaysUntil <= 90);
    }
    public class KycExpiryRow
    {
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string CustomerType { get; set; }
        public string DocumentType { get; set; } // Passport / Emirates ID / National ID
        public string DocumentNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int DaysUntil { get; set; }
        public string Bucket { get; set; }       // expired / 30 / 60 / 90 / future
    }

    // ── 2. Periodic Review Calendar ──
    public class PeriodicReviewVM
    {
        public List<PeriodicReviewRow> Rows { get; set; } = new List<PeriodicReviewRow>();
        public int Overdue => Rows.Count(r => r.DaysUntilReview < 0);
        public int DueIn30 => Rows.Count(r => r.DaysUntilReview >= 0 && r.DaysUntilReview <= 30);
    }
    public class PeriodicReviewRow
    {
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string RiskBand { get; set; } // High / Medium / Low / Unrated
        public DateTime LastReviewedOn { get; set; }
        public DateTime NextReviewDue { get; set; }
        public int DaysUntilReview { get; set; }
    }

    // ── 3. Customer Data Completeness Scorecard ──
    public class DataCompletenessVM
    {
        public int TotalCustomers { get; set; }
        public List<FieldCompleteness> Fields { get; set; } = new List<FieldCompleteness>();
        public List<CustomerCompletenessRow> Customers { get; set; } = new List<CustomerCompletenessRow>();
        public double OverallScore =>
            Fields.Count == 0 ? 0 : Math.Round(Fields.Average(f => f.Pct), 1);
    }
    public class FieldCompleteness
    {
        public string FieldName { get; set; }
        public string DisplayName { get; set; }
        public int Populated { get; set; }
        public int Total { get; set; }
        public double Pct => Total == 0 ? 0 : Math.Round(Populated * 100.0 / Total, 1);
    }
    public class CustomerCompletenessRow
    {
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public int FilledCount { get; set; }
        public int TotalChecked { get; set; }
        public double Pct => TotalChecked == 0 ? 0 : Math.Round(FilledCount * 100.0 / TotalChecked, 0);
        public List<string> MissingFields { get; set; } = new List<string>();
    }

    // ── 4. Onboarding Funnel ──
    public class OnboardingFunnelVM
    {
        public List<OnboardingMonth> Months { get; set; } = new List<OnboardingMonth>();
        public int TotalOnboarded => Months.Sum(m => m.Onboarded);
        public int TotalApproved => Months.Sum(m => m.Approved);
        public int TotalPending  => Months.Sum(m => m.Pending);
        public int TotalRejected => Months.Sum(m => m.Rejected);
        public double AvgDecisionDays { get; set; }
    }
    public class OnboardingMonth
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Label { get; set; }
        public int Onboarded { get; set; }
        public int Approved { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
        public int OnHold { get; set; }
    }

    // ── 5. Onboarding-Without-Screening Gap ──
    public class ScreeningGapVM
    {
        public List<ScreeningGapRow> Rows { get; set; } = new List<ScreeningGapRow>();
        public int TotalCustomers { get; set; }
        public int Unscreened => Rows.Count;
        public double UnscreenedPct =>
            TotalCustomers == 0 ? 0 : Math.Round(Unscreened * 100.0 / TotalCustomers, 1);
    }
    public class ScreeningGapRow
    {
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string CustomerType { get; set; }
        public string Nationality { get; set; }
        public DateTime CreatedOn { get; set; }
        public int DaysSinceOnboarding { get; set; }
    }

    // ── 6. Sanctions Hit Disposition Matrix ──
    public class SanctionsMatrixVM
    {
        public int TruePepDomestic { get; set; }
        public int TruePepForeign { get; set; }
        public int TrueAdverseMedia { get; set; }
        public int TrueUaeUnSanction { get; set; }
        public int TrueOtherSanction { get; set; }
        public int PartialPepDomestic { get; set; }
        public int PartialPepForeign { get; set; }
        public int PartialAdverseMedia { get; set; }
        public int TotalCases { get; set; }
        public int CasesWithAnyHit { get; set; }
        public List<SanctionMatrixCase> RecentCases { get; set; } = new List<SanctionMatrixCase>();
    }
    public class SanctionMatrixCase
    {
        public int CaseId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string HitTypes { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
    }

    // ── 7. PEP Inventory ──
    public class PepInventoryVM
    {
        public List<PepRow> Rows { get; set; } = new List<PepRow>();
        public int Domestic => Rows.Count(r => r.IsDomestic);
        public int Foreign => Rows.Count(r => r.IsForeign);
        public List<KeyValuePair<string,int>> ByJurisdiction =>
            Rows.GroupBy(r => string.IsNullOrEmpty(r.Nationality) ? "Unknown" : r.Nationality)
                .OrderByDescending(g => g.Count())
                .Select(g => new KeyValuePair<string,int>(g.Key, g.Count()))
                .Take(15)
                .ToList();
    }
    public class PepRow
    {
        public int CustomerMasterId { get; set; }
        public int CaseId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string Nationality { get; set; }
        public bool IsDomestic { get; set; }
        public bool IsForeign { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CaseStatus { get; set; }
    }

    // ── 8. Adverse Media Watch ──
    public class AdverseMediaVM
    {
        public List<AdverseMediaRow> Rows { get; set; } = new List<AdverseMediaRow>();
        public int Confirmed => Rows.Count(r => r.IsConfirmed);
        public int Partial => Rows.Count(r => !r.IsConfirmed);
        public int OverdueReview => Rows.Count(r => r.DaysSinceReview > 90);
    }
    public class AdverseMediaRow
    {
        public int CaseId { get; set; }
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string Nationality { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int DaysSinceReview { get; set; }
        public string Status { get; set; }
    }

    // ── 9. Cross-Border Customer Map ──
    public class CrossBorderMapVM
    {
        public List<CountryExposure> Countries { get; set; } = new List<CountryExposure>();
        public int TotalCustomers => Countries.Sum(c => c.NationalityCount);
        public int HighRiskExposure => Countries.Where(c => c.FatfStatus == "Black" || c.FatfStatus == "Grey").Sum(c => c.NationalityCount);
        public int DistinctCountries => Countries.Count;
    }
    public class CountryExposure
    {
        public string Code { get; set; }       // ISO-2 if known
        public string Name { get; set; }
        public int NationalityCount { get; set; }
        public int ResidenceCount { get; set; }
        public int SowCount { get; set; }
        public int Total => NationalityCount + ResidenceCount + SowCount;
        public string FatfStatus { get; set; }  // Black / Grey / null
    }

    // ── 10. Channel & Product Risk Mix ──
    public class ChannelProductMixVM
    {
        public List<ChannelMixCell> ChannelByRisk { get; set; } = new List<ChannelMixCell>();
        public List<ChannelMixCell> PaymentByRisk { get; set; } = new List<ChannelMixCell>();
        public List<ChannelMixCell> ProductByRisk { get; set; } = new List<ChannelMixCell>();
        public List<SankeyLink> Sankey { get; set; } = new List<SankeyLink>();
    }
    public class ChannelMixCell
    {
        public string Dimension { get; set; }    // delivery_channel | mode_of_payment | product_name
        public string Bucket { get; set; }
        public string RiskBand { get; set; }     // High/Medium/Low/Unrated
        public int Count { get; set; }
    }
    public class SankeyLink
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public int Value { get; set; }
    }

    // ── 11. Whitelist Governance Register ──
    public class WhitelistGovernanceVM
    {
        public List<WhitelistEvent> Events { get; set; } = new List<WhitelistEvent>();
        public int TotalEvents => Events.Count;
        public int Unjustified => Events.Count(e => string.IsNullOrWhiteSpace(e.Justification));
        public int OlderThan1Year => Events.Count(e => (DateTime.UtcNow - e.WhitelistedOn).TotalDays > 365);
    }
    public class WhitelistEvent
    {
        public int Id { get; set; }
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string CustomerType { get; set; }
        public DateTime WhitelistedOn { get; set; }
        public string WhitelistedBy { get; set; }
        public string Justification { get; set; }
        public int DaysAgo { get; set; }
    }

    // ── 12. Proliferation Finance Register ──
    public class ProliferationRegisterVM
    {
        public List<ProliferationRow> Rows { get; set; } = new List<ProliferationRow>();
        public int Total => Rows.Count;
        public int Pending => Rows.Count(r => string.Equals(r.Status, "Pending", StringComparison.OrdinalIgnoreCase));
        public int Approved => Rows.Count(r => string.Equals(r.Status, "Approved", StringComparison.OrdinalIgnoreCase));
        public int Rejected => Rows.Count(r => string.Equals(r.Status, "Rejected", StringComparison.OrdinalIgnoreCase));
        public List<KeyValuePair<string,int>> TopChemicals =>
            Rows.Where(r => !string.IsNullOrWhiteSpace(r.ChemicalName))
                .GroupBy(r => r.ChemicalName)
                .OrderByDescending(g => g.Count())
                .Select(g => new KeyValuePair<string,int>(g.Key, g.Count()))
                .Take(10)
                .ToList();
    }
    public class ProliferationRow
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string CustomerType { get; set; }
        public string HsCode { get; set; }
        public string CasNumber { get; set; }
        public string ChemicalName { get; set; }
        public string MatchedChemicalName { get; set; }
        public int? Score { get; set; }
        public string Status { get; set; }
        public string StatusReason { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
