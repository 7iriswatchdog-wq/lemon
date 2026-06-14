using AML.Core.Common.StaticResource;
using AML.DTO.DTO.AmlTracker;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.Core.RepositoryContract.AmlTracker
{
    public interface IAmlTrackerRepository
    {
        ServiceResponse<List<TrackerCaseDTO>> GetAmlTrackerCases(int clientId, string startDate, string endDate, string custType, int statusFilter, string search);
        List<TrackerCaseDTO> GetTrackerCases(int clientId, string startDate, string endDate, string custType, int statusFilter, string search);
        List<UserOptionDTO> GetUserOptions(int clientId);

        // QA actions
        int InsertQaRecord(CaseQaDTO qa);
        int MarkForQa(IList<int> caseIds, int clientId, int userId);
        int ReassignOwner(IList<int> caseIds, int newOwnerId, int clientId, int userId, string updatedUserName);
        int ReopenCases(IList<int> caseIds, int clientId, int userId, string updatedUserName);
        ServiceResponse<List<CaseMakerInfoDTO>> GetCaseMakersAndEmails(IList<int> caseIds, int clientId);
        List<CaseQaDTO> GetQaHistory(int caseId, int clientId);

        // Sampling
        int MarkRandomSampleForQa(int percent, int clientId, int userId, out int eligibleCount, out int sampledCount);
        int MarkStratifiedSampleForQa(int highPct, int mediumPct, int lowPct, int clientId, int userId,
            out int highEligible, out int highSampled, out int medEligible, out int medSampled, out int lowEligible, out int lowSampled);

        // SLA config
        List<TrackerSlaConfigDTO> GetSlaConfigs(int clientId);
        int UpsertSlaConfig(TrackerSlaConfigDTO cfg, int userId);
        int DeleteSlaConfig(int id, int clientId);

        // Saved views
        List<TrackerSavedViewDTO> GetSavedViews(int userId, int clientId);
        int InsertSavedView(TrackerSavedViewDTO view);
        int DeleteSavedView(int id, int userId, int clientId);

        // Audit log
        int InsertAuditLog(TrackerAuditLogDTO log);
        List<TrackerAuditLogDTO> GetAuditLogs(int clientId, int limit);

        // Notifications
        int InsertNotification(TrackerNotificationDTO n);
        List<TrackerNotificationDTO> GetNotifications(int userId, bool onlyUnread, int limit);
        int CountUnreadNotifications(int userId);
        int MarkNotificationRead(int notificationId, int userId);
        int MarkAllNotificationsRead(int userId);

        // SAR links
        TrackerSarLinkDTO GetSarLink(int caseId, int clientId);
        int UpsertSarLink(TrackerSarLinkDTO link, int userId);

        // Reviewer leaderboard
        List<ReviewerStatsDTO> GetReviewerStats(int clientId, int daysBack);

        // Cycle-time chart data
        List<CycleTimePointDTO> GetQaCycleTime(int clientId, int daysBack);

        // Reports — raw rows returned via dynamic for flexibility; service layer projects to VMs
        AML.ViewModel.ViewModels.AmlTracker.Customer360VM GetCustomer360(int customerMasterId, int clientId);
        List<AML.ViewModel.ViewModels.AmlTracker.RiskOverrideRow> GetRiskOverrides(int clientId, int daysBack);
        List<dynamic> GetUserGroupRights(int clientId);
        List<AML.ViewModel.ViewModels.AmlTracker.SanctionsFreshnessRow> GetSanctionsFreshness(int clientId);
        List<AML.ViewModel.ViewModels.AmlTracker.WhitelistAnalystRow> GetWhitelistAnalystPatterns(int clientId, int daysBack);
        List<AML.ViewModel.ViewModels.AmlTracker.InvestigationDepthRow> GetInvestigationDepth(int clientId);
        List<AML.ViewModel.ViewModels.AmlTracker.TopErrorRow> GetTopErrors(int daysBack, int limit);
        List<AML.ViewModel.ViewModels.AmlTracker.QaThroughputPoint> GetQaThroughput(int clientId, int daysBack);
        List<AML.ViewModel.ViewModels.AmlTracker.SarRegisterRow> GetSarRegister(int clientId);
        List<AML.ViewModel.ViewModels.AmlTracker.FourEyesAttemptRow> GetFourEyesAttempts(int clientId, int limit);
        List<dynamic> GetWorkloadByOwner(int clientId);
        List<AML.ViewModel.ViewModels.AmlTracker.PeriodicReviewRow> GetPeriodicReviewCandidates(int clientId);

        // AI chat history persistence
        long InsertChatMessage(int clientId, int userId, string sessionId, int? caseId, string role, string messageText, string model, int? promptTokens, int? completionTokens, int? latencyMs);

        AML.ViewModel.ViewModels.AmlTracker.ChatAnalyticsVM GetChatAnalytics(int clientId, int daysBack);

        // Workload assignment write
        int InsertCaseAssignment(int caseId, int userId, string comment, int actorUserId);

        // Reports continued
        List<AML.ViewModel.ViewModels.AmlTracker.HighRiskCustomerRow> GetHighRiskCustomers(int clientId);
        AML.ViewModel.ViewModels.AmlTracker.CustomerDemographicsVM GetCustomerDemographics(int clientId);
        List<AML.ViewModel.ViewModels.AmlTracker.LookbackRow> GetLookbackCandidates(int clientId, int daysBack);
        AML.ViewModel.ViewModels.AmlTracker.AnnualMlroVM GetAnnualMlroReport(int clientId, int year);

        AML.ViewModel.ViewModels.AmlTracker.AgedCasesVM GetAgedCases(int clientId);
        AML.ViewModel.ViewModels.AmlTracker.CustomerAcceptanceVM GetCustomerAcceptance(int clientId, int daysBack);
        AML.ViewModel.ViewModels.AmlTracker.DocumentAuditVM GetDocumentAudit(int clientId);
        AML.ViewModel.ViewModels.AmlTracker.CohortComparisonVM GetCohortComparison(int customerMasterId, int clientId);
        AML.ViewModel.ViewModels.AmlTracker.QuarterTrendVM GetQuarterlyTrend(int clientId, int quartersBack);

        // R-ASSOC: Customer Associations report
        AML.ViewModel.ViewModels.AmlTracker.CustomerAssociationsVM GetCustomerAssociations(int clientId);
    }

    public class UserOptionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    
}
