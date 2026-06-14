using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.ViewModel.ViewModels.AmlTracker
{
    public class AmlTrackerFilterVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CustomerType { get; set; }
        public int CaseStatus { get; set; } = 10;
        public string RiskLevel { get; set; }
        public string Search { get; set; }
        public int QaStatus { get; set; } = -1; // -1 = All; 0..4 map to QaStatusCode
        public string SortBy { get; set; } = "CreatedOn";
        public string SortDir { get; set; } = "desc";
    }

    public class AmlTrackerRowVM
    {
        public int CaseId { get; set; }
        public int CustomerMasterId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string Nationality { get; set; }

        public int StatusCode { get; set; }
        public string StatusLabel { get; set; }
        public string StageLabel { get; set; }

        public int? MatchScore { get; set; }
        public int? RiskScore { get; set; }
        public string FinalRiskScore { get; set; }
        public string RiskTier { get; set; }
        public string IsWhiteListed { get; set; }

        public bool IsPep { get; set; }
        public bool IsSanction { get; set; }
        public bool IsAdverseMedia { get; set; }

        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? WhitelistedOn { get; set; }

        public int? AgingDays { get; set; }
        public string SlaStatus { get; set; }

        public string CreatedUser { get; set; }
        public string UpdatedUser { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string Source { get; set; }

        // QA / maker-checker
        public int QaStatusCode { get; set; }
        public string QaStatusLabel { get; set; }
        public string QaOutcome { get; set; }
        public int? DaysSinceQa { get; set; }
        public bool QaEligible { get; set; }

        // Days the case has been in a terminal status without an active QA decision (Approved/Rejected).
        public int? DaysClosedWithoutQa { get; set; }
        public string QaSlaStatus { get; set; }

        public string QaReasonCode { get; set; }
        public bool IsStale { get; set; }
        public bool IsNewSinceLastVisit { get; set; }

        // SAR linkage (F8)
        public string SarStatus { get; set; }
        public string SarReference { get; set; }
        public DateTime? SarFilingDeadline { get; set; }
        public DateTime? SarFiledOn { get; set; }
        public int? DaysToSarDeadline { get; set; }

        // Compliance-export fields (Bucket A — sourced from customermaster joins)
        public string ClientCifNumber { get; set; }
        public string ProductType { get; set; }
        public string ProductValueAed { get; set; }
        public string IdType { get; set; }
        public string IdNumber { get; set; }
        public DateTime? IdExpiry { get; set; }
        public bool OtherSanctionFlag { get; set; }
        public string SeniorMgmtApprovalStatus { get; set; }   // derived from StatusCode
        public string PaymentMode { get; set; }
        public string DeliveryChannel { get; set; }
        public string ResidentStatus { get; set; }
        public string Profession { get; set; }

        // Compliance-export fields (Bucket B — derived / typed comments)
        public string NewOrExisting { get; set; }              // "New" | "Existing"
        public string CommentsPep { get; set; }
        public string CommentsUaeUnsc { get; set; }
        public string CommentsAdverseMedia { get; set; }
        public string CommentsOtherSanction { get; set; }
        public string OtherComments { get; set; }
        public DateTime? RiskAssessmentDate { get; set; }

        // Compliance-export fields (Bucket C — new columns on customercase, populated post-migration)
        public DateTime? PolicyIssueDate { get; set; }
        public DateTime? DateOfResponse { get; set; }
        public string ClientProductNumber { get; set; }
        public string PolicyHolder { get; set; }
        public string KycCheck { get; set; }
        public string KycCheckComments { get; set; }
        public DateTime? SeniorMgmtApprovalDate { get; set; }
        public DateTime? SanctionsScreeningDate { get; set; }

        public string ClientStatus { get; set; }

        public string DomesticPep { get; set; }
        public string ForeginPep { get; set; }
        public string RedFlags { get; set; }
        public string HighNetwork { get; set; }
         public string UAEUNSanction { get; set; }
         public string OtherSanction { get; set; }
        public string BusinessType { get; set; }
        public string LegalStatus { get; set; }

         public string ChangeStatus { get; set; }

        public string Gender { get; set; }


        public string DOB { get; set; }

        public string PassportId { get; set; }
        public string PassportIssueDate { get; set; }
        public string PassportExpiryDate { get; set; }
        public string EmiratesIdNumber { get; set; }
        public string EmiratesIdIssueDate { get; set; }
        public string EmiratesIdExpiryDate { get; set; }

        public string Residence { get; set; }

        public string SOWSOFCountry { get; set; }

        public string Employer { get; set; }

        public string EmployerIndustry { get; set; }

        public string EmployerSector { get; set; }

        public string GoldenVisa { get; set; }

        public string CounterParty { get; set; }

        public string ProductRefNo { get; set; }

        public string ProductValue { get; set; }

        public string TradeLicenseAuthority { get; set; }

        public string TradeLicenseSector { get; set; }

        public int Share { get; set; }
        public string Designation { get; set; }

        public string CounterPartyName { get; set; }
        public string Tradelicense { get; set; }

       public string StatusName { get; set; }

        public string GroupId { get; set; }

        public string Individual_final_risk_score { get; set; }

        public string corporate_final_risk_score { get; set; }

       public string QAReviewerName { get; set; }


       public string QAReason { get; set; }


      public string QAComments { get; set; }

         public string QAStatus { get; set; }

        public DateTime? QAReviewdOn { get; set; }
    }

    public class AmlTrackerIndexVM
    {
        public AmlTrackerFilterVM Filter { get; set; } = new AmlTrackerFilterVM();
        public List<AmlTrackerRowVM> Rows { get; set; } = new List<AmlTrackerRowVM>();
        public List<UserOption> OwnerOptions { get; set; } = new List<UserOption>();

        public int TotalCount { get; set; }
        public int OpenCount { get; set; }
        public int PendingSrMgmtCount { get; set; }
        public int CompletedCount { get; set; }
        public int SlaBreachCount { get; set; }

        public int HighRiskCount { get; set; }
        public int MediumRiskCount { get; set; }
        public int LowRiskCount { get; set; }
        public int UnclassifiedRiskCount { get; set; }

        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int WhitelistedCount { get; set; }
        public int OnHoldCount { get; set; }
        public int AutoCount { get; set; }

        public int TodayCount { get; set; }
        public int Last7DaysCount { get; set; }

        public int PepHits { get; set; }
        public int SanctionHits { get; set; }
        public int AdverseMediaHits { get; set; }

        public int IndividualCount { get; set; }
        public int CorporateCount { get; set; }

        public double AvgAgingDays { get; set; }
        public int MaxAgingDays { get; set; }

        // Maker-checker / QA KPIs
        public int QaPendingCount { get; set; }
        public int QaApprovedCount { get; set; }
        public int QaRejectedCount { get; set; }
        public int QaNotReviewedCount { get; set; }
        public int QaEligibleNotReviewedCount { get; set; }
        public int QaSlaBreachCount { get; set; }
        public DateTime? LastQaActivityAt { get; set; }
        public double AvgDaysSinceQa { get; set; }
        public bool CurrentUserCanQa { get; set; }

        // Stale + new + SAR KPIs
        public int StaleCount { get; set; }
        public int NewSinceLastVisitCount { get; set; }
        public int SarRequiredCount { get; set; }
        public int SarFiledCount { get; set; }
        public int SarOverdueCount { get; set; }

        // Saved views available to current user
        public List<SavedViewVM> SavedViews { get; set; } = new List<SavedViewVM>();

        // Notifications
        public int UnreadNotificationCount { get; set; }

        // Week-over-week (S4)
        public TrendDelta TotalTrend { get; set; }
        public TrendDelta CompletedTrend { get; set; }
        public TrendDelta QaApprovedTrend { get; set; }
        public TrendDelta QaRejectedTrend { get; set; }

        // Cycle-time mini chart (F4)
        public CycleTimeChartVM CycleTime { get; set; } = new CycleTimeChartVM();

        // Optional flash message after a batch action
        public string FlashMessage { get; set; }
        public string FlashLevel { get; set; } // "success" | "error" | "info"

        public bool DataLoaded { get; set; }


    }

    public class UserOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Bound from the bulk-action POST form
    public class AmlTrackerBatchActionVM
    {
        public string Action { get; set; }     // see controller switch
        public List<int> CaseIds { get; set; } = new List<int>();
        public string Comments { get; set; }
        public string ReasonCode { get; set; }
        public int? NewOwnerId { get; set; }
        public int? SamplePercent { get; set; }
        public int? HighPercent { get; set; }
        public int? MediumPercent { get; set; }
        public int? LowPercent { get; set; }
    }

    public class SavedViewVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FilterJson { get; set; }
        public bool IsShared { get; set; }
    }

    public class SarLinkVM
    {
        public int CaseId { get; set; }
        public string SarStatus { get; set; }
        public string SarReference { get; set; }
        public string SarFilingDeadline { get; set; }
        public string FiledOn { get; set; }
        public string Notes { get; set; }
    }

    public class SlaConfigVM
    {
        public int Id { get; set; }
        public string CustomerType { get; set; } = "*";
        public string RiskTier { get; set; } = "*";
        public int CaseSlaOnTrackDays { get; set; } = 7;
        public int CaseSlaAtRiskDays { get; set; } = 14;
        public int QaSlaOnTrackDays { get; set; } = 7;
        public int QaSlaAtRiskDays { get; set; } = 14;
        public int StaleCaseDays { get; set; } = 30;
        public bool IsActive { get; set; } = true;
    }

    public class AuditLogItemVM
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public int AffectedCount { get; set; }
        public string Details { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class NotificationItemVM
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Link { get; set; }
        public int? CaseId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class WorkspaceVM
    {
        public List<AmlTrackerRowVM> Rows { get; set; } = new List<AmlTrackerRowVM>();
        public bool CurrentUserCanQa { get; set; }
        public string FlashMessage { get; set; }
        public string FlashLevel { get; set; }
    }

    public class LeaderboardVM
    {
        public int DaysBack { get; set; } = 30;
        public List<ReviewerStatsVM> Reviewers { get; set; } = new List<ReviewerStatsVM>();
    }

    public class ReviewerStatsVM
    {
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public int CasesReviewed { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public double ApprovalRate { get; set; }
        public double AvgTurnaroundDays { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    public class CycleTimeChartVM
    {
        public List<CycleTimeBucketVM> Points { get; set; } = new List<CycleTimeBucketVM>();
    }

    public class CycleTimeBucketVM
    {
        public DateTime Day { get; set; }
        public int Decisions { get; set; }
        public double AvgHours { get; set; }
    }

    public class SummaryJsonVM
    {
        public int Total { get; set; }
        public int Open { get; set; }
        public int PendingSrMgmt { get; set; }
        public int Completed { get; set; }
        public int SlaBreach { get; set; }
        public int HighRisk { get; set; }
        public int Today { get; set; }
        public double AvgAging { get; set; }
        public int QaPending { get; set; }
        public int QaApproved { get; set; }
        public int QaRejected { get; set; }
        public int QaEligibleAwaiting { get; set; }
        public int QaSlaBreach { get; set; }
        public int Stale { get; set; }
        public DateTime ServerTime { get; set; }
        public int UnreadNotifications { get; set; }
    }

    public class TrendDelta
    {
        public int Current { get; set; }
        public int Previous { get; set; }
        public int Delta => Current - Previous;
        public double DeltaPct => Previous == 0 ? (Current > 0 ? 100 : 0) : Math.Round(((double)(Current - Previous) / Previous) * 100, 1);
    }

    public class QaHistoryItemVM
    {
        public int Id { get; set; }
        public int QaStatus { get; set; }
        public string QaStatusLabel { get; set; }
        public string QaOutcome { get; set; }
        public string QaReviewerName { get; set; }
        public DateTime? QaReviewedOn { get; set; }
        public string QaComments { get; set; }
    }
}
