using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.CaseDetail
{
    public class UsersModel
    {
        [JsonProperty("users")]
        public User Users { get; set; }
        public int Status { get; set; }

        public int caseid { get; set; }

        public string Type { get; set; }

        public string Resourcesid { get; set; }

        public string Category { get; set; }
        public string MatchUid { get; set; }

        public string CreationDate { get; set; }

    }
    public class User
    {
        public int Id { get; set; }
        public Title Title { get; set; }
        public object AlternativeTitle { get; set; }
        [JsonProperty("firstName")]
        public string Forename { get; set; }
        [JsonProperty("middleName")]
        public string Middlename { get; set; }
        [JsonProperty("lastName")]
        public string Surname { get; set; }
        public object DateOfBirth { get; set; }
        public object YearOfBirth { get; set; }
        public object DateOfDeath { get; set; }
        public object YearOfDeath { get; set; }
        public bool IsDeceased { get; set; }
        public string Gender { get; set; }
        public Nationality Nationality { get; set; }
        public string ImageURL { get; set; }
        public string TelephoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public object PepLevel { get; set; }
        public bool IsPEP { get; set; }
        public bool IsSanctionsCurrent { get; set; }
        public bool IsSanctionsPrevious { get; set; }
        public bool IsLawEnforcement { get; set; }
        public bool IsFinancialregulator { get; set; }
        public bool IsDisqualifiedDirector { get; set; }
        public bool IsInsolvent { get; set; }
        public bool IsAdverseMedia { get; set; }
        public List<Address> Addresses { get; set; }
        public List<Alias> Aliases { get; set; }
        public List<Article> Articles { get; set; }
        public List<Sanction> Sanctions { get; set; }
        public List<Note> Notes { get; set; }
        public List<LinkedBusiness> LinkedBusinesses { get; set; }
        public List<LinkedPerson> LinkedPersons { get; set; }
        public List<PoliticalPosition> PoliticalPositions { get; set; }
        public List<RelEntry> RelEntries { get; set; }
        [JsonProperty("rreEntries")]
        public List<RelEntry> RreEntries { get; set; }
        [JsonProperty("poiEntries")]
        public List<RelEntry> PoiEntries { get; set; }
        [JsonProperty("insEntries")]
        public List<RelEntry> InsEntries { get; set; }
        [JsonProperty("griEntries")]
        public List<RelEntry> GriEntries { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("businessTypes")]
        public List<string> BusinessTypes { get; set; }
        [JsonProperty("identifiers")]
        public List<Identifier> Identifiers { get; set; }
        public List<string> Datasets { get; set; }
    }


    public class RelEntry
    {
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public List<Event> Events { get; set; }
    }

    public class Event
    {
        public string Type { get; set; }
        public Period Period { get; set; }
        public List<string> EvidenceIds { get; set; }
        public string DateIso { get; set; }
    }

    public class Period
    {
        public int Years { get; set; }
    }




    public class Title
    {
        public string Description { get; set; }
    }



    public class Nationality
    {
        [JsonProperty("nationality")]
        public string Name { get; set; }
        public string NationalityText { get { return Name; } set { Name = value; } }
    }



    public class Country
    {
        public string Name { get; set; }
    }



    public class Address
    {
        [JsonProperty("addressType")]
        public string AddressType { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public Country Country { get; set; }
    }



    public class Alias
    {
        [JsonProperty("type")]
        public object Title { get; set; }
        public object AlternativeTitle { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("middleName")]
        public string MiddleName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
    }








    public class Category
    {
        public string Name { get; set; }
    }



    public class Snippet
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public List<string> KeywordsMatched { get; set; }
    }



    public class Article
    {
        public string OriginalURL { get; set; }
        public string DateCollected { get; set; }
        [JsonProperty("publicationDateIso")]
        public string DatePublished { get; set; }
        [JsonProperty("credibility")]
        public string CredibilityScore { get; set; }
        public string C6URL { get; set; }
        public List<Category> Categories { get; set; }
        public Snippet Snippet { get; set; }
        public List<string> Datasets { get; set; }
        public string EvidenceId { get; set; }
    }
    public class SanctionType
    {
        public string Description { get; set; }
    }



    public class Sanction
    {
        public SanctionType SanctionType { get; set; }
        public bool IsCurrent { get; set; }
    }



    public class Note
    {
        public DataSource DataSource { get; set; }
        [JsonProperty("value")]
        public string Text { get; set; }
    }



    public class LinkedBusiness
    {
        [JsonProperty("qrCode")]
        public int BusinessId { get; set; }
        [JsonProperty("name")]
        public string BusinessName { get; set; }
        public string Position { get; set; }
        [JsonProperty("relationship")]
        public string Association { get; set; }
    }



    public class LinkedPerson
    {
        [JsonProperty("qrCode")]
        public int PersonId { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("middleName")]
        public string MiddleName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        public string Name => ((FirstName ?? "") + " " + (MiddleName ?? "") + " " + (LastName ?? "")).Trim();
        public string Position { get; set; }
        [JsonProperty("relationship")]
        public string Association { get; set; }
    }

    public class Identifier
    {
        public string Category { get; set; }
        public string Value { get; set; }
    }




    public class PoliticalPosition
    {
        public string Description { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public Country Country { get; set; }
    }




    public class DataSource
    {
        public string Name { get; set; }
    }


}
