using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.CaseDocument;
using AML.ViewModel.ViewModels.Corporate;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.Kyc;
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

        public List<RiskReportModel> RiskVersionData { get; set; }
        public List<CaseModel> CaseComments { get; set; }
        public List<CaseModel> CaseAssignments { get; set; }
        public SelectList DocumentTypes { get; set; }
        public SelectList DocumentCategories { get; set; }
        public SelectList Users { get; set; }
        public List<DataListModel> DataList { get; set; }
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


        public string  DualGoodsMatchStatus { get; set; }

        //public List<PassportDetails> passportDetails { get; set; }
    }
}
