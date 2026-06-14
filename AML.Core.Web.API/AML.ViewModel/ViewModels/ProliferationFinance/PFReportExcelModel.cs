using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AML.ViewModel.ViewModels.ProliferationFinance
{
    public class PFReportExcelModel
    {
        [DisplayName("Details")]
        public string Details { get; set; }

        [DisplayName("Case ID")]
        public string CaseId { get; set; }

        [DisplayName("Created Date")]
        public string CreatedDate { get; set; }

        [DisplayName("Corporate ID")]
        public string CorporateId { get; set; }

        [DisplayName("Company Name")]
        public string CompanyName { get; set; }

        [DisplayName("Customer Type")]
        public string CustomerType { get; set; }

        [DisplayName("Product / Chemical Name")]
        public string ChemicalName { get; set; }

        [DisplayName("HS Code")]
        public string HsCode { get; set; }

        [DisplayName("CAS Number")]
        public string CasNumber { get; set; }

        [DisplayName("Status")]
        public string Status { get; set; }

        [DisplayName("Remarks")]
        public string Remarks { get; set; }
    }
}
