using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.CustomerScreening
{
    public class CustomerScreeningRQ
    {
        public string CASEID { get; set; }
        public string CUSTOMERCODE { get; set; }
        public string CUSTOMERCATEGORY { get; set; }
        public string CUSTOMERFULLNAME { get; set; }
        public string CUSTOMERIDTYPE { get; set; }
        public string CUSTOMERIDNUMBER { get; set; }
        public string CUSTOMERTYPE { get; set; }
        public string CUSTOMERCOUNTRY { get; set; }
        public string CUSTOMERNATIONALITY { get; set; }
        public string CUSTOMERDOB { get; set; }
        public string CUSTOMERMOBILENUMBER { get; set; }
        public string CREATEDON { get; set; }
        public string UPDATEDON { get; set; }

        public string WHITELISTINGDATE { get; set; }

        public string WHITELISTING { get; set; }

    }
    public class C6ScreeningRQ
    {
        public string Username { get; set; }
        public string CompanyID { get; set; }
        public string name { get; set; }
        public int threshold { get; set; } = 70;
        public string gender { get; set; }
        public List<string> countries { get; set; }
        public List<string> datasets { get; set; }
        public string dob { get; set; }
        public string dobMatching { get; set; }

        public string idtype { get; set; }

        public string idnumber { get; set; }

        public List<string> placeofbirth { get; set; }
    }

    public class WebScreeningRQ
    {
        
        public string searchName { get; set; }
        public int threshold { get; set; } = 70;
        
    }

    public class CustomerScreeningRS
    {
        public string caseId { get; set; }
        public int riskScore { get; set; }
        public string riskStatus { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public MatchedRecord data { get; set; }
    }

    public class MatchedRecord
    {
        public string matchuid { get; set; }
        public string matchtype { get; set; }
        public string matchcategory { get; set; }
        public string matchname { get; set; }
        public string matchscore { get; set; }
        public string matchnationality { get; set; }
        public string matchidno { get; set; }
        public string matchdob { get; set; }

        public string Status { get; set; }
    }



    public class Title
    {
        public string description { get; set; }
    }

    public class Nationality
    {
        public string nationality { get; set; }
    }

    public class Country
    {
        public string name { get; set; }
    }

    public class Address
    {
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string address4 { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string postcode { get; set; }
        public Country country { get; set; }
    }

    public class Alias
    {
        public object title { get; set; }
        public object alternativeTitle { get; set; }
        public string forename { get; set; }
        public string middlename { get; set; }
        public string surname { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
    }

    public class Snippet
    {
        public string title { get; set; }
        public string summary { get; set; }
        public List<string> keywordsMatched { get; set; }
    }

    public class Article
    {
        public string originalURL { get; set; }
        public string dateCollected { get; set; }
        public string c6URL { get; set; }
        public List<Category> categories { get; set; }
        public Snippet snippet { get; set; }
    }

    public class Note
    {
        public object dataSource { get; set; }
        public string text { get; set; }
    }

    public class LinkedBusiness
    {
        public int businessId { get; set; }
        public string businessName { get; set; }
        public string position { get; set; }
    }

    public class LinkedPerson
    {
        public int personId { get; set; }
        public string name { get; set; }
        public string association { get; set; }
    }

    public class Country2
    {
        public string name { get; set; }
    }

    public class PoliticalPosition
    {
        public string description { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public Country2 country { get; set; }
    }

    public class Match
    {
        public string qrCode { get; set; }
        public string resourceId { get; set; }
        public string resourceUri { get; set; }
        public int score { get; set; }
        public string match { get; set; }
        public string name { get; set; }
        public List<string> countries { get; set; }
        public List<string> datasets { get; set; }
        public List<string> datesOfBirth { get; set; }
        public string gender { get; set; }
        public ulong version { get; set; }

    }

    public class WebSearchRS
    {
        public string searchName { get; set; }
        public int threshold { get; set; }
        public int total { get; set; }
        public List<WebSearchResult> results { get; set; }
    }

    public class WebSearchResult
    {
        public int matchScore { get; set; }
        public string matchPercent { get; set; }
        public List<string> matchedNameTerms { get; set; }
        public List<string> crimeKeywordsFound { get; set; }
        public string title { get; set; }
        public string shortDescription { get; set; }
        public string URL { get; set; }
        public string engine { get; set; }
    }



    public class MatchResults
    {
        public int matchCount { get; set; }
        public List<Match> matches { get; set; }
    }

    public class Users
    {
        public int matchCount { get; set; }
        public MatchResults results { get; set; }
    }

    public class C6ScreeningRS
    {
        public Users users { get; set; }
        public int status { get; set; }
    }
}
