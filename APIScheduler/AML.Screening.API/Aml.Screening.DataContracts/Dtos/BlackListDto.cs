using System;
using System.Collections.Generic;
using System.Text;


namespace Aml.Screening.DataContracts.Dtos
{
    public class BlackListDto
    {
        public string MATCHNAME { get; set; }
        public double MATCHSCORE { get; set; }
        public string MATCHUID { get; set; }
        public string MATCHCATEGORY { get; set; }
        public string MATCHTYPE { get; set; }
        public string NATIONALITY { get; set; }
        public string MATCHIDNUMBER { get; set; }
        public string MATCHDOB { get; set; }
        public List<AKALISTDTO> AKANAMES { get; set; }
        public int CLIENTID { get; set; }
    }

    public class AKALISTDTO
    {
        public string AKAUID { get; set; }
        public string AKATYPE { get; set; }
        public string AKACATEGORY { get; set; }
        public string AKAFIRST_NAME { get; set; }
        public string AKALAST_NAME { get; set; }
    }
}
