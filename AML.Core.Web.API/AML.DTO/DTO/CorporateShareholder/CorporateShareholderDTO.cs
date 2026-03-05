using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.CorporateShareholder
{
    public class CorporateShareholderDTO
    {
        [Column("corporate_id")]
        public int CorporateID { get; set; }
        [Column("share_holder_id")]
        public int ShareholderID { get; set; }
        [Column("share_percentage")]
        public string SharePercentage { get; set; }
        [Column("cust_id_gen")]
        public string CustIDGen { get; set; }
        [Column("status")]
        public int Status { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("created_on")]
        public DateTime CreatedOn { get; set; }

        public string CustomerRefId { get; set; }
    }
}
