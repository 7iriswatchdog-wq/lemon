using Aml.Screening.MongoModal.Entity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aml.Screening.MongoModal.Modal
{
    [BsonIgnoreExtraElements]
    public class CASELOG : IEntity
    {
        public string CASEID { get; set; }
        public List<CASELOGMATCH> MATCHRECORDS { get; set; }
        public List<WEBLOGMATCH> WEBRECORDS { get; set; }
        public ObjectId _id { get; set; }
    }
    
    [BsonIgnoreExtraElements]
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

    [BsonIgnoreExtraElements]
    public class CASELOGMATCH
    {
        public string MATCHUID { get; set; }
        public string MATCHTYPE { get; set; }
        public string MATCHCATEGORY { get; set; }
        public string MATCHNAME { get; set; }
        public double MATCHSCORE { get; set; }
        public string MATCHNATIONALITY { get; set; }
        public string MATCHIDNO { get; set; }
        public string MATCHDOB { get; set; }
        public string REMARKS { get; set; }
        public string MATCHDATASETS { get; set; }

        public string MATCHRESOURCESID { get; set; }

        

        public string MATCHGENDER { get; set; }


        public List<string> SEARCHTYPES { get; set; }

        public string STATUS { get; set; }
        
        public string FLAGTYPE { get; set; }
    }
}
