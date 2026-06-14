using System;
using System.Collections.Generic;
using System.Text;

namespace AML.DTO.DTO.CorporateShareholder
{
    public class C6CorporateScreeningDTO
    {
        public Users users { get; set; }
        public string status { get; set; }
    }
    public class Users
    {
        public int recordsFound { get; set; }
        public List<Matches> matches { get; set; }
    }

    public class Matches
    {
        public int score { get; set; }
        public Business business { get; set; }
    }
    public class Business
    {
        public int id { get; set; }
        public string businessName { get; set; }
        public string telephoneNumber { get; set; }
        public string faxNumber { get; set; }
        public string website { get; set; }
        public bool isPEP { get; set; }
        public bool isSanctionsCurrent { get; set; }
        public bool isSanctionsPrevious { get; set; }
        public bool isLawEnforcement { get; set; }
        public bool isFinancialregulator { get; set; }
        public bool isDisqualifiedDirector { get; set; }
        public bool isInsolvent { get; set; }
        public bool isAdverseMedia { get; set; }
        public List<Addresses> addresses { get; set; }
        public List<Aliases> aliases { get; set; }
        public List<Articles> articles { get; set; }
        public dynamic sanctions { get; set; }
        public List<Notes> notes { get; set; }
        public List<LinkedBusinesses> linkedBusinesses { get; set; }
        public List<LinkedPersons> linkedPersons { get; set; }
    }

    public class Addresses
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
    public class Country
    {
        public string name { get; set; }
    }
    public class Aliases
    {
        public string businessName { get; set; }
    }
    public class Articles
    {
        public string originalURL { get; set; }
        public string dateCollected { get; set; }
        public string c6URL { get; set; }
        public List<Categories> categories { get; set; }
        public Snippet snippet { get; set; }
    }

    public class Snippet
    {
        public string title { get; set; }
        public string summary { get; set; }
        public dynamic keywordsMatched { get; set; }
    }
    public class Categories
    {
        public string name { get; set; }
    }
    public class Notes
    {
        public dynamic dataSource { get; set; }
        public string text { get; set; }
    }
    public class LinkedBusinesses
    {
        public int businessId { get; set; }
        public string businessName { get; set; }
        public string linkDescription { get; set; }
    }
    public class LinkedPersons
    {
        public string personId { get; set; }
        public string name { get; set; }
        public string position { get; set; }
    }

    public class C6CorporateScreeningRequestDTO
    {
        public string CompanyID { get; set; }
        public int Threshold { get; set; }
        public string BusinessName { get; set; }
        public bool PEP { get; set; }
        public bool PreviousSanctions { get; set; }
        public bool CurrentSanctions { get; set; }
        public bool LawEnforcement { get; set; }
        public bool FinancialRegulator { get; set; }
        public bool Insolvency { get; set; }
        public bool DisqualifiedDirector { get; set; }
        public bool AdverseMedia { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string Username { get; set; }
    }
}
