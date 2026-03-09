using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Report
{
    public class CaseReportRequestDTO
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string User { get; set; }
        public string Status { get; set; }
        public int ClientId { get; set; }
        public string Cust_type { get; set; }
        public string UpdatedByUserId { get; set; }

     
        public int NoMatch { get; set; }
        
        public int TrueDomesticpep { get; set; }
       
        public int TrueForeignpep { get; set; }
        
        public int TrueAdverseMedia { get; set; }
      
        public int PartialDomesticpep { get; set; }
       
        public int PartialForeignpep { get; set; }
        
        public int Partialadversemedia { get; set; }

        public int TrueUAEUNSanction { get; set; }
        public int TrueOtherSanction { get; set; }


        public DateTime expiryStartDate { get; set; }

        public DateTime expiryEndDate { get; set; }

        public string idstatus { get; set; }


        public string pepfrmclients { get; set; }

        public string SearchValue { get; set; }

       public string matchscore { get; set; }
        public int createdBy { get; set; }
        public int caseStatus { get; set; }
        public string riskLevel { get; set; }
        
        public string usergroupName { get; set; }




    }
}
