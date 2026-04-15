using System;
using System.Collections.Generic;
using AML.Core.DataContract.Enum;
using AML.DTO.DTO.Risk;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AML.ViewModel.ViewModels.Risk
{
    public class RiskModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public SelectList CustomerList { get; set; }
        public DateTime DateofAssessment { get; set; }

        public string IsWhiteListed { get; set; }

        public string Address { get; set; }
        public int MainNationality { get; set; }
        public string MainNationalityTxt { get; set; }
        public SelectList MainNationalityLists { get; set; }

        
        public string FinalRiskScore { get; set; }
        public int RiskScoreSum { get; set; }
        public int RiskScoreCount { get; set; }
        public string RiskScoreBeforeOverride { get; set; }
        public string Riskc12Override { get; set; }
        public string Riskc9c11Override { get; set; }
        public string Riskc19Toc22Override { get; set; }
        public string RiskScreeningAndFindingOverride { get; set; }
        public List<RiskTypeCategoryDTO> RiskTypeCategoryDTO { get; set; }
        public List<ReportDataDTO> ReportDataDTO { get; set; }
        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
        public int version { get; set; }
        public string FullName { get; set; }

        public string Dob { get; set; }

        public string Remarks { get; set; }

        public string ScreenType { get; set; }

        public int CaseId { get; set; }

        public string RiskOverRide { get; set; }

        public string Type { get; set; }



    }
    //public class IndividualExcel
    //{
    //    public int Id { get; set; }
    //    public int CustomerId { get; set; }
    //    public string CustomerCode { get; set; }
    //    public string CustomerName { get; set; }
    //    public SelectList CustomerList { get; set; }
    //    public DateTime DateofAssessment { get; set; }

    //    public string IsWhiteListed { get; set; }

    //    public string Address { get; set; }
    //    public int MainNationality { get; set; }
    //    public string MainNationalityTxt { get; set; }
    //    public SelectList MainNationalityLists { get; set; }


    //    public string FinalRiskScore { get; set; }
    //    public int RiskScoreSum { get; set; }
    //    public int RiskScoreCount { get; set; }
    //    public string RiskScoreBeforeOverride { get; set; }
    //    public string Riskc12Override { get; set; }
    //    public string Riskc9c11Override { get; set; }
    //    public string Riskc19Toc22Override { get; set; }
    //    public string RiskScreeningAndFindingOverride { get; set; }
    //    public List<RiskTypeCategoryDTO> RiskTypeCategoryDTO { get; set; }
    //    public List<ReportDataDTO> ReportDataDTO { get; set; }
    //    public bool userAuthorised { get; set; }
    //    public int ClientId { get; set; }
    //    public int CreatedBy { get; set; }
    //    public int version { get; set; }
    //}

    public class SaveVersionDTO
    {
        public string CustomerCode { get; set; }
        public int VersionId { get; set; }
        public string Type { get; set; }
    }
}
