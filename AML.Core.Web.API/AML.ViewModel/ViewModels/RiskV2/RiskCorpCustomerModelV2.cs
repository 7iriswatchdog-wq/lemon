using System;
using System.Collections.Generic;
using AML.Core.DataContract.Enum;
using AML.DTO.DTO.RiskV2;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AML.ViewModel.ViewModels.RiskV2
{
    public class RiskCorpCustomerModelV2
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public SelectList CustomerList { get; set; }
        public string LegalNameOfEntity { get; set; }
        public string UniqueID { get; set; }
        public DateTime DateofAssessment { get; set; }
        public int CountryOfIncorporation { get; set; }
        public string CountryOfIncorporationTxt { get; set; }
        public SelectList CountryOfIncorporationList { get; set; }
        
        public string IsWhiteListed { get; set; }


        public string RiskAssessmentRating { get; set; }
        public string RiskAssessmentRatingWithoutOverride { get; set; }
        public float RiskScoreSum { get; set; }
        public int RiskScoreCount { get; set; }
        public List<DTO.DTO.LovMaster.LovMasterDTO> RiskConfigData { get; set; }
        public List<RiskTypeCategoryDTOV2> RiskTypeCategoryDTO { get; set; }
        public List<DTO.DTO.RiskV2.RIskConfigurationMasterDTOV2> RiskTypeDTO { get; set; }
        public List<DTO.DTO.RiskV2.ReportDataDTOV2> ReportDataDTO { get; set; }
        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
        public int version { get; set; }

        public string Remarks { get; set; }
    }
}
