
using AML.DTO.DTO.Sanction;
using AML.ViewModel.ViewModels.CustomerCase;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.ViewModel.ViewModels.Sanction
{
    public class ScreeningSearchModel
    {
        public string Name { get; set; }
        public SelectList Nationalities { get; set; }
        public string Nationality { get; set; }
        public string DOB { get; set; }

        public string customerType { get; set; }
        public string SelectionProperty { get; set; }
        public string[] SelectionProperties { get; set; }
        public SelectList CustomerCategories { get; set; }

        public List<ApiResultModel> DataList = new List<ApiResultModel>();

        public List<SanctionScreeningLogModel> SearchLogs = new List<SanctionScreeningLogModel>();

        public List<CaseModel> CaseLogs = new List<CaseModel>();

        public int clientId { get; set; }

        
    }

    public class SanctionScreeningLogModel
    {
        
        public int ID { get; set; }
        
        public string CustomerName { get; set; }
        
        public string Nationality { get; set; }
        
        public string CustomerType { get; set; }
       
        public DateTime DOB { get; set; }
        
        public string SearchType { get; set; }
        
        public string MatchName { get; set; }
        
        public int MatchScore { get; set; }
      
        public string MatchUID { get; set; }
        
        public string MatchCategory { get; set; }
       
        public string MatchType { get; set; }
       
        public string MatchNationality { get; set; }
        
        public string MatchIDNum { get; set; }
        
        public DateTime MatchDOB { get; set; }
        
        public int CreatedBy { get; set; }
        
        public DateTime CreatedOn { get; set; }
        
        public int RecordCount { get; set; }
        public int ClientId { get; set; }
        
        public string CreatedUser { get; set; }
    }
    public class ApiResultModel
    {
        public string matchname { get; set; }
        public string matchscore { get; set; }
        public string matchuid { get; set; }
        public string matchcategory { get; set; }
        public string matchtype { get; set; }
        public string matchnationality { get; set; }
        public string matchidnumber { get; set; }
        public string matchdob { get; set; }
    }
}

