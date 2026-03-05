using AML.DTO.DTO.RiskV2;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.RiskV2
{
    public class RiskAssessmentBankModelV2
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
        public List<RiskTypeCategoryDTOV2> RiskTypeCategoryDTO { get; set; }
        public List<ReportDataDTOV2> ReportDataDTO { get; set; }
        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
        public int version { get; set; }
    }
}
