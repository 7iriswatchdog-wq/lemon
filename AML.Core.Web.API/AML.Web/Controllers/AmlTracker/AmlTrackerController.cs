using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.AmlTracker;
using AML.Core.ServiceContract.UserAccess;
using AML.Core.ServiceContract.UserGroup;
using AML.ViewModel.ViewModels.AmlTracker;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AML.Web.Controllers.AmlTracker
{
    [SessionAuthorize]
    [Route("AmlTracker")]
    public class AmlTrackerController : Controller
    {
        private readonly IAmlTrackerService _trackerService;
        private readonly IHttpClientHandler _clientHandler;
        private readonly IUserGroupService _userGroupService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        IUserGroupRightService _userGroupRightService;

        private const string LastVisitCookie = "AmlTrackerLastVisitedAt";

        public AmlTrackerController(
            IAmlTrackerService trackerService,
            IHttpClientHandler clientHandler,
            IUserGroupService userGroupService,
            IHttpContextAccessor httpContextAccessor, IUserGroupRightService userGroupRightService)
        {
            _trackerService = trackerService;
            _clientHandler = clientHandler;
            _userGroupService = userGroupService;
            _httpContextAccessor = httpContextAccessor;
            _userGroupRightService = userGroupRightService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index(AmlTrackerFilterVM filter)
        {
            var ctx = GetCurrentUserContext();
            filter ??= new AmlTrackerFilterVM();

            var lastVisited = ReadLastVisitCookie();

           



            // _mapper.Map<List<CaseModel>>(_trackerService.getAmlTrackerCases(filter, ctx.UserId, ctx.UserGroupName, ctx.ClientId, lastVisited);
            var vm = _trackerService.GetTracker(filter, ctx.UserId, ctx.UserGroupName, ctx.ClientId, lastVisited);

            var QCAccess = _userGroupRightService.CheckUserRightExixts("AmlTracker", "qcAccess", ctx.UserId, ctx.groupId, ctx.sessionId);
            vm.CurrentUserCanQa = QCAccess.Result;
            if (TempData["FlashMessage"] is string msg)
            {
                vm.FlashMessage = msg;
                vm.FlashLevel = TempData["FlashLevel"] as string ?? "info";
            }

            // Update last-visited cookie *after* computing IsNew flags
            WriteLastVisitCookie(DateTime.UtcNow);

            return View(vm);
        }

        [HttpGet("Workspace")]
        public IActionResult Workspace()
        {
            var ctx = GetCurrentUserContext();
            var vm = _trackerService.GetQaWorkspace(ctx.UserId, ctx.UserGroupName, ctx.ClientId);
            if (TempData["FlashMessage"] is string msg)
            {
                vm.FlashMessage = msg;
                vm.FlashLevel = TempData["FlashLevel"] as string ?? "info";
            }
            return View(vm);
        }

        // ---------------- Reports hub + reports ----------------

        [HttpGet("Reports")]
        public IActionResult Reports()
        {
            var vm = new AML.ViewModel.ViewModels.AmlTracker.ReportsHubVM();
            void Add(string code, string name, string desc, string cat, string url, string status)
                => vm.Tiles.Add(new AML.ViewModel.ViewModels.AmlTracker.ReportTile { Code = code, Name = name, Description = desc, Category = cat, Url = url, Status = status });

            // Operational
            Add("R1", "Daily Case Digest", "Today's opened / closed / in-flight summary.", "Operational", Url.Action("DailyDigest"), "Ready");
            Add("R3", "Aged Cases Register", "Open cases bucketed by age, sorted oldest first.", "Operational", Url.Action("AgedCases"), "Ready");
            Add("R52", "Quarterly Trend", "Case volumes per quarter, last 8 quarters by default.", "Operational", Url.Action("Quarterly"), "Ready");
            // KYC additions
            Add("R32", "New Customer Acceptance", "Customers onboarded recently with initial risk + intake-by-day chart.", "KYC", Url.Action("CustomerAcceptance"), "Ready");
            Add("R19", "Cohort Comparison", "Compare a customer to peers (same type + nationality).", "KYC", Url.Action("Cohort"), "Ready");
            // Audit
            Add("F3.4", "Document Audit", "Documents attached to cases — type, expiry, who uploaded when.", "Audit", Url.Action("DocumentAudit"), "Ready");
            // Risk
            Add("R20", "High-Risk Customer Register", "All customers currently rated High Risk with cases, last assessment.", "Risk", Url.Action("HighRiskRegister"), "Ready");
            Add("R39", "Look-back Register", "Customers whose risk just escalated to High — past activity needs review.", "Risk", Url.Action("Lookback"), "Ready");
            // KYC / Customer
            Add("R28", "Customer Demographics", "Distribution by type, nationality, profession, sector, residence.", "KYC", Url.Action("CustomerDemographics"), "Ready");
            // Compliance / Regulator
            Add("R40", "Annual MLRO Report", "Yearly aggregate compliance pack — case volumes, QA, risk, screening, SAR.", "Compliance", Url.Action("AnnualMlro"), "Ready");
            // Tier 1
            Add("F1.3", "Risk Override Audit", "Who overrode risk scores, when, and why.", "Risk", Url.Action("RiskOverrideReport"), "Ready");
            Add("F1.4", "Permission Heatmap", "User-group × module rights matrix.", "Audit", Url.Action("PermissionHeatmap"), "Ready");
            Add("F1.5", "Sanctions Freshness", "Per-source hit volume and last-seen.", "Screening", Url.Action("SanctionsFreshness"), "Ready");
            Add("F1.6", "Whitelist Patterns", "Who is whitelisting customers, and how often.", "Screening", Url.Action("WhitelistPatterns"), "Ready");
            Add("F1.7", "Case Investigation Depth", "Cases by comment-richness bucket.", "Operational", Url.Action("InvestigationDepth"), "Ready");
            Add("F1.8", "Top Error Trends", "Most frequent application errors (last 30 days).", "Engineering", Url.Action("TopErrors"), "Ready");
            // Tier 2
            Add("F2.1", "QA Throughput", "Daily QA decisions and turnaround.", "QA", Url.Action("QaThroughput"), "Tier 2");
            Add("F2.2", "SLA Breach Trend", "Distribution of cases across SLA buckets.", "Operational", Url.Action("SlaBreachTrend"), "Tier 2");
            Add("F2.5", "SAR / STR Filing Register", "All SAR-linked cases with deadline.", "Compliance", Url.Action("SarRegister"), "Tier 2");
            Add("F2.6", "4-Eyes Blocked Attempts", "Self-approval attempts caught by maker-checker.", "Audit", Url.Action("FourEyesAttempts"), "Tier 2");
            // Tier 3
            Add("F3.3", "Workload Heatmap", "Open cases per analyst × status.", "Operational", Url.Action("WorkloadHeatmap"), "Tier 3");
            Add("F3.5", "Periodic Review Register", "Customers due for KYC refresh.", "Compliance", Url.Action("PeriodicReviewRegister"), "Tier 3");
            Add("F3.2", "AI Chat Analytics", "Chat usage & per-user activity (populates as chat is used).", "AI", Url.Action("ChatAnalytics"), "Tier 3 (live)");

            return View(vm);
        }

        [HttpGet("Reports/Customer/{cmid:int}")]
        public IActionResult Customer360(int cmid)
        {
            var ctx = GetCurrentUserContext();
            var vm = _trackerService.GetCustomer360(cmid, ctx.ClientId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpGet("Reports/RiskOverride")]
        public IActionResult RiskOverrideReport(int days = 365)
        {
            var ctx = GetCurrentUserContext();
            var rows = _trackerService.GetRiskOverrides(ctx.ClientId, days);
            ViewBag.Days = days;
            return View("Reports/RiskOverride", rows);
        }

        [HttpGet("Reports/PermissionHeatmap")]
        public IActionResult PermissionHeatmap()
        {
            var ctx = GetCurrentUserContext();
            var vm = _trackerService.GetPermissionHeatmap(ctx.ClientId);
            return View("Reports/PermissionHeatmap", vm);
        }

        [HttpGet("Reports/SanctionsFreshness")]
        public IActionResult SanctionsFreshness()
        {
            var ctx = GetCurrentUserContext();
            var rows = _trackerService.GetSanctionsFreshness(ctx.ClientId);
            return View("Reports/SanctionsFreshness", rows);
        }

        [HttpGet("Reports/WhitelistPatterns")]
        public IActionResult WhitelistPatterns(int days = 90)
        {
            var ctx = GetCurrentUserContext();
            var rows = _trackerService.GetWhitelistPatterns(ctx.ClientId, days);
            ViewBag.Days = days;
            return View("Reports/WhitelistPatterns", rows);
        }

        [HttpGet("Reports/InvestigationDepth")]
        public IActionResult InvestigationDepth()
        {
            var ctx = GetCurrentUserContext();
            var rows = _trackerService.GetInvestigationDepth(ctx.ClientId);
            return View("Reports/InvestigationDepth", rows);
        }

        [HttpGet("Reports/TopErrors")]
        public IActionResult TopErrors(int days = 30, int limit = 50)
        {
            var rows = _trackerService.GetTopErrors(days, Math.Clamp(limit, 5, 500));
            ViewBag.Days = days;
            return View("Reports/TopErrors", rows);
        }

        [HttpGet("Reports/QaThroughput")]
        public IActionResult QaThroughput(int days = 30)
        {
            var ctx = GetCurrentUserContext();
            var vm = _trackerService.GetQaThroughputReport(ctx.ClientId, days);
            return View("Reports/QaThroughput", vm);
        }

        [HttpGet("Reports/SlaBreachTrend")]
        public IActionResult SlaBreachTrend()
        {
            var ctx = GetCurrentUserContext();
            var vm = _trackerService.GetSlaBreachTrend(ctx.ClientId);
            return View("Reports/SlaBreachTrend", vm);
        }

        [HttpGet("Reports/SarRegister")]
        public IActionResult SarRegister()
        {
            var ctx = GetCurrentUserContext();
            var rows = _trackerService.GetSarRegister(ctx.ClientId);
            return View("Reports/SarRegister", rows);
        }

        [HttpGet("Reports/FourEyesAttempts")]
        public IActionResult FourEyesAttempts(int limit = 200)
        {
            var ctx = GetCurrentUserContext();
            var rows = _trackerService.GetFourEyesAttempts(ctx.ClientId, Math.Clamp(limit, 10, 2000));
            return View("Reports/FourEyesAttempts", rows);
        }

        [HttpGet("Reports/WorkloadHeatmap")]
        public IActionResult WorkloadHeatmap()
        {
            var ctx = GetCurrentUserContext();
            var vm = _trackerService.GetWorkloadHeatmap(ctx.ClientId);
            return View("Reports/WorkloadHeatmap", vm);
        }

        [HttpGet("Reports/PeriodicReviewRegister")]
        public IActionResult PeriodicReviewRegister()
        {
            var ctx = GetCurrentUserContext();
            var rows = _trackerService.GetPeriodicReviewCandidates(ctx.ClientId);
            return View("Reports/PeriodicReviewRegister", rows);
        }

        [HttpGet("Reports/ChatAnalytics")]
        public IActionResult ChatAnalytics(int days = 30)
        {
            var ctx = GetCurrentUserContext();
            if (days <= 0 || days > 365) days = 30;
            var vm = _trackerService.GetChatAnalytics(ctx.ClientId, days);
            ViewBag.Days = days;
            return View("Reports/ChatAnalytics", vm);
        }

        [HttpGet("Reports/DailyDigest")]
        public IActionResult DailyDigest()
        {
            var ctx = GetCurrentUserContext();
            var vm = _trackerService.GetDailyDigest(ctx.UserId, ctx.UserGroupName, ctx.ClientId);
            return View("Reports/DailyDigest", vm);
        }

        [HttpGet("Reports/HighRiskRegister")]
        public IActionResult HighRiskRegister()
        {
            var ctx = GetCurrentUserContext();
            var rows = _trackerService.GetHighRiskCustomers(ctx.ClientId);
            return View("Reports/HighRiskRegister", rows);
        }

        [HttpGet("Reports/CustomerDemographics")]
        public IActionResult CustomerDemographics()
        {
            var ctx = GetCurrentUserContext();
            var vm = _trackerService.GetCustomerDemographics(ctx.ClientId);
            return View("Reports/CustomerDemographics", vm);
        }

        [HttpGet("Reports/Lookback")]
        public IActionResult Lookback(int days = 90)
        {
            var ctx = GetCurrentUserContext();
            if (days <= 0 || days > 365 * 3) days = 90;
            var rows = _trackerService.GetLookbackCandidates(ctx.ClientId, days);
            ViewBag.Days = days;
            return View("Reports/Lookback", rows);
        }

        [HttpGet("Reports/AnnualMlro")]
        public IActionResult AnnualMlro(int year = 0)
        {
            var ctx = GetCurrentUserContext();
            if (year <= 2000 || year > DateTime.UtcNow.Year + 1) year = DateTime.UtcNow.Year;
            var vm = _trackerService.GetAnnualMlroReport(ctx.ClientId, year);
            return View("Reports/AnnualMlro", vm);
        }

        [HttpGet("Reports/AgedCases")]
        public IActionResult AgedCases()
        {
            var ctx = GetCurrentUserContext();
            var vm = _trackerService.GetAgedCases(ctx.ClientId);
            return View("Reports/AgedCases", vm);
        }

        [HttpGet("Reports/CustomerAcceptance")]
        public IActionResult CustomerAcceptance(int days = 30)
        {
            var ctx = GetCurrentUserContext();
            if (days <= 0 || days > 365) days = 30;
            var vm = _trackerService.GetCustomerAcceptance(ctx.ClientId, days);
            return View("Reports/CustomerAcceptance", vm);
        }

        [HttpGet("Reports/DocumentAudit")]
        public IActionResult DocumentAudit()
        {
            var ctx = GetCurrentUserContext();
            var vm = _trackerService.GetDocumentAudit(ctx.ClientId);
            return View("Reports/DocumentAudit", vm);
        }

        [HttpGet("Reports/Cohort/{cmid:int?}")]
        public IActionResult Cohort(int? cmid)
        {
            var ctx = GetCurrentUserContext();
            if (cmid == null || cmid <= 0)
            {
                ViewBag.Picker = true;
                return View("Reports/Cohort", new AML.ViewModel.ViewModels.AmlTracker.CohortComparisonVM { Found = false, NotFoundMessage = "Pick a customer to compare." });
            }
            var vm = _trackerService.GetCohortComparison(cmid.Value, ctx.ClientId);
            return View("Reports/Cohort", vm);
        }

        [HttpGet("Reports/Quarterly")]
        public IActionResult Quarterly(int quarters = 8)
        {
            var ctx = GetCurrentUserContext();
            if (quarters <= 0 || quarters > 20) quarters = 8;
            var vm = _trackerService.GetQuarterlyTrend(ctx.ClientId, quarters);
            return View("Reports/Quarterly", vm);
        }

        // ---------------- /Reports ----------------

        [HttpGet("Leaderboard")]
        public IActionResult Leaderboard(int days = 30)
        {
            var ctx = GetCurrentUserContext();
            if (days <= 0 || days > 365) days = 30;
            var vm = _trackerService.GetReviewerLeaderboard(ctx.ClientId, days);
            return View(vm);
        }

        [HttpGet("AuditLog")]
        public IActionResult AuditLog(int limit = 200)
        {
            var ctx = GetCurrentUserContext();
            if (limit <= 0 || limit > 2000) limit = 200;
            var vm = _trackerService.GetAuditLogs(ctx.ClientId, limit);
            return View(vm);
        }

        [HttpGet("SlaConfig")]
        public IActionResult SlaConfig()
        {
            var ctx = GetCurrentUserContext();
            ViewBag.CanEdit = _trackerService.IsQaAuthorized(ctx.UserGroupName);
            var vm = _trackerService.GetSlaConfigs(ctx.ClientId);
            return View(vm);
        }

        [HttpPost("SlaConfig/Save")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSlaConfig(SlaConfigVM model)
        {
            var ctx = GetCurrentUserContext();
            if (!_trackerService.IsQaAuthorized(ctx.UserGroupName))
            {
                Flash("Not authorized to edit SLA config.", "error");
                return RedirectToAction(nameof(SlaConfig));
            }
            _trackerService.UpsertSlaConfig(model, ctx.ClientId, ctx.UserId);
            Flash("SLA config saved.", "success");
            return RedirectToAction(nameof(SlaConfig));
        }

        [HttpPost("SlaConfig/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSlaConfig(int id)
        {
            var ctx = GetCurrentUserContext();
            if (!_trackerService.IsQaAuthorized(ctx.UserGroupName))
            { Flash("Not authorized.", "error"); return RedirectToAction(nameof(SlaConfig)); }
            _trackerService.DeleteSlaConfig(id, ctx.ClientId);
            Flash("SLA rule deleted.", "success");
            return RedirectToAction(nameof(SlaConfig));
        }

        [HttpGet("Export")]
        public IActionResult Export(AmlTrackerFilterVM filter)
        {
            var ctx = GetCurrentUserContext();
            filter ??= new AmlTrackerFilterVM();
            var vm = _trackerService.GetTracker(filter, ctx.UserId, ctx.UserGroupName, ctx.ClientId, null);
            return BuildExcel(vm);
        }

        [HttpGet("ExportCsv")]
        public IActionResult ExportCsv(AmlTrackerFilterVM filter)
        {
            var ctx = GetCurrentUserContext();
            filter ??= new AmlTrackerFilterVM();
            var vm = _trackerService.GetTracker(filter, ctx.UserId, ctx.UserGroupName, ctx.ClientId, null);
            return BuildCsv(vm);
        }

        [HttpGet("QaHistory/{caseId:int}")]
        public IActionResult QaHistory(int caseId)
        {
            var ctx = GetCurrentUserContext();
            return Json(_trackerService.GetQaHistory(caseId, ctx.ClientId));
        }

        [HttpGet("Sar/{caseId:int}")]
        public IActionResult GetSar(int caseId)
        {
            var ctx = GetCurrentUserContext();
            return Json(_trackerService.GetSarLink(caseId, ctx.ClientId));
        }

        [HttpPost("Sar/Save")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSar(SarLinkVM model)
        {
            var ctx = GetCurrentUserContext();
            _trackerService.UpsertSarLink(model, ctx.UserId);
            Flash($"SAR linkage updated for case #{model.CaseId}.", "success");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Summary")]
        public IActionResult Summary()
        {
            var ctx = GetCurrentUserContext();
            return Json(_trackerService.GetSummary(ctx.UserId, ctx.ClientId));
        }

        [HttpGet("Notifications")]
        public IActionResult Notifications(bool onlyUnread = false, int limit = 30)
        {
            var ctx = GetCurrentUserContext();
            return Json(_trackerService.GetNotifications(ctx.UserId, onlyUnread, Math.Clamp(limit, 1, 200)));
        }

        [HttpPost("Notifications/Read/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult MarkNotificationRead(int id)
        {
            var ctx = GetCurrentUserContext();
            _trackerService.MarkNotificationRead(id, ctx.UserId);
            return Json(new { ok = true });
        }

        [HttpPost("Notifications/ReadAll")]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAllRead()
        {
            var ctx = GetCurrentUserContext();
            _trackerService.MarkAllNotificationsRead(ctx.UserId);
            return Json(new { ok = true });
        }

        [HttpPost("SavedView/Save")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveView(string name, string filterJson, bool isShared)
        {
            var ctx = GetCurrentUserContext();
            _trackerService.InsertSavedView(name, filterJson, isShared, ctx.UserId, ctx.ClientId);
            Flash($"Saved view '{name}'.", "success");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("SavedView/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteView(int id)
        {
            var ctx = GetCurrentUserContext();
            _trackerService.DeleteSavedView(id, ctx.UserId, ctx.ClientId);
            return Json(new { ok = true });
        }

        [HttpPost("BatchAction")]
        [ValidateAntiForgeryToken]
        public IActionResult BatchAction(AmlTrackerBatchActionVM model)
        {
            var ctx = GetCurrentUserContext();
            var action = (model?.Action ?? "").Trim();
            var reviewerName = (_httpContextAccessor.HttpContext?.Session?.GetString(StaticResource.sessUsername) ?? "").Trim();

            var qaActions = new[] { "MarkForQa", "QaApprove", "QaReject", "SampleForQa", "StratifiedSample" };
            if (qaActions.Contains(action) && !_trackerService.IsQaAuthorized(ctx.UserGroupName))
            {
                Flash($"Your role ('{ctx.UserGroupName}') is not allowed to perform QA actions.", "error");
                return RedirectToAction(nameof(Index));
            }

            if (action == "SampleForQa")
            {
                var pct = model?.SamplePercent ?? 10;
                if (pct <= 0 || pct > 100) { Flash("Sample percent must be 1-100.", "error"); return RedirectToAction(nameof(Index)); }
                try
                {
                    var s = _trackerService.SampleForQa(pct, ctx.ClientId, ctx.UserId, reviewerName);
                    Flash(s.EligibleCount == 0
                        ? "No QA-eligible cases available to sample."
                        : $"Sampled {s.SampledCount} of {s.EligibleCount} eligible at {pct}%. Marked Pending QA.", "success");
                }
                catch (Exception ex) { Flash($"Sampling failed: {ex.Message}", "error"); }
                return RedirectToAction(nameof(Index));
            }

            if (action == "StratifiedSample")
            {
                var hi = model?.HighPercent ?? 100;
                var md = model?.MediumPercent ?? 25;
                var lo = model?.LowPercent ?? 5;
                try
                {
                    var s = _trackerService.SampleStratifiedForQa(hi, md, lo, ctx.ClientId, ctx.UserId, reviewerName);
                    Flash($"Stratified sample: High {s.HighSampled}/{s.HighEligible}, Medium {s.MediumSampled}/{s.MediumEligible}, Low {s.LowSampled}/{s.LowEligible}. Total {s.TotalSampled} marked Pending QA.", "success");
                }
                catch (Exception ex) { Flash($"Stratified sampling failed: {ex.Message}", "error"); }
                return RedirectToAction(nameof(Index));
            }

            if (model == null || model.CaseIds == null || model.CaseIds.Count == 0)
            { Flash("No cases selected.", "error"); return RedirectToAction(nameof(Index)); }
            var ids = model.CaseIds.Distinct().ToList();

            try
            {
                int affected;
                switch (action)
                {
                    case "MarkForQa":
                        affected = _trackerService.MarkForQa(ids, ctx.ClientId, ctx.UserId, reviewerName);
                        Flash($"Marked {affected} case(s) as Pending QA.", "success");
                        break;

                    case "QaApprove":
                        {
                            var r = _trackerService.RecordQaDecision(ids, (int)AML.DTO.DTO.AmlTracker.QaStatusCode.Approved, model.Comments, model.ReasonCode, ctx.ClientId, ctx.UserId, reviewerName);
                            var msg = $"QA Approved for {r.RecordedCount} case(s).";
                            if (r.BlockedByFourEyesCount > 0) msg += $" {r.BlockedByFourEyesCount} blocked by 4-eyes (you were maker/owner).";
                            Flash(msg, r.RecordedCount > 0 ? "success" : "error");
                            break;
                        }

                    case "QaReject":
                        if (string.IsNullOrWhiteSpace(model.Comments)) { Flash("QA Reject requires a comment.", "error"); break; }
                        {
                            var r = _trackerService.RecordQaDecision(ids, (int)AML.DTO.DTO.AmlTracker.QaStatusCode.Rejected, model.Comments, model.ReasonCode, ctx.ClientId, ctx.UserId, reviewerName);
                            var msg = $"QA Rejected for {r.RecordedCount} case(s). {r.ReopenedCount} reopened. Emails: {r.EmailsSent}";
                            if (r.EmailsFailed > 0) msg += $" (failed: {r.EmailsFailed})";
                            if (r.BlockedByFourEyesCount > 0) msg += $". {r.BlockedByFourEyesCount} blocked by 4-eyes.";
                            Flash(msg, r.RecordedCount > 0 ? "success" : "error");
                            break;
                        }

                    case "Reassign":
                        if (model.NewOwnerId == null || model.NewOwnerId <= 0) { Flash("Pick an owner first.", "error"); break; }
                        affected = _trackerService.Reassign(ids, model.NewOwnerId.Value, ctx.ClientId, ctx.UserId, reviewerName);
                        Flash($"Reassigned {affected} case(s).", "success");
                        break;

                    default:
                        Flash($"Unknown action '{action}'.", "error"); break;
                }
            }
            catch (Exception ex) { Flash($"Action failed: {ex.Message}", "error"); }

            return RedirectToAction(nameof(Index));
        }

        // ---- helpers

        private IActionResult BuildExcel(AmlTrackerIndexVM vm)
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("AML Tracker");

            // 46-column compliance schema. Columns marked "(unmapped)" render blank until the
            // upstream data is wired in — schema is fixed so the file structure stays stable.
            var headers = new[]
            {
                "Date of Case Received",                                // 1  CreatedOn
                "Policy Issue Date",                                    // 2  unmapped
                "Date of Response",                                     // 3  unmapped
                "New/Existing",                                         // 4  unmapped
                "Cust Type",                                            // 5  CustomerType
                "Search System Cust ID",                              // 6  CustomerCode
                "Client product number",                                // 7  unmapped
                "Client CIF number",                                    // 8  unmapped
                "Unique Reference Number",                              // 9  CaseId
                "Name of the Customer",                                 // 10 CustomerName
                "Country/Nationality",                                  // 11 Nationality
                "Policy Holder",                                        // 12 unmapped
                "Product Type",                                         // 13 unmapped
                "Product value AED",                                    // 14 unmapped
                "KYC Check and Document collection",                    // 15 unmapped
                "Comments-KYC check and document collection",           // 16 unmapped
                "ID Type",                                              // 17 unmapped
                "ID Number",                                            // 18 unmapped
                "ID Expiry date",                                       // 19 unmapped
                "PEP",                                                  // 20 IsPep
                "Comments PEP",                                         // 21 unmapped
                "UAE and UNSC Sanctions",                               // 22 IsSanction (single sanction flag today)
                "Comments UAE and UNSC",                                // 23 unmapped
                "Adverse Media News",                                   // 24 IsAdverseMedia
                "Comments Adverse Media News",                          // 25 unmapped
                "Other Sanction",                                       // 26 unmapped
                "Comments Other Sanction",                              // 27 unmapped
                "Other Comments",                                       // 28 unmapped
                "Risk Rating",                                          // 29 RiskTier
                "Senior Management approval",                           // 30 unmapped
                "Date-Senior Management approval",                      // 31 unmapped
                "Compliance Approval",                                  // 32 QaOutcome (best-fit; QA = compliance review)
                "Sanctions Screening Date",                             // 33 unmapped
                "Risk Assessment Date",                                 // 34 unmapped
                "Whitelisted",                                          // 35 IsWhiteListed
                "Date of Whitelisting",                                 // 36 WhitelistedOn
                "Case Status",                                          // 37 StatusLabel
                "Payment Mode",                                         // 38 unmapped
                "Delivery Channel",                                     // 39 unmapped
                "Resident Status",                                      // 40 unmapped
                "Profession",                                           // 41 unmapped
                "QC Comments",                                          // 42 QaComments
                "QC",                                                   // 43 QaStatusLabel
                "Current Status",                                       // 44 StageLabel
                "Created By",                                           // 45 CreatedUser
                "Updated By"                                            // 46 UpdatedUser
            };

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Font.Bold = true;
                sheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
            int row = 2;
            foreach (var r in vm.Rows)
            {
                int c = 1;
                sheet.Cells[row, c++].Value = r.CreatedOn?.ToString("yyyy-MM-dd HH:mm");          // 1
                sheet.Cells[row, c++].Value = r.PolicyIssueDate?.ToString("yyyy-MM-dd");          // 2
                sheet.Cells[row, c++].Value = r.DateOfResponse?.ToString("yyyy-MM-dd HH:mm");     // 3
                sheet.Cells[row, c++].Value = r.NewOrExisting;                                    // 4
                sheet.Cells[row, c++].Value = r.CustomerType;                                     // 5
                sheet.Cells[row, c++].Value = r.CustomerCode;                                     // 6
                sheet.Cells[row, c++].Value = r.ClientProductNumber;                              // 7
                sheet.Cells[row, c++].Value = r.ClientCifNumber;                                  // 8
                sheet.Cells[row, c++].Value = r.CaseId;                                           // 9  Unique Ref
                sheet.Cells[row, c++].Value = r.CustomerName;                                     // 10
                sheet.Cells[row, c++].Value = r.Nationality;                                      // 11
                sheet.Cells[row, c++].Value = r.PolicyHolder;                                     // 12
                sheet.Cells[row, c++].Value = r.ProductType;                                      // 13
                sheet.Cells[row, c++].Value = r.ProductValueAed;                                  // 14
                sheet.Cells[row, c++].Value = r.KycCheck;                                         // 15
                sheet.Cells[row, c++].Value = r.KycCheckComments;                                 // 16
                sheet.Cells[row, c++].Value = r.IdType;                                           // 17
                sheet.Cells[row, c++].Value = r.IdNumber;                                         // 18
                sheet.Cells[row, c++].Value = r.IdExpiry?.ToString("yyyy-MM-dd");                 // 19
                sheet.Cells[row, c++].Value = r.IsPep ? "Yes" : "";                               // 20
                sheet.Cells[row, c++].Value = r.CommentsPep;                                      // 21
                sheet.Cells[row, c++].Value = r.IsSanction ? "Yes" : "";                          // 22
                sheet.Cells[row, c++].Value = r.CommentsUaeUnsc;                                  // 23
                sheet.Cells[row, c++].Value = r.IsAdverseMedia ? "Yes" : "";                      // 24
                sheet.Cells[row, c++].Value = r.CommentsAdverseMedia;                             // 25
                sheet.Cells[row, c++].Value = r.OtherSanctionFlag ? "Yes" : "";                   // 26
                sheet.Cells[row, c++].Value = r.CommentsOtherSanction;                            // 27
                sheet.Cells[row, c++].Value = r.OtherComments;                                    // 28
                sheet.Cells[row, c++].Value = r.RiskTier;                                         // 29
                sheet.Cells[row, c++].Value = r.SeniorMgmtApprovalStatus;                         // 30
                sheet.Cells[row, c++].Value = r.SeniorMgmtApprovalDate?.ToString("yyyy-MM-dd");   // 31
                sheet.Cells[row, c++].Value = r.QAStatus;                                        // 32 Compliance Approval
                sheet.Cells[row, c++].Value = r.SanctionsScreeningDate?.ToString("yyyy-MM-dd");   // 33
                sheet.Cells[row, c++].Value = r.RiskAssessmentDate?.ToString("yyyy-MM-dd");       // 34
                sheet.Cells[row, c++].Value = r.IsWhiteListed;                                    // 35
                sheet.Cells[row, c++].Value = r.WhitelistedOn?.ToString("yyyy-MM-dd");            // 36
                sheet.Cells[row, c++].Value = r.StatusLabel;                                      // 37
                sheet.Cells[row, c++].Value = r.PaymentMode;                                      // 38
                sheet.Cells[row, c++].Value = r.DeliveryChannel;                                  // 39
                sheet.Cells[row, c++].Value = r.ResidentStatus;                                   // 40
                sheet.Cells[row, c++].Value = r.Profession;                                       // 41
                sheet.Cells[row, c++].Value = r.QAComments;                                       // 42
                sheet.Cells[row, c++].Value = r.QaStatusLabel;                                    // 43 QC
                sheet.Cells[row, c++].Value = r.StageLabel;                                       // 44 Current Status
                sheet.Cells[row, c++].Value = r.CreatedUser;                                      // 45
                sheet.Cells[row, c++].Value = r.UpdatedUser;                                      // 46
                row++;
            }

            // Set reasonable column widths (AutoFitColumns requires libgdiplus, not on Linux/macOS by default)
            for (int i = 1; i <= headers.Length; i++) sheet.Column(i).Width = 22;
            var bytes = package.GetAsByteArray();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AMLTracker_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
        }

        private IActionResult BuildCsv(AmlTrackerIndexVM vm)
        {
            // Mirrors BuildExcel — same 46-column compliance schema in the same order.
            // Unmapped columns render blank until the upstream data is wired in.
            var headers = new[]
            {
                "Date of Case Received","Policy Issue Date","Date of Response","New/Existing","Cust Type",
                "Search System Cust ID","Client product number","Client CIF number","Unique Reference Number",
                "Name of the Customer","Country/Nationality","Policy Holder","Product Type","Product value AED",
                "KYC Check and Document collection","Comments-KYC check and document collection",
                "ID Type","ID Number","ID Expiry date",
                "PEP","Comments PEP","UAE and UNSC Sanctions","Comments UAE and UNSC",
                "Adverse Media News","Comments Adverse Media News","Other Sanction","Comments Other Sanction","Other Comments",
                "Risk Rating","Senior Management approval","Date-Senior Management approval","Compliance Approval",
                "Sanctions Screening Date","Risk Assessment Date","Whitelisted","Date of Whitelisting","Case Status",
                "Payment Mode","Delivery Channel","Resident Status","Profession",
                "QC Comments","QC","Current Status","Created By","Updated By"
            };

            var sb = new StringBuilder();
            string Q(string s) => s == null ? "" : "\"" + s.Replace("\"", "\"\"") + "\"";
            sb.AppendLine(string.Join(",", headers.Select(Q)));

            foreach (var r in vm.Rows)
            {
                var fields = new[]
                {
                    Q(r.CreatedOn?.ToString("yyyy-MM-dd HH:mm")),                          // 1
                    Q(r.PolicyIssueDate?.ToString("yyyy-MM-dd")),                          // 2
                    Q(r.DateOfResponse?.ToString("yyyy-MM-dd HH:mm")),                     // 3
                    Q(r.NewOrExisting),                                                    // 4
                    Q(r.CustomerType),                                                     // 5
                    Q(r.CustomerCode),                                                     // 6
                    Q(r.ClientProductNumber),                                              // 7
                    Q(r.ClientCifNumber),                                                  // 8
                    r.CaseId.ToString(),                                                   // 9  Unique Ref
                    Q(r.CustomerName),                                                     // 10
                    Q(r.Nationality),                                                      // 11
                    Q(r.PolicyHolder),                                                     // 12
                    Q(r.ProductType),                                                      // 13
                    Q(r.ProductValueAed),                                                  // 14
                    Q(r.KycCheck),                                                         // 15
                    Q(r.KycCheckComments),                                                 // 16
                    Q(r.IdType),                                                           // 17
                    Q(r.IdNumber),                                                         // 18
                    Q(r.IdExpiry?.ToString("yyyy-MM-dd")),                                 // 19
                    r.IsPep ? "Yes" : "",                                                  // 20
                    Q(r.CommentsPep),                                                      // 21
                    r.IsSanction ? "Yes" : "",                                             // 22
                    Q(r.CommentsUaeUnsc),                                                  // 23
                    r.IsAdverseMedia ? "Yes" : "",                                         // 24
                    Q(r.CommentsAdverseMedia),                                             // 25
                    r.OtherSanctionFlag ? "Yes" : "",                                      // 26
                    Q(r.CommentsOtherSanction),                                            // 27
                    Q(r.OtherComments),                                                    // 28
                    Q(r.RiskTier),                                                         // 29
                    Q(r.SeniorMgmtApprovalStatus),                                         // 30
                    Q(r.SeniorMgmtApprovalDate?.ToString("yyyy-MM-dd")),                   // 31
                    Q(r.QaOutcome),                                                        // 32 Compliance Approval
                    Q(r.SanctionsScreeningDate?.ToString("yyyy-MM-dd")),                   // 33
                    Q(r.RiskAssessmentDate?.ToString("yyyy-MM-dd")),                       // 34
                    Q(r.IsWhiteListed),                                                    // 35
                    Q(r.WhitelistedOn?.ToString("yyyy-MM-dd")),                            // 36
                    Q(r.StatusLabel),                                                      // 37
                    Q(r.PaymentMode),                                                      // 38
                    Q(r.DeliveryChannel),                                                  // 39
                    Q(r.ResidentStatus),                                                   // 40
                    Q(r.Profession),                                                       // 41
                    Q(r.QAComments),                                                       // 42
                    Q(r.QaStatusLabel),                                                    // 43 QC
                    Q(r.StageLabel),                                                       // 44 Current Status
                    Q(r.CreatedUser),                                                      // 45
                    Q(r.UpdatedUser)                                                       // 46
                };
                sb.AppendLine(string.Join(",", fields));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"AMLTracker_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        private DateTime? ReadLastVisitCookie()
        {
            var v = Request.Cookies[LastVisitCookie];
            if (!string.IsNullOrEmpty(v) && DateTime.TryParse(v, out var d)) return d;
            return null;
        }

        private void WriteLastVisitCookie(DateTime when)
        {
            Response.Cookies.Append(LastVisitCookie, when.ToString("o"), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(60),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            });
        }

        private void Flash(string msg, string level)
        {
            TempData["FlashMessage"] = msg;
            TempData["FlashLevel"] = level;
        }

        private (int UserId, int ClientId, string UserGroupName,int groupId,string sessionId) GetCurrentUserContext()
        {
            var userId = _clientHandler.GetUserId();
            var clientId = _clientHandler.GetClientId();
            var groupId = _clientHandler.GetGroupId();
            string sessionId = this.HttpContext.Session.GetString("SessID");
            var userGroupName = "";
            if (groupId > 0)
            {
                var group = _userGroupService.GetDetails(groupId);
                userGroupName = group?.Name ?? "";
            }
            return (userId, clientId, userGroupName, groupId, sessionId);
        }
    }
}
