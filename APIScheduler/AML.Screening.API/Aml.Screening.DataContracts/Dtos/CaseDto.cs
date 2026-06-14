using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Aml.Screening.DataContracts.Dtos
{
    class CaseDto
    {
    }
    public class CaseRequestDto
    {
        [Required (ErrorMessage = "Case ID Required")]
        public string CASEID { get; set; }
    }

    public class TranCaseRequestDto
    {
        [Required(ErrorMessage = "Transaction Reference No Required")]
        public string TRANREFNO { get; set; }
      
    }

    public class DateRequestDto
    {
        [Required(ErrorMessage = "Created Date is Mandatory")]
        public DateTime CreatedDate { get; set; }
    }
}
