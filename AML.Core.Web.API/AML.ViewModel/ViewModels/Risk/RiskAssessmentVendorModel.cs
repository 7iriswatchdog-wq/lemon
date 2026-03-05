using AML.DTO.DTO.Risk;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.Risk
{
    public class RiskAssessmentVendorModel
    {
        public int CustomerId { get; set; }
        public SelectList CustomerList { get; set; }
        public string VendorName { get; set; }
        public DateTime RegisteredDate { get; set; }
        public string RegistrationNumber { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public SelectList CountryLists { get; set; }

        public string IsWhiteListed { get; set; }

        //KYC Doc
        
        public string RiskAssessmentRating { get; set; }
        public string RiskAssessmentRatingWithoutOverride { get; set; }
        public int RiskScoreSum { get; set; }
        public int RiskScoreCount { get; set; }
        public string TransactionMonitoringOverride { get; set; }
        public List<RiskTypeCategoryDTO> RiskTypeCategoryDTO { get; set; }
        public List<ReportDataDTO> ReportDataDTO { get; set; }
        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
        public int version { get; set; }
    }
}
