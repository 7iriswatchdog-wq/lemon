using System;
using System.Collections.Generic;
using AML.Core.DataContract.Enum;
using AML.DTO.DTO.Risk;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AML.ViewModel.ViewModels.Risk
{
    public class RiskCorpCustomerModel
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
        public int RiskScoreSum { get; set; }
        public int RiskScoreCount { get; set; }
        public List<DTO.DTO.LovMaster.LovMasterDTO> RiskConfigData { get; set; }
        public List<RiskTypeCategoryDTO> RiskTypeCategoryDTO { get; set; }
        public List<DTO.DTO.Risk.RIskConfigurationMasterDTO> RiskTypeDTO { get; set; }
        public List<DTO.DTO.Risk.ReportDataDTO> ReportDataDTO { get; set; }
        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
        public int version { get; set; }

        public string Remarks { get; set; }

        public string ScreenType { get; set; }

        public int CaseId { get; set; }

        public string UserGroup { get; set; }
    }
}
