using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.TransactionMonitor
{
    public class TMSCase
    {
        public int Id { get; set; }
        public string TranRefno { get; set; } //Transaction reference number
        public string TranDate { get; set; } //Transaction reference number
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string RemitterNationality { get; set; }
        public string BeneficiaryId { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneficiaryCountry { get; set; }
        public string BeneficiaryNationCode { get; set; }

        public string RuleViolated { get; set; }
        public string Created_on { get; set; }
       public string TransactionScore { get; set; }
        public int Status { get; set; }
        public int UpdatedBy { get; set; }
        public string updatedon { get; set; }
        public string comments { get; set; }
        public List<TMSCaseNewModel> TMSCaseNewModels { get; set; }
        public List<TMSCaseNewModel> TMSCaseViewModel { get; set; }
        public List<TMSCaseNewModel> TMSBeneficiaryViewModel { get; set; }
        public List<TMSCaseNewModel> TMSCaseExclusiveModel { get; set; }

    }
    public class TMSCaseNewModel
    {
        public int Id { get; set; }
        public string TranRefno { get; set; } //Transaction reference number
        public string TranDate { get; set; } //Transaction reference Date
        public string TranType { get; set; } //Transaction Type
        public string CustomerId { get; set; }//RemitterId
        public string CustomerName { get; set; }//Remitter Name
        public string BeneficiaryName { get; set; }
        public string BeneficiaryCountry { get; set; }
        public string BeneficiaryAccountNo { get; set; }
        public string Amount { get; set; }
        public string StatusDescription { get; set; }




    }

}
