using Aml.Screening.MongoModal.Entity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System;



namespace Aml.Screening.MongoModal.Modal
{
    [BsonIgnoreExtraElements]
    public class NAMELIST : IEntity
    {
        public ObjectId _id { get; set; }
        public string P_id { get; set; }
        public string CATEGORY { get; set; }
        public string SUBCATEGORY { get; set; }
        public string UID { get; set; }
        public string FULLNAME { get; set; }
        public List<string> SOUNDEX { get; set; }
        public string FIRSTNAME { get; set; }
        public string LASTNAME { get; set; }
        public string NATIONALITY { get; set; }
        public List<IDDETAIL> IDDETAILS { get; set; }
        public List<DOBLIST> DOB { get; set; }
        public List<AKALIST> AKALIST { get; set; }
        public string CREATEDON { get; set; }
        public string UPDATEDON { get; set; }
        public string STATUS { get; set; }
        public string TYPE { get; set; }
        public string FULLNAME_NL { get; set; }
        public int CLIENTID { get; set; }

        public string REMARKS { get; set; }

         public double MATCHSCORE { get; set; }

        public class IDDETAIL
        {
            public string IDTYPE { get; set; }
            public string IDNUMBER { get; set; }
        }        
    }

    [BsonIgnoreExtraElements]
    public class BLACKLIST : IEntity
    {
        public ObjectId _id { get; set; }
        public string CATEGORY { get; set; }
        public string SUBCATEGORY { get; set; }
        public string UID { get; set; }
        public string FIRST_NAME { get; set; }
        public string MIDDLE_NAME { get; set; }
        public string LAST_NAME { get; set; }
        public string FULLNAME { get; set; }
        public string FULLNAME_AR { get; set; }
        public string FULLALIAS { get; set; }
        public string CREATEDON { get; set; }
        public string UPDATEDON { get; set; }
        public DateTime CREATEDDATE { get; set; }
        public DateTime UPDATEDDATE { get; set; }
        public string TITLE { get; set; }
        public string POSITION { get; set; }
        public string REMARKS { get; set; }
        public string TYPE { get; set; }
        public string PROGRAMLIST { get; set; }
        public List<IDLIST> IDLIST { get; set; }
        public List<AKALIST> AKALIST { get; set; }
        public List<ADDRESSLIST> ADDRESSLIST { get; set; }
        public List<NATIONALITYLIST> NATIONALITYLIST { get; set; }
        public List<CITIZENSHIPLIST> CITIZENSHIPLIST { get; set; }
        public List<DOBLIST> DOBLIST { get; set; }
        public List<PLACEOBLIST> PLACEOBLIST { get; set; }
        public List<ADDINFOLIST> ADDINFOLIST { get; set; }
        public string STATUS { get; set; }
        public string MOBILENO { get; set; }

        public string IDNUMBER { get; set; }
        public int CLIENTID { get; set; }

        //public string REMARKS { get; set; }
    }
    public class PROGRAMLIST
    {
        public string PROGRAM { get; set; }
    }
    public class IDLIST
    {
        public string IDUID { get; set; }
        public string IDTYPE { get; set; }
        public string IDNUMBER { get; set; }
        public string IDCOUNTRY { get; set; }
        public string IDISSUEDATE { get; set; }
        public string IDEXPIRYDATE { get; set; }
    }
    public class AKALIST
    {
        public string AKAUID { get; set; }
        public string AKATYPE { get; set; }
        public string AKACATEGORY { get; set; }
        public string AKAFIRST_NAME { get; set; }
        public string AKALAST_NAME { get; set; }
        public List<string>  AKASOUNDEX { get; set; }
    }
    public class ADDRESSLIST
    {
        public string ADDRESSUID { get; set; }
        public string ADDRESS1 { get; set; }
        public string ADDRESS2 { get; set; }
        public string ADDRESS3 { get; set; }
        public string ADDRESSCITY { get; set; }
        public string ADDRESSSTATE { get; set; }
        public string ADDRESSPOCODE { get; set; }
        public string ADDRESSCOUNTRY { get; set; }
    }
    public class NATIONALITYLIST
    {
        public string NATUID { get; set; }
        public string NATMAINENTRY { get; set; }
        public string NATCOUNTRY { get; set; }
    }
    public class CITIZENSHIPLIST
    {
        public string CITUID { get; set; }
        public string CITMAINENTRY { get; set; }
        public string CITCOUNTRY { get; set; }
    }
    public class DOBLIST
    {
        public string DOBUID { get; set; }
        public string DOBMAINENTRY { get; set; }
        public string DOB { get; set; }
        public string AGE { get; set; }
        public string ASOFDATE { get; set; }
        public string ISDECEASED { get; set; }
    }
    public class PLACEOBLIST
    {
        public string POBUID { get; set; }
        public string POBMAINENTRY { get; set; }
        public string POB { get; set; }
    }
    public class ADDINFOLIST
    {
        public string ADDINFO1 { get; set; }
        public string ADDINFO2 { get; set; }
        public string ADDINFO3 { get; set; }
        public string ADDINFO4 { get; set; }
        public string ADDINFO5 { get; set; }
        public string ADDINFO6 { get; set; }
        public string ADDINFO7 { get; set; }
        public string ADDINFO8 { get; set; }
    }
}
