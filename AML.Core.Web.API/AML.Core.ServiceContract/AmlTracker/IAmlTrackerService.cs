using AML.DTO.DTO.AmlTracker;
using AML.ViewModel.ViewModels.AmlTracker;
using System.Collections.Generic;

namespace AML.Core.ServiceContract.AmlTracker
{
    public interface IAmlTrackerService
    {
        AmlTrackerIndexVM GetTracker(AmlTrackerFilterVM filter, int userId, string userGroupName, int clientId, System.DateTime? lastVisitedAt);
        WorkspaceVM GetQaWorkspace(int userId, string userGroupName, int clientId);
        SummaryJsonVM GetSummary(int userId, int clientId);

        bool IsQaAuthorized(string userGroupName);

        // QA actions
        int MarkForQa(IList<int> caseIds, int clientId, int userId, string userName);
        QaDecisionResult RecordQaDecision(IList<int> caseIds, int qaStatus, string comments, string reasonCode, int clientId, int userId, string reviewerName);
        int Reassign(IList<int> caseIds, int newOwnerId, int clientId, int userId, string updatedUserName);
        QaSampleResult SampleForQa(int percent, int clientId, int userId, string userName);
        StratifiedSampleResult SampleStratifiedForQa(int highPct, int medPct, int lowPct, int clientId, int userId, string userName);
        List<QaHistoryItemVM> GetQaHistory(int caseId, int clientId);

        // SLA config
        List<SlaConfigVM> GetSlaConfigs(int clientId);
        int UpsertSlaConfig(SlaConfigVM cfg, int clientId, int userId);
        int DeleteSlaConfig(int id, int clientId);

        // Saved views
        List<SavedViewVM> GetSavedViews(int userId, int clientId);
        int InsertSavedView(string name, string filterJson, bool isShared, int userId, int clientId);
        int DeleteSavedView(int id, int userId, int clientId);

        // Audit log
        List<AuditLogItemVM> GetAuditLogs(int clientId, int limit);

        // Notifications
        List<NotificationItemVM> GetNotifications(int userId, bool onlyUnread, int limit);
        int CountUnreadNotifications(int userId);
        int MarkNotificationRead(int notificationId, int userId);
        int MarkAllNotificationsRead(int userId);

        // SAR
        SarLinkVM GetSarLink(int caseId, int clientId);
        int UpsertSarLink(SarLinkVM model, int userId);

        // Leaderboard + cycle time
        LeaderboardVM GetReviewerLeaderboard(int clientId, int daysBack);

        // Reports
        Customer360VM GetCustomer360(int customerMasterId, int clientId);
        List<RiskOverrideRow> GetRiskOverrides(int clientId, int daysBack);
        PermissionHeatmapVM GetPermissionHeatmap(int clientId);
        List<SanctionsFreshnessRow> GetSanctionsFreshness(int clientId);
        List<WhitelistAnalystRow> GetWhitelistPatterns(int clientId, int daysBack);
        List<InvestigationDepthRow> GetInvestigationDepth(int clientId);
        List<TopErrorRow> GetTopErrors(int daysBack, int limit);
        QaThroughputVM GetQaThroughputReport(int clientId, int daysBack);
        SlaBreachTrendVM GetSlaBreachTrend(int clientId);
        List<SarRegisterRow> GetSarRegister(int clientId);
        List<FourEyesAttemptRow> GetFourEyesAttempts(int clientId, int limit);
        WorkloadHeatmapVM GetWorkloadHeatmap(int clientId);
        List<PeriodicReviewRow> GetPeriodicReviewCandidates(int clientId);

        // AI chat persistence + analytics
        long PersistChatMessage(int clientId, int userId, string sessionId, int? caseId, string role, string messageText, string model, int? promptTokens, int? completionTokens, int? latencyMs);
        ChatAnalyticsVM GetChatAnalytics(int clientId, int daysBack);

        // Daily case digest (R1)
        DailyCaseDigestVM GetDailyDigest(int userId, string userGroupName, int clientId);

        // R20, R28, R39, R40
        List<HighRiskCustomerRow> GetHighRiskCustomers(int clientId);
        CustomerDemographicsVM GetCustomerDemographics(int clientId);
        List<LookbackRow> GetLookbackCandidates(int clientId, int daysBack);
        AnnualMlroVM GetAnnualMlroReport(int clientId, int year);

        // R3, R32, R19, R52, Document Audit
        AgedCasesVM GetAgedCases(int clientId);
        CustomerAcceptanceVM GetCustomerAcceptance(int clientId, int daysBack);
        DocumentAuditVM GetDocumentAudit(int clientId);
        CohortComparisonVM GetCohortComparison(int customerMasterId, int clientId);
        QuarterTrendVM GetQuarterlyTrend(int clientId, int quartersBack);

        // R-ASSOC
        CustomerAssociationsVM GetCustomerAssociations(int clientId);
    }

    public class QaDecisionResult
    {
        public int RecordedCount { get; set; }
        public int BlockedByFourEyesCount { get; set; }
        public int ReopenedCount { get; set; }
        public int EmailsSent { get; set; }
        public int EmailsFailed { get; set; }
        public List<int> BlockedCaseIds { get; set; } = new List<int>();
    }

    public class QaSampleResult
    {
        public int EligibleCount { get; set; }
        public int SampledCount { get; set; }
        public int InsertedCount { get; set; }
    }

    public class StratifiedSampleResult
    {
        public int HighEligible { get; set; }
        public int HighSampled { get; set; }
        public int MediumEligible { get; set; }
        public int MediumSampled { get; set; }
        public int LowEligible { get; set; }
        public int LowSampled { get; set; }
        public int TotalSampled => HighSampled + MediumSampled + LowSampled;
    }
}
