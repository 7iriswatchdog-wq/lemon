using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Parameter
{
    public class ParamDTO
    {
        [Column("Para_type")]
        public string paraType { get; set; }

        [Column("Para_type_desc")]
        public string paraTypeDesc { get; set; }

        [Column("Para_code")]
        public string paraCode { get; set; }

        [Column("Para_code_desc")]
        public string paraCodeDesc { get; set; }

        [Column("Para_type_desc_L2")]
        public string paraTypeDescL2 { get; set; }

        [Column("Para_code_desc_L2")]
        public string paraCodeDescL2 { get; set; }

        [Column("Para_value")]
        public string paraValue { get; set; }

        [Column("Para_value_L2")]
        public string paraValueL2 { get; set; }

        [Column("Para_notes")]
        public string paraNotes { get; set; }

        [Column("Para_notes_L2")]
        public string paraNotesL2 { get; set; }

        [Column("Para_active_YN")]
        public int paraActiveYn { get; set; }

        [Column("Para_CR_BY")]
        public int paraCrBy { get; set; }

        [Column("Para_CR_DT")]
        public int paraCrDt { get; set; }

        [Column("Para_UP_BY")]
        public int paraUpBy { get; set; }

        [Column("Para_UP_DT")]
        public string paraUpDt { get; set; }

    }
}
