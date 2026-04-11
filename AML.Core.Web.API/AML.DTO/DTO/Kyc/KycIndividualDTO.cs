using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.Kyc
{
    public class KycIndividualDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("customer_id")]
        public string CustomerId { get; set; }
        [Column("lname")]
        public string FullName { get; set; }
        [Column("dob")] 
        public string DOB { get; set; }
        [Column("nationality")]
        public string Nationality { get; set; }
        [Column("residence_status")]
        public string ResidenceStatus { get; set; }
        [Column("occupation_type")]
        public string OccupatinTypeTxt { get; set; }
        [Column("employer_name")]
        public string EmployerName { get; set; }
        [Column("employer_address")]
        public string EmployerAddress { get; set; }
        [Column("residence_address")]
        public string ResidenceAddress { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("mobile")]
        public string Mobile { get; set; }
        [Column("cust_id_type")]
        public string CustomerIdType { get; set; }
        [Column("cust_id_number")]
        public string CustomerIdNumber { get; set; }
        [Column("threshold")]
        public int Threshold { get; set; }

        [Column("pep_status")]
        public string PEPStatus { get; set; }
        [Column("address")]
        public string Address { get; set; }
        [Column("guardian_name")]
        public string GuardianName { get; set; }
        [Column("guardian_relation")]
        public string GuardianRelation { get; set; }
        [Column("remark")]
        public string Remarks { get; set; }
        public string IsPep { get; set; }
        public string IsPeP { get; set; }

        [Column("placeofbirth")]
        public string PlaceOfBirth { get; set; }
        [Column("martialstatus")]
        public string MaritalStatus { get; set; }
       
        [Column("idexpirydate")]
        public string IdExpdate { get; set; }
       
        [Column("bankaccnumber")]
        public string BankAccountNo { get; set; }
        [Column("bankaccname")]
        public string BankAccountName { get; set; }
        [Column("bankbranch")]
        public string BankAccountBranch { get; set; }
        [Column("sourceofincome")]
        public string Sourceofincome { get; set; }
        [Column("delivery_channel")]
        public string DeliveryChannelName { get; set; }
        [Column("product_type")]
        public string ProductName { get; set; }

        [Column("mode_of_payment")]
        public string ModeOfPayment { get; set; }

        public string Domesticpep   { get; set; }

        public string ForeignPep { get; set; }

        public string RedFlags { get; set; }

        public string SanctionMatch { get; set; }

        public string UAEORUNSC { get; set; }

        public string HighestRiskProduct { get; set; }

        public string HighNetworkIndividual { get; set; }

        public string DualUseGoods { get; set; }

        public string MoreDualUseGoods { get; set; }




    }
}
