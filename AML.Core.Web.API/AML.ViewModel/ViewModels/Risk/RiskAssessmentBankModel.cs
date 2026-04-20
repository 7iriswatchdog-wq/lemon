using AML.DTO.DTO.Risk;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.Risk
{
    public class RiskAssessmentBankModel
    {

        public int CustomerId { get; set; }
        public SelectList CustomerList { get; set; }

        public string LegalNameOfTheEntity { get; set; }
        public DateTime EntityDate { get; set; }
        public string UniqueIDNumber { get; set; }
        public string CountryOfIncorporation { get; set; }
        public string TxtCountryOfIncorporation { get; set; }
        public SelectList CountryOfIncorporationLists { get; set; }
        public string IsWhiteListed { get; set; }

        public string RiskAssessmentRating { get; set; }
        public string RiskAssessmentRatingWithoutOverride { get; set; }
        public int RiskScoreSum { get; set; }
        public int RiskScoreCount { get; set; }
        public List<RiskTypeCategoryDTO> RiskTypeCategoryDTO { get; set; }
        public List<ReportDataDTO> ReportDataDTO { get; set; }
        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
        public int version { get; set; }
        public string Remarks { get; set; }
        public string ProductReference { get; set; }
        public decimal? ProductValue { get; set; }
        public string RiskComments { get; set; }
    }
}
