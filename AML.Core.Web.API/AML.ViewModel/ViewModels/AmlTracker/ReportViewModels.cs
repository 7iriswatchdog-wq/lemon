using System;
using System.Collections.Generic;
using System.Linq;

namespace AML.ViewModel.ViewModels.AmlTracker
{
    // Customer 360
    public class Customer360VM
    {
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string CustomerType { get; set; }
        public string Nationality { get; set; }
        public DateTime? DOB { get; set; }
        public string Mobile { get; set; }
        public string Profession { get; set; }
        public string Employer { get; set; }
        public string EmployerSector { get; set; }
        public string Residence { get; set; }
        public string IsWhiteListed { get; set; }
        public DateTime? WhitelistedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public List<Customer360CaseRow> Cases { get; set; } = new List<Customer360CaseRow>();
        public List<Customer360RiskPoint> RiskHistory { get; set; } = new List<Customer360RiskPoint>();
        public List<Customer360CommentRow> Comments { get; set; } = new List<Customer360CommentRow>();
        public List<Customer360WhitelistRow> WhitelistHistory { get; set; } = new List<Customer360WhitelistRow>();
    }
    public class Customer360CaseRow
    {
        public int CaseId { get; set; }
        public string Status { get; set; }
        public int? MatchScore { get; set; }
        public int? RiskScore { get; set; }
        public string Source { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
    public class Customer360RiskPoint
    {
        public DateTime AssessmentDate { get; set; }
        public string FinalRiskScore { get; set; }
        public int? Version { get; set; }
        public string Override { get; set; }
    }
    public class Customer360CommentRow
    {
        public int CaseId { get; set; }
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
    public class Customer360WhitelistRow
    {
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public DateTime? PerformedOn { get; set; }
    }

    // Risk override audit (F1.3)
    public class RiskOverrideRow
    {
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public DateTime? AssessmentDate { get; set; }
        public string RiskBefore { get; set; }
        public string RiskAfter { get; set; }
        public string OverrideReason { get; set; }
        public string Comments { get; set; }
        public int? AssessmentVersion { get; set; }
    }

    // Permission heatmap (F1.4)
    public class PermissionHeatmapVM
    {
        public List<string> Modules { get; set; } = new List<string>();
        public List<PermissionGroupRow> Groups { get; set; } = new List<PermissionGroupRow>();
    }
    public class PermissionGroupRow
    {
        public string GroupName { get; set; }
        public Dictionary<string, int> ModuleCounts { get; set; } = new Dictionary<string, int>();
        public int TotalRights { get; set; }
    }

    // Sanctions freshness (F1.5)
    public class SanctionsFreshnessRow
    {
        public string Source { get; set; }
        public int Hits30d { get; set; }
        public DateTime? LastHitOn { get; set; }
        public int? DaysSinceLastHit { get; set; }
        public DateTime? LastListUpdate { get; set; }
    }

    // Whitelist patterns (F1.6)
    public class WhitelistAnalystRow
    {
        public string AnalystName { get; set; }
        public int Whitelistings { get; set; }
        public DateTime? LastAction { get; set; }
    }

    // Case investigation depth (F1.7)
    public class InvestigationDepthRow
    {
        public int CaseId { get; set; }
        public string CustomerName { get; set; }
        public int Status { get; set; }
        public string StatusLabel { get; set; }
        public int CommentCount { get; set; }
        public DateTime? LatestComment { get; set; }
        public string Bucket { get; set; }
    }

    // Top errors (F1.8)
    public class TopErrorRow
    {
        public string Source { get; set; }
        public string ErrorType { get; set; }
        public int Occurrences { get; set; }
        public DateTime? LastSeen { get; set; }
    }

    // QA throughput (F2.1)
    public class QaThroughputPoint
    {
        public DateTime Day { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public double AvgTurnaroundHours { get; set; }
    }
    public class QaThroughputVM
    {
        public int DaysBack { get; set; }
        public List<QaThroughputPoint> Points { get; set; } = new List<QaThroughputPoint>();
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public double AvgTurnaroundHours { get; set; }
    }

    // SLA breach trend (F2.2)
    public class SlaBreachBucketVM
    {
        public string Bucket { get; set; }
        public int Count { get; set; }
    }
    public class SlaBreachTrendVM
    {
        public List<SlaBreachBucketVM> Buckets { get; set; } = new List<SlaBreachBucketVM>();
        public int OnTrack { get; set; }
        public int AtRisk { get; set; }
        public int Breach { get; set; }
        public int Closed { get; set; }
    }

    // SAR filing register (F2.5)
    public class SarRegisterRow
    {
        public int CaseId { get; set; }
        public string CustomerName { get; set; }
        public string SarStatus { get; set; }
        public string SarReference { get; set; }
        public DateTime? FilingDeadline { get; set; }
        public DateTime? FiledOn { get; set; }
        public int? DaysToDeadline { get; set; }
        public bool Overdue { get; set; }
    }

    // 4-eyes blocked attempts (F2.6)
    public class FourEyesAttemptRow
    {
        public DateTime? CreatedOn { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string CaseIdsCsv { get; set; }
    }

    // Workload heatmap (F3.3)
    public class WorkloadCellVM
    {
        public string OwnerName { get; set; }
        public int OwnerId { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new Dictionary<string, int>();
        public int Total { get; set; }
    }
    public class WorkloadHeatmapVM
    {
        public List<string> StatusColumns { get; set; } = new List<string>();
        public List<WorkloadCellVM> Owners { get; set; } = new List<WorkloadCellVM>();
        public int Unassigned { get; set; }
    }

    // Periodic review register (F3.5)
    public class PeriodicReviewRow
    {
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string CustomerType { get; set; }
        public string RiskTier { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastTouched { get; set; }
        public int IntervalMonths { get; set; }
        public DateTime? NextReviewDue { get; set; }
        public int DaysUntilDue { get; set; }
        public string Status { get; set; }
    }

    // ── Customer Associations Report (R-ASSOC) ──
    // One AssociationGroup per main party. Shareholders / signatories /
    // related parties hang off MainParty.RelatedParties as a flat list, with
    // each row carrying its parent code so the view can rebuild the tree.
    public class CustomerAssociationsVM
    {
        public List<AssociationGroup> Groups { get; set; } = new List<AssociationGroup>();
        public int TotalGroups => Groups.Count;
        public int TotalRelatedParties => Groups.Sum(g => g.RelatedParties?.Count ?? 0);
    }

    public class AssociationGroup
    {
        public string MainPartyCode { get; set; }
        public string MainPartyName { get; set; }
        public string MainPartyType { get; set; }     // I = Individual, C = Corporate
        public string MainPartyNationality { get; set; }
        public DateTime? CreatedOn { get; set; }
        public List<AssociationRow> RelatedParties { get; set; } = new List<AssociationRow>();
    }

    public class AssociationRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FlagType { get; set; }      // shareholder / signatories / management / related_shareholder / etc.
        public string Relationship { get; set; }  // free-text e.g. Spouse / CEO
        public string ParentId { get; set; }      // immediate parent's code (null = direct child of main party)
        public string Nationality { get; set; }
    }

    // Reports hub
    public class ReportTile
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Url { get; set; }
        public string Status { get; set; } // Ready / Tier 2 / Tier 3
    }
    public class ReportsHubVM
    {
        public List<ReportTile> Tiles { get; set; } = new List<ReportTile>();
    }

    // AI Chat Analytics
    public class ChatAnalyticsVM
    {
        public int TotalMessages { get; set; }
        public int UserMessages { get; set; }
        public int AssistantMessages { get; set; }
        public int UniqueUsers { get; set; }
        public int UniqueSessions { get; set; }
        public int Last7Days { get; set; }
        public List<ChatDailyPoint> DailyVolume { get; set; } = new List<ChatDailyPoint>();
        public List<ChatUserRow> TopUsers { get; set; } = new List<ChatUserRow>();
        public List<ChatRecentRow> RecentMessages { get; set; } = new List<ChatRecentRow>();
    }
    public class ChatDailyPoint
    {
        public DateTime Day { get; set; }
        public int Messages { get; set; }
    }
    public class ChatUserRow
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Messages { get; set; }
        public int Sessions { get; set; }
        public DateTime? LastActivity { get; set; }
    }
    public class ChatRecentRow
    {
        public DateTime? CreatedOn { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Snippet { get; set; }
        public int? CaseId { get; set; }
    }

    // Daily Case Digest (R1)
    public class DailyCaseDigestVM
    {
        public DateTime Date { get; set; }
        public int OpenedToday { get; set; }
        public int ClosedToday { get; set; }
        public int InFlight { get; set; }
        public int AwaitingQa { get; set; }
        public int SlaBreaching { get; set; }
        public int HighRiskOpen { get; set; }
        public int PendingSrMgmt { get; set; }
        public List<DigestStatusBucket> ByStatus { get; set; } = new List<DigestStatusBucket>();
        public List<AmlTrackerRowVM> NewToday { get; set; } = new List<AmlTrackerRowVM>();
        public List<AmlTrackerRowVM> ClosedTodayList { get; set; } = new List<AmlTrackerRowVM>();
    }
    public class DigestStatusBucket
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    // High-Risk Customer Register (R20)
    public class HighRiskCustomerRow
    {
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string CustomerType { get; set; }
        public string Nationality { get; set; }
        public string RiskTier { get; set; }
        public string FinalRiskScore { get; set; }
        public DateTime? AssessmentDate { get; set; }
        public int OpenCases { get; set; }
        public int TotalCases { get; set; }
        public DateTime? LastTouched { get; set; }
        public string IsWhiteListed { get; set; }
    }

    // Customer Demographics (R28)
    public class DemographicBucket
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }
    public class CustomerDemographicsVM
    {
        public int Total { get; set; }
        public int IndividualCount { get; set; }
        public int CorporateCount { get; set; }
        public List<DemographicBucket> ByNationality { get; set; } = new List<DemographicBucket>();
        public List<DemographicBucket> ByProfession { get; set; } = new List<DemographicBucket>();
        public List<DemographicBucket> ByEmployerSector { get; set; } = new List<DemographicBucket>();
        public List<DemographicBucket> ByResidence { get; set; } = new List<DemographicBucket>();
        public List<DemographicBucket> ByCustomerType { get; set; } = new List<DemographicBucket>();
    }

    // Look-back Register (R39)
    public class LookbackRow
    {
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string PreviousRisk { get; set; }
        public string CurrentRisk { get; set; }
        public DateTime? AssessmentDate { get; set; }
        public int? AssessmentVersion { get; set; }
        public int RecentCases { get; set; }
        public int DaysSince { get; set; }
    }

    // Annual MLRO Report (R40)
    public class AnnualMlroVM
    {
        public int Year { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        // Case volumes
        public int CasesOpened { get; set; }
        public int CasesClosed { get; set; }
        public int CasesAutoCleared { get; set; }
        public int CasesApproved { get; set; }
        public int CasesRejected { get; set; }
        public int CasesWhitelisted { get; set; }
        public int CasesPendingAtYearEnd { get; set; }

        // QA
        public int QaApproved { get; set; }
        public int QaRejected { get; set; }
        public double QaRejectRate { get; set; }
        public double AvgQaTurnaroundHours { get; set; }

        // Risk
        public int HighRisk { get; set; }
        public int MediumRisk { get; set; }
        public int LowRisk { get; set; }
        public int RiskOverrides { get; set; }

        // Screening
        public int TotalScreeningHits { get; set; }
        public int PepHits { get; set; }
        public int SanctionHits { get; set; }
        public int AdverseMediaHits { get; set; }

        // SAR
        public int SarRequired { get; set; }
        public int SarFiled { get; set; }
        public int SarOverdue { get; set; }

        // Whitelist + customer onboarding
        public int CustomersOnboarded { get; set; }
        public int CustomersWhitelisted { get; set; }

        // Top contributors
        public List<DemographicBucket> TopJurisdictions { get; set; } = new List<DemographicBucket>();
        public List<DemographicBucket> TopAnalysts { get; set; } = new List<DemographicBucket>();
        public List<DemographicBucket> TopRejectReasons { get; set; } = new List<DemographicBucket>();
    }

    // R3 Aged cases register
    public class AgedCaseRow
    {
        public int CaseId { get; set; }
        public int CustomerMasterId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public int StatusCode { get; set; }
        public string StatusLabel { get; set; }
        public string OwnerName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int AgeDays { get; set; }
        public int DaysSinceTouch { get; set; }
        public string AgeBucket { get; set; }
    }
    public class AgedCasesVM
    {
        public List<AgedCaseRow> Rows { get; set; } = new List<AgedCaseRow>();
        public Dictionary<string, int> ByBucket { get; set; } = new Dictionary<string, int>();
    }

    // R32 New customer acceptance
    public class CustomerAcceptanceRow
    {
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string FullName { get; set; }
        public string CustomerType { get; set; }
        public string Nationality { get; set; }
        public string Profession { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string InitialRisk { get; set; }
        public int Cases { get; set; }
    }
    public class CustomerAcceptanceVM
    {
        public int DaysBack { get; set; }
        public int Total { get; set; }
        public int Individuals { get; set; }
        public int Corporates { get; set; }
        public List<CustomerAcceptanceRow> Rows { get; set; } = new List<CustomerAcceptanceRow>();
        public List<DemographicBucket> ByDay { get; set; } = new List<DemographicBucket>();
    }

    // Document audit (the missing F3.4)
    public class DocumentAuditRow
    {
        public int Id { get; set; }
        public int? CaseId { get; set; }
        public string DocumentName { get; set; }
        public string FileName { get; set; }
        public string CategoryLabel { get; set; }
        public string TypeLabel { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Remarks { get; set; }
    }
    public class DocumentAuditVM
    {
        public int Total { get; set; }
        public int Last30d { get; set; }
        public int Expired { get; set; }
        public int ExpiringIn30d { get; set; }
        public List<DocumentAuditRow> Rows { get; set; } = new List<DocumentAuditRow>();
    }

    // R19 Cohort comparison
    public class CohortMetric
    {
        public string Label { get; set; }
        public double Customer { get; set; }
        public double CohortAvg { get; set; }
        public int? Percentile { get; set; }
        public string Verdict { get; set; }
    }
    public class CohortComparisonVM
    {
        public int CustomerMasterId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerType { get; set; }
        public string Nationality { get; set; }
        public string Profession { get; set; }
        public string CohortDescription { get; set; }
        public int CohortSize { get; set; }
        public List<CohortMetric> Metrics { get; set; } = new List<CohortMetric>();
        public bool Found { get; set; }
        public string NotFoundMessage { get; set; }
    }

    // R52 Quarter-over-quarter case volume
    public class QuarterPoint
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public string Label { get; set; }
        public int Opened { get; set; }
        public int Closed { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }
    public class QuarterTrendVM
    {
        public List<QuarterPoint> Points { get; set; } = new List<QuarterPoint>();
        public int QuartersBack { get; set; }
    }
}
