using AML.Core.ServiceContract.AI;
using AML.DTO.DTO.AI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AML.Core.Service.AI
{
    public class MockAIService : BaseService, IAIService
    {
        public MockAIService(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<string> GetIntelligentReplyAsync(string prompt, string context)
        {
            var promptLower = prompt.ToLowerInvariant();
            if (promptLower.Contains("status") || promptLower.Contains("state") || promptLower.Contains("about user") || promptLower.Contains("due diligence"))
                return GetStatusResponse(context);
            if (promptLower.Contains("next") || promptLower.Contains("do next") || promptLower.Contains("action") || promptLower.Contains("step"))
                return GetNextStepsResponse(context);
            if (promptLower.Contains("history") || promptLower.Contains("comment") || promptLower.Contains("past") || promptLower.Contains("audit trail") || promptLower.Contains("log"))
                return GetHistoryResponse(context);
            return GetGeneralResponse(prompt, context);
        }

        public async IAsyncEnumerable<string> GetIntelligentReplyStreamAsync(string prompt, string context, List<ChatMessageDTO> history = null, bool isNewChat = false)
        {
            var promptLower = prompt.ToLowerInvariant();
            if (promptLower.Contains("status") || promptLower.Contains("state") || promptLower.Contains("about user") || promptLower.Contains("due diligence"))
                yield return GetStatusResponse(context);
            else if (promptLower.Contains("next") || promptLower.Contains("do next") || promptLower.Contains("action") || promptLower.Contains("step"))
                yield return GetNextStepsResponse(context);
            else if (promptLower.Contains("history") || promptLower.Contains("comment") || promptLower.Contains("past") || promptLower.Contains("audit trail") || promptLower.Contains("log"))
                yield return GetHistoryResponse(context);
            else
                yield return GetGeneralResponse(prompt, context);
        }

        public async IAsyncEnumerable<string> GetGeneralReplyStreamAsync(string prompt, string moduleContext, List<ChatMessageDTO> history = null, bool isNewChat = false)
        {
            yield return GetGeneralResponse(prompt, moduleContext);
        }

        private int ParseDashboardMetric(string context, string key)
        {
            try
            {
                int idx = context.IndexOf(key);
                if (idx < 0) return 0;
                var sub = context.Substring(idx + key.Length).Trim();
                var sb = new System.Text.StringBuilder();
                foreach (char c in sub)
                {
                    if (char.IsDigit(c)) sb.Append(c);
                    else break;
                }
                if (int.TryParse(sb.ToString(), out int val)) return val;
            }
            catch {}
            return 0;
        }

        private string GetGeneralResponse(string prompt, string context = null)
        {
            var promptLower = prompt.ToLowerInvariant();

            // Dashboard
            if (promptLower.Contains("high-risk") || promptLower.Contains("high risk") || promptLower.Contains("pending") || promptLower.Contains("dashboard") || promptLower.Contains("stats") || promptLower.Contains("breakdown") || promptLower.Contains("summary"))
            {
                int pending = 0, approved = 0, rejected = 0, seniorMgmt = 0, auto = 0, scheduler = 0, highRisk = 0, total = 0;
                string topCases = "None";
                
                if (!string.IsNullOrEmpty(context) && context.Contains("[LIVE DASHBOARD SUMMARY]"))
                {
                    pending = ParseDashboardMetric(context, "Pending:");
                    approved = ParseDashboardMetric(context, "Approved:");
                    rejected = ParseDashboardMetric(context, "Rejected:");
                    seniorMgmt = ParseDashboardMetric(context, "In Senior Management:");
                    auto = ParseDashboardMetric(context, "Auto:");
                    scheduler = ParseDashboardMetric(context, "Daily Scheduler:");
                    highRisk = ParseDashboardMetric(context, "High Risk Count:");
                    total = ParseDashboardMetric(context, "Total Cases:");
                    
                    var topCasesIdx = context.IndexOf("TOP CASES:");
                    if (topCasesIdx >= 0)
                    {
                        var afterTop = context.Substring(topCasesIdx + "TOP CASES:".Length);
                        var endIdx = afterTop.IndexOf(". Total Cases:");
                        if (endIdx >= 0)
                        {
                            topCases = afterTop.Substring(0, endIdx).Trim();
                            if (string.IsNullOrEmpty(topCases)) topCases = "None";
                        }
                    }
                }
                
                if (promptLower.Contains("breakdown") || promptLower.Contains("status breakdown") || promptLower.Contains("summary") || promptLower.Contains("dashboard") || promptLower.Contains("stats"))
                {
                    return $@"### 📊 Dashboard Status Breakdown
Here is the real-time breakdown of all cases registered in the system:
- **Pending (Initial Queue):** {pending} case(s)
- **Approved:** {approved} case(s)
- **Rejected:** {rejected} case(s)
- **Pending Senior Management:** {seniorMgmt} case(s)
- **Auto-Resolved:** {auto} case(s)
- **Pending Daily Scheduler:** {scheduler} case(s)
- **Total Registered Cases:** {total} case(s)

### ⚠️ High Risk Summary
- **High Risk Count (Match Score > 80%):** **{highRisk}**
- **Top Flagged Cases:** {topCases}

*Note: The background scheduler rescreens active cases daily against latest sanction/PEP watchlists.*";
                }
                
                if (promptLower.Contains("high-risk") || promptLower.Contains("high risk") || promptLower.Contains("pending"))
                {
                    return $"**High-Risk Pending Cases**: There are currently **{highRisk}** high-risk cases flagged in the system. The overall status dashboard shows **{pending}** cases in the initial review queue and **{seniorMgmt}** cases pending with Senior Management.";
                }
                
                return $"**Dashboard Summary**: The system tracks Pending, Approved, Rejected, Senior Management, and Daily Scheduler statuses. Cases with a match score above 80 are flagged as High Risk. Currently, there are **{highRisk}** high-risk cases and **{pending}** cases pending initial review.";
            }
            
            // Admin
            if (promptLower.Contains("user group") || promptLower.Contains("permission") || promptLower.Contains("client") || promptLower.Contains("admin"))
                return "**Admin**: User Groups include Admin, Compliance/Reviewer, and View-Only. Rights are defined as Add, Edit, Delete, or View per menu. Feature flags (OCR, PF) are configured per Client ID.";
            
            // PF
            if (promptLower.Contains("dual-use") || promptLower.Contains("hs code") || promptLower.Contains("chemical") || promptLower.Contains("proliferation") || promptLower.Contains("prolif"))
                return "**Proliferation Finance**: Screening is performed against UAE Cabinet Decision No. 156 of 2025. Search by HS Code, CAS Number, or Chemical Name. Decisions include No Match, Potential Match, or Confirmed Hit.";
            
            // Reports
            if (promptLower.Contains("export") || promptLower.Contains("report") || promptLower.Contains("audit"))
                return "**Reports**: The Export button generates a Case Process PDF — the official audit trail. Every action is logged in the system's ScreeningLogs table.";
            
            // Due Diligence / Case Creation
            if (promptLower.Contains("mandatory") || promptLower.Contains("ocr") || promptLower.Contains("bulk") || promptLower.Contains("screening"))
                return "**Screening & Case Creation**: Mandatory fields are Full Name, Nationality, Gender, Date of Birth, ID Number, and ID Type. Input options are Manual, OCR (passport image), or Bulk Excel upload. Cases are screened against PEP, SAN, UN, and UAE Local lists.";

            // AmlTracker
            if (promptLower.Contains("queue") || promptLower.Contains("sla") || promptLower.Contains("assign") || promptLower.Contains("amltracker") || promptLower.Contains("aml tracker"))
                return "**AML Tracker**:\n- **Queue**: Your queue displays all cases pending review, categorized by priority (High/Medium/Low) based on risk scoring.\n- **SLA Tracking**: High-risk cases have a 24-hour SLA limit, while Medium and Low cases have 48 and 72-hour limits respectively. Escalations are triggered automatically when deadlines approach.\n- **Assignment**: Click the \"Assign\" button on any case card in the board view to delegate the case to a compliance officer or reviewer.";

            // SanctionScreening
            if (promptLower.Contains("lists") || promptLower.Contains("match score") || promptLower.Contains("different") || promptLower.Contains("sanction") || promptLower.Contains("sanctionscreening"))
                return "**Sanction Screening**:\n- **Screened Lists**: Screening is performed against global watchlists including PEP (Politically Exposed Persons), Sanctions (OFAC, UN, EU), UAE Local Terrorist List, and Adverse Media.\n- **Match Score**: Calculated using fuzzy string matching algorithm (Jaro-Winkler). Scores > 80 indicate strong phonetic or spelling similarity and require manual verification.\n- **Vs. Case Creation**: Sanction Screening is a lightweight, check-only query that doesn't create a persistent workflow case. Case Creation registers a full audit-trailed workflow.";

            // ComplianceHub
            if (promptLower.Contains("regulation") || promptLower.Contains("policy") || promptLower.Contains("fatf") || promptLower.Contains("compliancehub"))
                return "**Compliance Hub**:\n- **Regulations**: Covers UAE Central Bank regulations, FATF recommendations, and local AML compliance laws (Decree-Law No. 20 of 2018).\n- **Policy Search**: Use the search bar in the Compliance Hub to find internal policies on KYC, Proliferation Finance, and suspicious activity reporting.\n- **FATF Guidance**: Provides guidelines on risk-based approach for virtual assets, legal persons, and high-risk jurisdictions.";

            // STM
            if (promptLower.Contains("suspicious") || promptLower.Contains("str") || promptLower.Contains("threshold") || promptLower.Contains("stm"))
                return "**Sectoral Transaction Monitoring (STM)**:\n- **Flagged Transactions**: Transactions are flagged using pre-defined rule sets such as rapid movement of funds, structurings, or unexplained round-sum transactions.\n- **STR (Suspicious Transaction Report)**: If investigations validate a flag, the system assists in preparing and directly e-filing an STR to the Financial Intelligence Unit (FIU) via the goAML portal.\n- **Rule Thresholds**: Custom thresholds (e.g. single transaction > 55,000 AED) can be adjusted per customer risk profile under the STM configuration menu.";

            return @"### 🤖 AML Case Assistant
I can assist you with case status inquiries, regulatory guidelines, and system configurations. How can I help you today?

#### 💡 Suggested Inquiries:
- **Case Status:** *""What is the status of Case #<ID>?""* or search by customer name.
- **Next Steps:** *""What should I do next for Case #<ID>?""*
- **Dashboard Stats:** *""Show me status breakdown for today""* or *""How many high-risk cases are pending?""*
- **System Modules:** Ask about **AML Tracker**, **Sanction Screening**, **Compliance Hub**, or **Sectoral Transaction Monitoring (STM)**.";
        }

        private (string Id, string PkId, string Name, string Status, string Risk, string CustType, string CreatedOn, string CreatedBy, string GroupId, string GroupEntityOf, string RelatedParties, string PfSummary, string Comments) ParseFullContext(string context)
        {
            string id = "N/A";
            string pkId = "N/A";
            string name = "N/A";
            string status = "Pending";
            string risk = "N/A";
            string custType = "Individual";
            string createdOn = "2026-05-29 10:00:00";
            string createdBy = "System";
            string groupId = "None";
            string groupEntityOf = "None";
            var relatedPartiesList = new List<string>();
            var pfList = new List<string>();
            var commentsList = new List<string>();

            if (string.IsNullOrEmpty(context))
                return (id, "N/A", name, status, risk, custType, createdOn, createdBy, groupId, groupEntityOf, "None", "No matches found", "None");

            var lines = context.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string section = "";

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Case Details:")) { section = "details"; continue; }
                if (trimmed.StartsWith("Shareholders:")) { section = "shareholders"; continue; }
                if (trimmed.StartsWith("Search Result Findings:")) { section = "pf"; continue; }
                if (trimmed.StartsWith("Latest Risk Assessment History:")) { section = "risk"; continue; }
                if (trimmed.StartsWith("Case Comments:")) { section = "comments"; continue; }

                if (section == "details")
                {
                    if (trimmed.StartsWith("- ID:"))
                        id = trimmed.Substring("- ID:".Length).Trim();
                    else if (trimmed.StartsWith("- PK_ID:"))
                        pkId = trimmed.Substring("- PK_ID:".Length).Trim();
                    else if (trimmed.StartsWith("- Name:"))
                        name = trimmed.Substring("- Name:".Length).Trim();
                    else if (trimmed.StartsWith("- Current Status:"))
                        status = trimmed.Substring("- Current Status:".Length).Trim();
                    else if (trimmed.StartsWith("- Risk Score:"))
                        risk = trimmed.Substring("- Risk Score:".Length).Trim();
                    else if (trimmed.StartsWith("- Customer Type:"))
                        custType = trimmed.Substring("- Customer Type:".Length).Trim();
                    else if (trimmed.StartsWith("- Created On:"))
                        createdOn = trimmed.Substring("- Created On:".Length).Trim();
                    else if (trimmed.StartsWith("- Created By:"))
                        createdBy = trimmed.Substring("- Created By:".Length).Trim();
                    else if (trimmed.StartsWith("- Group ID:"))
                        groupId = trimmed.Substring("- Group ID:".Length).Trim();
                    else if (trimmed.StartsWith("- Group Entity Of:"))
                        groupEntityOf = trimmed.Substring("- Group Entity Of:".Length).Trim();
                }
                else if (section == "shareholders")
                {
                    if (trimmed.StartsWith("-") && !trimmed.Contains("None"))
                        relatedPartiesList.Add(trimmed);
                }
                else if (section == "pf")
                {
                    if (trimmed.StartsWith("-") && !trimmed.Contains("No proliferation finance"))
                        pfList.Add(trimmed);
                }
                else if (section == "comments")
                {
                    if (trimmed.StartsWith("-") && !trimmed.Contains("None"))
                        commentsList.Add(trimmed);
                }
            }

            if (custType == "I" || custType.Equals("individual", StringComparison.OrdinalIgnoreCase)) custType = "Individual";
            else if (custType == "C" || custType.Equals("corporate", StringComparison.OrdinalIgnoreCase)) custType = "Corporate";

            if (status.StartsWith("Case #"))
            {
                int idx = status.IndexOf("is currently");
                if (idx >= 0)
                {
                    status = status.Substring(idx + "is currently".Length).Trim();
                    int riskIdx = status.IndexOf("with risk score");
                    if (riskIdx >= 0)
                    {
                        status = status.Substring(0, riskIdx).Trim();
                    }
                }
            }

            string relatedParties = relatedPartiesList.Any() ? string.Join("\n", relatedPartiesList) : "None";
            string pfSummary = pfList.Any() ? string.Join("\n", pfList) : "No proliferation finance matches found";
            string comments = commentsList.Any() ? string.Join("\n", commentsList) : "None";

            return (id, pkId, name, status, risk, custType, createdOn, createdBy, groupId, groupEntityOf, relatedParties, pfSummary, comments);
        }

        private string GetStatusResponse(string context)
        {
            if (!context.Contains("ID:"))
                return "Please select a case or provide a Case ID to check the status.";

            var info = ParseFullContext(context);

            int pendingDays = 0;
            string formattedCreatedOn = info.CreatedOn;
            if (DateTime.TryParse(info.CreatedOn, out var createdDate))
            {
                var today = DateTime.Now; 
                pendingDays = (int)(today.Date - createdDate.Date).TotalDays;
                if (pendingDays < 0) pendingDays = 0;
                
                // Format date as DD/MM/YYYY
                formattedCreatedOn = createdDate.ToString("dd/MM/yyyy");
            }

            string queueLocation = "the system queue";
            string reasonForStatus = "initial submission and pending review";
            string lowerStatus = info.Status.ToLowerInvariant();
            
            if (lowerStatus == "pending") 
            {
                queueLocation = "Due Diligence";
                
                bool hasUnapprovedShareholders = info.RelatedParties != "None" && 
                    (info.RelatedParties.Contains("Status: Pending", StringComparison.OrdinalIgnoreCase) || 
                     info.RelatedParties.Contains("Status: 0") ||
                     info.RelatedParties.Contains("Status: Rejected") ||
                     info.RelatedParties.Contains("Status: 3"));
                bool hasPfHits = info.PfSummary != "No proliferation finance matches found";
                bool isHighRisk = info.Risk.Contains("High", StringComparison.OrdinalIgnoreCase) || info.Risk.Contains("H(", StringComparison.OrdinalIgnoreCase);

                if (hasUnapprovedShareholders)
                    reasonForStatus = "unapproved shareholders";
                else if (hasPfHits)
                    reasonForStatus = "unresolved Proliferation Finance hits";
                else if (isHighRisk)
                    reasonForStatus = "a high risk profile review";
                else
                    reasonForStatus = "its initial submission and pending review";
            }
            else if (lowerStatus == "approved") 
            {
                queueLocation = "Completed Cases";
                reasonForStatus = "successful verification";
            }
            else if (lowerStatus == "rejected") 
            {
                queueLocation = "Completed Cases";
                reasonForStatus = "failed verification or policy violations";
            }
            else if (lowerStatus == "auto") 
            {
                queueLocation = "Due Diligence";
                reasonForStatus = "automatic processing rules";
            }
            else if (lowerStatus.Contains("senior management")) 
            {
                queueLocation = "Senior Management";
                reasonForStatus = "escalation for a high risk or policy exception";
            }
            else if (lowerStatus.Contains("scheduler")) 
            {
                queueLocation = "the Daily Scheduler";
                reasonForStatus = "scheduled rescreening";
            }

            string displayId = info.Id;

            return $"The case profile for **{info.Name}** (ID: `{displayId}` | {info.CustType}) carries a risk rating of **{info.Risk}**. It is currently **{info.Status}** and has been in **{queueLocation}** due to {reasonForStatus}, with **{info.CreatedBy}** for **{pendingDays}** day(s).";
        }

        private string GetNextStepsResponse(string context)
        {
            if (!context.Contains("ID:"))
                return "Please select a case or provide a Case ID to get the next steps.";

            var info = ParseFullContext(context);

            // Determine if escalation is needed based on risk and status
            bool isHighRisk = info.Risk.Contains("High", StringComparison.OrdinalIgnoreCase) || info.Risk.Contains("H(", StringComparison.OrdinalIgnoreCase);
            bool alreadyEscalated = info.Status.Contains("Senior Management", StringComparison.OrdinalIgnoreCase);
            bool hasPfHits = info.PfSummary != "No proliferation finance matches found";

            string escalationDecision;
            if (alreadyEscalated)
            {
                escalationDecision = $"This case (Risk: **{info.Risk}**) has **already been submitted to Senior Management** and is currently locked for editing. No further action is required until Senior Management makes a decision.";
            }
            else if (isHighRisk || hasPfHits)
            {
                string reason = isHighRisk && hasPfHits
                    ? $"the case carries a **{info.Risk}** risk rating and has Proliferation Finance hits recorded"
                    : isHighRisk
                        ? $"the case carries a **{info.Risk}** risk rating"
                        : "Proliferation Finance hits have been recorded against this case";

                escalationDecision = $"**Submit this case to Senior Management now** — {reason}. " +
                    $"Click **Submit To Senior Management**, enter your justification in the mandatory Remarks field, and confirm. " +
                    $"Once submitted, the case will be locked to read-only until Senior Management approves or overrides it.";
            }
            else
            {
                escalationDecision = $"This case (Risk: **{info.Risk}**) does not currently meet the threshold for Senior Management escalation. " +
                    $"Review the case details, complete any outstanding due diligence checks, and update the status accordingly.";
            }

            return $@"### 🚀 Recommended Next Actions
- **{(alreadyEscalated ? "✅ Submitted to Senior Management" : (isHighRisk || hasPfHits ? "⚠️ Escalate to Senior Management" : "📋 Continue Due Diligence"))}:** {escalationDecision}";
        }

        private string GetHistoryResponse(string context)
        {
            if (!context.Contains("ID:"))
                return "Please select a case or provide a Case ID to check the history.";

            var info = ParseFullContext(context);
            string createdDateStr = info.CreatedOn;
            
            string day1 = createdDateStr;
            if (DateTime.TryParse(createdDateStr, out var cDate))
            {
                day1 = cDate.ToString("dd/MM/yyyy hh:mm tt");
            }

            var commentsLogList = new List<string>();
            if (info.Comments != "None" && !string.IsNullOrEmpty(info.Comments))
            {
                var commentLines = info.Comments.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var commentLine in commentLines)
                {
                    var clean = commentLine.Trim();
                    if (clean.StartsWith("- ")) clean = clean.Substring(2);
                    
                    string dateStr = "N/A";
                    string userStr = "System";
                    string commentStr = "";
                    
                    int dateIdx = clean.IndexOf("Date: ");
                    int userIdx = clean.IndexOf(", User: ");
                    int commentIdx = clean.IndexOf(", Comment: ");
                    
                    if (dateIdx >= 0 && userIdx >= 0)
                    {
                        dateStr = clean.Substring(dateIdx + 6, userIdx - (dateIdx + 6)).Trim();
                    }
                    if (userIdx >= 0 && commentIdx >= 0)
                    {
                        userStr = clean.Substring(userIdx + 8, commentIdx - (userIdx + 8)).Trim();
                    }
                    if (commentIdx >= 0)
                    {
                        commentStr = clean.Substring(commentIdx + 11).Trim();
                    }
                    else
                    {
                        commentStr = clean;
                    }
                    
                    commentsLogList.Add($"- **{dateStr} ({userStr}):** *\"{commentStr}\"*");
                }
            }

            var logs = new List<string>();
            logs.Add($"- **{day1} (System):** Case created automatically. Preliminary risk score calculated as **{info.Risk}**.");
            
            if (commentsLogList.Any())
            {
                logs.AddRange(commentsLogList);
            }
            else
            {
                logs.Add($"- **{day1} ({info.CreatedBy}):** Case opened for manual compliance review.");
            }

            int totalLogs = logs.Count;
            string summaryParagraph = $"The case has a total of **{totalLogs}** chronological event(s) logged in its history. This includes the initial automated system creation and subsequent manual compliance updates, providing a full audit trail of the review process.";

            return $@"### 📜 Case History & Comments Summary
{summaryParagraph}

**Audit Log:**
{string.Join("\n", logs)}

### 🔗 Useful Links
- **[Link to Due Diligence Case History](/case/Process/{info.PkId}#history)**";
        }
    }
}
