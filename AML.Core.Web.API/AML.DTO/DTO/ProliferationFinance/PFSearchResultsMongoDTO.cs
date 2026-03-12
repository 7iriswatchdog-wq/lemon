using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AML.DTO.DTO.ProliferationFinance
{
    public class PFSearchResultsMongoDTO
    {
        [BsonIgnoreExtraElements]
        public class PF_SEARCHRESULT
        {
            [BsonId]
            public ObjectId _id { get; set; }
            public int CaseId { get; set; }
            public string ProductName { get; set; }
            public DateTime UpdatedOn { get; set; }
            public List<PF_Hit> Hits { get; set; }
        }

        public class PF_Hit
        {
            public int? Index { get; set; }
            public string SearchType { get; set; } // Chemical, Non-Chemical
            public string Decision { get; set; } // True Match, False Match
            public string Remarks { get; set; }
            
            // For Chemical hits
            public int? ChemicalId { get; set; }
            public string MatchedName { get; set; }
            public string HsCode { get; set; }
            public string CasNumber { get; set; }
            public string Eccn { get; set; }
            
            // For Non-Chemical hits
            public string Snippet { get; set; }
            
            public DateTime? FoundOn { get; set; }
        }
    }
}
