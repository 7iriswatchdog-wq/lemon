using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace AML.DTO.DTO.FreeSource
{
    public class TransactionCaseLogsMongoDTO
    {
        [BsonIgnoreExtraElements]
        public class TRANSACTION_CASELOG
        {
            public ObjectId _id { get; set; }
            public string TRANREFNO { get; set; }
            public string CASEID { get; set; }
            public List<MatchTranRecordsDTO> MATCHRECORDS { get; set; }
        }
    }

    public class MatchTranRecordsDTO
    {
        public string MATCHUID { get; set; }
        public string MATCHTYPE { get; set; }
        public string MATCHCATEGORY { get; set; }
        public string MATCHNAME { get; set; }
        public int MATCHSCORE { get; set; }
        public string MATCHNATIONALITY { get; set; }
        public string MATCHIDNO { get; set; }
        public string MATCHDOB { get; set; }
        public string REMARKS { get; set; }
    }
}

