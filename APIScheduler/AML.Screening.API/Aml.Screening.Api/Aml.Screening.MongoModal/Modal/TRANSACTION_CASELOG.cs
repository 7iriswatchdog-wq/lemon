using Aml.Screening.MongoModal.Entity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aml.Screening.MongoModal.Modal
{
    [BsonIgnoreExtraElements]
  

    public class TRANSACTION_CASELOG : IEntity
    {
        public string CASEID { get; set; }
        public string TRANREFNO { get; set; }
        public List<TRANCASELOGMATCH> MATCHRECORDS { get; set; }
        public ObjectId _id { get; set; }
    }
    public class TRANCASELOGMATCH
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
    }
}
