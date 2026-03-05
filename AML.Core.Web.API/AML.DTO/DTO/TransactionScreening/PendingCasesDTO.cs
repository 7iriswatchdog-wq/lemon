using AML.Core.Common.StaticResource;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.TransactionScreening
{
    public class PendingCasesDTO
    {
      
        [Column("id")]
        public int id { get; set; }
        [Column("tranrefno")]
        public string tranrefno { get; set; }
        [Column("custrefno")]
        public string custrefno { get; set; }
        [Column("name")]
        public string name { get; set; }
        [Column("matchscore")]
        public int matchscore { get; set; }
        [Column("createdon")]
      
        public DateTime? CreatedOnDB { get; set; }
        public string CreatedOn
        {
            get
            {
                return CreatedOnDB.ToUIDDateFormat();
            }
            set
            {
                CreatedOnDB = value.ParseDB();
            }
        }


    }
    public class PendingCasesRequestDTO
    {
      
        [Column("tranrefno")]
        public string tranrefno { get; set; }
       
    }
}
