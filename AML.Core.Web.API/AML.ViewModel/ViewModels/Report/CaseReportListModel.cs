using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using AML.DTO.DTO.CustomerCase;
using AML.ViewModel.ViewModels.CaseDocument;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AML.ViewModel.ViewModels.Report
{
    public class CaseReportListModel
    {
        public string CustomerID { get; set; }
        public string CustomerType { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedOn { get; set; }

        public string Match { get; set; }
        public string IsMatched { get; set; }
        public int ClientId { get; set; }

        public string CompanyCode { get; set; }
        public string Details { get; set; }

        
        public string Nationality { get; set; }
     
        public string dob { get; set; }

        //public DateTime DOB { get; set; }
       
      
        public string ResidenceStatus { get; set; }
      
        public string OccupatinTypeTxt { get; set; }
        public string EmployerName { get; set; }
        public string EmployerAddress { get; set; }
        public string ResidenceAddress { get; set; }
        
        public string Email { get; set; }
        
        public string Mobile { get; set; }
        public string CustomerIdType { get; set; }
        public string CustomerIdNumber { get; set; }
        public string PEPStatus { get; set; }
        public string GuardianName { get; set; }
        public string GuardianRelation { get; set; }
        public string Remarks { get; set; }
        public int C6Threshold { get; set; }
        public int Threshold { get; set; }
        public string PlaceOfBirth { get; set; }
        public string MaritalStatus { get; set; }

        public string IdExpdate { get; set; }

        public string BankAccountNo { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountBranch { get; set; }
        public string Sourceofincome { get; set; }
       
        public string DeliveryChannelName { get; set; }
        public string ProductName { get; set; }
       

       
        public string Modeofpayment { get; set; }


        public string EntityTypeTxt { get; set; }
       
        public string CorporateWebsite { get; set; }

       

        public string VATRegistrationNumber { get; set; }

       
        public string Country { get; set; }

       
        public string Emirate { get; set; }

      
        public string City { get; set; }

       
        public string POBox { get; set; }

        
        public string LicenseNumber { get; set; }
      
        public string LicenseIssueDate { get; set; }
        
        public string LicenseIssuingAuthority { get; set; }
        
        public string LicenseExpiryDate { get; set; }
       
        public string PlaceofIssue { get; set; }

        public string BusinessType { get; set; }

        public string LicenseTypeTxt { get; set; }

        
        public string IsWhiteListed { get; set; }

        public string CaseStatus { get; set; }

        
        public string Individual_final_risk_score { get; set; }

       
        public string corporate_final_risk_score { get; set; }





    }

    public class CaseReportDownloadModel
    {
        public string UploadedBy { get; set; }
        public int TotalRows { get; set; }
        public int Matched { get; set; }
        public int UnMatched { get; set; }
        public List<CaseReportListModel> Data { get; set; }
        public List<ApiResultModel> ApiResultsjson { get; set; }
    }

    public class clientSearch
    { 
        public int client_id { get; set; }

        public string client_name { get; set; }
        public int count { get; set; }


        
    }
}
