using AML.Core.ServiceContract.SectoralTMS;
using AML.DTO.DTO.SectoralTMS;
using AML.ViewModel.ViewModels.SectoralTMS;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AML.Web.Controllers.SectoralTMS
{
    /// <summary>
    /// MVC controller for the Sectoral Transaction Monitoring (STM) module.
    /// Provides pages for:
    ///   - Rules management (Insurance + Real Estate)
    ///   - Transaction entry with customer pull-in & multi-party
    ///   - Open cases (rule-hit)
    ///   - Completed/reviewed cases
    ///   - Reports
    /// </summary>
    [SessionAuthorize]
    public class StmController : Controller
    {
        private readonly IStmService _stm;
        private readonly IMapper _mapper;
        private readonly IHttpClientHandler _clientHandler;
        private readonly IToastNotification _toast;

        public StmController(IStmService stm, IMapper mapper, IHttpClientHandler clientHandler, IToastNotification toast)
        {
            _stm = stm;
            _mapper = mapper;
            _clientHandler = clientHandler;
            _toast = toast;
        }

        private int CurrentClientId => _clientHandler.GetClientId();
        private int CurrentUserId => _clientHandler.GetUserId();
        private string CurrentUserName => HttpContext.Session.GetString("SessUsername") ?? "user";

        // ==================================================================
        // DASHBOARD
        // ==================================================================
        [Route("/stm")]
        public IActionResult Index()
        {
            return View();
        }

        // ==================================================================
        // RULES
        // ==================================================================
        [Route("/stm/rules")]
        public IActionResult Rules(string sector = null)
        {
            var rules = _stm.GetRules(CurrentClientId, sector).Result ?? new List<StmRuleDTO>();
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            ViewBag.Sectors = new SelectList(sectors, "SectorCode", "SectorName", sector);
            ViewBag.ActiveSector = sector;
            return View(_mapper.Map<List<StmRuleModel>>(rules));
        }

        [Route("/stm/rules/create")]
        public IActionResult CreateRule()
        {
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            var model = new StmRuleModel
            {
                IsActive = 1,
                LogicalOperator = "AND",
                ActionOnHit = "CREATE_CASE",
                RiskRating = "High",
                RuleScore = 50,
                Conditions = new List<StmRuleConditionModel>
                {
                    new StmRuleConditionModel { SequenceNo = 1, Conjunction = "AND" }
                },
                SectorList = new SelectList(sectors, "Id", "SectorName")
            };
            return View("RuleEditor", model);
        }

        [Route("/stm/rules/edit/{id:int}")]
        public IActionResult EditRule(int id)
        {
            var ruleResponse = _stm.GetRule(id);
            if (ruleResponse.Status != 200 || ruleResponse.Result == null)
            {
                _toast.AddErrorToastMessage("Rule not found");
                return RedirectToAction("Rules");
            }
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            var model = _mapper.Map<StmRuleModel>(ruleResponse.Result);
            model.SectorList = new SelectList(sectors, "Id", "SectorName", model.SectorId);
            return View("RuleEditor", model);
        }

        [HttpPost("/stm/rules/save")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveRule(StmRuleModel model)
        {
            var dto = _mapper.Map<StmRuleDTO>(model);
            dto.ClientId = CurrentClientId;
            dto.CreatedBy = CurrentUserId;
            dto.UpdatedBy = CurrentUserId;

            if (dto.Id > 0)
            {
                _stm.UpdateRule(dto);
                _toast.AddSuccessToastMessage("Rule updated successfully");
            }
            else
            {
                _stm.CreateRule(dto);
                _toast.AddSuccessToastMessage("Rule created successfully");
            }
            return RedirectToAction("Rules");
        }

        [HttpPost("/stm/rules/toggle/{id:int}")]
        public JsonResult ToggleRule(int id, int isActive)
        {
            var resp = _stm.ToggleRuleStatus(id, isActive, CurrentUserId);
            return Json(new { success = resp.Status == 200, message = resp.Message });
        }

        // ==================================================================
        // TRANSACTIONS
        // ==================================================================
        [Route("/stm/transactions")]
        public IActionResult Transactions(string sector = null)
        {
            var search = new StmTransactionSearchDTO { ClientId = CurrentClientId, SectorCode = sector };
            var list = _stm.SearchTransactions(search).Result ?? new List<StmTransactionDTO>();
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            ViewBag.Sectors = new SelectList(sectors, "SectorCode", "SectorName", sector);
            ViewBag.ActiveSector = sector;
            return View(_mapper.Map<List<StmTransactionModel>>(list));
        }

        [Route("/stm/transactions/create")]
        public IActionResult CreateTransaction()
        {
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            var model = new StmTransactionModel
            {
                TranDate = DateTime.Now,
                Currency = "AED",
                ClientId = CurrentClientId,
                SectorList = new SelectList(sectors, "Id", "SectorName")
            };
            return View(model);
        }

        [HttpPost("/stm/transactions/save")]
        [ValidateAntiForgeryToken]
        public JsonResult SaveTransaction([FromBody] StmTransactionModel model)
        {
            var dto = _mapper.Map<StmTransactionDTO>(model);
            dto.ClientId = CurrentClientId;
            dto.IsMultiParty = (model.Parties != null && model.Parties.Count > 0) ? 1 : 0;
            var resp = _stm.SubmitTransaction(dto, CurrentUserId);
            return Json(new
            {
                success = resp.Status == 200,
                message = resp.Message,
                data = resp.Result
            });
        }

        [Route("/stm/transactions/view/{id:int}")]
        public IActionResult ViewTransaction(int id, string returnUrl = null)
        {
            var resp = _stm.GetTransaction(id);
            if (resp.Status != 200 || resp.Result == null) return NotFound();
            // Anti-open-redirect: only honour same-host relative paths.
            ViewBag.ReturnUrl = (!string.IsNullOrWhiteSpace(returnUrl)
                                 && Url.IsLocalUrl(returnUrl))
                ? returnUrl
                : "/stm/transactions";
            return View(_mapper.Map<StmTransactionModel>(resp.Result));
        }

        // ==================================================================
        // CASES - OPEN
        // ==================================================================
        [Route("/stm/cases/open")]
        public IActionResult OpenCases(string sector = null)
        {
            var search = new StmCaseSearchDTO { ClientId = CurrentClientId, SectorCode = sector, Status = "OPEN" };
            var list = _stm.GetOpenCases(search).Result ?? new List<StmCaseDTO>();
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            ViewBag.Sectors = new SelectList(sectors, "SectorCode", "SectorName", sector);
            ViewBag.ActiveSector = sector;
            ViewBag.PageTitle = "Open Cases";
            return View("CaseList", _mapper.Map<List<StmCaseModel>>(list));
        }

        // ==================================================================
        // CASES - COMPLETED / REVIEWED
        // ==================================================================
        [Route("/stm/cases/completed")]
        public IActionResult CompletedCases(string sector = null)
        {
            var search = new StmCaseSearchDTO { ClientId = CurrentClientId, SectorCode = sector };
            var list = _stm.GetCompletedCases(search).Result ?? new List<StmCaseDTO>();
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            ViewBag.Sectors = new SelectList(sectors, "SectorCode", "SectorName", sector);
            ViewBag.ActiveSector = sector;
            ViewBag.PageTitle = "Completed / Reviewed Cases";
            return View("CaseList", _mapper.Map<List<StmCaseModel>>(list));
        }

        [Route("/stm/cases/view/{id:int}")]
        public IActionResult ViewCase(int id)
        {
            var resp = _stm.GetCase(id);
            if (resp.Status != 200 || resp.Result == null) return NotFound();
            return View(_mapper.Map<StmCaseModel>(resp.Result));
        }

        [HttpPost("/stm/cases/review")]
        [ValidateAntiForgeryToken]
        public JsonResult ReviewCase(int caseId, string decision, string status, string remarks)
        {
            var resp = _stm.ReviewCase(caseId, decision, remarks, status, CurrentUserId, CurrentUserName);
            return Json(new { success = resp.Status == 200, message = resp.Message });
        }

        [HttpPost("/stm/cases/comment")]
        [ValidateAntiForgeryToken]
        public JsonResult AddComment(int caseId, string comment)
        {
            var resp = _stm.AddCaseComment(caseId, comment, CurrentUserId, CurrentUserName);
            return Json(new { success = resp.Status == 200, message = resp.Message });
        }

        // ==================================================================
        // REPORTS
        // ==================================================================
        [Route("/stm/reports")]
        public IActionResult Reports()
        {
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            var model = new StmReportFilterModel
            {
                FromDate = DateTime.Now.AddMonths(-3),
                ToDate = DateTime.Now,
                SectorList = new SelectList(sectors, "SectorCode", "SectorName")
            };
            return View(model);
        }

        [HttpPost("/stm/reports/customer-transactions")]
        public JsonResult ReportCustomerTransactions(string customerId, string sector = null,
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return Json(new { success = false, message = "Customer is required" });

            var list = _stm.GetCustomerTransactions(customerId, CurrentClientId).Result
                       ?? new List<StmTransactionDTO>();
            if (!string.IsNullOrEmpty(sector))
                list = list.Where(t => string.Equals(t.SectorName, sector, StringComparison.OrdinalIgnoreCase)
                                    || t.SectorId.ToString() == sector).ToList();
            if (fromDate.HasValue) list = list.Where(t => t.TranDate >= fromDate.Value).ToList();
            if (toDate.HasValue)   list = list.Where(t => t.TranDate <= toDate.Value).ToList();

            // Also pull cases for this customer (both open and reviewed). Merge unique by Id.
            var openSearch = new StmCaseSearchDTO
            {
                ClientId = CurrentClientId, CustomerId = customerId, SectorCode = sector,
                FromDate = fromDate, ToDate = toDate, Status = "OPEN"
            };
            var openCases = _stm.GetOpenCases(openSearch).Result ?? new List<StmCaseDTO>();
            var doneCases = _stm.GetCompletedCases(new StmCaseSearchDTO
            {
                ClientId = CurrentClientId, CustomerId = customerId, SectorCode = sector,
                FromDate = fromDate, ToDate = toDate
            }).Result ?? new List<StmCaseDTO>();
            var caseDict = openCases.ToDictionary(c => c.Id);
            foreach (var c in doneCases) caseDict[c.Id] = c;
            var cases = caseDict.Values.OrderByDescending(c => c.CreatedOn).ToList();

            return Json(new { success = true, data = list, cases = cases });
        }

        // ==================================================================
        // CUSTOMER LOOKUP (used by transaction-entry screen)
        // ==================================================================
        [HttpGet("/stm/customers/search")]
        public JsonResult SearchCustomers(string q)
        {
            var resp = _stm.SearchCustomers(q, CurrentClientId);
            return Json(new { success = resp.Status == 200, data = resp.Result });
        }

        [HttpGet("/stm/customers/details/{customerId}")]
        public JsonResult GetCustomerDetails(string customerId)
        {
            var resp = _stm.GetCustomerDetails(customerId, CurrentClientId);
            return Json(new { success = resp.Status == 200, data = resp.Result });
        }

        // ==================================================================
        // TRANSACTION RISK (Insurance + Real Estate)
        // ==================================================================

        // List of all factors for a sector (including inactive) for the management page
        [Route("/stm/risk/factors")]
        public IActionResult RiskFactors(string sector = "INS")
        {
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            var sectorDto = sectors.FirstOrDefault(s => string.Equals(s.SectorCode, sector, StringComparison.OrdinalIgnoreCase))
                            ?? sectors.FirstOrDefault();
            List<StmTxnRiskFactorDTO> factors = new List<StmTxnRiskFactorDTO>();
            if (sectorDto != null)
                factors = _stm.GetAllTxnRiskFactors(sectorDto.Id, CurrentClientId).Result ?? factors;
            ViewBag.Sectors = new SelectList(sectors, "SectorCode", "SectorName", sector);
            ViewBag.ActiveSector = sector;
            ViewBag.SectorName = sectorDto?.SectorName ?? "";
            ViewBag.SectorList = sectors;
            return View(factors);
        }

        // Render the editor for a new factor
        [Route("/stm/risk/factors/create")]
        public IActionResult CreateRiskFactor(string sector = "INS")
        {
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            var sectorDto = sectors.FirstOrDefault(s => string.Equals(s.SectorCode, sector, StringComparison.OrdinalIgnoreCase))
                            ?? sectors.FirstOrDefault();
            var model = new StmTxnRiskFactorDTO
            {
                SectorId = sectorDto?.Id ?? 0,
                SectorCode = sectorDto?.SectorCode,
                SectorName = sectorDto?.SectorName,
                FactorType = "NUMERIC",
                Weight = 1,
                IsActive = 1,
                SequenceNo = 99,
                ClientId = CurrentClientId,
                Bands = new List<StmTxnRiskBandDTO>
                {
                    new StmTxnRiskBandDTO { SequenceNo = 1, Score = 1, Rating = "Low" }
                }
            };
            ViewBag.SectorList = new SelectList(sectors, "Id", "SectorName", model.SectorId);
            return View("RiskFactorEditor", model);
        }

        // Render the editor for an existing factor
        [Route("/stm/risk/factors/edit/{id:int}")]
        public IActionResult EditRiskFactor(int id)
        {
            var resp = _stm.GetTxnRiskFactor(id);
            if (resp.Status != 200 || resp.Result == null)
            {
                _toast.AddErrorToastMessage("Risk factor not found");
                return RedirectToAction("RiskFactors");
            }
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            ViewBag.SectorList = new SelectList(sectors, "Id", "SectorName", resp.Result.SectorId);
            return View("RiskFactorEditor", resp.Result);
        }

        // Form-post target for both create and edit
        [HttpPost("/stm/risk/factors/save")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveRiskFactor(StmTxnRiskFactorDTO model)
        {
            // Default the band sequence numbers / bind nullable numerics that come in blank
            if (model.Bands != null)
            {
                for (int i = 0; i < model.Bands.Count; i++)
                {
                    if (model.Bands[i].SequenceNo <= 0) model.Bands[i].SequenceNo = i + 1;
                    if (string.IsNullOrWhiteSpace(model.Bands[i].MatchValue))   model.Bands[i].MatchValue   = null;
                    if (string.IsNullOrWhiteSpace(model.Bands[i].MatchInList))  model.Bands[i].MatchInList  = null;
                }
            }
            model.ClientId = CurrentClientId;
            model.CreatedBy = CurrentUserId;

            if (model.Id > 0)
            {
                _stm.UpdateTxnRiskFactor(model);
                _toast.AddSuccessToastMessage("Risk factor updated");
            }
            else
            {
                _stm.CreateTxnRiskFactor(model);
                _toast.AddSuccessToastMessage("Risk factor created");
            }
            // After save, send the user back to the list filtered by the factor's sector.
            var sectors = _stm.GetSectors(CurrentClientId).Result ?? new List<StmSectorDTO>();
            var sectorCode = sectors.FirstOrDefault(s => s.Id == model.SectorId)?.SectorCode ?? "INS";
            return RedirectToAction("RiskFactors", new { sector = sectorCode });
        }

        [HttpPost("/stm/risk/factors/delete/{id:int}")]
        public JsonResult DeleteRiskFactor(int id)
        {
            var resp = _stm.DeleteTxnRiskFactor(id);
            return Json(new { success = resp.Status == 200, message = resp.Message });
        }

        [HttpPost("/stm/risk/factors/toggle/{id:int}")]
        public JsonResult ToggleRiskFactor(int id, int isActive)
        {
            var resp = _stm.GetTxnRiskFactor(id);
            if (resp.Status != 200 || resp.Result == null)
                return Json(new { success = false, message = "Not found" });
            resp.Result.IsActive = isActive;
            var updateResp = _stm.UpdateTxnRiskFactor(resp.Result);
            return Json(new { success = updateResp.Status == 200, message = updateResp.Message });
        }

        // Re-run risk on a transaction (admin tool)
        [HttpPost("/stm/risk/evaluate/{transactionId:int}")]
        public JsonResult EvaluateRisk(int transactionId)
        {
            var resp = _stm.EvaluateTxnRisk(transactionId);
            return Json(new
            {
                success = resp.Status == 200,
                message = resp.Message,
                data = resp.Result
            });
        }

        // Get risk result for a transaction (used by the case detail + transaction view)
        [HttpGet("/stm/risk/result/{transactionId:int}")]
        public JsonResult GetRisk(int transactionId)
        {
            var saved = _stm.GetSavedRiskResult(transactionId).Result;
            return Json(new
            {
                success = saved != null,
                data = saved == null ? null : new
                {
                    saved.TotalScore,
                    saved.RiskRating,
                    saved.FactorCount,
                    saved.ComputedOn,
                    Breakdown = string.IsNullOrEmpty(saved.FactorBreakdown)
                        ? null
                        : Newtonsoft.Json.JsonConvert.DeserializeObject<List<StmTxnRiskItemResult>>(saved.FactorBreakdown)
                }
            });
        }
    }
}
