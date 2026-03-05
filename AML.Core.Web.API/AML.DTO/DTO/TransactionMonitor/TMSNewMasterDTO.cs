using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.TransactionMonitor
{
    [Serializable]
    public class TMSNewMasterDTO
    {
        [Column("id")]
        public int id { get; set; }
        [Required(ErrorMessage = "Customer Id required")]
        public string CustomerId { get; set; }

        [Column("TranRefno")]
        public string TranRefno { get; set; }
        [Column("TranDate")]
        public DateTime TranDate { get; set; }
        [Column("TranType")]
        public string TranType { get; set; }
        [Column("RemitterId")]
        public string RemitterId { get; set; }
        [Column("RemitterName")]
        public string RemitterName { get; set; }
        [Column("RemitterType")]
        public string RemitterType { get; set; }
        [Column("RemitterNationCode")]
        public string RemitterNationCode { get; set; }
        [Column("RemitterCountryCode")]
        public string RemitterCountryCode { get; set; }
        [Column("Currency")]
        public string Currency { get; set; }
        [Column("Amount")]
        public double Amount { get; set; }
        [Column("RemittancePurpose")]
        public string RemittancePurpose { get; set; }
        [Column("BeneficiaryId")]
        public string BeneficiaryId { get; set; }
        [Column("BeneficiaryName")]
        public string BeneficiaryName { get; set; }
        [Column("BeneficiaryType")]
        public string BeneficiaryType { get; set; }
        [Column("BeneficiaryNationCode")]
        public string BeneficiaryNationCode { get; set; }
        [Column("BeneficiaryCountryCode")]
        public string BeneficiaryCountryCode { get; set; }
        [Column("BeneficiaryCurrencyCode")]
        public string BeneficiaryCurrencyCode { get; set; }
        [Column("BeneficiaryBankname")]
        public string BeneficiaryBankname { get; set; }
        [Column("BeneficiaryAccountNo")]
        public string BeneficiaryAccountNo { get; set; }
        [Column("FCYAmount")]
        public double FCYAmount { get; set; }
        [Column("IPAddress")]
        public string IPAddress { get; set; }
        [Column("KioskID")]
        public string KioskID { get; set; }
        [Column("RemitterCountry")]
        public string RemitterCountry { get; set; }
        [Column("BeneficiaryCountry")]
        public string BeneficiaryCountry { get; set; }
        [Column("IsHighRiskCountry")]
        public string IsHighRiskCountry { get; set; }     
        [Column("IsHighRiskCustomer")]
        public string IsHighRiskCustomer { get; set; }
        [Column("BeneficiaryBankCOI")]
        public string BeneficiaryBankCOI { get; set; }
        [Column("comments")]
        public string comments { get; set; }

        [Column("assignedto")]
        public int? assignedto { get; set; }
        [Column("status")]
        public int status { get; set; }
        [Column("batchno")]
        public int batchno { get; set; }
        [Column("tmsstatus")]
        public int tmsstatus { get; set; }

        [Column("createdby")]
        public int? createdby { get; set; }
        [Column("createdon")]
        public DateTime createdon { get; set; }
        [Column("updatedby")]
        public int updatedby { get; set; }
        [Column("updatedon")]
        public DateTime updatedon { get; set; }
        [Column("sendmail")]
        public int sendmail { get; set; }
        [Column("DebitAmount")]
        public float DebitAmount { get; set; }
        [Column("CreditAmount")]
        public float CreditAmount { get; set; }

        [Required(ErrorMessage = "User Id required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Company Name required")]
        public string CompanyName { get; set; }

        public int ClientId { get; set; }

    }
}
