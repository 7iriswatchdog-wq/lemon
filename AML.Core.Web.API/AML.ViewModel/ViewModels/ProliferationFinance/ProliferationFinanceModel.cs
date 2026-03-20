namespace AML.ViewModel.ViewModels.ProliferationFinance
{
    public class ProliferationFinanceModel
    {
        public string CustomerType { get; set; } // Chemical or NonChemical
        public string CorporateId { get; set; }
        public string CompanyName { get; set; }
        public string HsCode { get; set; }
        public string CasNumber { get; set; }
        public string Eccn { get; set; }
        public string ChemicalName { get; set; }
        public string SynonymName { get; set; }
        public string SearchKeyword { get; set; }
        public string MatchedParagraph { get; set; }
        public int CaseId { get; set; }
        public string Status { get; set; }
        public System.DateTime? CreatedOn { get; set; }
        public System.DateTime? UpdatedOn { get; set; }
        public string Score { get; set; }
        public string StatusReason { get; set; }
        public string Type { get; set; } // Corporate/Individual
        public string MatchedChemicalName { get; set; }
        public string SearchHitDetails { get; set; }
        public System.Collections.Generic.List<AML.DTO.DTO.ProliferationFinance.UAEControlListDTO> MatchedChemicals { get; set; } = new System.Collections.Generic.List<AML.DTO.DTO.ProliferationFinance.UAEControlListDTO>();
        public System.Collections.Generic.List<AML.DTO.DTO.ProliferationFinance.PFSearchResultsMongoDTO.PF_Hit> MongoHits { get; set; } = new System.Collections.Generic.List<AML.DTO.DTO.ProliferationFinance.PFSearchResultsMongoDTO.PF_Hit>();
        public System.Collections.Generic.List<AML.DTO.DTO.CaseComment.CaseCommentDTO> Comments { get; set; } = new System.Collections.Generic.List<AML.DTO.DTO.CaseComment.CaseCommentDTO>();
    }
}
