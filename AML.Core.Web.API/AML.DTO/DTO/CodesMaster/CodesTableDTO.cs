using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.CodesMaster
{
    public class CodesTableDTO
    {

        [Column("COD_TYPE")] 
        public string ccType { get; set; }

        [Column("COD_CODE")]
        public string ccCode { get; set; }

        [Column("COD_NAME")]
        public string ccName { get; set; }

        [Column("COD_CODE_CATG")]
        public string ccCodeCatg { get; set; }

        [Column("COD_DETAILS")]
        public string ccDetails { get; set; }
        
        [Column("COD_FlEXI1")]
        public string ccFlexi1 { get; set; }

        [Column("COD_FLEXI2")]
        public string ccFlexi2 { get; set; }

        [Column("COD_FLEXI3")]
        public string ccFlexi3 { get; set; }

        [Column("COD_ACTIVE_YN")]
        public bool ccActiveYn { get; set; }

        [Column("COD_CLIENT_ID")]
        public int ccClientId { get; set; }

        //[Column("COD_CR_BY")]
        //public int CcCrBy { get; set; }

        //[Column("COD_CR_DT")]
        //public string CcCrDt { get; set; }

        //[Column("COD_UP_BY")]
        //public int CcUpBy { get; set; }

        //[Column("COD_UP_DT")]
        //public string CcUpDt { get; set; }

}

}
