using AML.Core.RepositoryContract.ComplianceHub;
using AML.ViewModel.ViewModels.ComplianceHub;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.ComplianceHub
{
    public class ComplianceHubRepository : BaseRepository, IComplianceHubRepository
    {
        public ComplianceHubRepository(IConfiguration configuration, IHttpContextAccessor context)
            : base(configuration, context) { }

        // ---------------- 1. KYC Document Expiry Calendar ----------------
        public KycExpiryVM GetKycExpiry(int clientId, int horizonDays)
        {
            // Pull every customermaster row with at least one document expiry date.
            // Pivot in C# so we get one KycExpiryRow per (customer × document).
            const string sql = @"
SELECT cm.id                                                AS CustomerMasterId,
       cm.cust_ref_id                                       AS CustomerCode,
       TRIM(CONCAT_WS(' ', cm.fname, cm.mname, cm.lname))   AS FullName,
       cm.cust_type                                         AS CustomerType,
       cm.PassportId                                        AS PassportId,
       cm.PassportExpiryDate                                AS PassportExpiry,
       cm.EmiratesIdNumber                                  AS EmiratesId,
       cm.EmiratesIdExpiryDate                              AS EmiratesExpiry,
       cm.cust_id_number                                    AS NationalId,
       cm.id_expiry_date                                    AS IdExpiry
  FROM customermaster cm
 WHERE cm.Client_Id = @clientId
   AND IFNULL(cm.is_deleted, 0) = 0;";

            var raw = Get<dynamic>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            var vm = new KycExpiryVM();
            var today = DateTime.UtcNow.Date;

            void Add(dynamic r, string docType, string number, DateTime? expiry)
            {
                if (!expiry.HasValue) return;
                var days = (int)Math.Floor((expiry.Value.Date - today).TotalDays);
                string bucket = days < 0 ? "expired"
                              : days <= 30 ? "30"
                              : days <= 60 ? "60"
                              : days <= 90 ? "90"
                              : "future";
                vm.Rows.Add(new KycExpiryRow
                {
                    CustomerMasterId = (int)r.CustomerMasterId,
                    CustomerCode = (string)r.CustomerCode,
                    FullName = string.IsNullOrWhiteSpace((string)r.FullName) ? (string)r.CustomerCode : (string)r.FullName,
                    CustomerType = (string)r.CustomerType,
                    DocumentType = docType,
                    DocumentNumber = number,
                    ExpiryDate = expiry.Value,
                    DaysUntil = days,
                    Bucket = bucket
                });
            }

            foreach (var r in raw)
            {
                Add(r, "Passport",     (string)r.PassportId, (DateTime?)r.PassportExpiry);
                Add(r, "Emirates ID",  (string)r.EmiratesId, (DateTime?)r.EmiratesExpiry);
                Add(r, "National ID",  (string)r.NationalId, (DateTime?)r.IdExpiry);
            }

            vm.Rows = vm.Rows.OrderBy(r => r.DaysUntil).ToList();
            return vm;
        }

        // ---------------- 2. Periodic Review Calendar ----------------
        public PeriodicReviewVM GetPeriodicReview(int clientId)
        {
            // Last review = max(latest assessment date, customer creation date).
            // Cadence: High = 1 year, Medium = 2 years, Low = 3 years, Unrated = 1 year.
            const string sql = @"
SELECT cm.id                                                AS CustomerMasterId,
       cm.cust_ref_id                                       AS CustomerCode,
       TRIM(CONCAT_WS(' ', cm.fname, cm.mname, cm.lname))   AS FullName,
       cm.created_on                                        AS CreatedOn,
       cm.customer_final_risk_score                         AS RiskScore,
       (SELECT MAX(tri.dateofassessment) FROM transaction_risk_individual tri
         WHERE tri.customercode = cm.cust_ref_id AND tri.IsDeleted = 0)    AS LastIndAssessment,
       (SELECT MAX(trc.dateofassessment) FROM transaction_risk_corporate trc
         WHERE trc.customercode = cm.cust_ref_id AND trc.IsDeleted = 0)    AS LastCorpAssessment
  FROM customermaster cm
 WHERE cm.Client_Id = @clientId
   AND IFNULL(cm.is_deleted, 0) = 0;";

            var raw = Get<dynamic>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            var vm = new PeriodicReviewVM();
            var today = DateTime.UtcNow.Date;

            foreach (var r in raw)
            {
                DateTime? created = (DateTime?)r.CreatedOn;
                DateTime? lastInd = (DateTime?)r.LastIndAssessment;
                DateTime? lastCorp = (DateTime?)r.LastCorpAssessment;
                var lastReview = new[] { created, lastInd, lastCorp }.Where(d => d.HasValue).Max() ?? today;

                var risk = ((string)r.RiskScore ?? "Unrated").Trim();
                string band = risk.Contains("High", StringComparison.OrdinalIgnoreCase) ? "High"
                            : risk.Contains("Medium", StringComparison.OrdinalIgnoreCase) ? "Medium"
                            : risk.Contains("Low", StringComparison.OrdinalIgnoreCase) ? "Low"
                            : "Unrated";
                int years = band == "High" ? 1 : band == "Medium" ? 2 : band == "Low" ? 3 : 1;
                var due = lastReview.AddYears(years).Date;
                vm.Rows.Add(new PeriodicReviewRow
                {
                    CustomerMasterId = (int)r.CustomerMasterId,
                    CustomerCode = (string)r.CustomerCode,
                    FullName = string.IsNullOrWhiteSpace((string)r.FullName) ? (string)r.CustomerCode : (string)r.FullName,
                    RiskBand = band,
                    LastReviewedOn = lastReview,
                    NextReviewDue = due,
                    DaysUntilReview = (int)Math.Floor((due - today).TotalDays)
                });
            }
            vm.Rows = vm.Rows.OrderBy(r => r.DaysUntilReview).ToList();
            return vm;
        }

        // ---------------- 3. Customer Data Completeness Scorecard ----------------
        public DataCompletenessVM GetDataCompleteness(int clientId)
        {
            // The fields below are the ones the regulator actually wants populated.
            // Add/remove here to retune the scorecard; the SQL doesn't need to change.
            var checkedFields = new (string Col, string Display)[]
            {
                ("nationality",       "Nationality"),
                ("profession",        "Profession"),
                ("employer",          "Employer"),
                ("employersector",    "Employer Sector"),
                ("residence",         "Residence"),
                ("mobile",            "Mobile"),
                ("cust_id_number",    "ID Number"),
                ("PassportId",        "Passport"),
                ("dob",               "Date of Birth")
            };

            const string sql = @"
SELECT cm.id                                                AS CustomerMasterId,
       cm.cust_ref_id                                       AS CustomerCode,
       TRIM(CONCAT_WS(' ', cm.fname, cm.mname, cm.lname))   AS FullName,
       cm.nationality, cm.profession, cm.employer, cm.employersector,
       cm.residence, cm.mobile, cm.cust_id_number, cm.PassportId, cm.dob
  FROM customermaster cm
 WHERE cm.Client_Id = @clientId
   AND IFNULL(cm.is_deleted, 0) = 0;";

            var raw = Get<dynamic>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            var vm = new DataCompletenessVM { TotalCustomers = raw.Count };

            foreach (var (col, disp) in checkedFields)
            {
                int populated = raw.Count(r =>
                {
                    var dict = (IDictionary<string, object>)r;
                    if (!dict.TryGetValue(col, out var v) || v == null) return false;
                    if (v is string s) return !string.IsNullOrWhiteSpace(s);
                    if (v is DateTime dt) return dt > new DateTime(1900, 1, 1);
                    return true;
                });
                vm.Fields.Add(new FieldCompleteness
                {
                    FieldName = col, DisplayName = disp,
                    Populated = populated, Total = raw.Count
                });
            }

            foreach (var r in raw)
            {
                var dict = (IDictionary<string, object>)r;
                int filled = 0;
                var missing = new List<string>();
                foreach (var (col, disp) in checkedFields)
                {
                    bool present = dict.TryGetValue(col, out var v) && v != null
                        && !(v is string s && string.IsNullOrWhiteSpace(s))
                        && !(v is DateTime dt && dt <= new DateTime(1900, 1, 1));
                    if (present) filled++; else missing.Add(disp);
                }
                vm.Customers.Add(new CustomerCompletenessRow
                {
                    CustomerMasterId = (int)r.CustomerMasterId,
                    CustomerCode = (string)r.CustomerCode,
                    FullName = string.IsNullOrWhiteSpace((string)r.FullName) ? (string)r.CustomerCode : (string)r.FullName,
                    FilledCount = filled,
                    TotalChecked = checkedFields.Length,
                    MissingFields = missing
                });
            }
            vm.Customers = vm.Customers.OrderBy(c => c.Pct).ToList();
            return vm;
        }

        // ---------------- 4. Onboarding Funnel ----------------
        public OnboardingFunnelVM GetOnboardingFunnel(int clientId, int monthsBack)
        {
            const string sql = @"
SELECT YEAR(cm.created_on)                                  AS yr,
       MONTH(cm.created_on)                                  AS mo,
       COUNT(*)                                              AS Onboarded,
       SUM(CASE WHEN ca.status = 2 THEN 1 ELSE 0 END)        AS Approved,
       SUM(CASE WHEN ca.status = 3 THEN 1 ELSE 0 END)        AS Rejected,
       SUM(CASE WHEN ca.status IN (4,7) THEN 1 ELSE 0 END)   AS OnHold,
       SUM(CASE WHEN ca.status IN (0,1,5,6) OR ca.status IS NULL THEN 1 ELSE 0 END) AS Pending
  FROM customermaster cm
  LEFT JOIN customercase ca ON ca.cust_master_id = cm.id AND ca.is_deleted = 0
 WHERE cm.Client_Id = @clientId
   AND IFNULL(cm.is_deleted, 0) = 0
   AND cm.created_on >= DATE_SUB(NOW(), INTERVAL @monthsBack MONTH)
 GROUP BY YEAR(cm.created_on), MONTH(cm.created_on)
 ORDER BY yr, mo;";

            var rows = Get<dynamic>(sql, new { clientId, monthsBack }, commandType: CommandType.Text).ToList();
            var vm = new OnboardingFunnelVM();
            foreach (var r in rows)
            {
                int yr = Convert.ToInt32(r.yr); int mo = Convert.ToInt32(r.mo);
                vm.Months.Add(new OnboardingMonth
                {
                    Year = yr, Month = mo,
                    Label = new DateTime(yr, mo, 1).ToString("MMM yyyy"),
                    Onboarded = Convert.ToInt32(r.Onboarded),
                    Approved = Convert.ToInt32(r.Approved),
                    Rejected = Convert.ToInt32(r.Rejected),
                    OnHold = Convert.ToInt32(r.OnHold),
                    Pending = Convert.ToInt32(r.Pending)
                });
            }

            const string lagSql = @"
SELECT AVG(DATEDIFF(IFNULL(ca.updated_on, ca.created_on), cm.created_on))
  FROM customermaster cm
  INNER JOIN customercase ca ON ca.cust_master_id = cm.id AND ca.is_deleted = 0
 WHERE cm.Client_Id = @clientId
   AND ca.status IN (2, 3)
   AND IFNULL(cm.is_deleted, 0) = 0
   AND cm.created_on >= DATE_SUB(NOW(), INTERVAL @monthsBack MONTH);";
            using var conn = GetConnections();
            var avg = conn.ExecuteScalar<double?>(lagSql, new { clientId, monthsBack });
            vm.AvgDecisionDays = Math.Round(avg ?? 0, 1);
            return vm;
        }

        // ---------------- 5. Onboarding-Without-Screening Gap ----------------
        public ScreeningGapVM GetScreeningGap(int clientId)
        {
            const string totalSql = "SELECT COUNT(*) FROM customermaster WHERE Client_Id = @clientId AND IFNULL(is_deleted,0) = 0;";
            using var conn = GetConnections();
            var total = conn.ExecuteScalar<int>(totalSql, new { clientId });

            const string sql = @"
SELECT cm.id                                                AS CustomerMasterId,
       cm.cust_ref_id                                       AS CustomerCode,
       TRIM(CONCAT_WS(' ', cm.fname, cm.mname, cm.lname))   AS FullName,
       cm.cust_type                                         AS CustomerType,
       cm.nationality                                       AS Nationality,
       cm.created_on                                        AS CreatedOn
  FROM customermaster cm
  LEFT JOIN customercase ca ON ca.cust_master_id = cm.id AND ca.is_deleted = 0
 WHERE cm.Client_Id = @clientId
   AND IFNULL(cm.is_deleted, 0) = 0
   AND ca.id IS NULL
 ORDER BY cm.created_on DESC;";

            var raw = Get<dynamic>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            var vm = new ScreeningGapVM { TotalCustomers = total };
            var today = DateTime.UtcNow.Date;
            foreach (var r in raw)
            {
                DateTime created = (DateTime?)r.CreatedOn ?? today;
                vm.Rows.Add(new ScreeningGapRow
                {
                    CustomerMasterId = (int)r.CustomerMasterId,
                    CustomerCode = (string)r.CustomerCode,
                    FullName = string.IsNullOrWhiteSpace((string)r.FullName) ? (string)r.CustomerCode : (string)r.FullName,
                    CustomerType = (string)r.CustomerType,
                    Nationality = (string)r.Nationality,
                    CreatedOn = created,
                    DaysSinceOnboarding = (int)Math.Floor((today - created.Date).TotalDays)
                });
            }
            return vm;
        }

        // ---------------- 6. Sanctions Hit Disposition Matrix ----------------
        public SanctionsMatrixVM GetSanctionsMatrix(int clientId)
        {
            const string sql = @"
SELECT
  COUNT(*)                                                                             AS TotalCases,
  SUM(CASE WHEN ca.true_dometic_pep = 1 THEN 1 ELSE 0 END)                             AS TruePepDomestic,
  SUM(CASE WHEN ca.true_foreign_pep = 1 THEN 1 ELSE 0 END)                             AS TruePepForeign,
  SUM(CASE WHEN ca.true_adversemedia = 1 THEN 1 ELSE 0 END)                            AS TrueAdverseMedia,
  SUM(CASE WHEN ca.true_uae_un_sanction = 1 THEN 1 ELSE 0 END)                         AS TrueUaeUnSanction,
  SUM(CASE WHEN ca.true_other_sanction = 1 THEN 1 ELSE 0 END)                          AS TrueOtherSanction,
  SUM(CASE WHEN ca.partial_domestic_pep = 1 THEN 1 ELSE 0 END)                         AS PartialPepDomestic,
  SUM(CASE WHEN ca.partial_foreign_pep = 1 THEN 1 ELSE 0 END)                          AS PartialPepForeign,
  SUM(CASE WHEN ca.partial_adversemedia = 1 THEN 1 ELSE 0 END)                         AS PartialAdverseMedia,
  SUM(CASE WHEN (ca.true_dometic_pep + ca.true_foreign_pep + ca.true_adversemedia
              + ca.true_uae_un_sanction + ca.true_other_sanction
              + ca.partial_domestic_pep + ca.partial_foreign_pep + ca.partial_adversemedia) > 0
           THEN 1 ELSE 0 END)                                                          AS CasesWithAnyHit
  FROM customercase ca
 WHERE ca.Client_Id = @clientId AND ca.is_deleted = 0;";

            using var conn = GetConnections();
            var totals = conn.QuerySingleOrDefault<dynamic>(sql, new { clientId });
            var vm = new SanctionsMatrixVM();
            if (totals != null)
            {
                vm.TotalCases           = Convert.ToInt32(totals.TotalCases ?? 0);
                vm.TruePepDomestic      = Convert.ToInt32(totals.TruePepDomestic ?? 0);
                vm.TruePepForeign       = Convert.ToInt32(totals.TruePepForeign ?? 0);
                vm.TrueAdverseMedia     = Convert.ToInt32(totals.TrueAdverseMedia ?? 0);
                vm.TrueUaeUnSanction    = Convert.ToInt32(totals.TrueUaeUnSanction ?? 0);
                vm.TrueOtherSanction    = Convert.ToInt32(totals.TrueOtherSanction ?? 0);
                vm.PartialPepDomestic   = Convert.ToInt32(totals.PartialPepDomestic ?? 0);
                vm.PartialPepForeign    = Convert.ToInt32(totals.PartialPepForeign ?? 0);
                vm.PartialAdverseMedia  = Convert.ToInt32(totals.PartialAdverseMedia ?? 0);
                vm.CasesWithAnyHit      = Convert.ToInt32(totals.CasesWithAnyHit ?? 0);
            }

            const string recentSql = @"
SELECT ca.id                                                AS CaseId,
       cm.cust_ref_id                                       AS CustomerCode,
       TRIM(CONCAT_WS(' ', cm.fname, cm.mname, cm.lname))   AS FullName,
       ca.created_on                                        AS CreatedOn,
       ca.status                                            AS StatusNum,
       ca.true_dometic_pep, ca.true_foreign_pep, ca.true_adversemedia,
       ca.true_uae_un_sanction, ca.true_other_sanction,
       ca.partial_domestic_pep, ca.partial_foreign_pep, ca.partial_adversemedia
  FROM customercase ca
  INNER JOIN customermaster cm ON cm.id = ca.cust_master_id
 WHERE ca.Client_Id = @clientId AND ca.is_deleted = 0
   AND (ca.true_dometic_pep + ca.true_foreign_pep + ca.true_adversemedia
      + ca.true_uae_un_sanction + ca.true_other_sanction
      + ca.partial_domestic_pep + ca.partial_foreign_pep + ca.partial_adversemedia) > 0
 ORDER BY ca.created_on DESC LIMIT 50;";
            var rows = conn.Query<dynamic>(recentSql, new { clientId }).ToList();
            string StatusName(int s) => s switch { 0 => "Open", 1 => "In Progress", 2 => "Approved", 3 => "Rejected", 4 => "On Hold", 5 => "Closed", _ => "Other" };
            foreach (var r in rows)
            {
                var hits = new List<string>();
                if (Convert.ToInt32(r.true_dometic_pep)      == 1) hits.Add("PEP-Dom");
                if (Convert.ToInt32(r.true_foreign_pep)      == 1) hits.Add("PEP-For");
                if (Convert.ToInt32(r.true_adversemedia)     == 1) hits.Add("Adv-Media");
                if (Convert.ToInt32(r.true_uae_un_sanction)  == 1) hits.Add("UAE/UN");
                if (Convert.ToInt32(r.true_other_sanction)   == 1) hits.Add("Sanction");
                if (Convert.ToInt32(r.partial_domestic_pep)  == 1) hits.Add("~PEP-Dom");
                if (Convert.ToInt32(r.partial_foreign_pep)   == 1) hits.Add("~PEP-For");
                if (Convert.ToInt32(r.partial_adversemedia)  == 1) hits.Add("~Adv-Media");
                vm.RecentCases.Add(new SanctionMatrixCase
                {
                    CaseId = (int)r.CaseId,
                    CustomerCode = (string)r.CustomerCode,
                    FullName = (string)r.FullName,
                    HitTypes = string.Join(", ", hits),
                    CreatedOn = (DateTime)r.CreatedOn,
                    Status = StatusName(Convert.ToInt32(r.StatusNum))
                });
            }
            return vm;
        }

        // ---------------- 7. PEP Inventory ----------------
        public PepInventoryVM GetPepInventory(int clientId)
        {
            const string sql = @"
SELECT cm.id                                                AS CustomerMasterId,
       ca.id                                                AS CaseId,
       cm.cust_ref_id                                       AS CustomerCode,
       TRIM(CONCAT_WS(' ', cm.fname, cm.mname, cm.lname))   AS FullName,
       cm.nationality                                       AS Nationality,
       ca.true_dometic_pep                                   AS IsDomestic,
       ca.true_foreign_pep                                   AS IsForeign,
       ca.created_on                                         AS CreatedOn,
       ca.status                                             AS StatusNum
  FROM customercase ca
  INNER JOIN customermaster cm ON cm.id = ca.cust_master_id
 WHERE ca.Client_Id = @clientId AND ca.is_deleted = 0
   AND (ca.true_dometic_pep = 1 OR ca.true_foreign_pep = 1)
 ORDER BY ca.created_on DESC;";

            var raw = Get<dynamic>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            string StatusName(int s) => s switch { 0 => "Open", 1 => "In Progress", 2 => "Approved", 3 => "Rejected", 4 => "On Hold", 5 => "Closed", _ => "Other" };
            var vm = new PepInventoryVM();
            foreach (var r in raw)
            {
                vm.Rows.Add(new PepRow
                {
                    CustomerMasterId = (int)r.CustomerMasterId,
                    CaseId = (int)r.CaseId,
                    CustomerCode = (string)r.CustomerCode,
                    FullName = (string)r.FullName,
                    Nationality = (string)r.Nationality,
                    IsDomestic = Convert.ToInt32(r.IsDomestic) == 1,
                    IsForeign = Convert.ToInt32(r.IsForeign) == 1,
                    CreatedOn = (DateTime)r.CreatedOn,
                    CaseStatus = StatusName(Convert.ToInt32(r.StatusNum))
                });
            }
            return vm;
        }

        // ---------------- 8. Adverse Media Watch ----------------
        public AdverseMediaVM GetAdverseMedia(int clientId)
        {
            const string sql = @"
SELECT ca.id                                                AS CaseId,
       cm.id                                                AS CustomerMasterId,
       cm.cust_ref_id                                       AS CustomerCode,
       TRIM(CONCAT_WS(' ', cm.fname, cm.mname, cm.lname))   AS FullName,
       cm.nationality                                       AS Nationality,
       ca.true_adversemedia                                  AS TrueFlag,
       ca.created_on                                         AS CreatedOn,
       ca.updated_on                                         AS UpdatedOn,
       ca.status                                             AS StatusNum
  FROM customercase ca
  INNER JOIN customermaster cm ON cm.id = ca.cust_master_id
 WHERE ca.Client_Id = @clientId AND ca.is_deleted = 0
   AND (ca.true_adversemedia = 1 OR ca.partial_adversemedia = 1)
 ORDER BY ca.created_on DESC;";

            var raw = Get<dynamic>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            var vm = new AdverseMediaVM();
            string StatusName(int s) => s switch { 0 => "Open", 1 => "In Progress", 2 => "Approved", 3 => "Rejected", 4 => "On Hold", 5 => "Closed", _ => "Other" };
            var today = DateTime.UtcNow;
            foreach (var r in raw)
            {
                DateTime? upd = (DateTime?)r.UpdatedOn;
                DateTime created = (DateTime)r.CreatedOn;
                var lastTouched = upd ?? created;
                vm.Rows.Add(new AdverseMediaRow
                {
                    CaseId = (int)r.CaseId,
                    CustomerMasterId = (int)r.CustomerMasterId,
                    CustomerCode = (string)r.CustomerCode,
                    FullName = (string)r.FullName,
                    Nationality = (string)r.Nationality,
                    IsConfirmed = Convert.ToInt32(r.TrueFlag) == 1,
                    CreatedOn = created,
                    UpdatedOn = upd,
                    DaysSinceReview = (int)Math.Floor((today - lastTouched).TotalDays),
                    Status = StatusName(Convert.ToInt32(r.StatusNum))
                });
            }
            return vm;
        }

        // ---------------- 9. Cross-Border Customer Map ----------------
        // FATF grey/blacklist as of mid-2024 – embedded so we don't depend on an external table.
        // Update by editing the constants below; consider moving to lovmaster long-term.
        private static readonly HashSet<string> FatfBlack = new(StringComparer.OrdinalIgnoreCase)
        {
            "Iran", "Korea, Democratic People's Republic of", "North Korea", "Myanmar", "Burma"
        };
        private static readonly HashSet<string> FatfGrey = new(StringComparer.OrdinalIgnoreCase)
        {
            "Algeria", "Angola", "Bulgaria", "Burkina Faso", "Cameroon", "Cote d'Ivoire", "Croatia",
            "Democratic Republic of the Congo", "Haiti", "Kenya", "Lao", "Laos", "Lebanon",
            "Mali", "Monaco", "Mozambique", "Namibia", "Nepal", "Nigeria", "Philippines",
            "Senegal", "South Africa", "South Sudan", "Syria", "Tanzania", "Venezuela",
            "Vietnam", "Yemen"
        };

        public CrossBorderMapVM GetCrossBorderMap(int clientId)
        {
            const string natSql = @"
SELECT IFNULL(NULLIF(TRIM(nationality), ''), 'Unknown') AS Country, COUNT(*) AS Cnt
  FROM customermaster
 WHERE Client_Id = @clientId AND IFNULL(is_deleted, 0) = 0
 GROUP BY Country;";
            const string resSql = @"
SELECT IFNULL(NULLIF(TRIM(residence), ''), 'Unknown') AS Country, COUNT(*) AS Cnt
  FROM customermaster
 WHERE Client_Id = @clientId AND IFNULL(is_deleted, 0) = 0
 GROUP BY Country;";
            const string sowSql = @"
SELECT IFNULL(NULLIF(TRIM(sowsofcountry), ''), 'Unknown') AS Country, COUNT(*) AS Cnt
  FROM customermaster
 WHERE Client_Id = @clientId AND IFNULL(is_deleted, 0) = 0
 GROUP BY Country;";

            using var conn = GetConnections();
            var nat = conn.Query<(string Country, int Cnt)>(natSql, new { clientId }).ToList();
            var res = conn.Query<(string Country, int Cnt)>(resSql, new { clientId }).ToList();
            var sow = conn.Query<(string Country, int Cnt)>(sowSql, new { clientId }).ToList();

            var map = new Dictionary<string, CountryExposure>(StringComparer.OrdinalIgnoreCase);
            CountryExposure For(string c) {
                if (!map.TryGetValue(c, out var e)) { e = new CountryExposure { Name = c }; map[c] = e; }
                return e;
            }
            foreach (var (c, n) in nat) For(c).NationalityCount = n;
            foreach (var (c, n) in res) For(c).ResidenceCount = n;
            foreach (var (c, n) in sow) For(c).SowCount = n;

            foreach (var e in map.Values)
            {
                if (FatfBlack.Contains(e.Name)) e.FatfStatus = "Black";
                else if (FatfGrey.Contains(e.Name)) e.FatfStatus = "Grey";
            }

            var vm = new CrossBorderMapVM
            {
                Countries = map.Values
                    .Where(c => !string.Equals(c.Name, "Unknown", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(c => c.Total).ToList()
            };
            return vm;
        }

        // ---------------- 10. Channel & Product Risk Mix ----------------
        public ChannelProductMixVM GetChannelProductMix(int clientId)
        {
            string Band(string score) =>
                  string.IsNullOrEmpty(score) ? "Unrated"
                : score.Contains("High",  StringComparison.OrdinalIgnoreCase) ? "High"
                : score.Contains("Medium",StringComparison.OrdinalIgnoreCase) ? "Medium"
                : score.Contains("Low",   StringComparison.OrdinalIgnoreCase) ? "Low"
                : "Unrated";

            const string sql = @"
SELECT IFNULL(NULLIF(TRIM(delivery_channel),''),'Unspecified') AS Channel,
       IFNULL(NULLIF(TRIM(mode_of_payment),''),'Unspecified')  AS Payment,
       IFNULL(NULLIF(TRIM(product_name),''),'Unspecified')      AS Product,
       customer_final_risk_score                                AS RiskScore
  FROM customermaster
 WHERE Client_Id = @clientId AND IFNULL(is_deleted, 0) = 0;";
            var rows = Get<dynamic>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            var vm = new ChannelProductMixVM();

            void Bucket(List<ChannelMixCell> bucket, string dim, Func<dynamic, string> sel)
            {
                bucket.AddRange(rows
                    .GroupBy(r => new { K = sel(r), B = Band((string)r.RiskScore) })
                    .Select(g => new ChannelMixCell
                    {
                        Dimension = dim, Bucket = g.Key.K, RiskBand = g.Key.B, Count = g.Count()
                    })
                    .OrderByDescending(c => c.Count));
            }

            Bucket(vm.ChannelByRisk, "delivery_channel", r => (string)r.Channel);
            Bucket(vm.PaymentByRisk, "mode_of_payment", r => (string)r.Payment);
            Bucket(vm.ProductByRisk, "product_name",    r => (string)r.Product);

            // Sankey: channel → product → riskBand
            foreach (var g in rows.GroupBy(r => new { Ch = (string)r.Channel, Pr = (string)r.Product }))
                vm.Sankey.Add(new SankeyLink { Source = g.Key.Ch, Target = g.Key.Pr, Value = g.Count() });
            foreach (var g in rows.GroupBy(r => new { Pr = (string)r.Product, Bd = Band((string)r.RiskScore) }))
                vm.Sankey.Add(new SankeyLink { Source = g.Key.Pr, Target = g.Key.Bd, Value = g.Count() });
            return vm;
        }

        // ---------------- 11. Whitelist Governance Register ----------------
        public WhitelistGovernanceVM GetWhitelistGovernance(int clientId)
        {
            const string sql = @"
SELECT wl.id                                                AS Id,
       wl.cust_master_id                                    AS CustomerMasterId,
       cm.cust_ref_id                                       AS CustomerCode,
       TRIM(CONCAT_WS(' ', cm.fname, cm.mname, cm.lname))   AS FullName,
       cm.cust_type                                         AS CustomerType,
       wl.created_on                                        AS WhitelistedOn,
       (SELECT TRIM(CONCAT_WS(' ', u.fname, u.lname)) FROM user u WHERE u.id = wl.created_by) AS WhitelistedBy,
       (SELECT cc.comment FROM casecomment cc
         INNER JOIN customercase ca ON ca.id = cc.case_id
         WHERE ca.cust_master_id = wl.cust_master_id
           AND cc.comment_type LIKE '%hitelist%'
         ORDER BY cc.created_on DESC LIMIT 1)                AS Justification
  FROM customer_whitelist_log wl
  INNER JOIN customermaster cm ON cm.id = wl.cust_master_id
 WHERE cm.Client_Id = @clientId AND IFNULL(cm.is_deleted, 0) = 0
 ORDER BY wl.created_on DESC;";

            var raw = Get<dynamic>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            var vm = new WhitelistGovernanceVM();
            var today = DateTime.UtcNow;
            foreach (var r in raw)
            {
                DateTime when = (DateTime?)r.WhitelistedOn ?? today;
                vm.Events.Add(new WhitelistEvent
                {
                    Id = (int)r.Id,
                    CustomerMasterId = (int)r.CustomerMasterId,
                    CustomerCode = (string)r.CustomerCode,
                    FullName = (string)r.FullName,
                    CustomerType = (string)r.CustomerType,
                    WhitelistedOn = when,
                    WhitelistedBy = (string)r.WhitelistedBy,
                    Justification = (string)r.Justification,
                    DaysAgo = (int)Math.Floor((today - when).TotalDays)
                });
            }
            return vm;
        }

        // ---------------- 12. Proliferation Finance Register ----------------
        public ProliferationRegisterVM GetProliferationRegister(int clientId)
        {
            const string sql = @"
SELECT pf.Id                                                AS Id,
       pf.CompanyName                                       AS CompanyName,
       pf.CustomerType                                      AS CustomerType,
       pf.HsCode                                            AS HsCode,
       pf.CasNumber                                         AS CasNumber,
       pf.ChemicalName                                      AS ChemicalName,
       pf.MatchedChemicalName                               AS MatchedChemicalName,
       pf.Score                                             AS Score,
       pf.Status                                            AS Status,
       pf.StatusReason                                      AS StatusReason,
       pf.CreatedOn                                         AS CreatedOn
  FROM proliferationfinancecase pf
 WHERE pf.Client_Id = @clientId
 ORDER BY pf.CreatedOn DESC;";

            var raw = Get<dynamic>(sql, new { clientId }, commandType: CommandType.Text).ToList();
            var vm = new ProliferationRegisterVM();
            foreach (var r in raw)
            {
                vm.Rows.Add(new ProliferationRow
                {
                    Id = (int)r.Id,
                    CompanyName = (string)r.CompanyName,
                    CustomerType = (string)r.CustomerType,
                    HsCode = (string)r.HsCode,
                    CasNumber = (string)r.CasNumber,
                    ChemicalName = (string)r.ChemicalName,
                    MatchedChemicalName = (string)r.MatchedChemicalName,
                    Score = r.Score == null ? null : (int?)Convert.ToInt32(r.Score),
                    Status = (string)r.Status,
                    StatusReason = (string)r.StatusReason,
                    CreatedOn = (DateTime?)r.CreatedOn ?? DateTime.UtcNow
                });
            }
            return vm;
        }

        // ---------------- Hub live stats (cheap counts only) ----------------
        public ComplianceHubLiveStats GetHubLiveStats(int clientId)
        {
            using var conn = GetConnections();
            var s = new ComplianceHubLiveStats();
            int? Scalar(string sql) {
                try { return conn.ExecuteScalar<int?>(sql, new { clientId }); }
                catch { return 0; }
            }

            s.DocsExpiring30 = Scalar(@"
SELECT
  SUM(CASE WHEN PassportExpiryDate IS NOT NULL AND PassportExpiryDate <= DATE_ADD(NOW(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) +
  SUM(CASE WHEN EmiratesIdExpiryDate IS NOT NULL AND EmiratesIdExpiryDate <= DATE_ADD(NOW(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) +
  SUM(CASE WHEN id_expiry_date IS NOT NULL AND id_expiry_date <= DATE_ADD(NOW(), INTERVAL 30 DAY) THEN 1 ELSE 0 END)
FROM customermaster WHERE Client_Id = @clientId AND IFNULL(is_deleted,0) = 0;") ?? 0;

            s.OnboardedThisMonth = Scalar("SELECT COUNT(*) FROM customermaster WHERE Client_Id = @clientId AND YEAR(created_on)=YEAR(NOW()) AND MONTH(created_on)=MONTH(NOW());") ?? 0;
            s.ScreeningGapCount = Scalar(@"
SELECT COUNT(*) FROM customermaster cm
LEFT JOIN customercase ca ON ca.cust_master_id = cm.id AND ca.is_deleted = 0
WHERE cm.Client_Id = @clientId AND IFNULL(cm.is_deleted,0) = 0 AND ca.id IS NULL;") ?? 0;

            s.SanctionsHits = Scalar(@"
SELECT COUNT(*) FROM customercase
WHERE Client_Id = @clientId AND is_deleted = 0
  AND (true_dometic_pep + true_foreign_pep + true_adversemedia
     + true_uae_un_sanction + true_other_sanction
     + partial_domestic_pep + partial_foreign_pep + partial_adversemedia) > 0;") ?? 0;

            s.PepCount = Scalar("SELECT COUNT(*) FROM customercase WHERE Client_Id = @clientId AND is_deleted = 0 AND (true_dometic_pep = 1 OR true_foreign_pep = 1);") ?? 0;
            s.AdverseMediaCount = Scalar("SELECT COUNT(*) FROM customercase WHERE Client_Id = @clientId AND is_deleted = 0 AND (true_adversemedia = 1 OR partial_adversemedia = 1);") ?? 0;

            s.DistinctCountries = Scalar("SELECT COUNT(DISTINCT NULLIF(TRIM(nationality),'')) FROM customermaster WHERE Client_Id = @clientId AND IFNULL(is_deleted,0) = 0;") ?? 0;
            s.WhitelistEvents = Scalar(@"SELECT COUNT(*) FROM customer_whitelist_log wl
INNER JOIN customermaster cm ON cm.id = wl.cust_master_id WHERE cm.Client_Id = @clientId;") ?? 0;
            s.ProliferationCases = Scalar("SELECT COUNT(*) FROM proliferationfinancecase WHERE Client_Id = @clientId;") ?? 0;

            // For the others we just need cheap approximations — we'll leave them at 0 here
            // and calculate at-render; the hub doesn't need to be perfectly accurate.
            return s;
        }
    }
}
