using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.AmlTracker;
using AML.DTO.DTO.AmlTracker;
using AML.DTO.DTO.Country;
using AML.ViewModel.ViewModels.AmlTracker;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace AML.Core.Repository.AmlTracker
{
    public class AmlTrackerRepository : BaseRepository, IAmlTrackerRepository
    {
        public AmlTrackerRepository(IConfiguration configuration, IHttpContextAccessor context)
            : base(configuration, context) { }


        public ServiceResponse<List<TrackerCaseDTO>> GetAmlTrackerCases(int clientId, string startDate, string endDate, string custType, int statusFilter, string search)
        {
            ServiceResponse<List<TrackerCaseDTO>> serviceResponse = new ServiceResponse<List<TrackerCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                parameters.Add("@c_from", startDate);
                parameters.Add("@c_to", endDate);
                parameters.Add("@p_custtype", custType);
                parameters.Add("@p_statusfilter", statusFilter);
                parameters.Add("@p_search", search);
                serviceResponse.Result = Get<TrackerCaseDTO>("get_all_amltracker_cases", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Tracker Cases details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public List<TrackerCaseDTO> GetTrackerCases(int clientId, string startDate, string endDate, string custType, int statusFilter, string search)
        {
            const string sql = @"
SELECT
    ca.id                                     AS Id,
    ca.cust_master_id                         AS CustomerMasterId,
    cm.cust_ref_id                            AS CustomerCode,
    cm.fname                                  AS FirstName,
    cm.mname                                  AS MiddleName,
    cm.lname                                  AS LastName,
    cm.cust_type                              AS CustomerType,
    cm.nationality                            AS Nationality,
    cm.whitelisted_for_screening              AS IsWhiteListed,
    cm.dateofwhitelisting                     AS DateOfWhitelisting,
    ca.status                                 AS Status,
    ca.match_score                            AS MatchScore,
    ca.risk_score                             AS RiskScore,
    ca.created_on                             AS CreatedOn,
    ca.updated_on                             AS UpdatedOn,
    ca.source                                 AS Source,
    ca.owner                                  AS Owner,
    ca.created_by                             AS CreatedBy,
    (SELECT CONCAT(IFNULL(u.fname,''),' ',IFNULL(u.lname,'')) FROM user u WHERE u.id = ca.created_by) AS CreatedUser,
    (SELECT CONCAT(IFNULL(u.fname,''),' ',IFNULL(u.lname,'')) FROM user u WHERE u.id = ca.updated_by) AS UpdatedUser,
    (SELECT CONCAT(IFNULL(u.fname,''),' ',IFNULL(u.lname,'')) FROM user u WHERE u.id = ca.owner)      AS OwnerName,
    ca.true_dometic_pep                       AS TrueDomesticPep,
    ca.true_foreign_pep                       AS TrueForeignPep,
    ca.true_adversemedia                      AS TrueAdverseMedia,
    ca.partial_domestic_pep                   AS PartialDomesticPep,
    ca.partial_foreign_pep                    AS PartialForeignPep,
    ca.partial_adversemedia                   AS PartialAdverseMedia,
    ca.true_uae_un_sanction                   AS TrueUaeUnSanction,
    ca.true_other_sanction                    AS TrueOtherSanction,
    (SELECT tri.final_risk_score FROM transaction_risk_individual tri
        WHERE tri.customercode = ca.cust_id AND tri.IsDeleted = 0 AND tri.assessment_version >= 2
        ORDER BY tri.dateofassessment DESC, tri.id DESC LIMIT 1) AS IndividualRiskScore,
    (SELECT trc.final_risk_score FROM transaction_risk_corporate trc
        WHERE trc.customercode = ca.cust_id AND trc.IsDeleted = 0 AND trc.assessment_version >= 2
        ORDER BY trc.dateofassessment DESC, trc.id DESC LIMIT 1) AS CorporateRiskScore,
    qa.id                                     AS QaId,
    qa.qa_status                              AS QaStatus,
    qa.qa_outcome                             AS QaOutcome,
    qa.qa_reason_code                         AS QaReasonCode,
    qa.qa_reviewer_id                         AS QaReviewerId,
    qa.qa_reviewer_name                       AS QaReviewerName,
    qa.qa_reviewed_on                         AS QaReviewedOn,
    qa.qa_comments                            AS QaComments,
    sl.sar_status                             AS SarStatus,
    sl.sar_reference                          AS SarReference,
    sl.sar_filing_deadline                    AS SarFilingDeadline,
    sl.filed_on                               AS SarFiledOn,

    -- Compliance-export: customermaster columns (Bucket A — verified to exist on customermaster)
    cm.created_on                             AS CustomerCreatedOn,
    cm.cif_number                             AS ClientCifNumber,
    cm.product_name                           AS ProductType,
    cm.cust_id_type                           AS IdType,
    cm.cust_id_number                         AS IdNumber,
    cm.id_expiry_date                         AS IdExpiry,
    cm.mode_of_payment                        AS PaymentMode,
    cm.delivery_channel                       AS DeliveryChannel,
    cm.residence_status                       AS ResidentStatus,
    cm.profession                             AS Profession,

    -- product_value lives on the risk-assessment tables, not customermaster.
    -- Returns NULL until we add a per-case product-value field or join risk tables.
    NULL                                      AS ProductValueAed,

    -- Latest typed comment per category. The case-comment table is accessed only via
    -- stored procedures in this codebase (table name varies per environment), so we
    -- can't safely subquery it directly. Returns NULL until we wire a stored-proc lookup
    -- or the actual table name is confirmed for every target environment.
    NULL                                      AS CommentsPep,
    NULL                                      AS CommentsUaeUnsc,
    NULL                                      AS CommentsAdverseMedia,
    NULL                                      AS CommentsOtherSanction,
    NULL                                      AS CommentsGeneral,

    -- Most recent risk assessment date (Bucket B; corporate falls back to individual).
    -- Existing tracker SQL already uses these tables for risk-score lookups, so they're safe.
    COALESCE(
        (SELECT tri.dateofassessment FROM transaction_risk_individual tri
            WHERE tri.customercode = ca.cust_id AND tri.IsDeleted = 0
            ORDER BY tri.dateofassessment DESC, tri.id DESC LIMIT 1),
        (SELECT trc.dateofassessment FROM transaction_risk_corporate trc
            WHERE trc.customercode = ca.cust_id AND trc.IsDeleted = 0
            ORDER BY trc.dateofassessment DESC, trc.id DESC LIMIT 1)
    )                                         AS RiskAssessmentDate,

    -- Bucket C — added by Db/Migrations/2026-05-02_aml_tracker_compliance_columns.sql.
    -- Until you run that migration, these stay NULL. After the migration, swap each
    -- NULL for the matching ca.<column_name> reference (commented out below).
    NULL                                      AS PolicyIssueDate,         -- ca.policy_issue_date
    NULL                                      AS DateOfResponse,          -- ca.date_of_response
    NULL                                      AS ClientProductNumber,     -- ca.client_product_number
    NULL                                      AS PolicyHolder,            -- ca.policy_holder
    NULL                                      AS KycCheck,                -- ca.kyc_check
    NULL                                      AS KycCheckComments,        -- ca.kyc_check_comments
    NULL                                      AS SeniorMgmtApprovalDate,  -- ca.senior_mgmt_approval_date
    NULL                                      AS SanctionsScreeningDate   -- ca.sanctions_screening_date
FROM customercase ca
INNER JOIN customermaster cm ON cm.id = ca.cust_master_id
LEFT JOIN customercase_qa qa
    ON qa.case_id = ca.id
   AND qa.is_active = 1
   AND qa.id = (SELECT MAX(q2.id) FROM customercase_qa q2 WHERE q2.case_id = ca.id AND q2.is_active = 1)
LEFT JOIN tracker_sar_link sl
    ON sl.case_id = ca.id
   AND sl.is_active = 1
   AND sl.id = (SELECT MAX(s2.id) FROM tracker_sar_link s2 WHERE s2.case_id = ca.id AND s2.is_active = 1)
WHERE ca.is_deleted = 0
  AND ca.Client_Id = @clientId
  AND (@startDate IS NULL OR ca.created_on >= @startDate)
  AND (@endDate   IS NULL OR ca.created_on <  DATE_ADD(@endDate, INTERVAL 1 DAY))
  AND (@custType  = '0' OR @custType = '' OR cm.cust_type = @custType)
  AND (@statusFilter = 10 OR ca.status = @statusFilter)
  AND (@search = '' OR cm.fname LIKE CONCAT('%', @search, '%')
                    OR cm.lname LIKE CONCAT('%', @search, '%')
                    OR cm.cust_ref_id LIKE CONCAT('%', @search, '%')
                    OR ca.cust_id LIKE CONCAT('%', @search, '%'))
AND cm.version = (
        SELECT MAX(cu2.version)
        FROM customermaster cu2
        WHERE cu2.cust_ref_id = cm.cust_ref_id
  )
ORDER BY ca.id DESC
LIMIT 5000;";

            var p = new DynamicParameters();
            p.Add("@clientId", clientId);
            p.Add("@startDate", ParseDateOrNull(startDate));
            p.Add("@endDate", ParseDateOrNull(endDate));
            p.Add("@custType", custType ?? "0");
            p.Add("@statusFilter", statusFilter);
            p.Add("@search", (search ?? "").Trim());

            return Get<TrackerCaseDTO>(sql, p, commandType: CommandType.Text).ToList();
        }

        public int InsertQaRecord(CaseQaDTO qa)
        {
            const string sql = @"
UPDATE customercase_qa SET is_active = 0
 WHERE case_id = @CaseId AND client_id = @ClientId AND is_active = 1;

INSERT INTO customercase_qa
    (case_id, client_id, qa_status, qa_outcome, qa_reason_code, qa_reviewer_id, qa_reviewer_name,
     qa_reviewed_on, qa_comments, is_active, created_by, created_on, updated_by, updated_on)
VALUES
    (@CaseId, @ClientId, @QaStatus, @QaOutcome, @QaReasonCode, @QaReviewerId, @QaReviewerName,
     CASE WHEN @QaStatus IN (2,3,4) THEN NOW() ELSE NULL END,
     @QaComments, 1, @CreatedBy, NOW(), @CreatedBy, NOW());";
            return Execute(sql, qa, commandType: CommandType.Text);
        }

        public int MarkForQa(IList<int> caseIds, int clientId, int userId)
        {
            if (caseIds == null || caseIds.Count == 0) return 0;
            const string sql = @"
UPDATE customercase_qa SET is_active = 0
 WHERE case_id IN @CaseIds AND client_id = @ClientId AND is_active = 1;

INSERT INTO customercase_qa
    (case_id, client_id, qa_status, is_active, created_by, created_on, updated_by, updated_on)
SELECT id, @ClientId, 1, 1, @UserId, NOW(), @UserId, NOW()
  FROM customercase
 WHERE id IN @CaseIds AND Client_Id = @ClientId AND is_deleted = 0;";
            return Execute(sql, new { CaseIds = caseIds, ClientId = clientId, UserId = userId }, commandType: CommandType.Text);
        }

        public int ReassignOwner(IList<int> caseIds, int newOwnerId, int clientId, int userId, string updatedUserName)
        {
            if (caseIds == null || caseIds.Count == 0) return 0;
            const string sql = @"
UPDATE customercase
   SET owner = @NewOwnerId, updated_by = @UserId, updated_user = @UpdatedUserName, updated_on = NOW()
 WHERE id IN @CaseIds AND Client_Id = @ClientId AND is_deleted = 0;";
            return Execute(sql, new { CaseIds = caseIds, NewOwnerId = newOwnerId, ClientId = clientId, UserId = userId, UpdatedUserName = updatedUserName ?? "" }, commandType: CommandType.Text);
        }

        public int ReopenCases(IList<int> caseIds, int clientId, int userId, string updatedUserName)
        {
            if (caseIds == null || caseIds.Count == 0) return 0;
            const string sql = @"
UPDATE customercase SET status = 0, updated_by = @UserId, updated_user = @UpdatedUserName, updated_on = NOW()
 WHERE id IN @CaseIds AND Client_Id = @ClientId AND is_deleted = 0;";
            return Execute(sql, new { CaseIds = caseIds, ClientId = clientId, UserId = userId, UpdatedUserName = updatedUserName ?? "" }, commandType: CommandType.Text);
        }

        public ServiceResponse<List<CaseMakerInfoDTO>> GetCaseMakersAndEmails(IList<int> caseIds, int clientId)
        {
            ServiceResponse<List<CaseMakerInfoDTO>> serviceResponse = new ServiceResponse<List<CaseMakerInfoDTO>>();
            try
            {
                var caseIdsCsv = string.Join(",", caseIds);
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_CaseIds", caseIdsCsv);
                parameters.Add("p_clientid", clientId);
                serviceResponse.Result = Get<CaseMakerInfoDTO>("getcase_makers_emails", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Tracker Cases details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
            
        }

        public List<CaseQaDTO> GetQaHistory(int caseId, int clientId)
        {
            const string sql = @"
SELECT id AS Id, case_id AS CaseId, client_id AS ClientId, qa_status AS QaStatus,
       qa_outcome AS QaOutcome, qa_reason_code AS QaReasonCode,
       qa_reviewer_id AS QaReviewerId, qa_reviewer_name AS QaReviewerName,
       qa_reviewed_on AS QaReviewedOn, qa_comments AS QaComments, created_by AS CreatedBy
  FROM customercase_qa
 WHERE case_id = @CaseId AND client_id = @ClientId
 ORDER BY id DESC;";
            return Get<CaseQaDTO>(sql, new { CaseId = caseId, ClientId = clientId }, commandType: CommandType.Text).ToList();
        }

        public int MarkRandomSampleForQa(int percent, int clientId, int userId, out int eligibleCount, out int sampledCount)
        {
            eligibleCount = 0;
            sampledCount = 0;
            if (percent <= 0) return 0;
            if (percent > 100) percent = 100;

            const string countSql = @"
SELECT COUNT(*) FROM customercase ca
 WHERE ca.is_deleted = 0 AND ca.Client_Id = @ClientId AND ca.status IN (1,2,3,5)
   AND NOT EXISTS (SELECT 1 FROM customercase_qa qa WHERE qa.case_id = ca.id AND qa.is_active = 1);";
            using (var conn = GetConnections())
            {
                eligibleCount = conn.ExecuteScalar<int>(countSql, new { ClientId = clientId });
            }
            if (eligibleCount == 0) return 0;

            sampledCount = (int)Math.Ceiling(eligibleCount * (percent / 100.0));
            if (sampledCount < 1) sampledCount = 1;
            if (sampledCount > eligibleCount) sampledCount = eligibleCount;

            const string insertSql = @"
INSERT INTO customercase_qa
    (case_id, client_id, qa_status, is_active, created_by, created_on, updated_by, updated_on)
SELECT ca.id, @ClientId, 1, 1, @UserId, NOW(), @UserId, NOW()
  FROM customercase ca
 WHERE ca.is_deleted = 0 AND ca.Client_Id = @ClientId AND ca.status IN (1,2,3,5)
   AND NOT EXISTS (SELECT 1 FROM customercase_qa qa WHERE qa.case_id = ca.id AND qa.is_active = 1)
 ORDER BY RAND() LIMIT @Limit;";

            return Execute(insertSql, new { ClientId = clientId, UserId = userId, Limit = sampledCount }, commandType: CommandType.Text);
        }

        public int MarkStratifiedSampleForQa(int highPct, int medPct, int lowPct, int clientId, int userId,
            out int highEligible, out int highSampled, out int medEligible, out int medSampled, out int lowEligible, out int lowSampled)
        {
            highEligible = SampleByTier("high", highPct, clientId, userId, out highSampled);
            medEligible = SampleByTier("medium", medPct, clientId, userId, out medSampled);
            lowEligible = SampleByTier("low", lowPct, clientId, userId, out lowSampled);
            return highSampled + medSampled + lowSampled;
        }

        private int SampleByTier(string tier, int percent, int clientId, int userId, out int sampled)
        {
            sampled = 0;
            if (percent <= 0) return CountEligibleByTier(tier, clientId);
            if (percent > 100) percent = 100;

            int eligible = CountEligibleByTier(tier, clientId);
            if (eligible == 0) return 0;
            int limit = (int)Math.Ceiling(eligible * (percent / 100.0));
            if (limit < 1) limit = 1;
            if (limit > eligible) limit = eligible;

            string insertSql = @"
INSERT INTO customercase_qa
    (case_id, client_id, qa_status, is_active, created_by, created_on, updated_by, updated_on)
SELECT ca.id, @ClientId, 1, 1, @UserId, NOW(), @UserId, NOW()
  FROM customercase ca
 WHERE ca.is_deleted = 0 AND ca.Client_Id = @ClientId AND ca.status IN (1,2,3,5)
   AND NOT EXISTS (SELECT 1 FROM customercase_qa qa WHERE qa.case_id = ca.id AND qa.is_active = 1)
   AND " + TierWhereClause(tier) + @"
 ORDER BY RAND() LIMIT @Limit;";

            sampled = Execute(insertSql, new { ClientId = clientId, UserId = userId, Limit = limit }, commandType: CommandType.Text);
            return eligible;
        }

        private int CountEligibleByTier(string tier, int clientId)
        {
            string sql = @"
SELECT COUNT(*) FROM customercase ca
 WHERE ca.is_deleted = 0 AND ca.Client_Id = @ClientId AND ca.status IN (1,2,3,5)
   AND NOT EXISTS (SELECT 1 FROM customercase_qa qa WHERE qa.case_id = ca.id AND qa.is_active = 1)
   AND " + TierWhereClause(tier) + ";";
            using var conn = GetConnections();
            return conn.ExecuteScalar<int>(sql, new { ClientId = clientId });
        }

        private static string TierWhereClause(string tier)
        {
            // Inline tier filter as SQL — tier value is internal only, never user-supplied.
            switch ((tier ?? "").ToLowerInvariant())
            {
                case "high":
                    return @"(
                        (SELECT tri.final_risk_score FROM transaction_risk_individual tri WHERE tri.customercode = ca.cust_id AND tri.IsDeleted = 0 AND tri.assessment_version >= 2 ORDER BY tri.dateofassessment DESC, tri.id DESC LIMIT 1) LIKE '%High%'
                        OR (SELECT trc.final_risk_score FROM transaction_risk_corporate trc WHERE trc.customercode = ca.cust_id AND trc.IsDeleted = 0 AND trc.assessment_version >= 2 ORDER BY trc.dateofassessment DESC, trc.id DESC LIMIT 1) LIKE '%High%')";
                case "medium":
                    return @"(
                        (SELECT tri.final_risk_score FROM transaction_risk_individual tri WHERE tri.customercode = ca.cust_id AND tri.IsDeleted = 0 AND tri.assessment_version >= 2 ORDER BY tri.dateofassessment DESC, tri.id DESC LIMIT 1) LIKE '%Medium%'
                        OR (SELECT trc.final_risk_score FROM transaction_risk_corporate trc WHERE trc.customercode = ca.cust_id AND trc.IsDeleted = 0 AND trc.assessment_version >= 2 ORDER BY trc.dateofassessment DESC, trc.id DESC LIMIT 1) LIKE '%Medium%')";
                case "low":
                    return @"(
                        ((SELECT tri.final_risk_score FROM transaction_risk_individual tri WHERE tri.customercode = ca.cust_id AND tri.IsDeleted = 0 AND tri.assessment_version >= 2 ORDER BY tri.dateofassessment DESC, tri.id DESC LIMIT 1) LIKE '%Low%')
                        OR ((SELECT trc.final_risk_score FROM transaction_risk_corporate trc WHERE trc.customercode = ca.cust_id AND trc.IsDeleted = 0 AND trc.assessment_version >= 2 ORDER BY trc.dateofassessment DESC, trc.id DESC LIMIT 1) LIKE '%Low%'))";
                default:
                    return "1=1";
            }
        }

        // SLA config
        public List<TrackerSlaConfigDTO> GetSlaConfigs(int clientId)
        {
            const string sql = @"
SELECT id AS Id, client_id AS ClientId, customer_type AS CustomerType, risk_tier AS RiskTier,
       case_sla_on_track_days AS CaseSlaOnTrackDays, case_sla_at_risk_days AS CaseSlaAtRiskDays,
       qa_sla_on_track_days AS QaSlaOnTrackDays, qa_sla_at_risk_days AS QaSlaAtRiskDays,
       stale_case_days AS StaleCaseDays, is_active AS IsActive
  FROM tracker_sla_config WHERE client_id = @clientId AND is_active = 1
 ORDER BY customer_type, risk_tier;";
            return Get<TrackerSlaConfigDTO>(sql, new { clientId }, commandType: CommandType.Text).ToList();
        }

        public int UpsertSlaConfig(TrackerSlaConfigDTO cfg, int userId)
        {
            if (cfg.Id > 0)
            {
                const string update = @"
UPDATE tracker_sla_config SET
   customer_type = @CustomerType, risk_tier = @RiskTier,
   case_sla_on_track_days = @CaseSlaOnTrackDays, case_sla_at_risk_days = @CaseSlaAtRiskDays,
   qa_sla_on_track_days = @QaSlaOnTrackDays, qa_sla_at_risk_days = @QaSlaAtRiskDays,
   stale_case_days = @StaleCaseDays, is_active = @IsActive,
   updated_by = @UserId, updated_on = NOW()
 WHERE id = @Id AND client_id = @ClientId;";
                return Execute(update, new { cfg.Id, cfg.ClientId, cfg.CustomerType, cfg.RiskTier,
                    cfg.CaseSlaOnTrackDays, cfg.CaseSlaAtRiskDays, cfg.QaSlaOnTrackDays, cfg.QaSlaAtRiskDays,
                    cfg.StaleCaseDays, cfg.IsActive, UserId = userId }, commandType: CommandType.Text);
            }
            const string insert = @"
INSERT INTO tracker_sla_config (client_id, customer_type, risk_tier,
   case_sla_on_track_days, case_sla_at_risk_days, qa_sla_on_track_days, qa_sla_at_risk_days,
   stale_case_days, is_active, created_by, created_on, updated_by, updated_on)
VALUES (@ClientId, @CustomerType, @RiskTier,
   @CaseSlaOnTrackDays, @CaseSlaAtRiskDays, @QaSlaOnTrackDays, @QaSlaAtRiskDays,
   @StaleCaseDays, @IsActive, @UserId, NOW(), @UserId, NOW());";
            return Execute(insert, new { cfg.ClientId, cfg.CustomerType, cfg.RiskTier,
                cfg.CaseSlaOnTrackDays, cfg.CaseSlaAtRiskDays, cfg.QaSlaOnTrackDays, cfg.QaSlaAtRiskDays,
                cfg.StaleCaseDays, cfg.IsActive, UserId = userId }, commandType: CommandType.Text);
        }

        public int DeleteSlaConfig(int id, int clientId)
            => Execute("UPDATE tracker_sla_config SET is_active = 0 WHERE id = @id AND client_id = @clientId;", new { id, clientId });

        // Saved views
        public List<TrackerSavedViewDTO> GetSavedViews(int userId, int clientId)
        {
            const string sql = @"
SELECT id AS Id, user_id AS UserId, client_id AS ClientId, name AS Name,
       filter_json AS FilterJson, is_shared AS IsShared, created_on AS CreatedOn
  FROM tracker_saved_view
 WHERE client_id = @clientId AND (user_id = @userId OR is_shared = 1)
 ORDER BY name;";
            return Get<TrackerSavedViewDTO>(sql, new { userId, clientId }, commandType: CommandType.Text).ToList();
        }

        public int InsertSavedView(TrackerSavedViewDTO v)
        {
            const string sql = @"
INSERT INTO tracker_saved_view (user_id, client_id, name, filter_json, is_shared, created_on)
VALUES (@UserId, @ClientId, @Name, @FilterJson, @IsShared, NOW());";
            return Execute(sql, v, commandType: CommandType.Text);
        }

        public int DeleteSavedView(int id, int userId, int clientId)
            => Execute("DELETE FROM tracker_saved_view WHERE id = @id AND user_id = @userId AND client_id = @clientId;", new { id, userId, clientId });

        // Audit log
        public int InsertAuditLog(TrackerAuditLogDTO log)
        {
            const string sql = @"
INSERT INTO tracker_audit_log (client_id, user_id, user_name, action, affected_count, case_ids_csv, details, created_on)
VALUES (@ClientId, @UserId, @UserName, @Action, @AffectedCount, @CaseIdsCsv, @Details, NOW());";
            return Execute(sql, log, commandType: CommandType.Text);
        }

        public List<TrackerAuditLogDTO> GetAuditLogs(int clientId, int limit)
        {
            const string sql = @"
SELECT id AS Id, client_id AS ClientId, user_id AS UserId, user_name AS UserName,
       action AS Action, affected_count AS AffectedCount, case_ids_csv AS CaseIdsCsv,
       details AS Details, created_on AS CreatedOn
  FROM tracker_audit_log WHERE client_id = @clientId
 ORDER BY id DESC LIMIT @limit;";
            return Get<TrackerAuditLogDTO>(sql, new { clientId, limit }, commandType: CommandType.Text).ToList();
        }

        // Notifications
        public int InsertNotification(TrackerNotificationDTO n)
        {
            const string sql = @"
INSERT INTO tracker_notification (user_id, client_id, type, title, body, link, case_id, is_read, created_on)
VALUES (@UserId, @ClientId, @Type, @Title, @Body, @Link, @CaseId, 0, NOW());";
            return Execute(sql, n, commandType: CommandType.Text);
        }

        public List<TrackerNotificationDTO> GetNotifications(int userId, bool onlyUnread, int limit)
        {
            string sql = @"
SELECT id AS Id, user_id AS UserId, client_id AS ClientId, type AS Type,
       title AS Title, body AS Body, link AS Link, case_id AS CaseId,
       is_read AS IsRead, created_on AS CreatedOn
  FROM tracker_notification WHERE user_id = @userId" +
   (onlyUnread ? " AND is_read = 0" : "") +
   " ORDER BY id DESC LIMIT @limit;";
            return Get<TrackerNotificationDTO>(sql, new { userId, limit }, commandType: CommandType.Text).ToList();
        }

        public int CountUnreadNotifications(int userId)
        {
            using var conn = GetConnections();
            return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM tracker_notification WHERE user_id = @userId AND is_read = 0;", new { userId });
        }

        public int MarkNotificationRead(int notificationId, int userId)
            => Execute("UPDATE tracker_notification SET is_read = 1 WHERE id = @id AND user_id = @userId;", new { id = notificationId, userId });

        public int MarkAllNotificationsRead(int userId)
            => Execute("UPDATE tracker_notification SET is_read = 1 WHERE user_id = @userId AND is_read = 0;", new { userId });

        // SAR links
        public TrackerSarLinkDTO GetSarLink(int caseId, int clientId)
        {
            const string sql = @"
SELECT id AS Id, case_id AS CaseId, client_id AS ClientId, sar_status AS SarStatus,
       sar_reference AS SarReference, sar_filing_deadline AS SarFilingDeadline,
       filed_on AS FiledOn, notes AS Notes, is_active AS IsActive, created_by AS CreatedBy
  FROM tracker_sar_link
 WHERE case_id = @caseId AND client_id = @clientId AND is_active = 1
 ORDER BY id DESC LIMIT 1;";
            return GetFirstOrDefault<TrackerSarLinkDTO>(sql, new { caseId, clientId }, commandType: CommandType.Text);
        }

        public int UpsertSarLink(TrackerSarLinkDTO link, int userId)
        {
            const string sql = @"
UPDATE tracker_sar_link SET is_active = 0 WHERE case_id = @CaseId AND client_id = @ClientId AND is_active = 1;
INSERT INTO tracker_sar_link (case_id, client_id, sar_status, sar_reference, sar_filing_deadline, filed_on, notes, is_active, created_by, created_on, updated_by, updated_on)
VALUES (@CaseId, @ClientId, @SarStatus, @SarReference, @SarFilingDeadline, @FiledOn, @Notes, 1, @UserId, NOW(), @UserId, NOW());";
            return Execute(sql, new { link.CaseId, link.ClientId, link.SarStatus, link.SarReference,
                link.SarFilingDeadline, link.FiledOn, link.Notes, UserId = userId }, commandType: CommandType.Text);
        }

        // Reviewer leaderboard
        public List<ReviewerStatsDTO> GetReviewerStats(int clientId, int daysBack)
        {
            const string sql = @"
SELECT qa.qa_reviewer_id                                         AS ReviewerId,
       MAX(qa.qa_reviewer_name)                                  AS ReviewerName,
       SUM(CASE WHEN qa.qa_status IN (2,3) THEN 1 ELSE 0 END)    AS CasesReviewed,
       SUM(CASE WHEN qa.qa_status = 2 THEN 1 ELSE 0 END)         AS Approved,
       SUM(CASE WHEN qa.qa_status = 3 THEN 1 ELSE 0 END)         AS Rejected,
       AVG(CASE WHEN qa.qa_status IN (2,3)
                THEN GREATEST(0, TIMESTAMPDIFF(HOUR, ca.updated_on, qa.qa_reviewed_on)/24.0)
                ELSE NULL END)                                   AS AvgTurnaroundDays,
       MAX(qa.qa_reviewed_on)                                    AS LastActivity
  FROM customercase_qa qa
  INNER JOIN customercase ca ON ca.id = qa.case_id
 WHERE qa.client_id = @clientId
   AND qa.qa_reviewed_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
   AND qa.qa_reviewer_id IS NOT NULL
 GROUP BY qa.qa_reviewer_id
 ORDER BY CasesReviewed DESC, Approved DESC;";
            return Get<ReviewerStatsDTO>(sql, new { clientId, daysBack }, commandType: CommandType.Text).ToList();
        }

        // Cycle-time chart
        public List<CycleTimePointDTO> GetQaCycleTime(int clientId, int daysBack)
        {
            const string sql = @"
SELECT DATE(qa.qa_reviewed_on)                                                 AS Day,
       COUNT(*)                                                                AS Decisions,
       AVG(GREATEST(0, TIMESTAMPDIFF(HOUR, ca.updated_on, qa.qa_reviewed_on))) AS MedianHours
  FROM customercase_qa qa
  INNER JOIN customercase ca ON ca.id = qa.case_id
 WHERE qa.client_id = @clientId
   AND qa.qa_status IN (2,3)
   AND qa.qa_reviewed_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
 GROUP BY DATE(qa.qa_reviewed_on)
 ORDER BY DATE(qa.qa_reviewed_on);";
            return Get<CycleTimePointDTO>(sql, new { clientId, daysBack }, commandType: CommandType.Text).ToList();
        }

        public List<UserOptionDTO> GetUserOptions(int clientId)
        {
            const string sql = @"
SELECT u.id AS Id,
       CONCAT(IFNULL(u.fname,''),' ',IFNULL(u.lname,'')) AS Name
  FROM user u
 WHERE u.Client_Id = @clientId
   AND IFNULL(u.is_deleted, 0) = 0
   AND IFNULL(u.is_active, 1) = 1
 ORDER BY u.fname, u.lname;";
            return Get<UserOptionDTO>(sql, new { clientId }, commandType: CommandType.Text).ToList();
        }

        // ---------------- Reports ----------------

        public Customer360VM GetCustomer360(int customerMasterId, int clientId)
        {
            const string custSql = @"
SELECT cm.id AS CustomerMasterId, cm.cust_ref_id AS CustomerCode,
       CONCAT_WS(' ', cm.fname, cm.mname, cm.lname) AS FullName,
       cm.cust_type AS CustomerType, cm.nationality AS Nationality,
       cm.dob AS DOB, cm.mobile AS Mobile, cm.profession AS Profession,
       cm.employer AS Employer, cm.employersector AS EmployerSector,
       cm.residence AS Residence, cm.whitelisted_for_screening AS IsWhiteListed,
       cm.dateofwhitelisting AS WhitelistedOn,
       (SELECT CONCAT_WS(' ',u.fname,u.lname) FROM user u WHERE u.id = cm.created_by) AS CreatedBy,
       cm.created_on AS CreatedOn
  FROM customermaster cm
 WHERE cm.id = @cmid AND cm.Client_Id = @cid;";

            const string casesSql = @"
SELECT ca.id AS CaseId, ca.status AS Status, ca.match_score AS MatchScore,
       ca.risk_score AS RiskScore, ca.source AS Source,
       ca.created_on AS CreatedOn, ca.updated_on AS UpdatedOn
  FROM customercase ca
 WHERE ca.cust_master_id = @cmid AND ca.Client_Id = @cid AND ca.is_deleted = 0
 ORDER BY ca.id DESC;";

            const string riskSql = @"
SELECT tri.dateofassessment AS AssessmentDate, tri.final_risk_score AS FinalRiskScore,
       tri.assessment_version AS Version, tri.riskoverride AS Override
  FROM transaction_risk_individual tri
  INNER JOIN customermaster cm ON cm.cust_ref_id = tri.customercode
 WHERE cm.id = @cmid AND tri.IsDeleted = 0
 ORDER BY tri.dateofassessment;";

            const string commentsSql = @"
SELECT cc.case_id AS CaseId, cc.comment AS Comment,
       (SELECT CONCAT_WS(' ',u.fname,u.lname) FROM user u WHERE u.id = cc.created_by) AS CreatedBy,
       cc.created_on AS CreatedOn
  FROM casecomment cc
  INNER JOIN customercase ca ON ca.id = cc.case_id
 WHERE ca.cust_master_id = @cmid AND ca.Client_Id = @cid
 ORDER BY cc.created_on DESC LIMIT 50;";

            const string wlSql = @"
SELECT 'Whitelisted' AS Action,
       (SELECT CONCAT_WS(' ',u.fname,u.lname) FROM user u WHERE u.id = wl.created_by) AS PerformedBy,
       wl.created_on AS PerformedOn
  FROM customer_whitelist_log wl
  INNER JOIN customermaster cm ON cm.cust_ref_id = wl.cust_ref_id
 WHERE cm.id = @cmid
 ORDER BY wl.created_on DESC LIMIT 50;";

            var p = new { cmid = customerMasterId, cid = clientId };
            var vm = GetFirstOrDefault<Customer360VM>(custSql, p, commandType: CommandType.Text);
            if (vm == null) return null;
            vm.Cases = Get<Customer360CaseRow>(casesSql, p, commandType: CommandType.Text).ToList();
            vm.RiskHistory = Get<Customer360RiskPoint>(riskSql, p, commandType: CommandType.Text).ToList();
            vm.Comments = Get<Customer360CommentRow>(commentsSql, p, commandType: CommandType.Text).ToList();
            vm.WhitelistHistory = Get<Customer360WhitelistRow>(wlSql, new { cmid = customerMasterId }, commandType: CommandType.Text).ToList();
            return vm;
        }

        public List<RiskOverrideRow> GetRiskOverrides(int clientId, int daysBack)
        {
            const string sql = @"
SELECT tri.customercode                     AS CustomerCode,
       tri.customername                     AS CustomerName,
       tri.dateofassessment                 AS AssessmentDate,
       tri.risk_score_before_override       AS RiskBefore,
       tri.final_risk_score                 AS RiskAfter,
       tri.riskoverride                     AS OverrideReason,
       tri.comments                         AS Comments,
       tri.assessment_version               AS AssessmentVersion
  FROM transaction_risk_individual tri
 WHERE tri.IsDeleted = 0
   AND tri.dateofassessment >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
   AND (tri.riskoverride IS NOT NULL AND TRIM(tri.riskoverride) <> ''
        OR (tri.risk_score_before_override IS NOT NULL
            AND tri.risk_score_before_override <> tri.final_risk_score))
 ORDER BY tri.dateofassessment DESC LIMIT 1000;";
            return Get<RiskOverrideRow>(sql, new { daysBack }, commandType: CommandType.Text).ToList();
        }

        public List<dynamic> GetUserGroupRights(int clientId)
        {
            // Pivot-friendly: rows are usergroup × module with a per-group right count
            const string sql = @"
SELECT ug.name AS GroupName, m.module_name AS ModuleName, COUNT(ugr.id) AS Rights
  FROM usergroupright ugr
  INNER JOIN usergroup ug ON ug.id = ugr.user_group_id
  INNER JOIN module m ON m.id = ugr.module_id
 WHERE ugr.client_id = @clientId OR ugr.client_id = 0 OR ugr.client_id IS NULL
 GROUP BY ug.name, m.module_name
 ORDER BY ug.name, m.module_name;";
            using var conn = GetConnections();
            return conn.Query(sql, new { clientId }).ToList();
        }

        public List<SanctionsFreshnessRow> GetSanctionsFreshness(int clientId)
        {
            const string sql = @"
SELECT IFNULL(source, 'Unknown')                 AS Source,
       SUM(CASE WHEN created_on >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) AS Hits30d,
       MAX(created_on)                           AS LastHitOn,
       DATEDIFF(NOW(), MAX(created_on))          AS DaysSinceLastHit,
       (SELECT MAX(sdl.created_on) FROM screening_database_logs sdl) AS LastListUpdate
  FROM sanction_screening_log
 WHERE client_id = @clientId OR @clientId = 0
 GROUP BY source
 ORDER BY LastHitOn DESC;";
            try { return Get<SanctionsFreshnessRow>(sql, new { clientId }, commandType: CommandType.Text).ToList(); }
            catch { return new List<SanctionsFreshnessRow>(); }
        }

        public List<WhitelistAnalystRow> GetWhitelistAnalystPatterns(int clientId, int daysBack)
        {
            const string sql = @"
SELECT (SELECT CONCAT_WS(' ',u.fname,u.lname) FROM user u WHERE u.id = wl.created_by) AS AnalystName,
       COUNT(*)         AS Whitelistings,
       MAX(wl.created_on) AS LastAction
  FROM customer_whitelist_log wl
 WHERE wl.created_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
 GROUP BY wl.created_by
 ORDER BY Whitelistings DESC;";
            return Get<WhitelistAnalystRow>(sql, new { daysBack }, commandType: CommandType.Text).ToList();
        }

        public List<InvestigationDepthRow> GetInvestigationDepth(int clientId)
        {
            const string sql = @"
SELECT ca.id                                                      AS CaseId,
       CONCAT_WS(' ', cm.fname, cm.lname)                         AS CustomerName,
       ca.status                                                  AS Status,
       (SELECT COUNT(*) FROM casecomment cc WHERE cc.case_id = ca.id) AS CommentCount,
       (SELECT MAX(cc.created_on) FROM casecomment cc WHERE cc.case_id = ca.id) AS LatestComment
  FROM customercase ca
  INNER JOIN customermaster cm ON cm.id = ca.cust_master_id
 WHERE ca.is_deleted = 0 AND ca.Client_Id = @clientId
 ORDER BY CommentCount DESC, ca.id DESC LIMIT 500;";
            return Get<InvestigationDepthRow>(sql, new { clientId }, commandType: CommandType.Text).ToList();
        }

        public List<TopErrorRow> GetTopErrors(int daysBack, int limit)
        {
            const string sql = @"
SELECT IFNULL(source, 'Unknown') AS Source,
       LEFT(IFNULL(error_message, message), 120) AS ErrorType,
       COUNT(*) AS Occurrences,
       MAX(created_on) AS LastSeen
  FROM error_log_table
 WHERE created_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
 GROUP BY Source, ErrorType
 ORDER BY Occurrences DESC LIMIT @limit;";
            try { return Get<TopErrorRow>(sql, new { daysBack, limit }, commandType: CommandType.Text).ToList(); }
            catch
            {
                // Fallback if error_log_table column names differ
                const string fallback = @"
SELECT 'errors' AS Source, 'aggregate' AS ErrorType, COUNT(*) AS Occurrences, MAX(created_on) AS LastSeen
  FROM error_log_table WHERE created_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY);";
                return Get<TopErrorRow>(fallback, new { daysBack }, commandType: CommandType.Text).ToList();
            }
        }

        public List<QaThroughputPoint> GetQaThroughput(int clientId, int daysBack)
        {
            const string sql = @"
SELECT DATE(qa.qa_reviewed_on)                                                AS Day,
       SUM(CASE WHEN qa.qa_status = 2 THEN 1 ELSE 0 END)                      AS Approved,
       SUM(CASE WHEN qa.qa_status = 3 THEN 1 ELSE 0 END)                      AS Rejected,
       AVG(GREATEST(0, TIMESTAMPDIFF(HOUR, ca.updated_on, qa.qa_reviewed_on))) AS AvgTurnaroundHours
  FROM customercase_qa qa
  INNER JOIN customercase ca ON ca.id = qa.case_id
 WHERE qa.client_id = @clientId
   AND qa.qa_status IN (2,3)
   AND qa.qa_reviewed_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
 GROUP BY DATE(qa.qa_reviewed_on)
 ORDER BY DATE(qa.qa_reviewed_on);";
            return Get<QaThroughputPoint>(sql, new { clientId, daysBack }, commandType: CommandType.Text).ToList();
        }

        public List<SarRegisterRow> GetSarRegister(int clientId)
        {
            const string sql = @"
SELECT sl.case_id                                          AS CaseId,
       CONCAT_WS(' ', cm.fname, cm.lname)                  AS CustomerName,
       sl.sar_status                                       AS SarStatus,
       sl.sar_reference                                    AS SarReference,
       sl.sar_filing_deadline                              AS FilingDeadline,
       sl.filed_on                                         AS FiledOn,
       DATEDIFF(sl.sar_filing_deadline, NOW())             AS DaysToDeadline,
       (sl.sar_filing_deadline IS NOT NULL
         AND sl.sar_filing_deadline < NOW()
         AND sl.sar_status <> 'Filed')                     AS Overdue
  FROM tracker_sar_link sl
  INNER JOIN customercase ca ON ca.id = sl.case_id
  INNER JOIN customermaster cm ON cm.id = ca.cust_master_id
 WHERE sl.is_active = 1 AND sl.client_id = @clientId
 ORDER BY (sl.sar_filing_deadline IS NULL), sl.sar_filing_deadline ASC LIMIT 500;";
            return Get<SarRegisterRow>(sql, new { clientId }, commandType: CommandType.Text).ToList();
        }

        public List<FourEyesAttemptRow> GetFourEyesAttempts(int clientId, int limit)
        {
            const string sql = @"
SELECT created_on AS CreatedOn, user_name AS UserName, action AS Action,
       details AS Details, case_ids_csv AS CaseIdsCsv
  FROM tracker_audit_log
 WHERE client_id = @clientId
   AND details LIKE '%blocked4eyes=%'
   AND details NOT LIKE '%blocked4eyes=0%'
 ORDER BY id DESC LIMIT @limit;";
            return Get<FourEyesAttemptRow>(sql, new { clientId, limit }, commandType: CommandType.Text).ToList();
        }

        public List<dynamic> GetWorkloadByOwner(int clientId)
        {
            const string sql = @"
SELECT IFNULL(ca.owner, 0)                                                              AS OwnerId,
       IFNULL((SELECT CONCAT_WS(' ',u.fname,u.lname) FROM user u WHERE u.id = ca.owner), 'Unassigned') AS OwnerName,
       ca.status                                                                        AS Status,
       COUNT(*)                                                                         AS Cases
  FROM customercase ca
 WHERE ca.is_deleted = 0 AND ca.Client_Id = @clientId
 GROUP BY ca.owner, ca.status
 ORDER BY OwnerName, ca.status;";
            using var conn = GetConnections();
            return conn.Query(sql, new { clientId }).ToList();
        }

        public List<PeriodicReviewRow> GetPeriodicReviewCandidates(int clientId)
        {
            // Review interval per risk tier:
            //   High = 6 months, Medium = 12 months, Low = 24 months, default 12.
            const string sql = @"
SELECT cm.id                                                                  AS CustomerMasterId,
       cm.cust_ref_id                                                         AS CustomerCode,
       CONCAT_WS(' ', cm.fname, cm.mname, cm.lname)                           AS FullName,
       cm.cust_type                                                           AS CustomerType,
       (SELECT tri.final_risk_score FROM transaction_risk_individual tri
         WHERE tri.customercode = cm.cust_ref_id AND tri.IsDeleted = 0
         ORDER BY tri.dateofassessment DESC, tri.id DESC LIMIT 1)             AS RiskTier,
       cm.created_on                                                          AS CreatedOn,
       (SELECT MAX(ca.updated_on) FROM customercase ca WHERE ca.cust_master_id = cm.id) AS LastTouched
  FROM customermaster cm
 WHERE cm.Client_Id = @clientId AND IFNULL(cm.is_deleted, 0) = 0
 ORDER BY LastTouched ASC LIMIT 1000;";

            var rows = Get<PeriodicReviewRow>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            var today = DateTime.UtcNow.Date;
            foreach (var r in rows)
            {
                var tier = (r.RiskTier ?? "").ToLowerInvariant();
                int months = tier.Contains("high") ? 6 : tier.Contains("low") ? 24 : 12;
                r.IntervalMonths = months;
                DateTime anchor = r.LastTouched ?? r.CreatedOn ?? today;
                r.NextReviewDue = anchor.AddMonths(months).Date;
                r.DaysUntilDue = (r.NextReviewDue.Value - today).Days;
                r.Status = r.DaysUntilDue < 0 ? "Overdue"
                         : r.DaysUntilDue <= 30 ? "Due Soon"
                         : "On Track";
            }
            return rows.OrderBy(r => r.DaysUntilDue).ToList();
        }

        public long InsertChatMessage(int clientId, int userId, string sessionId, int? caseId, string role, string messageText, string model, int? promptTokens, int? completionTokens, int? latencyMs)
        {
            const string sql = @"
INSERT INTO ai_chat_history (client_id, user_id, session_id, case_id, role, message_text, model, prompt_tokens, completion_tokens, latency_ms, created_on)
VALUES (@clientId, @userId, @sessionId, @caseId, @role, @messageText, @model, @promptTokens, @completionTokens, @latencyMs, NOW());
SELECT LAST_INSERT_ID();";
            using var conn = GetConnections();
            return conn.ExecuteScalar<long>(sql, new { clientId, userId, sessionId, caseId, role, messageText, model, promptTokens, completionTokens, latencyMs });
        }

        public ChatAnalyticsVM GetChatAnalytics(int clientId, int daysBack)
        {
            var vm = new ChatAnalyticsVM();
            const string totalsSql = @"
SELECT COUNT(*)                                                           AS TotalMessages,
       SUM(CASE WHEN role = 'user'      THEN 1 ELSE 0 END)                AS UserMessages,
       SUM(CASE WHEN role = 'assistant' THEN 1 ELSE 0 END)                AS AssistantMessages,
       COUNT(DISTINCT user_id)                                            AS UniqueUsers,
       COUNT(DISTINCT session_id)                                         AS UniqueSessions,
       SUM(CASE WHEN created_on >= DATE_SUB(NOW(), INTERVAL 7 DAY) THEN 1 ELSE 0 END) AS Last7Days
  FROM ai_chat_history
 WHERE (client_id = @clientId OR @clientId = 0)
   AND created_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY);";
            using (var conn = GetConnections())
            {
                var totals = conn.QueryFirstOrDefault(totalsSql, new { clientId, daysBack });
                if (totals != null)
                {
                    vm.TotalMessages = Convert.ToInt32(totals.TotalMessages ?? 0);
                    vm.UserMessages = Convert.ToInt32(totals.UserMessages ?? 0);
                    vm.AssistantMessages = Convert.ToInt32(totals.AssistantMessages ?? 0);
                    vm.UniqueUsers = Convert.ToInt32(totals.UniqueUsers ?? 0);
                    vm.UniqueSessions = Convert.ToInt32(totals.UniqueSessions ?? 0);
                    vm.Last7Days = Convert.ToInt32(totals.Last7Days ?? 0);
                }
            }

            const string dailySql = @"
SELECT DATE(created_on) AS Day, COUNT(*) AS Messages
  FROM ai_chat_history
 WHERE (client_id = @clientId OR @clientId = 0)
   AND created_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
 GROUP BY DATE(created_on) ORDER BY DATE(created_on);";
            vm.DailyVolume = Get<ChatDailyPoint>(dailySql, new { clientId, daysBack }, commandType: CommandType.Text).ToList();

            const string topUsersSql = @"
SELECT ach.user_id                                                            AS UserId,
       (SELECT CONCAT_WS(' ', u.fname, u.lname) FROM user u WHERE u.id = ach.user_id) AS UserName,
       COUNT(*)                                                               AS Messages,
       COUNT(DISTINCT ach.session_id)                                         AS Sessions,
       MAX(ach.created_on)                                                    AS LastActivity
  FROM ai_chat_history ach
 WHERE (ach.client_id = @clientId OR @clientId = 0)
   AND ach.created_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
 GROUP BY ach.user_id ORDER BY Messages DESC LIMIT 20;";
            vm.TopUsers = Get<ChatUserRow>(topUsersSql, new { clientId, daysBack }, commandType: CommandType.Text).ToList();

            const string recentSql = @"
SELECT ach.created_on AS CreatedOn,
       (SELECT CONCAT_WS(' ', u.fname, u.lname) FROM user u WHERE u.id = ach.user_id) AS UserName,
       ach.role AS Role,
       LEFT(ach.message_text, 200) AS Snippet,
       ach.case_id AS CaseId
  FROM ai_chat_history ach
 WHERE (ach.client_id = @clientId OR @clientId = 0)
 ORDER BY ach.id DESC LIMIT 25;";
            vm.RecentMessages = Get<ChatRecentRow>(recentSql, new { clientId }, commandType: CommandType.Text).ToList();

            return vm;
        }

        public int InsertCaseAssignment(int caseId, int userId, string comment, int actorUserId)
        {
            const string sql = @"
INSERT INTO caseassignment (case_id, user_id, comment, created_by, created_on)
VALUES (@caseId, @userId, @comment, @actorUserId, NOW());";
            return Execute(sql, new { caseId, userId, comment = comment ?? "", actorUserId }, commandType: CommandType.Text);
        }

        // ---------------- Reports continued ----------------

        public List<HighRiskCustomerRow> GetHighRiskCustomers(int clientId)
        {
            const string sql = @"
SELECT cm.id                                                                  AS CustomerMasterId,
       cm.cust_ref_id                                                         AS CustomerCode,
       CONCAT_WS(' ', cm.fname, cm.mname, cm.lname)                           AS FullName,
       cm.cust_type                                                           AS CustomerType,
       cm.nationality                                                         AS Nationality,
       (SELECT tri.final_risk_score FROM transaction_risk_individual tri
         WHERE tri.customercode = cm.cust_ref_id AND tri.IsDeleted = 0
         ORDER BY tri.dateofassessment DESC, tri.id DESC LIMIT 1)             AS FinalRiskScore,
       (SELECT tri.dateofassessment FROM transaction_risk_individual tri
         WHERE tri.customercode = cm.cust_ref_id AND tri.IsDeleted = 0
         ORDER BY tri.dateofassessment DESC, tri.id DESC LIMIT 1)             AS AssessmentDate,
       (SELECT COUNT(*) FROM customercase ca WHERE ca.cust_master_id = cm.id AND ca.is_deleted = 0 AND ca.status IN (0,4,6,7)) AS OpenCases,
       (SELECT COUNT(*) FROM customercase ca WHERE ca.cust_master_id = cm.id AND ca.is_deleted = 0) AS TotalCases,
       (SELECT MAX(ca.updated_on) FROM customercase ca WHERE ca.cust_master_id = cm.id) AS LastTouched,
       cm.whitelisted_for_screening                                           AS IsWhiteListed
  FROM customermaster cm
 WHERE cm.Client_Id = @clientId
   AND IFNULL(cm.is_deleted, 0) = 0
HAVING FinalRiskScore LIKE '%High%'
 ORDER BY AssessmentDate DESC LIMIT 1000;";
            var rows = Get<HighRiskCustomerRow>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            foreach (var r in rows) r.RiskTier = "High";
            return rows;
        }

        public CustomerDemographicsVM GetCustomerDemographics(int clientId)
        {
            var vm = new CustomerDemographicsVM();
            using var conn = GetConnections();
            vm.Total = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customermaster WHERE Client_Id = @clientId AND IFNULL(is_deleted,0) = 0;", new { clientId });

            List<DemographicBucket> Slice(string col)
            {
                var sql = $@"
SELECT IFNULL(NULLIF(TRIM({col}), ''), 'Unknown') AS Label, COUNT(*) AS Count
  FROM customermaster
 WHERE Client_Id = @clientId AND IFNULL(is_deleted,0) = 0
 GROUP BY Label ORDER BY Count DESC LIMIT 25;";
                return conn.Query<DemographicBucket>(sql, new { clientId }).ToList();
            }
            vm.ByNationality = Slice("nationality");
            vm.ByProfession = Slice("profession");
            vm.ByEmployerSector = Slice("employersector");
            vm.ByResidence = Slice("residence");

            // Customer-type breakdown
            const string typeSql = @"
SELECT CASE cust_type
            WHEN 'I' THEN 'Individual' WHEN 'C' THEN 'Corporate'
            WHEN 'S' THEN 'Shareholder' WHEN 'B' THEN 'Beneficiary'
            WHEN 'V' THEN 'Vendor' ELSE IFNULL(cust_type, 'Unknown')
       END                                       AS Label,
       COUNT(*)                                  AS Count
  FROM customermaster WHERE Client_Id = @clientId AND IFNULL(is_deleted,0) = 0
 GROUP BY cust_type ORDER BY Count DESC;";
            vm.ByCustomerType = conn.Query<DemographicBucket>(typeSql, new { clientId }).ToList();
            vm.IndividualCount = vm.ByCustomerType.FirstOrDefault(b => b.Label == "Individual")?.Count ?? 0;
            vm.CorporateCount = vm.ByCustomerType.FirstOrDefault(b => b.Label == "Corporate")?.Count ?? 0;
            return vm;
        }

        public List<LookbackRow> GetLookbackCandidates(int clientId, int daysBack)
        {
            // Customers whose latest risk is High but the previous version was lower —
            // these need a look-back review on past activity.
            const string sql = @"
SELECT cm.id                                                                AS CustomerMasterId,
       cm.cust_ref_id                                                       AS CustomerCode,
       CONCAT_WS(' ', cm.fname, cm.mname, cm.lname)                         AS FullName,
       cur.final_risk_score                                                 AS CurrentRisk,
       prev.final_risk_score                                                AS PreviousRisk,
       cur.dateofassessment                                                 AS AssessmentDate,
       cur.assessment_version                                               AS AssessmentVersion,
       (SELECT COUNT(*) FROM customercase ca
         WHERE ca.cust_master_id = cm.id AND ca.is_deleted = 0
           AND ca.created_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY))     AS RecentCases,
       DATEDIFF(NOW(), cur.dateofassessment)                                AS DaysSince
  FROM customermaster cm
  INNER JOIN transaction_risk_individual cur
          ON cur.customercode = cm.cust_ref_id
         AND cur.IsDeleted = 0
         AND cur.id = (SELECT MAX(t.id) FROM transaction_risk_individual t
                        WHERE t.customercode = cm.cust_ref_id AND t.IsDeleted = 0)
  LEFT JOIN transaction_risk_individual prev
          ON prev.customercode = cm.cust_ref_id
         AND prev.IsDeleted = 0
         AND prev.id = (SELECT MAX(t.id) FROM transaction_risk_individual t
                         WHERE t.customercode = cm.cust_ref_id AND t.IsDeleted = 0
                           AND t.id < cur.id)
 WHERE cm.Client_Id = @clientId
   AND IFNULL(cm.is_deleted,0) = 0
   AND cur.final_risk_score LIKE '%High%'
   AND (prev.final_risk_score IS NULL OR prev.final_risk_score NOT LIKE '%High%')
   AND cur.dateofassessment >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
 ORDER BY cur.dateofassessment DESC LIMIT 500;";
            return Get<LookbackRow>(sql, new { clientId, daysBack }, commandType: CommandType.Text).ToList();
        }

        public AnnualMlroVM GetAnnualMlroReport(int clientId, int year)
        {
            var vm = new AnnualMlroVM
            {
                Year = year,
                PeriodStart = new DateTime(year, 1, 1),
                PeriodEnd = new DateTime(year, 12, 31, 23, 59, 59)
            };

            using var conn = GetConnections();
            var p = new { clientId, ps = vm.PeriodStart, pe = vm.PeriodEnd };

            // Case volumes
            vm.CasesOpened = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customercase WHERE Client_Id=@clientId AND is_deleted=0 AND created_on BETWEEN @ps AND @pe;", p);
            vm.CasesClosed = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customercase WHERE Client_Id=@clientId AND is_deleted=0 AND status IN (1,2,3,5) AND updated_on BETWEEN @ps AND @pe;", p);
            vm.CasesAutoCleared = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customercase WHERE Client_Id=@clientId AND is_deleted=0 AND status=5 AND updated_on BETWEEN @ps AND @pe;", p);
            vm.CasesApproved = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customercase WHERE Client_Id=@clientId AND is_deleted=0 AND status=2 AND updated_on BETWEEN @ps AND @pe;", p);
            vm.CasesRejected = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customercase WHERE Client_Id=@clientId AND is_deleted=0 AND status=3 AND updated_on BETWEEN @ps AND @pe;", p);
            vm.CasesWhitelisted = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customercase WHERE Client_Id=@clientId AND is_deleted=0 AND status=1 AND updated_on BETWEEN @ps AND @pe;", p);
            vm.CasesPendingAtYearEnd = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customercase WHERE Client_Id=@clientId AND is_deleted=0 AND status IN (0,4,6,7) AND created_on <= @pe;", p);

            // QA
            vm.QaApproved = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customercase_qa WHERE client_id=@clientId AND qa_status=2 AND qa_reviewed_on BETWEEN @ps AND @pe;", p);
            vm.QaRejected = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customercase_qa WHERE client_id=@clientId AND qa_status=3 AND qa_reviewed_on BETWEEN @ps AND @pe;", p);
            int qaTotal = vm.QaApproved + vm.QaRejected;
            vm.QaRejectRate = qaTotal == 0 ? 0 : Math.Round(vm.QaRejected * 100.0 / qaTotal, 1);
            vm.AvgQaTurnaroundHours = Math.Round(conn.ExecuteScalar<double?>(
                @"SELECT AVG(GREATEST(0, TIMESTAMPDIFF(HOUR, ca.updated_on, qa.qa_reviewed_on)))
                    FROM customercase_qa qa INNER JOIN customercase ca ON ca.id = qa.case_id
                   WHERE qa.client_id=@clientId AND qa.qa_status IN (2,3) AND qa.qa_reviewed_on BETWEEN @ps AND @pe;", p) ?? 0, 1);

            // Risk distribution among customers with assessments in the year
            const string riskSql = @"
SELECT IFNULL(LOWER(tri.final_risk_score), '') AS Tier, COUNT(*) AS C
  FROM transaction_risk_individual tri
 WHERE tri.IsDeleted=0 AND tri.dateofassessment BETWEEN @ps AND @pe
 GROUP BY Tier;";
            foreach (var row in conn.Query(riskSql, p))
            {
                string t = (string)row.Tier ?? "";
                int c = Convert.ToInt32(row.C);
                if (t.Contains("high")) vm.HighRisk += c;
                else if (t.Contains("medium")) vm.MediumRisk += c;
                else if (t.Contains("low")) vm.LowRisk += c;
            }
            vm.RiskOverrides = conn.ExecuteScalar<int>(
                @"SELECT COUNT(*) FROM transaction_risk_individual
                   WHERE IsDeleted=0 AND dateofassessment BETWEEN @ps AND @pe
                     AND (TRIM(IFNULL(riskoverride,'')) <> '' OR risk_score_before_override <> final_risk_score);", p);

            // Screening hit aggregates from customercase columns within the period
            var hits = conn.QueryFirstOrDefault<dynamic>(
                @"SELECT
                    SUM(IFNULL(true_dometic_pep,0) + IFNULL(true_foreign_pep,0)
                        + IFNULL(partial_domestic_pep,0) + IFNULL(partial_foreign_pep,0)) AS Pep,
                    SUM(IFNULL(true_uae_un_sanction,0) + IFNULL(true_other_sanction,0))    AS San,
                    SUM(IFNULL(true_adversemedia,0) + IFNULL(partial_adversemedia,0))      AS Am,
                    SUM(IFNULL(match_score,0))                                              AS TotalMatch
                  FROM customercase
                 WHERE Client_Id=@clientId AND is_deleted=0 AND created_on BETWEEN @ps AND @pe;", p);
            if (hits != null)
            {
                vm.PepHits = Convert.ToInt32(hits.Pep ?? 0);
                vm.SanctionHits = Convert.ToInt32(hits.San ?? 0);
                vm.AdverseMediaHits = Convert.ToInt32(hits.Am ?? 0);
                vm.TotalScreeningHits = vm.PepHits + vm.SanctionHits + vm.AdverseMediaHits;
            }

            // SAR
            vm.SarRequired = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM tracker_sar_link WHERE client_id=@clientId AND is_active=1 AND sar_status IN ('Required','Drafted','Filed') AND created_on BETWEEN @ps AND @pe;", p);
            vm.SarFiled = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM tracker_sar_link WHERE client_id=@clientId AND is_active=1 AND sar_status='Filed' AND filed_on BETWEEN @ps AND @pe;", p);
            vm.SarOverdue = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM tracker_sar_link WHERE client_id=@clientId AND is_active=1 AND sar_status<>'Filed' AND sar_filing_deadline < NOW() AND created_on BETWEEN @ps AND @pe;", p);

            // Customers
            vm.CustomersOnboarded = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customermaster WHERE Client_Id=@clientId AND IFNULL(is_deleted,0)=0 AND created_on BETWEEN @ps AND @pe;", p);
            vm.CustomersWhitelisted = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM customer_whitelist_log WHERE created_on BETWEEN @ps AND @pe;", p);

            // Top jurisdictions
            vm.TopJurisdictions = conn.Query<DemographicBucket>(
                @"SELECT IFNULL(NULLIF(TRIM(cm.nationality),''), 'Unknown') AS Label, COUNT(*) AS Count
                    FROM customermaster cm INNER JOIN customercase ca ON ca.cust_master_id = cm.id
                   WHERE ca.Client_Id=@clientId AND ca.is_deleted=0 AND ca.created_on BETWEEN @ps AND @pe
                   GROUP BY Label ORDER BY Count DESC LIMIT 10;", p).ToList();

            // Top analysts (by case decisions)
            vm.TopAnalysts = conn.Query<DemographicBucket>(
                @"SELECT (SELECT CONCAT_WS(' ',u.fname,u.lname) FROM user u WHERE u.id = ca.updated_by) AS Label,
                         COUNT(*) AS Count
                    FROM customercase ca
                   WHERE ca.Client_Id=@clientId AND ca.is_deleted=0 AND ca.status IN (1,2,3,5)
                     AND ca.updated_on BETWEEN @ps AND @pe
                   GROUP BY ca.updated_by ORDER BY Count DESC LIMIT 10;", p).ToList();

            // Top QA reject reasons
            vm.TopRejectReasons = conn.Query<DemographicBucket>(
                @"SELECT IFNULL(NULLIF(TRIM(qa_reason_code), ''), 'Unspecified') AS Label, COUNT(*) AS Count
                    FROM customercase_qa
                   WHERE client_id=@clientId AND qa_status=3 AND qa_reviewed_on BETWEEN @ps AND @pe
                   GROUP BY qa_reason_code ORDER BY Count DESC LIMIT 10;", p).ToList();

            return vm;
        }

        public AgedCasesVM GetAgedCases(int clientId)
        {
            const string sql = @"
SELECT ca.id                                              AS CaseId,
       ca.cust_master_id                                  AS CustomerMasterId,
       CONCAT_WS(' ', cm.fname, cm.lname)                 AS CustomerName,
       cm.cust_ref_id                                     AS CustomerCode,
       ca.status                                          AS StatusCode,
       (SELECT CONCAT_WS(' ',u.fname,u.lname) FROM user u WHERE u.id = ca.owner) AS OwnerName,
       ca.created_on                                      AS CreatedOn,
       ca.updated_on                                      AS UpdatedOn,
       DATEDIFF(NOW(), ca.created_on)                     AS AgeDays,
       DATEDIFF(NOW(), IFNULL(ca.updated_on, ca.created_on)) AS DaysSinceTouch
  FROM customercase ca
  INNER JOIN customermaster cm ON cm.id = ca.cust_master_id
 WHERE ca.is_deleted = 0
   AND ca.Client_Id = @clientId
   AND ca.status IN (0, 4, 6, 7)
 ORDER BY ca.created_on ASC LIMIT 1000;";
            var rows = Get<AgedCaseRow>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            foreach (var r in rows)
            {
                r.StatusLabel = r.StatusCode switch
                {
                    0 => "Pending", 4 => "Pending Sr. Mgmt", 6 => "Daily Batch",
                    7 => "On Hold", _ => $"Status {r.StatusCode}"
                };
                r.AgeBucket = r.AgeDays <= 7 ? "≤ 7 days"
                            : r.AgeDays <= 14 ? "8–14 days"
                            : r.AgeDays <= 30 ? "15–30 days"
                            : r.AgeDays <= 60 ? "31–60 days"
                            : "> 60 days";
            }
            var vm = new AgedCasesVM { Rows = rows };
            foreach (var b in new[] { "≤ 7 days", "8–14 days", "15–30 days", "31–60 days", "> 60 days" })
                vm.ByBucket[b] = rows.Count(r => r.AgeBucket == b);
            return vm;
        }

        public CustomerAcceptanceVM GetCustomerAcceptance(int clientId, int daysBack)
        {
            const string rowsSql = @"
SELECT cm.id                                                      AS CustomerMasterId,
       cm.cust_ref_id                                             AS CustomerCode,
       CONCAT_WS(' ', cm.fname, cm.mname, cm.lname)               AS FullName,
       CASE cm.cust_type WHEN 'I' THEN 'Individual' WHEN 'C' THEN 'Corporate' ELSE cm.cust_type END AS CustomerType,
       cm.nationality                                             AS Nationality,
       cm.profession                                              AS Profession,
       (SELECT CONCAT_WS(' ', u.fname, u.lname) FROM user u WHERE u.id = cm.created_by) AS CreatedBy,
       cm.created_on                                              AS CreatedOn,
       (SELECT tri.final_risk_score FROM transaction_risk_individual tri
         WHERE tri.customercode = cm.cust_ref_id AND tri.IsDeleted = 0
         ORDER BY tri.dateofassessment ASC, tri.id ASC LIMIT 1)   AS InitialRisk,
       (SELECT COUNT(*) FROM customercase ca WHERE ca.cust_master_id = cm.id AND ca.is_deleted = 0) AS Cases
  FROM customermaster cm
 WHERE cm.Client_Id = @clientId
   AND IFNULL(cm.is_deleted, 0) = 0
   AND cm.created_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
 ORDER BY cm.created_on DESC LIMIT 1000;";
            var rows = Get<CustomerAcceptanceRow>(rowsSql, new { clientId, daysBack }, commandType: CommandType.Text).ToList();

            const string daySql = @"
SELECT DATE(cm.created_on)                                    AS Label,
       COUNT(*)                                               AS Count
  FROM customermaster cm
 WHERE cm.Client_Id = @clientId AND IFNULL(cm.is_deleted, 0) = 0
   AND cm.created_on >= DATE_SUB(NOW(), INTERVAL @daysBack DAY)
 GROUP BY DATE(cm.created_on) ORDER BY DATE(cm.created_on);";
            var byDay = Get<DemographicBucket>(daySql, new { clientId, daysBack }, commandType: CommandType.Text).ToList();

            return new CustomerAcceptanceVM
            {
                DaysBack = daysBack,
                Total = rows.Count,
                Individuals = rows.Count(r => r.CustomerType == "Individual"),
                Corporates = rows.Count(r => r.CustomerType == "Corporate"),
                Rows = rows,
                ByDay = byDay
            };
        }

        public DocumentAuditVM GetDocumentAudit(int clientId)
        {
            const string sql = @"
SELECT cd.id                                              AS Id,
       cd.case_id                                         AS CaseId,
       cd.document_name                                   AS DocumentName,
       cd.document_file_name                              AS FileName,
       (SELECT lm.lov_name FROM lovmaster lm WHERE lm.id = cd.document_category) AS CategoryLabel,
       (SELECT lm.lov_name FROM lovmaster lm WHERE lm.id = cd.document_type)     AS TypeLabel,
       cd.issued_date                                     AS IssuedDate,
       cd.expiry_date                                     AS ExpiryDate,
       (SELECT CONCAT_WS(' ', u.fname, u.lname) FROM user u WHERE u.id = cd.created_by) AS CreatedBy,
       cd.created_on                                      AS CreatedOn,
       cd.remarks                                         AS Remarks
  FROM casedocument cd
  LEFT JOIN customercase ca ON ca.id = cd.case_id
 WHERE (ca.Client_Id = @clientId OR ca.Client_Id IS NULL)
 ORDER BY cd.id DESC LIMIT 1000;";
            List<DocumentAuditRow> rows;
            try { rows = Get<DocumentAuditRow>(sql, new { clientId }, commandType: CommandType.Text).ToList(); }
            catch
            {
                // If lovmaster column name differs, retry without category labels
                const string fallback = @"
SELECT cd.id AS Id, cd.case_id AS CaseId, cd.document_name AS DocumentName, cd.document_file_name AS FileName,
       cd.issued_date AS IssuedDate, cd.expiry_date AS ExpiryDate,
       (SELECT CONCAT_WS(' ', u.fname, u.lname) FROM user u WHERE u.id = cd.created_by) AS CreatedBy,
       cd.created_on AS CreatedOn, cd.remarks AS Remarks
  FROM casedocument cd ORDER BY cd.id DESC LIMIT 1000;";
                rows = Get<DocumentAuditRow>(fallback, null, commandType: CommandType.Text).ToList();
            }

            var today = DateTime.UtcNow.Date;
            return new DocumentAuditVM
            {
                Rows = rows,
                Total = rows.Count,
                Last30d = rows.Count(r => r.CreatedOn >= today.AddDays(-30)),
                Expired = rows.Count(r => r.ExpiryDate.HasValue && r.ExpiryDate.Value.Date < today),
                ExpiringIn30d = rows.Count(r => r.ExpiryDate.HasValue && r.ExpiryDate.Value.Date >= today && r.ExpiryDate.Value.Date <= today.AddDays(30))
            };
        }

        public CohortComparisonVM GetCohortComparison(int customerMasterId, int clientId)
        {
            const string custSql = @"
SELECT cm.id AS CustomerMasterId, cm.cust_ref_id AS CustomerCode,
       CONCAT_WS(' ', cm.fname, cm.mname, cm.lname) AS CustomerName,
       CASE cm.cust_type WHEN 'I' THEN 'Individual' WHEN 'C' THEN 'Corporate' ELSE cm.cust_type END AS CustomerType,
       cm.nationality AS Nationality, cm.profession AS Profession
  FROM customermaster cm WHERE cm.id = @cmid AND cm.Client_Id = @cid;";
            var hdr = GetFirstOrDefault<CohortComparisonVM>(custSql, new { cmid = customerMasterId, cid = clientId }, commandType: CommandType.Text);
            if (hdr == null)
                return new CohortComparisonVM { Found = false, NotFoundMessage = "Customer not found in tenant." };

            // Cohort: same customer-type + nationality. Falls back to type-only if nationality is blank.
            string nationalityClause = string.IsNullOrWhiteSpace(hdr.Nationality)
                ? ""
                : "AND IFNULL(cm.nationality,'') = IFNULL(@nat,'')";
            string sql = $@"
SELECT
  COUNT(DISTINCT cm.id) AS CohortSize,
  AVG((SELECT COUNT(*) FROM customercase ca WHERE ca.cust_master_id = cm.id AND ca.is_deleted = 0)) AS AvgCases,
  AVG((SELECT IFNULL(MAX(ca.match_score), 0) FROM customercase ca WHERE ca.cust_master_id = cm.id AND ca.is_deleted = 0)) AS AvgMaxMatch,
  AVG(CASE WHEN cm.whitelisted_for_screening = 'Y' THEN 1.0 ELSE 0 END) * 100 AS PctWhitelisted,
  AVG((SELECT IFNULL(SUM(IFNULL(ca.true_dometic_pep,0)+IFNULL(ca.true_foreign_pep,0)),0) FROM customercase ca WHERE ca.cust_master_id = cm.id)) AS AvgPepHits,
  AVG((SELECT IFNULL(SUM(IFNULL(ca.true_uae_un_sanction,0)+IFNULL(ca.true_other_sanction,0)),0) FROM customercase ca WHERE ca.cust_master_id = cm.id)) AS AvgSanHits
FROM customermaster cm
WHERE cm.Client_Id = @cid AND IFNULL(cm.is_deleted,0)=0
  AND cm.cust_type = (SELECT cust_type FROM customermaster WHERE id = @cmid) {nationalityClause};";
            using var conn = GetConnections();
            var agg = conn.QueryFirstOrDefault(sql, new { cmid = customerMasterId, cid = clientId, nat = hdr.Nationality });

            // Subject metrics
            var subjectSql = @"
SELECT
  (SELECT COUNT(*) FROM customercase ca WHERE ca.cust_master_id = @cmid AND ca.is_deleted = 0) AS Cases,
  (SELECT IFNULL(MAX(ca.match_score),0) FROM customercase ca WHERE ca.cust_master_id = @cmid AND ca.is_deleted = 0) AS MaxMatch,
  (SELECT IFNULL(SUM(IFNULL(ca.true_dometic_pep,0)+IFNULL(ca.true_foreign_pep,0)),0) FROM customercase ca WHERE ca.cust_master_id = @cmid) AS PepHits,
  (SELECT IFNULL(SUM(IFNULL(ca.true_uae_un_sanction,0)+IFNULL(ca.true_other_sanction,0)),0) FROM customercase ca WHERE ca.cust_master_id = @cmid) AS SanHits,
  (SELECT CASE WHEN whitelisted_for_screening = 'Y' THEN 100.0 ELSE 0 END FROM customermaster WHERE id = @cmid) AS PctWhitelisted;";
            var subj = conn.QueryFirstOrDefault(subjectSql, new { cmid = customerMasterId });

            hdr.Found = true;
            hdr.CohortDescription = string.IsNullOrWhiteSpace(hdr.Nationality)
                ? $"All {hdr.CustomerType} customers"
                : $"{hdr.CustomerType} customers from {hdr.Nationality}";
            hdr.CohortSize = agg == null ? 0 : Convert.ToInt32(agg.CohortSize ?? 0);

            string Verdict(double a, double b)
            {
                if (b == 0) return a > 0 ? "Above peers" : "On par";
                var ratio = a / b;
                if (ratio > 1.5) return "Materially above peers";
                if (ratio > 1.1) return "Above peers";
                if (ratio < 0.5) return "Materially below peers";
                if (ratio < 0.9) return "Below peers";
                return "On par";
            }

            void AddMetric(string label, double s, double a)
            {
                hdr.Metrics.Add(new CohortMetric
                {
                    Label = label,
                    Customer = Math.Round(s, 2),
                    CohortAvg = Math.Round(a, 2),
                    Verdict = Verdict(s, a)
                });
            }
            if (subj != null && agg != null)
            {
                AddMetric("Total cases", Convert.ToDouble(subj.Cases ?? 0), Convert.ToDouble(agg.AvgCases ?? 0));
                AddMetric("Highest match score", Convert.ToDouble(subj.MaxMatch ?? 0), Convert.ToDouble(agg.AvgMaxMatch ?? 0));
                AddMetric("PEP hits", Convert.ToDouble(subj.PepHits ?? 0), Convert.ToDouble(agg.AvgPepHits ?? 0));
                AddMetric("Sanction hits", Convert.ToDouble(subj.SanHits ?? 0), Convert.ToDouble(agg.AvgSanHits ?? 0));
                AddMetric("Whitelisted (%)", Convert.ToDouble(subj.PctWhitelisted ?? 0), Convert.ToDouble(agg.PctWhitelisted ?? 0));
            }
            return hdr;
        }

        public QuarterTrendVM GetQuarterlyTrend(int clientId, int quartersBack)
        {
            if (quartersBack <= 0) quartersBack = 8;
            const string sql = @"
SELECT YEAR(ca.created_on) AS Year, QUARTER(ca.created_on) AS Quarter,
       SUM(1) AS Opened,
       SUM(CASE WHEN ca.status IN (1,2,3,5) AND YEAR(ca.updated_on) = YEAR(ca.created_on) AND QUARTER(ca.updated_on) = QUARTER(ca.created_on) THEN 1 ELSE 0 END) AS Closed,
       SUM(CASE WHEN ca.status = 2 THEN 1 ELSE 0 END) AS Approved,
       SUM(CASE WHEN ca.status = 3 THEN 1 ELSE 0 END) AS Rejected
  FROM customercase ca
 WHERE ca.is_deleted = 0 AND ca.Client_Id = @clientId
   AND ca.created_on >= DATE_SUB(NOW(), INTERVAL @months MONTH)
 GROUP BY YEAR(ca.created_on), QUARTER(ca.created_on)
 ORDER BY YEAR(ca.created_on), QUARTER(ca.created_on);";
            var pts = Get<QuarterPoint>(sql, new { clientId, months = quartersBack * 3 }, commandType: CommandType.Text).ToList();
            foreach (var p in pts) p.Label = $"Q{p.Quarter} {p.Year}";
            return new QuarterTrendVM { Points = pts, QuartersBack = quartersBack };
        }

        // ---------------- Customer Associations Report ----------------

        // Related parties live in customermaster itself, with parent_id pointing
        // to the parent's cust_ref_id. So for any tenant we have a forest:
        //   - top-level rows have parent_id NULL/empty
        //   - related parties (shareholders, signatories, owners) point to one
        //     of those, and may themselves be parents of further nested parties.
        // We pull every customermaster row once, then build the tree in memory.

        private class CustomerMasterFlatRow
        {
            public int Id { get; set; }
            public string CustRefId { get; set; }
            public string FullName { get; set; }
            public string CustType { get; set; }
            public string Nationality { get; set; }
            public string ParentId { get; set; }
            public string FlagType { get; set; }
            public string Relationship { get; set; }
            public DateTime? CreatedOn { get; set; }
        }

        public CustomerAssociationsVM GetCustomerAssociations(int clientId)
        {
            const string sql = @"
SELECT cm.id                                                     AS Id,
       cm.cust_ref_id                                            AS CustRefId,
       TRIM(CONCAT_WS(' ', cm.fname, cm.mname, cm.lname))        AS FullName,
       cm.cust_type                                              AS CustType,
       cm.nationality                                            AS Nationality,
       cm.parent_id                                              AS ParentId,
       cm.flagtype                                               AS FlagType,
       cm.relationship                                           AS Relationship,
       cm.created_on                                             AS CreatedOn
  FROM customermaster cm
 WHERE cm.Client_Id = @clientId
   AND IFNULL(cm.is_deleted, 0) = 0
 ORDER BY cm.created_on DESC, cm.cust_ref_id;";

            var all = Get<CustomerMasterFlatRow>(sql, new { clientId }, commandType: CommandType.Text).ToList();

            // Lookup row by cust_ref_id so we can resolve parent_id (a cust_ref_id
            // string) to the parent's numeric Id — that's what the view uses as
            // the nesting key.
            var byRefId  = all.Where(r => !string.IsNullOrEmpty(r.CustRefId))
                              .GroupBy(r => r.CustRefId)
                              .ToDictionary(g => g.Key, g => g.First());

            var roots    = all.Where(r => string.IsNullOrEmpty(r.ParentId) || r.ParentId == "0").ToList();
            var childMap = all
                .Where(r => !string.IsNullOrEmpty(r.ParentId) && r.ParentId != "0")
                .GroupBy(r => r.ParentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Walk every descendant of a root, flattening them into the group.
            // ParentId is rewritten to the parent's numeric Id so the view's
            // nesting lookup (`byParent[row.Id.ToString()]`) resolves correctly.
            // For direct children of the main party we set ParentId = null so
            // the view places them under the main-party root node.
            void Collect(string rootRefId, string parentRefId, List<AssociationRow> bucket)
            {
                if (string.IsNullOrEmpty(parentRefId)) return;
                if (!childMap.TryGetValue(parentRefId, out var kids)) return;
                foreach (var k in kids)
                {
                    string mappedParentId = null;
                    if (parentRefId != rootRefId && byRefId.TryGetValue(parentRefId, out var pr))
                        mappedParentId = pr.Id.ToString();

                    bucket.Add(new AssociationRow
                    {
                        Id           = k.Id,
                        Name         = string.IsNullOrWhiteSpace(k.FullName) ? "(Unnamed)" : k.FullName,
                        FlagType     = k.FlagType,
                        Relationship = k.Relationship,
                        ParentId     = mappedParentId,
                        Nationality  = k.Nationality
                    });
                    Collect(rootRefId, k.CustRefId, bucket);
                }
            }

            var vm = new CustomerAssociationsVM();
            foreach (var root in roots)
            {
                var group = new AssociationGroup
                {
                    MainPartyCode        = root.CustRefId,
                    MainPartyName        = string.IsNullOrWhiteSpace(root.FullName) ? root.CustRefId : root.FullName,
                    MainPartyType        = root.CustType,
                    MainPartyNationality = root.Nationality,
                    CreatedOn            = root.CreatedOn
                };
                Collect(root.CustRefId, root.CustRefId, group.RelatedParties);
                vm.Groups.Add(group);
            }
            return vm;
        }

        // ---------------- /Reports continued ----------------

        // ---------------- /Reports ----------------

        private static DateTime? ParseDateOrNull(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            string[] formats = { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(raw.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var d)) return d;
            if (DateTime.TryParse(raw.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var f)) return f;
            return null;
        }
    }
}
