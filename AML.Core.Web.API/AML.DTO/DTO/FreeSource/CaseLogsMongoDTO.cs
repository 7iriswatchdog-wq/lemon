using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Dynamic;

namespace AML.DTO.DTO.FreeSource
{
    public class CaseLogsMongoDTO
    {
        [BsonIgnoreExtraElements]
        public class CASELOG
        {
            public ObjectId _id { get; set; }
            public string CASEID { get; set; }
            public List<MatchRecordsDTO> MATCHRECORDS { get; set; }

            public List<WEBLOGMATCH> WEBRECORDS { get; set; }
        }
    }

    public class MatchRecordsDTO
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

        public string MATCHRESOURCESID { get; set; }

        public string MATCHDATASETS { get; set; }

        public string MATCHGENDER { get; set; }

        public List<string> SEARCHTYPES { get; set; }

        public string STATUS { get; set; }
    }

    public class WEBLOGMATCH
    {

        public int MATCHSCORE { get; set; }
    public string MATCHPRESENT { get; set; }
    public List<string> MATCHEDNAMETERMS{ get; set; }
    public List<string> CRIMEKEYWORDSFOUND { get; set; }
    public string TITLE { get; set; }
    public string SHORTDESCRIPTION { get; set; }
    public string URL { get; set; }
    public string ENGINE{ get; set; }
}
}
