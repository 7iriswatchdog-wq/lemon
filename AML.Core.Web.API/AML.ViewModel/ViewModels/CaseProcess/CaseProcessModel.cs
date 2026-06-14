using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Risk;
using AML.DTO.DTO.SectoralTMS;
using AML.ViewModel.ViewModels.CaseDocument;
using AML.ViewModel.ViewModels.Corporate;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.ProliferationFinance;
using AML.ViewModel.ViewModels.Risk;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.CaseProcess
{
    public class CaseProcessModel
    {
        public int Index { get; set; }
        public CaseModel Case { get; set; }
        public List<CaseDocumentModel> CaseDocuments { get; set; }
        public List<CaseModel> ShareholdersData { get; set; }

        public List<CaseModel> CustomerCaseData { get; set; }

        public List<RiskReportModel> RiskVersionData { get; set; }
        public List<CaseModel> CaseComments { get; set; }
        public List<CaseModel> CaseAssignments { get; set; }
        public SelectList DocumentTypes { get; set; }
        public SelectList DocumentCategories { get; set; }
        public SelectList Users { get; set; }
        public List<DataListModel> DataList { get; set; }

        public List<WebListModel> WebDataList { get; set; }

        public List<ProliferationFinanceModel> ProliferationFinanceData { get; set; }
        public string Url { get; set; }
        public List<ApiResultModel> apiResultModels { get; set; }

        public List<RiskTypeCategoryDTO> RiskTypeCategoryDTO { get; set; }
        public string IsWhiteListed { get; set; }

        public string Address { get; set; }
        public int MainNationality { get; set; }
        public string MainNationalityTxt { get; set; }
        public string FinalRiskScore { get; set; }
        public int RiskScoreSum { get; set; }
        public int RiskScoreCount { get; set; }
        public string RiskScoreBeforeOverride { get; set; }
        public string Riskc12Override { get; set; }
        public string Riskc9c11Override { get; set; }
        public string Riskc19Toc22Override { get; set; }
        public string RiskScreeningAndFindingOverride { get; set; }
        public DateTime DateofAssessment { get; set; }
        public string Remarks { get; set; }

        public bool userAuthorised { get; set; }

        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }

        public SelectList CustomerList { get; set; }

        public string ScreenType { get; set; }

        public string LegalNameOfEntity { get; set; }
        public string UniqueID { get; set; }

        public string RiskAssessmentRating { get; set; }
        public string RiskAssessmentRatingWithoutOverride { get; set; }

        public string RiskOverRide { get; set; }

        public bool RiskCreation { get; set; }

        public bool CommentCase { get; set; }

        public bool DocumentsCase { get; set; }

        public bool TransferCase { get; set; }

        public bool SaveSearResult { get; set; }

        public Dictionary<string, string> ActionRights { get; set; }


        public bool IsReadOnly { get; set; }
        public string DualGoodsMatchStatus { get; set; }
        public string MilitaryGoodsMatchStatus { get; set; }

        public string Type { get; set; }
        public string ProductReference { get; set; }
        public decimal? ProductValue { get; set; }
        public string RiskComments { get; set; }

        //public List<PassportDetails> passportDetails { get; set; }

        // ---------- Sectoral Transaction Monitoring (STM) ----------
        // Populated only when the client has the "Transaction Monitoring" module
        // right enabled (menu_master.Menu_Id = 12) AND the customer's transactions
        // fall within the sectors the client has access to via stm_client_sector_access.
        // When the module is OFF, IsStmEnabled stays false and the view falls back
        // to the previous behaviour.
        public bool IsStmEnabled { get; set; }
        public List<string> StmAllowedSectors { get; set; } = new List<string>();
        public List<StmTransactionDTO> StmTransactions { get; set; } = new List<StmTransactionDTO>();
        // Parallel collection keyed by transaction id so views can pull the
        // saved Transaction Risk breakdown without re-querying inside loops.
        public Dictionary<int, StmTxnRiskResultDTO> StmTxnRiskResults { get; set; } = new Dictionary<int, StmTxnRiskResultDTO>();
    }
}
