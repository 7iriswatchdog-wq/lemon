using AML.Core.DataContract.Enum;
using AML.Core.RepositoryContract.AmlTracker;
using AML.Core.ServiceContract.AmlTracker;
using AML.Core.ServiceContract.Common;
using AML.DTO.DTO.AmlTracker;
using AML.ViewModel.ViewModels.AmlTracker;
using AML.ViewModel.ViewModels.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AML.Core.Service.AmlTracker
{
    public class AmlTrackerService : IAmlTrackerService
    {
        private readonly IAmlTrackerRepository _trackerRepo;
        private readonly ICommonService _commonService;
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        // Default SLA — overridden per-tenant by tracker_sla_config when present
        private const int DefaultSlaOnTrackDays = 7;
        private const int DefaultSlaAtRiskDays = 14;
        private const int DefaultStaleCaseDays = 30;

        public AmlTrackerService(IAmlTrackerRepository trackerRepo, ICommonService commonService)
        {
            _trackerRepo = trackerRepo;
            _commonService = commonService;
        }

        public bool IsQaAuthorized(string userGroupName)
        {
            if (string.IsNullOrWhiteSpace(userGroupName)) return false;
            var g = userGroupName.ToLowerInvariant();
            return g.Contains("compliance")
                || g.Contains("senior management")
                || g.Contains("quality assurance")
                || g.Contains("qa reviewer")
                || g.Contains("qa team")
                || g.Contains("audit")
                || g.Contains("branch manager");
        }

        public AmlTrackerIndexVM GetTracker(AmlTrackerFilterVM filter, int userId, string userGroupName, int clientId, DateTime? lastVisitedAt)
        {
            var vm = new AmlTrackerIndexVM
            {
                Filter = filter ?? new AmlTrackerFilterVM(),
                DataLoaded = true,
                CurrentUserCanQa = IsQaAuthorized(userGroupName)
            };

            if (string.IsNullOrWhiteSpace(filter?.StartDate))
            {
                filter.StartDate = DateTime.Now.AddYears(-1).ToString("dd/MM/yyyy");
            }

            if (string.IsNullOrWhiteSpace(filter?.EndDate))
            {
                filter.EndDate = DateTime.Now.ToString("dd/MM/yyyy");
            }

            var startDate = DateTime.ParseExact(
                                filter.StartDate,
                                "dd/MM/yyyy",
                                CultureInfo.InvariantCulture)
                            .ToString("yyyy-MM-dd");

            var endDate = DateTime.ParseExact(
                              filter.EndDate,
                              "dd/MM/yyyy",
                              CultureInfo.InvariantCulture)
                          .ToString("yyyy-MM-dd");
    //        var startDate = string.IsNullOrWhiteSpace(filter?.StartDate)
    //? ""
    //: DateTime.ParseExact(filter.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
    //          .ToString("yyyy-MM-dd");

    //        var endDate = string.IsNullOrWhiteSpace(filter?.EndDate)
    //            ? ""
    //            : DateTime.ParseExact(filter.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
    //                      .ToString("yyyy-MM-dd");
            
            var custType = string.IsNullOrWhiteSpace(filter?.CustomerType) ? "0" : filter.CustomerType;
            var caseStatus = filter?.CaseStatus ?? 10;
            var search = filter?.Search?.Trim() ?? "";

            //var raw = _trackerRepo.GetTrackerCases(clientId, startDate, endDate, custType, caseStatus, search) ?? new List<TrackerCaseDTO>();
           List<TrackerCaseDTO> raw = _trackerRepo
    .GetAmlTrackerCases(clientId, startDate, endDate, custType, caseStatus, search)?
    .Result ?? new List<TrackerCaseDTO>();
            var slaCfg = (_trackerRepo.GetSlaConfigs(clientId) ?? new List<TrackerSlaConfigDTO>()).ToList();

            vm.Rows = raw.Select(r => MapRow(r, slaCfg, lastVisitedAt)).ToList();

            //if (filter != null && filter.QaStatus >= 0 && filter.QaStatus <= 4)
            //    vm.Rows = vm.Rows.Where(r => r.QaStatusCode == filter.QaStatus).ToList();

            //// Risk-tier filter (client-side; matches normalized RiskTier from MapRow)
            //if (filter != null && !string.IsNullOrWhiteSpace(filter.RiskLevel))
            //{
            //    var rl = filter.RiskLevel.ToLowerInvariant();
            //    string match = rl.Contains("high") ? "High"
            //                 : rl.Contains("medium") ? "Medium"
            //                 : rl.Contains("low") ? "Low"
            //                 : rl.Contains("unclassified") ? "Unclassified"
            //                 : null;
            //    if (match != null)
            //        vm.Rows = vm.Rows.Where(r => string.Equals(r.RiskTier, match, StringComparison.OrdinalIgnoreCase)).ToList();
            //}

            vm.Rows = ApplySort(vm.Rows, filter?.SortBy, filter?.SortDir);

            vm.OwnerOptions = (_trackerRepo.GetUserOptions(clientId) ?? new List<UserOptionDTO>())
                .Select(u => new UserOption { Id = u.Id, Name = (u.Name ?? "").Trim() })
                .Where(u => !string.IsNullOrWhiteSpace(u.Name)).ToList();

            vm.SavedViews = (_trackerRepo.GetSavedViews(userId, clientId) ?? new List<TrackerSavedViewDTO>())
                .Select(s => new SavedViewVM { Id = s.Id, Name = s.Name, FilterJson = s.FilterJson, IsShared = s.IsShared })
                .ToList();

            vm.UnreadNotificationCount = _trackerRepo.CountUnreadNotifications(userId);

            ComputeKpis(vm);
            ComputeWeekOverWeekTrends(vm, raw);
            vm.CycleTime = BuildCycleTimeChart(clientId);

            return vm;
        }

        public WorkspaceVM GetQaWorkspace(int userId, string userGroupName, int clientId)
        {
            var vm = new WorkspaceVM { CurrentUserCanQa = IsQaAuthorized(userGroupName) };
            var raw = _trackerRepo.GetTrackerCases(clientId, "", "", "0", 10, "") ?? new List<TrackerCaseDTO>();
            var slaCfg = _trackerRepo.GetSlaConfigs(clientId) ?? new List<TrackerSlaConfigDTO>();

            vm.Rows = raw.Select(r => MapRow(r, slaCfg, null))
                         .Where(r => r.QaStatusCode == (int)QaStatusCode.Pending
                                  || (r.QaEligible && r.QaStatusCode == (int)QaStatusCode.NotReviewed))
                         .OrderByDescending(r => r.DaysClosedWithoutQa ?? r.AgingDays ?? 0)
                         .ToList();
            return vm;
        }

        public SummaryJsonVM GetSummary(int userId, int clientId)
        {
            var raw = _trackerRepo.GetTrackerCases(clientId, "", "", "0", 10, "") ?? new List<TrackerCaseDTO>();
            var slaCfg = _trackerRepo.GetSlaConfigs(clientId) ?? new List<TrackerSlaConfigDTO>();
            var rows = raw.Select(r => MapRow(r, slaCfg, null)).ToList();

            return new SummaryJsonVM
            {
                Total = rows.Count,
                Open = rows.Count(r => IsOpenStatus(r.StatusCode)),
                PendingSrMgmt = rows.Count(r => r.StatusCode == (int)ReportsCaseStatus.PendingWithSeniorManagement),
                Completed = rows.Count(r => IsTerminalStatus(r.StatusCode)),
                SlaBreach = rows.Count(r => string.Equals(r.SlaStatus, "Breach", StringComparison.OrdinalIgnoreCase)),
                HighRisk = rows.Count(r => IsRiskTier(r.RiskTier, "High")),
                Today = rows.Count(r => r.CreatedOn?.Date == DateTime.UtcNow.Date),
                AvgAging = rows.Where(r => r.AgingDays.HasValue).Select(r => (double)r.AgingDays.Value).DefaultIfEmpty(0).Average(),
                QaPending = rows.Count(r => r.QaStatusCode == (int)QaStatusCode.Pending),
                QaApproved = rows.Count(r => r.QaStatusCode == (int)QaStatusCode.Approved),
                QaRejected = rows.Count(r => r.QaStatusCode == (int)QaStatusCode.Rejected),
                QaEligibleAwaiting = rows.Count(r => r.QaEligible && r.QaStatusCode == (int)QaStatusCode.NotReviewed),
                QaSlaBreach = rows.Count(r => string.Equals(r.QaSlaStatus, "Breach", StringComparison.OrdinalIgnoreCase)),
                Stale = rows.Count(r => r.IsStale),
                ServerTime = DateTime.UtcNow,
                UnreadNotifications = _trackerRepo.CountUnreadNotifications(userId)
            };
        }

        public int MarkForQa(IList<int> caseIds, int clientId, int userId, string userName)
        {
            var n = _trackerRepo.MarkForQa(caseIds, clientId, userId);
            WriteAudit(clientId, userId, userName, "MarkForQa", n, caseIds, "");
            return n;
        }

        public QaDecisionResult RecordQaDecision(IList<int> caseIds, int qaStatus, string comments, string reasonCode, int clientId, int userId, string reviewerName)
        {
            var result = new QaDecisionResult();
            if (caseIds == null || caseIds.Count == 0) return result;

            //var makers = _trackerRepo.GetCaseMakersAndEmails(caseIds, clientId);
            //var makersById = makers.ToDictionary(m => m.CaseId);

            var makersResponse = _trackerRepo.GetCaseMakersAndEmails(caseIds, clientId);
            var makersById = makersResponse.Result
                               .ToDictionary(m => m.CaseId);

         

            var allowed = new List<int>();
            foreach (var cid in caseIds.Distinct())
            {
                if (!makersById.TryGetValue(cid, out var info))
                { result.BlockedByFourEyesCount++; result.BlockedCaseIds.Add(cid); continue; }
                //if (info.MakerId == userId)
                //{ result.BlockedByFourEyesCount++; result.BlockedCaseIds.Add(cid); continue; }
                allowed.Add(cid);
            }

            foreach (var cid in allowed)
            {
                var qa = new CaseQaDTO
                {
                    CaseId = cid, ClientId = clientId, QaStatus = qaStatus,
                    QaOutcome = MapQaOutcome(qaStatus), QaReasonCode = reasonCode,
                    QaReviewerId = userId, QaReviewerName = reviewerName,
                    QaComments = comments, CreatedBy = userId
                };
                result.RecordedCount += _trackerRepo.InsertQaRecord(qa);
            }

            if (qaStatus == (int)QaStatusCode.Rejected && allowed.Count > 0)
            {
                result.ReopenedCount = _trackerRepo.ReopenCases(allowed, clientId, userId, reviewerName);

                foreach (var cid in allowed)
                {
                    if (!makersById.TryGetValue(cid, out var info)) continue;

                    // Notification (in-app)
                    if (info.MakerId > 0)
                    {
                        try
                        {
                            _trackerRepo.InsertNotification(new TrackerNotificationDTO
                            {
                                UserId = info.MakerId,
                                ClientId = clientId,
                                Type = "QaRejected",
                                Title = $"Case #{cid} QA Rejected",
                                Body = $"Reviewer {reviewerName?.Trim()} rejected QA: {comments?.Trim()}",
                                Link = $"/AmlTracker/Index?Search={cid}",
                                CaseId = cid
                            });
                        }
                        catch (Exception ex) { _log.Warn(ex, "QA reject notification failed for case {0}", cid); }
                    }

                    // Email (best-effort)
                    if (!string.IsNullOrWhiteSpace(info.MakerEmail))
                    {
                        try
                        {
                            var subject = $"AML Case #{cid} — QA Rejected, returned for rework";
                            var body = $@"Hi {info.MakerName?.Trim()},

Case #{cid} (Customer: {info.CustomerName?.Trim()}) was reviewed by {reviewerName?.Trim()} and QA-rejected.

Reason: {reasonCode}
Comments:
{comments?.Trim()}

The case has been reopened (status: Pending) and is back in your queue.

— AML Tracker";
                            _commonService.SendEmail(new EmailModel(info.MakerEmail.Trim(), subject, body, isBodyHtml: false));
                            result.EmailsSent++;
                        }
                        catch (Exception ex)
                        {
                            _log.Warn(ex, "QA reject email failed for case {0}", cid);
                            result.EmailsFailed++;
                        }
                    }
                }
            }

            var actionName = qaStatus == (int)QaStatusCode.Approved ? "QaApprove" : qaStatus == (int)QaStatusCode.Rejected ? "QaReject" : "QaDecision";
            WriteAudit(clientId, userId, reviewerName, actionName, result.RecordedCount, allowed,
                $"reason={reasonCode}; blocked4eyes={result.BlockedByFourEyesCount}; reopened={result.ReopenedCount}; emailsSent={result.EmailsSent}");
            return result;
        }

        public int Reassign(IList<int> caseIds, int newOwnerId, int clientId, int userId, string updatedUserName)
        {
            var n = _trackerRepo.ReassignOwner(caseIds, newOwnerId, clientId, userId, updatedUserName);
            // Capture historical assignment for workload-trend reports
            foreach (var cid in caseIds ?? new List<int>())
            {
                try { _trackerRepo.InsertCaseAssignment(cid, newOwnerId, $"Reassigned by {updatedUserName}", userId); }
                catch (Exception ex) { _log.Warn(ex, "caseassignment insert failed for case {0}", cid); }
            }
            WriteAudit(clientId, userId, updatedUserName, "Reassign", n, caseIds, $"newOwnerId={newOwnerId}");
            return n;
        }

        public QaSampleResult SampleForQa(int percent, int clientId, int userId, string userName)
        {
            var inserted = _trackerRepo.MarkRandomSampleForQa(percent, clientId, userId, out var eligible, out var sampled);
            WriteAudit(clientId, userId, userName, "SampleForQa", inserted, null, $"percent={percent}; eligible={eligible}; sampled={sampled}");
            return new QaSampleResult { EligibleCount = eligible, SampledCount = sampled, InsertedCount = inserted };
        }

        public StratifiedSampleResult SampleStratifiedForQa(int highPct, int medPct, int lowPct, int clientId, int userId, string userName)
        {
            _trackerRepo.MarkStratifiedSampleForQa(highPct, medPct, lowPct, clientId, userId,
                out var hE, out var hS, out var mE, out var mS, out var lE, out var lS);
            var r = new StratifiedSampleResult
            {
                HighEligible = hE, HighSampled = hS,
                MediumEligible = mE, MediumSampled = mS,
                LowEligible = lE, LowSampled = lS
            };
            WriteAudit(clientId, userId, userName, "StratifiedSample", r.TotalSampled, null,
                $"high={highPct}%/{hS}of{hE}; med={medPct}%/{mS}of{mE}; low={lowPct}%/{lS}of{lE}");
            return r;
        }

        public List<QaHistoryItemVM> GetQaHistory(int caseId, int clientId)
        {
            return (_trackerRepo.GetQaHistory(caseId, clientId) ?? new List<CaseQaDTO>())
                .Select(q => new QaHistoryItemVM
                {
                    Id = q.Id, QaStatus = q.QaStatus, QaStatusLabel = MapQaStatusLabel(q.QaStatus),
                    QaOutcome = q.QaOutcome, QaReviewerName = q.QaReviewerName,
                    QaReviewedOn = q.QaReviewedOn, QaComments = q.QaComments
                }).ToList();
        }

        // SLA config
        public List<SlaConfigVM> GetSlaConfigs(int clientId)
            => (_trackerRepo.GetSlaConfigs(clientId) ?? new List<TrackerSlaConfigDTO>())
                .Select(c => new SlaConfigVM
                {
                    Id = c.Id, CustomerType = c.CustomerType, RiskTier = c.RiskTier,
                    CaseSlaOnTrackDays = c.CaseSlaOnTrackDays, CaseSlaAtRiskDays = c.CaseSlaAtRiskDays,
                    QaSlaOnTrackDays = c.QaSlaOnTrackDays, QaSlaAtRiskDays = c.QaSlaAtRiskDays,
                    StaleCaseDays = c.StaleCaseDays, IsActive = c.IsActive
                }).ToList();

        public int UpsertSlaConfig(SlaConfigVM cfg, int clientId, int userId)
        {
            var dto = new TrackerSlaConfigDTO
            {
                Id = cfg.Id, ClientId = clientId,
                CustomerType = string.IsNullOrWhiteSpace(cfg.CustomerType) ? "*" : cfg.CustomerType,
                RiskTier = string.IsNullOrWhiteSpace(cfg.RiskTier) ? "*" : cfg.RiskTier,
                CaseSlaOnTrackDays = cfg.CaseSlaOnTrackDays, CaseSlaAtRiskDays = cfg.CaseSlaAtRiskDays,
                QaSlaOnTrackDays = cfg.QaSlaOnTrackDays, QaSlaAtRiskDays = cfg.QaSlaAtRiskDays,
                StaleCaseDays = cfg.StaleCaseDays, IsActive = cfg.IsActive
            };
            return _trackerRepo.UpsertSlaConfig(dto, userId);
        }

        public int DeleteSlaConfig(int id, int clientId) => _trackerRepo.DeleteSlaConfig(id, clientId);

        // Saved views
        public List<SavedViewVM> GetSavedViews(int userId, int clientId)
            => (_trackerRepo.GetSavedViews(userId, clientId) ?? new List<TrackerSavedViewDTO>())
                .Select(v => new SavedViewVM { Id = v.Id, Name = v.Name, FilterJson = v.FilterJson, IsShared = v.IsShared })
                .ToList();

        public int InsertSavedView(string name, string filterJson, bool isShared, int userId, int clientId)
            => _trackerRepo.InsertSavedView(new TrackerSavedViewDTO
            {
                UserId = userId, ClientId = clientId,
                Name = (name ?? "Untitled").Trim(),
                FilterJson = filterJson ?? "{}", IsShared = isShared
            });

        public int DeleteSavedView(int id, int userId, int clientId)
            => _trackerRepo.DeleteSavedView(id, userId, clientId);

        // Audit log
        public List<AuditLogItemVM> GetAuditLogs(int clientId, int limit)
            => (_trackerRepo.GetAuditLogs(clientId, limit) ?? new List<TrackerAuditLogDTO>())
                .Select(a => new AuditLogItemVM
                {
                    Id = a.Id, UserName = a.UserName, Action = a.Action,
                    AffectedCount = a.AffectedCount, Details = a.Details, CreatedOn = a.CreatedOn
                }).ToList();

        // Notifications
        public List<NotificationItemVM> GetNotifications(int userId, bool onlyUnread, int limit)
            => (_trackerRepo.GetNotifications(userId, onlyUnread, limit) ?? new List<TrackerNotificationDTO>())
                .Select(n => new NotificationItemVM
                {
                    Id = n.Id, Type = n.Type, Title = n.Title, Body = n.Body,
                    Link = n.Link, CaseId = n.CaseId, IsRead = n.IsRead, CreatedOn = n.CreatedOn
                }).ToList();

        public int CountUnreadNotifications(int userId) => _trackerRepo.CountUnreadNotifications(userId);
        public int MarkNotificationRead(int notificationId, int userId) => _trackerRepo.MarkNotificationRead(notificationId, userId);
        public int MarkAllNotificationsRead(int userId) => _trackerRepo.MarkAllNotificationsRead(userId);

        // SAR
        public SarLinkVM GetSarLink(int caseId, int clientId)
        {
            var dto = _trackerRepo.GetSarLink(caseId, clientId);
            if (dto == null)
                return new SarLinkVM { CaseId = caseId, SarStatus = "NotRequired" };
            return new SarLinkVM
            {
                CaseId = dto.CaseId,
                SarStatus = dto.SarStatus,
                SarReference = dto.SarReference,
                SarFilingDeadline = dto.SarFilingDeadline?.ToString("yyyy-MM-dd"),
                FiledOn = dto.FiledOn?.ToString("yyyy-MM-dd"),
                Notes = dto.Notes
            };
        }

        public int UpsertSarLink(SarLinkVM model, int userId)
        {
            DateTime? deadline = ParseDate(model.SarFilingDeadline);
            DateTime? filedOn = ParseDate(model.FiledOn);
            var dto = new TrackerSarLinkDTO
            {
                CaseId = model.CaseId,
                SarStatus = string.IsNullOrWhiteSpace(model.SarStatus) ? "NotRequired" : model.SarStatus,
                SarReference = model.SarReference,
                SarFilingDeadline = deadline,
                FiledOn = filedOn,
                Notes = model.Notes
            };
            return _trackerRepo.UpsertSarLink(dto, userId);
        }

        // Leaderboard
        public LeaderboardVM GetReviewerLeaderboard(int clientId, int daysBack)
        {
            var stats = _trackerRepo.GetReviewerStats(clientId, daysBack) ?? new List<ReviewerStatsDTO>();
            var vm = new LeaderboardVM { DaysBack = daysBack };
            vm.Reviewers = stats.Select(s => new ReviewerStatsVM
            {
                ReviewerId = s.ReviewerId,
                ReviewerName = (s.ReviewerName ?? "").Trim(),
                CasesReviewed = s.CasesReviewed,
                Approved = s.Approved,
                Rejected = s.Rejected,
                ApprovalRate = s.CasesReviewed > 0 ? Math.Round((double)s.Approved / s.CasesReviewed * 100, 1) : 0,
                AvgTurnaroundDays = Math.Round(s.AvgTurnaroundDays, 1),
                LastActivity = s.LastActivity
            }).ToList();
            return vm;
        }

        // ---------------- Reports ----------------

        public Customer360VM GetCustomer360(int customerMasterId, int clientId)
            => _trackerRepo.GetCustomer360(customerMasterId, clientId);

        public List<RiskOverrideRow> GetRiskOverrides(int clientId, int daysBack)
            => _trackerRepo.GetRiskOverrides(clientId, daysBack) ?? new List<RiskOverrideRow>();

        public PermissionHeatmapVM GetPermissionHeatmap(int clientId)
        {
            var raw = _trackerRepo.GetUserGroupRights(clientId) ?? new List<dynamic>();
            var modules = new List<string>();
            var groups = new Dictionary<string, PermissionGroupRow>();

            foreach (var row in raw)
            {
                string g = (string)row.GroupName;
                string m = (string)row.ModuleName;
                int rights = Convert.ToInt32(row.Rights);
                if (!modules.Contains(m)) modules.Add(m);
                if (!groups.TryGetValue(g, out var gr))
                {
                    gr = new PermissionGroupRow { GroupName = g };
                    groups[g] = gr;
                }
                gr.ModuleCounts[m] = rights;
                gr.TotalRights += rights;
            }
            return new PermissionHeatmapVM
            {
                Modules = modules.OrderBy(x => x).ToList(),
                Groups = groups.Values.OrderByDescending(g => g.TotalRights).ToList()
            };
        }

        public List<SanctionsFreshnessRow> GetSanctionsFreshness(int clientId)
            => _trackerRepo.GetSanctionsFreshness(clientId) ?? new List<SanctionsFreshnessRow>();

        public List<WhitelistAnalystRow> GetWhitelistPatterns(int clientId, int daysBack)
            => _trackerRepo.GetWhitelistAnalystPatterns(clientId, daysBack) ?? new List<WhitelistAnalystRow>();

        public List<InvestigationDepthRow> GetInvestigationDepth(int clientId)
        {
            var rows = _trackerRepo.GetInvestigationDepth(clientId) ?? new List<InvestigationDepthRow>();
            foreach (var r in rows)
            {
                r.StatusLabel = MapStatusLabel(r.Status);
                r.Bucket = r.CommentCount == 0 ? "None" : r.CommentCount <= 2 ? "Sparse" : r.CommentCount <= 5 ? "Adequate" : "Rich";
            }
            return rows;
        }

        public List<TopErrorRow> GetTopErrors(int daysBack, int limit)
            => _trackerRepo.GetTopErrors(daysBack, limit) ?? new List<TopErrorRow>();

        public QaThroughputVM GetQaThroughputReport(int clientId, int daysBack)
        {
            var pts = _trackerRepo.GetQaThroughput(clientId, daysBack) ?? new List<QaThroughputPoint>();
            var vm = new QaThroughputVM { DaysBack = daysBack, Points = pts };
            vm.TotalApproved = pts.Sum(p => p.Approved);
            vm.TotalRejected = pts.Sum(p => p.Rejected);
            vm.AvgTurnaroundHours = pts.Count == 0 ? 0 : Math.Round(pts.Average(p => p.AvgTurnaroundHours), 1);
            return vm;
        }

        public SlaBreachTrendVM GetSlaBreachTrend(int clientId)
        {
            // Computed from current Tracker state — no historical SLA snapshot table yet
            var rows = _trackerRepo.GetTrackerCases(clientId, "", "", "0", 10, "") ?? new List<TrackerCaseDTO>();
            var slaCfg = _trackerRepo.GetSlaConfigs(clientId) ?? new List<TrackerSlaConfigDTO>();
            var mapped = rows.Select(r => MapRow(r, slaCfg, null)).ToList();

            var vm = new SlaBreachTrendVM
            {
                OnTrack = mapped.Count(r => r.SlaStatus == "On Track"),
                AtRisk = mapped.Count(r => r.SlaStatus == "At Risk"),
                Breach = mapped.Count(r => r.SlaStatus == "Breach"),
                Closed = mapped.Count(r => r.SlaStatus == "Closed")
            };
            vm.Buckets.Add(new SlaBreachBucketVM { Bucket = "On Track", Count = vm.OnTrack });
            vm.Buckets.Add(new SlaBreachBucketVM { Bucket = "At Risk", Count = vm.AtRisk });
            vm.Buckets.Add(new SlaBreachBucketVM { Bucket = "Breach", Count = vm.Breach });
            vm.Buckets.Add(new SlaBreachBucketVM { Bucket = "Closed", Count = vm.Closed });
            return vm;
        }

        public List<SarRegisterRow> GetSarRegister(int clientId)
            => _trackerRepo.GetSarRegister(clientId) ?? new List<SarRegisterRow>();

        public List<FourEyesAttemptRow> GetFourEyesAttempts(int clientId, int limit)
            => _trackerRepo.GetFourEyesAttempts(clientId, limit) ?? new List<FourEyesAttemptRow>();

        public WorkloadHeatmapVM GetWorkloadHeatmap(int clientId)
        {
            var raw = _trackerRepo.GetWorkloadByOwner(clientId) ?? new List<dynamic>();
            var statuses = new List<string>();
            var owners = new Dictionary<int, WorkloadCellVM>();
            int unassigned = 0;

            foreach (var row in raw)
            {
                int ownerId = Convert.ToInt32(row.OwnerId);
                string ownerName = (string)row.OwnerName;
                int statusCode = Convert.ToInt32(row.Status);
                int cases = Convert.ToInt32(row.Cases);
                string statusLabel = MapStatusLabel(statusCode);

                if (ownerId == 0) { unassigned += cases; continue; }
                if (!statuses.Contains(statusLabel)) statuses.Add(statusLabel);
                if (!owners.TryGetValue(ownerId, out var cell))
                {
                    cell = new WorkloadCellVM { OwnerId = ownerId, OwnerName = ownerName };
                    owners[ownerId] = cell;
                }
                cell.StatusCounts[statusLabel] = cases;
                cell.Total += cases;
            }
            return new WorkloadHeatmapVM
            {
                StatusColumns = statuses.OrderBy(s => s).ToList(),
                Owners = owners.Values.OrderByDescending(o => o.Total).ToList(),
                Unassigned = unassigned
            };
        }

        public List<PeriodicReviewRow> GetPeriodicReviewCandidates(int clientId)
            => _trackerRepo.GetPeriodicReviewCandidates(clientId) ?? new List<PeriodicReviewRow>();

        public long PersistChatMessage(int clientId, int userId, string sessionId, int? caseId, string role, string messageText, string model, int? promptTokens, int? completionTokens, int? latencyMs)
        {
            try
            {
                return _trackerRepo.InsertChatMessage(clientId, userId, sessionId, caseId, role, messageText, model, promptTokens, completionTokens, latencyMs);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Chat persistence failed for session {0}", sessionId);
                return 0;
            }
        }

        public ChatAnalyticsVM GetChatAnalytics(int clientId, int daysBack)
            => _trackerRepo.GetChatAnalytics(clientId, daysBack) ?? new ChatAnalyticsVM();

        public List<HighRiskCustomerRow> GetHighRiskCustomers(int clientId)
            => _trackerRepo.GetHighRiskCustomers(clientId) ?? new List<HighRiskCustomerRow>();

        public CustomerDemographicsVM GetCustomerDemographics(int clientId)
            => _trackerRepo.GetCustomerDemographics(clientId) ?? new CustomerDemographicsVM();

        public List<LookbackRow> GetLookbackCandidates(int clientId, int daysBack)
            => _trackerRepo.GetLookbackCandidates(clientId, daysBack) ?? new List<LookbackRow>();

        public AnnualMlroVM GetAnnualMlroReport(int clientId, int year)
            => _trackerRepo.GetAnnualMlroReport(clientId, year) ?? new AnnualMlroVM { Year = year };

        public AgedCasesVM GetAgedCases(int clientId)
            => _trackerRepo.GetAgedCases(clientId) ?? new AgedCasesVM();

        public CustomerAcceptanceVM GetCustomerAcceptance(int clientId, int daysBack)
            => _trackerRepo.GetCustomerAcceptance(clientId, daysBack) ?? new CustomerAcceptanceVM { DaysBack = daysBack };

        public DocumentAuditVM GetDocumentAudit(int clientId)
            => _trackerRepo.GetDocumentAudit(clientId) ?? new DocumentAuditVM();

        public CohortComparisonVM GetCohortComparison(int customerMasterId, int clientId)
            => _trackerRepo.GetCohortComparison(customerMasterId, clientId) ?? new CohortComparisonVM { Found = false, NotFoundMessage = "No data." };

        public QuarterTrendVM GetQuarterlyTrend(int clientId, int quartersBack)
            => _trackerRepo.GetQuarterlyTrend(clientId, quartersBack) ?? new QuarterTrendVM { QuartersBack = quartersBack };

        public CustomerAssociationsVM GetCustomerAssociations(int clientId)
            => _trackerRepo.GetCustomerAssociations(clientId) ?? new CustomerAssociationsVM();

        public DailyCaseDigestVM GetDailyDigest(int userId, string userGroupName, int clientId)
        {
            var rows = _trackerRepo.GetTrackerCases(clientId, "", "", "0", 10, "") ?? new List<TrackerCaseDTO>();
            var slaCfg = _trackerRepo.GetSlaConfigs(clientId) ?? new List<TrackerSlaConfigDTO>();
            var mapped = rows.Select(r => MapRow(r, slaCfg, null)).ToList();
            var today = DateTime.UtcNow.Date;
            var vm = new DailyCaseDigestVM
            {
                Date = today,
                OpenedToday = mapped.Count(r => r.CreatedOn?.Date == today),
                ClosedToday = mapped.Count(r => IsTerminalStatus(r.StatusCode) && r.UpdatedOn?.Date == today),
                InFlight = mapped.Count(r => IsOpenStatus(r.StatusCode)),
                AwaitingQa = mapped.Count(r => r.QaEligible && r.QaStatusCode == (int)QaStatusCode.NotReviewed),
                SlaBreaching = mapped.Count(r => r.SlaStatus == "Breach"),
                HighRiskOpen = mapped.Count(r => IsRiskTier(r.RiskTier, "High") && IsOpenStatus(r.StatusCode)),
                PendingSrMgmt = mapped.Count(r => r.StatusCode == (int)ReportsCaseStatus.PendingWithSeniorManagement),
                NewToday = mapped.Where(r => r.CreatedOn?.Date == today).OrderByDescending(r => r.CreatedOn).Take(20).ToList(),
                ClosedTodayList = mapped.Where(r => IsTerminalStatus(r.StatusCode) && r.UpdatedOn?.Date == today).OrderByDescending(r => r.UpdatedOn).Take(20).ToList()
            };
            vm.ByStatus = mapped.GroupBy(r => r.StatusLabel)
                .Select(g => new DigestStatusBucket { Status = g.Key, Count = g.Count() })
                .OrderByDescending(b => b.Count).ToList();
            return vm;
        }

        // ---------------- /Reports ----------------

        // ---------- helpers ----------

        private void WriteAudit(int clientId, int userId, string userName, string action, int affected, IEnumerable<int> caseIds, string details)
        {
            try
            {
                _trackerRepo.InsertAuditLog(new TrackerAuditLogDTO
                {
                    ClientId = clientId, UserId = userId, UserName = userName ?? "",
                    Action = action, AffectedCount = affected,
                    CaseIdsCsv = caseIds == null ? "" : string.Join(",", caseIds),
                    Details = details ?? ""
                });
            }
            catch (Exception ex) { _log.Warn(ex, "Audit log insert failed for {0}", action); }
        }

        private CycleTimeChartVM BuildCycleTimeChart(int clientId)
        {
            var raw = _trackerRepo.GetQaCycleTime(clientId, 30) ?? new List<CycleTimePointDTO>();
            return new CycleTimeChartVM
            {
                Points = raw.Select(p => new CycleTimeBucketVM
                {
                    Day = p.Day,
                    Decisions = p.Decisions,
                    AvgHours = Math.Round(p.MedianHours, 1)
                }).ToList()
            };
        }

        private void ComputeWeekOverWeekTrends(AmlTrackerIndexVM vm, List<TrackerCaseDTO> raw)
        {
            var now = DateTime.UtcNow;
            var thisWeekStart = now.AddDays(-7);
            var lastWeekStart = now.AddDays(-14);

            int thisCreated = raw.Count(r => r.CreatedOn >= thisWeekStart);
            int lastCreated = raw.Count(r => r.CreatedOn >= lastWeekStart && r.CreatedOn < thisWeekStart);
            int thisCompleted = raw.Count(r => IsTerminalStatus(r.Status) && r.UpdatedOn >= thisWeekStart);
            int lastCompleted = raw.Count(r => IsTerminalStatus(r.Status) && r.UpdatedOn >= lastWeekStart && r.UpdatedOn < thisWeekStart);
            int thisQaApp = raw.Count(r => r.QaStatus == (int)QaStatusCode.Approved && r.QaReviewedOn >= thisWeekStart);
            int lastQaApp = raw.Count(r => r.QaStatus == (int)QaStatusCode.Approved && r.QaReviewedOn >= lastWeekStart && r.QaReviewedOn < thisWeekStart);
            int thisQaRej = raw.Count(r => r.QaStatus == (int)QaStatusCode.Rejected && r.QaReviewedOn >= thisWeekStart);
            int lastQaRej = raw.Count(r => r.QaStatus == (int)QaStatusCode.Rejected && r.QaReviewedOn >= lastWeekStart && r.QaReviewedOn < thisWeekStart);

            vm.TotalTrend = new TrendDelta { Current = thisCreated, Previous = lastCreated };
            vm.CompletedTrend = new TrendDelta { Current = thisCompleted, Previous = lastCompleted };
            vm.QaApprovedTrend = new TrendDelta { Current = thisQaApp, Previous = lastQaApp };
            vm.QaRejectedTrend = new TrendDelta { Current = thisQaRej, Previous = lastQaRej };
        }

        private void ComputeKpis(AmlTrackerIndexVM vm)
        {
            var rows = vm.Rows;
            vm.TotalCount = rows.Count;
            vm.OpenCount = rows.Count(r => IsOpenStatus(r.StatusCode));
            vm.PendingSrMgmtCount = rows.Count(r => r.StatusCode == (int)ReportsCaseStatus.PendingWithSeniorManagement);
            vm.CompletedCount = rows.Count(r => IsTerminalStatus(r.StatusCode));
            vm.SlaBreachCount = rows.Count(r => string.Equals(r.SlaStatus, "Breach", StringComparison.OrdinalIgnoreCase));

            vm.HighRiskCount = rows.Count(r => IsRiskTier(r.RiskTier, "High"));
            vm.MediumRiskCount = rows.Count(r => IsRiskTier(r.RiskTier, "Medium"));
            vm.LowRiskCount = rows.Count(r => IsRiskTier(r.RiskTier, "Low"));
            vm.UnclassifiedRiskCount = rows.Count(r => string.IsNullOrWhiteSpace(r.RiskTier) || string.Equals(r.RiskTier, "Unclassified", StringComparison.OrdinalIgnoreCase));

            vm.ApprovedCount = rows.Count(r => r.StatusCode == (int)ReportsCaseStatus.Approved);
            vm.RejectedCount = rows.Count(r => r.StatusCode == (int)ReportsCaseStatus.Rejected);
            vm.WhitelistedCount = rows.Count(r => r.StatusCode == (int)ReportsCaseStatus.WhiteListed);
            vm.OnHoldCount = rows.Count(r => r.StatusCode == (int)ReportsCaseStatus.OnHold);
            vm.AutoCount = rows.Count(r => r.StatusCode == (int)ReportsCaseStatus.Auto);

            vm.IndividualCount = rows.Count(r => string.Equals(r.CustomerType, "Individual", StringComparison.OrdinalIgnoreCase));
            vm.CorporateCount = rows.Count(r => string.Equals(r.CustomerType, "Corporate", StringComparison.OrdinalIgnoreCase));

            vm.PepHits = rows.Count(r => r.IsPep);
            vm.SanctionHits = rows.Count(r => r.IsSanction);
            vm.AdverseMediaHits = rows.Count(r => r.IsAdverseMedia);

            var today = DateTime.UtcNow.Date;
            vm.TodayCount = rows.Count(r => r.CreatedOn?.Date == today);
            vm.Last7DaysCount = rows.Count(r => r.CreatedOn != null && (today - r.CreatedOn.Value.Date).TotalDays <= 7);

            var ages = rows.Where(r => r.AgingDays.HasValue).Select(r => r.AgingDays.Value).ToList();
            vm.AvgAgingDays = ages.Count > 0 ? Math.Round(ages.Average(), 1) : 0;
            vm.MaxAgingDays = ages.Count > 0 ? ages.Max() : 0;

            vm.QaPendingCount = rows.Count(r => r.QaStatusCode == (int)QaStatusCode.Pending);
            vm.QaApprovedCount = rows.Count(r => r.QaStatusCode == (int)QaStatusCode.Approved);
            vm.QaRejectedCount = rows.Count(r => r.QaStatusCode == (int)QaStatusCode.Rejected);
            vm.QaNotReviewedCount = rows.Count(r => r.QaStatusCode == (int)QaStatusCode.NotReviewed);
            vm.QaEligibleNotReviewedCount = rows.Count(r => r.QaEligible && r.QaStatusCode == (int)QaStatusCode.NotReviewed);
            vm.QaSlaBreachCount = rows.Count(r => string.Equals(r.QaSlaStatus, "Breach", StringComparison.OrdinalIgnoreCase));
            vm.LastQaActivityAt = rows.Where(r => r.QAReviewdOn.HasValue).Select(r => r.QAReviewdOn.Value).OrderByDescending(d => d).FirstOrDefault();
            var qaAges = rows.Where(r => r.DaysSinceQa.HasValue).Select(r => r.DaysSinceQa.Value).ToList();
            vm.AvgDaysSinceQa = qaAges.Count > 0 ? Math.Round(qaAges.Average(), 1) : 0;

            vm.StaleCount = rows.Count(r => r.IsStale);
            vm.NewSinceLastVisitCount = rows.Count(r => r.IsNewSinceLastVisit);
            vm.SarRequiredCount = rows.Count(r => string.Equals(r.SarStatus, "Required", StringComparison.OrdinalIgnoreCase) || string.Equals(r.SarStatus, "Drafted", StringComparison.OrdinalIgnoreCase));
            vm.SarFiledCount = rows.Count(r => string.Equals(r.SarStatus, "Filed", StringComparison.OrdinalIgnoreCase));
            vm.SarOverdueCount = rows.Count(r => r.SarFilingDeadline.HasValue && r.SarFilingDeadline.Value < DateTime.UtcNow.Date && !string.Equals(r.SarStatus, "Filed", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsRiskTier(string tier, string match)
            => !string.IsNullOrWhiteSpace(tier) && tier.IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0;

        private AmlTrackerRowVM MapRow(TrackerCaseDTO c, List<TrackerSlaConfigDTO> slaCfg, DateTime? lastVisitedAt)
        {
            var fullName = JoinName(c.FirstName, c.MiddleName, c.LastName);
            var custTypeLabel = MapCustomerTypeLabel(c.CustomerType);

            var finalRisk = !string.IsNullOrWhiteSpace(c.IndividualRiskScore) ? c.IndividualRiskScore : c.CorporateRiskScore;
            var riskTier = NormalizeRiskTier(finalRisk);

            var aging = c.CreatedOn.HasValue ? (int?)Math.Max(0, (DateTime.UtcNow - c.CreatedOn.Value).Days) : null;

            var matched = ResolveSla(slaCfg, c.CustomerType, riskTier);
            var slaStatus = ComputeSlaStatus(c.Status, aging, matched.CaseSlaOnTrackDays, matched.CaseSlaAtRiskDays);

            var isPep = (c.TrueDomesticPep ?? 0) > 0 || (c.TrueForeignPep ?? 0) > 0
                || (c.PartialDomesticPep ?? 0) > 0 || (c.PartialForeignPep ?? 0) > 0
                || ContainsAny(c.Source, "PEP");
            var isSanction = (c.TrueUaeUnSanction ?? 0) > 0 || (c.TrueOtherSanction ?? 0) > 0
                || ContainsAny(c.Source, "OFAC", "UN", "EU", "SANCTION", "SDN", "CB");
            var isAdverseMedia = (c.TrueAdverseMedia ?? 0) > 0 || (c.PartialAdverseMedia ?? 0) > 0
                || ContainsAny(c.Source, "ADVERSE", "MEDIA");

            var qaStatusCode = c.QaStatus ?? 0;
            var daysSinceQa = c.QaReviewedOn.HasValue ? (int?)Math.Max(0, (DateTime.UtcNow - c.QaReviewedOn.Value).Days) : null;
            var qaEligible = IsTerminalStatus(c.Status);

            int? daysClosedWithoutQa = null;
            if (qaEligible
                && (qaStatusCode == (int)QaStatusCode.NotReviewed || qaStatusCode == (int)QaStatusCode.Pending)
                && c.UpdatedOn.HasValue)
            {
                daysClosedWithoutQa = Math.Max(0, (DateTime.UtcNow - c.UpdatedOn.Value).Days);
            }
            var qaSla = ComputeQaSla(qaEligible, qaStatusCode, daysClosedWithoutQa, matched.QaSlaOnTrackDays, matched.QaSlaAtRiskDays);

            var lastTouched = c.UpdatedOn ?? c.CreatedOn;
            var isStale = lastTouched.HasValue && (DateTime.UtcNow - lastTouched.Value).TotalDays > matched.StaleCaseDays && !IsTerminalStatus(c.Status);
            var isNew = lastVisitedAt.HasValue && c.CreatedOn.HasValue && c.CreatedOn.Value > lastVisitedAt.Value;

            int? daysToSarDeadline = null;
            if (c.SarFilingDeadline.HasValue)
                daysToSarDeadline = (int)(c.SarFilingDeadline.Value.Date - DateTime.UtcNow.Date).TotalDays;

            return new AmlTrackerRowVM
            {
                CaseId = c.CaseId,
                CustomerMasterId = c.CustomerMasterId,
                CustomerCode = c.CustomerCode,
                CustomerName = fullName,
                CustomerType = custTypeLabel,
                Nationality = c.Nationality,

                StatusCode = c.Status,
                StatusLabel = MapStatusLabel(c.Status),
                StageLabel = MapStageLabel(c.Status),

                MatchScore = c.MatchScore,
                RiskScore = c.RiskScore,
                FinalRiskScore = finalRisk,
                RiskTier = riskTier,
                IsWhiteListed = c.IsWhiteListed,

                IsPep = isPep, IsSanction = isSanction, IsAdverseMedia = isAdverseMedia,

                CreatedOn = c.CreatedOn, UpdatedOn = c.UpdatedOn, WhitelistedOn = c.DateOfWhitelisting,
                AgingDays = aging, SlaStatus = slaStatus,

                CreatedUser = (c.CreatedUser ?? "").Trim(),
                UpdatedUser = (c.UpdatedUser ?? "").Trim(),
                OwnerId = c.Owner, OwnerName = (c.OwnerName ?? "").Trim(),
                Source = c.Source,
                DaysSinceQa = daysSinceQa,
                QaEligible = qaEligible,

                DaysClosedWithoutQa = daysClosedWithoutQa,
               

                IsStale = isStale,
                IsNewSinceLastVisit = isNew,

                SarStatus = c.SarStatus,
                SarReference = c.SarReference,
                SarFilingDeadline = c.SarFilingDeadline,
                SarFiledOn = c.SarFiledOn,
                DaysToSarDeadline = daysToSarDeadline,

                // Compliance-export fields (Bucket A — straight from joined customermaster)
                ClientCifNumber = c.ClientCifNumber,
                ProductType = c.ProductType,
                ProductValueAed = c.ProductValueAed,
                IdType = c.IdType,
                IdNumber = c.IdNumber,
                IdExpiry = c.IdExpiry,
                OtherSanctionFlag = (c.TrueOtherSanction ?? 0) > 0,
                SeniorMgmtApprovalStatus = MapSeniorMgmtApprovalStatus(c.Status),
                PaymentMode = c.PaymentMode,
                DeliveryChannel = c.DeliveryChannel,
                ResidentStatus = c.ResidentStatus,
                Profession = c.Profession,

                // Compliance-export fields (Bucket B — derived / typed comments)
                NewOrExisting = DeriveNewOrExisting(c.CustomerCreatedOn, c.CreatedOn),
                CommentsPep = c.CommentsPep,
                CommentsUaeUnsc = c.CommentsUaeUnsc,
                CommentsAdverseMedia = c.CommentsAdverseMedia,
                CommentsOtherSanction = c.CommentsOtherSanction,
                OtherComments = c.CommentsGeneral,
                RiskAssessmentDate = c.RiskAssessmentDate,

                // Compliance-export fields (Bucket C — populated post-migration)
                PolicyIssueDate = c.PolicyIssueDate,
                DateOfResponse = c.DateOfResponse,
                ClientProductNumber = c.ClientProductNumber,
                PolicyHolder = c.PolicyHolder,
                KycCheck = c.KycCheck,
                KycCheckComments = c.KycCheckComments,
                SeniorMgmtApprovalDate = c.SeniorMgmtApprovalDate,
                SanctionsScreeningDate = c.SanctionsScreeningDate,
                DomesticPep=c.DomesticPep,
                ForeginPep=c.ForeginPep,
                RedFlags=c.RedFlags,
                HighNetwork=c.HighNetwork,
                UAEUNSanction=c.UAEUNSanction,
                OtherSanction=c.OtherSanction,
                BusinessType=c.BusinessType,
                LegalStatus=c.LegalStatus,
                ClientStatus=c.ClientStatus,
                Employer=c.Employer,
                EmployerIndustry=c.EmployerIndustry,
                EmployerSector=c.EmployerSector,
                EmiratesIdNumber=c.EmiratesIdNumber,
                EmiratesIdIssueDate=c.EmiratesIdIssueDate,
                EmiratesIdExpiryDate=c.EmiratesIdExpiryDate,
                PassportId=c.PassportId,
                PassportIssueDate=c.PassportIssueDate,
                PassportExpiryDate=c.PassportExpiryDate,
                ChangeStatus = c.ChangeStatus,
                StatusName=c.StatusName,
                Gender=c.Gender,
                SOWSOFCountry=c.SOWSOFCountry,
                ProductValue=c.ProductValue,
                ProductRefNo=c.ProductRefNo,
                CounterParty=c.CounterParty,
                CounterPartyName=c.CounterPartyName,
                DOB=c.DOB,
                Individual_final_risk_score=c.Individual_final_risk_score,
                corporate_final_risk_score=c.corporate_final_risk_score,
                QAComments=c.QAComments,
                QAReason=c.QAReason,
                QAReviewerName=c.QAReviewerName,
                QAReviewdOn=c.QAReviewdOn,
                QAStatus=c.QAStatus,
                GroupId=c.GroupId


            };
        }

        // "New" if the customer master row was created within ~1 minute of the case;
        // "Existing" if the customer pre-dated the case by more than that.
        private static string DeriveNewOrExisting(DateTime? customerCreatedOn, DateTime? caseCreatedOn)
        {
            if (!customerCreatedOn.HasValue || !caseCreatedOn.HasValue) return "";
            var lagSeconds = (caseCreatedOn.Value - customerCreatedOn.Value).TotalSeconds;
            return lagSeconds > 60 ? "Existing" : "New";
        }

        // Map customercase.status numeric to the senior-management approval gate.
        // Status 4 = PendingWithSeniorManagement (per Common.cs). Cases that have moved past 4 are treated as approved.
        private static string MapSeniorMgmtApprovalStatus(int status)
        {
            return status switch
            {
                0 => "",            // Open / new
                1 => "",            // Auto / not yet at SR Mgmt gate
                2 => "",            // In review
                3 => "",            // Compliance-only review
                4 => "Pending",     // Pending with Senior Management
                5 => "Approved",
                6 => "Rejected",
                7 => "Approved",    // Closed-approved variants
                _ => ""
            };
        }

        // Match the most specific SLA config. Specificity priority: (custType+riskTier) > (custType,*) > (*,riskTier) > (*,*) > defaults
        private static (int CaseSlaOnTrackDays, int CaseSlaAtRiskDays, int QaSlaOnTrackDays, int QaSlaAtRiskDays, int StaleCaseDays)
            ResolveSla(List<TrackerSlaConfigDTO> cfgs, string customerType, string riskTier)
        {
            if (cfgs == null || cfgs.Count == 0)
                return (DefaultSlaOnTrackDays, DefaultSlaAtRiskDays, DefaultSlaOnTrackDays, DefaultSlaAtRiskDays, DefaultStaleCaseDays);

            string ct = string.IsNullOrWhiteSpace(customerType) ? "*" : customerType;
            string rt = string.IsNullOrWhiteSpace(riskTier) ? "*" : riskTier;

            TrackerSlaConfigDTO Pick(string c, string r)
                => cfgs.FirstOrDefault(x => string.Equals(x.CustomerType, c, StringComparison.OrdinalIgnoreCase) && string.Equals(x.RiskTier, r, StringComparison.OrdinalIgnoreCase));

            var cfg = Pick(ct, rt) ?? Pick(ct, "*") ?? Pick("*", rt) ?? Pick("*", "*");
            if (cfg == null)
                return (DefaultSlaOnTrackDays, DefaultSlaAtRiskDays, DefaultSlaOnTrackDays, DefaultSlaAtRiskDays, DefaultStaleCaseDays);
            return (cfg.CaseSlaOnTrackDays, cfg.CaseSlaAtRiskDays, cfg.QaSlaOnTrackDays, cfg.QaSlaAtRiskDays, cfg.StaleCaseDays);
        }

        private static string ComputeQaSla(bool eligible, int qaStatusCode, int? daysClosedWithoutQa, int onTrack, int atRisk)
        {
            if (!eligible) return "—";
            if (qaStatusCode == (int)QaStatusCode.Approved || qaStatusCode == (int)QaStatusCode.Rejected) return "Reviewed";
            if (daysClosedWithoutQa == null) return "—";
            if (daysClosedWithoutQa <= onTrack) return "On Track";
            if (daysClosedWithoutQa <= atRisk) return "At Risk";
            return "Breach";
        }

        private static bool ContainsAny(string s, params string[] terms)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var upper = s.ToUpperInvariant();
            return terms.Any(t => upper.Contains(t.ToUpperInvariant()));
        }

        private static string NormalizeRiskTier(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Unclassified";
            var s = raw.ToLowerInvariant();
            if (s.Contains("high")) return "High";
            if (s.Contains("medium")) return "Medium";
            if (s.Contains("low")) return "Low";
            if (s.Contains("unclassified")) return "Unclassified";
            return raw.Trim();
        }

        private static List<AmlTrackerRowVM> ApplySort(List<AmlTrackerRowVM> rows, string sortBy, string sortDir)
        {
            var desc = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
            sortBy = sortBy ?? "CreatedOn";

            IOrderedEnumerable<AmlTrackerRowVM> sorted = sortBy.ToLowerInvariant() switch
            {
                "caseid" => desc ? rows.OrderByDescending(r => r.CaseId) : rows.OrderBy(r => r.CaseId),
                "customername" => desc ? rows.OrderByDescending(r => r.CustomerName ?? "") : rows.OrderBy(r => r.CustomerName ?? ""),
                "status" => desc ? rows.OrderByDescending(r => r.StatusCode) : rows.OrderBy(r => r.StatusCode),
                "matchscore" => desc ? rows.OrderByDescending(r => r.MatchScore ?? 0) : rows.OrderBy(r => r.MatchScore ?? 0),
                "riskscore" => desc ? rows.OrderByDescending(r => r.RiskScore ?? 0) : rows.OrderBy(r => r.RiskScore ?? 0),
                "risktier" => desc ? rows.OrderByDescending(r => RiskTierRank(r.RiskTier)) : rows.OrderBy(r => RiskTierRank(r.RiskTier)),
                "aging" => desc ? rows.OrderByDescending(r => r.AgingDays ?? -1) : rows.OrderBy(r => r.AgingDays ?? int.MaxValue),
                "qastatus" => desc ? rows.OrderByDescending(r => r.QaStatusCode) : rows.OrderBy(r => r.QaStatusCode),
                "dayssinceqa" => desc ? rows.OrderByDescending(r => r.DaysSinceQa ?? -1) : rows.OrderBy(r => r.DaysSinceQa ?? int.MaxValue),
                "qasla" => desc ? rows.OrderByDescending(r => r.DaysClosedWithoutQa ?? -1) : rows.OrderBy(r => r.DaysClosedWithoutQa ?? int.MaxValue),
                "createdon" => desc ? rows.OrderByDescending(r => r.CreatedOn ?? DateTime.MinValue) : rows.OrderBy(r => r.CreatedOn ?? DateTime.MaxValue),
                "updatedon" => desc ? rows.OrderByDescending(r => r.UpdatedOn ?? DateTime.MinValue) : rows.OrderBy(r => r.UpdatedOn ?? DateTime.MaxValue),
                _ => desc ? rows.OrderByDescending(r => r.CreatedOn ?? DateTime.MinValue) : rows.OrderBy(r => r.CreatedOn ?? DateTime.MaxValue),
            };
            return sorted.ToList();
        }

        private static int RiskTierRank(string tier) => tier switch { "High" => 3, "Medium" => 2, "Low" => 1, _ => 0 };

        private static string JoinName(string first, string middle, string last)
        {
            var parts = new[] { first, middle, last }.Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(" ", parts).Trim();
        }

        private static string MapCustomerTypeLabel(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "";
            return code.Trim() switch
            {
                "I" => "Individual", "C" => "Corporate", "S" => "Shareholder",
                "B" => "Beneficiary", "V" => "Vendor", _ => code.Trim()
            };
        }

        private static string MapStatusLabel(int s) => s switch
        {
            (int)ReportsCaseStatus.Pending => "Pending",
            (int)ReportsCaseStatus.WhiteListed => "Whitelisted",
            (int)ReportsCaseStatus.Approved => "Approved",
            (int)ReportsCaseStatus.Rejected => "Rejected",
            (int)ReportsCaseStatus.PendingWithSeniorManagement => "Pending Sr. Mgmt",
            (int)ReportsCaseStatus.Auto => "Auto-cleared",
            (int)ReportsCaseStatus.PendingCaseCreatedFromDailyScheduler => "Daily Batch",
            (int)ReportsCaseStatus.OnHold => "On Hold",
            _ => $"Status {s}"
        };

        private static string MapStageLabel(int s) => s switch
        {
            (int)ReportsCaseStatus.Pending => "Awaiting Analyst Review",
            (int)ReportsCaseStatus.WhiteListed => "Closed — Whitelisted",
            (int)ReportsCaseStatus.Approved => "Closed — Approved",
            (int)ReportsCaseStatus.Rejected => "Closed — Rejected",
            (int)ReportsCaseStatus.PendingWithSeniorManagement => "Awaiting Sr. Mgmt Approval",
            (int)ReportsCaseStatus.Auto => "Closed — Auto",
            (int)ReportsCaseStatus.PendingCaseCreatedFromDailyScheduler => "New (Daily Batch)",
            (int)ReportsCaseStatus.OnHold => "On Hold",
            _ => "—"
        };

        private static string MapQaStatusLabel(int qa) => qa switch
        {
            (int)QaStatusCode.Pending => "Pending QA",
            (int)QaStatusCode.Approved => "QA Approved",
            (int)QaStatusCode.Rejected => "QA Rejected",
            (int)QaStatusCode.Reopened => "QA Reopened",
            _ => "Not Reviewed"
        };

        private static string MapQaOutcome(int qa) => qa switch
        {
            (int)QaStatusCode.Pending => "Pending",
            (int)QaStatusCode.Approved => "Approved",
            (int)QaStatusCode.Rejected => "Rejected",
            (int)QaStatusCode.Reopened => "Reopened",
            _ => "NotReviewed"
        };

        private static bool IsOpenStatus(int s)
            => s == (int)ReportsCaseStatus.Pending
            || s == (int)ReportsCaseStatus.PendingWithSeniorManagement
            || s == (int)ReportsCaseStatus.PendingCaseCreatedFromDailyScheduler
            || s == (int)ReportsCaseStatus.OnHold;

        private static bool IsTerminalStatus(int s)
            => s == (int)ReportsCaseStatus.WhiteListed
            || s == (int)ReportsCaseStatus.Approved
            || s == (int)ReportsCaseStatus.Rejected
            || s == (int)ReportsCaseStatus.Auto;

        private static string ComputeSlaStatus(int status, int? aging, int onTrack, int atRisk)
        {
            if (IsTerminalStatus(status)) return "Closed";
            if (aging == null) return "—";
            if (aging.Value <= onTrack) return "On Track";
            if (aging.Value <= atRisk) return "At Risk";
            return "Breach";
        }

        private static DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParse(s.Trim(), out var d)) return d;
            return null;
        }
    }
}
