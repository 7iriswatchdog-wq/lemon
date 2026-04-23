using System;

namespace AML.DTO.DTO.Notepad
{
    public class Notepadv2DTO
    {
        public int id { get; set; }
        public DateTime? date_of_receipt { get; set; }
        public string cust_type { get; set; }
        public string wds_cust_id { get; set; }
        public string customer_name { get; set; }
        public string Comment_kyc { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public DateTime? created_on { get; set; }
        public DateTime? updated_on { get; set; }
        public int? client_id { get; set; }
        public string case_type { get; set; }
        public string unit_ref_no { get; set; }
        public string cust_type_realestate { get; set; }
        public string kyc_check { get; set; }
        
        // Helper properties for display
        public string CreatedByName { get; set; }
        public string UpdatedByName { get; set; }
    }
}
