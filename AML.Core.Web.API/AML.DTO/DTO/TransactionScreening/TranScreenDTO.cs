using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace AML.DTO.DTO.TransactionScreening
{
    public class TranScreenDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Transaction Reference No required")]
        [Column("tranrefno")]
        public string TranRefNo { get; set; }
        [Required(ErrorMessage = "Customer Reference No required")]
        [Column("custrefno")]
        public string CustRefNo { get; set; }
        [Column("custtype")]
        public string CustType { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("country")]
        public string Country { get; set; }
        [Column("dob")]
        public DateTime DOB { get; set; }
        public string CustDOB
        {

            get
            {
                return DOB.ToUIDDateFormat();
            }
            set
            {
                DOB = value.ParseDB().GetValueOrDefault();
            }
        }

        [Column("threshold")]
        public int Threshold { get; set; }
        
        [Column("ismatched")]
        public int IsMatched { get; set; }
        [Column("matchname")]
        public string MatchName { get; set; }
        [Column("matchscore")]
        public int MatchScore { get; set; }
        [Column("match_category")]
        public string MatchCategory { get; set; }
        [Column("match_type")]
        public string MatchType { get; set; }
        [Column("source")]
        public string Source { get; set; }
        [Column("source_unique_id")]
        public string SourceUniqueId { get; set; }
        [Column("comments")]
        public string Comments { get; set; }
        [Column("assignedto")]
        public int Assignedto { get; set; }
        
        [Column("status")]
        public int Status { get; set; }
        [Column("createdby")]
        public int CreatedBy { get; set; }
        [Column("createdon")]
        public DateTime? CreatedOnDB { get; set; }

        public string CreatedOn
        {
            get
            {
                return CreatedOnDB.ToUIDDateFormat();
            }
            set
            {
                CreatedOnDB = value.ParseDB();
            }
        }
        [Column("updatedby")]
        public int UpdatedBy { get; set; }
        [Column("updatedon")]
        public DateTime? UpdatedOnDB { get; set; }

        public string UpdatedOn
        {
            get
            {
                return UpdatedOnDB.ToUIDDateFormat();
            }
            set
            {
                UpdatedOnDB = value.ParseDB();
            }
        }

        [Column("iswhitelisted")]
        public string IsWhiteListed { get; set; }

        public List<ApiResModel> ApiRes { get; set; }
        public List<ApiResModel> ApiResjson { get; set; }

        public int sendMail { get; set; }
        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("user_name")]
        public string UserId { get; set; }
        [Column("Client_Name")]
        public string CompanyName { get; set; }




    }
    public class ApiResModel
    {
        public string matchname { get; set; }
        public string matchscore { get; set; }
        public string matchuid { get; set; }
        public string matchcategory { get; set; }
        public string matchtype { get; set; }
        public string nationality { get; set; }
        public string matchidnumber { get; set; }
        public string matchdob { get; set; }
        public string remarks { get; set; }
      }

    public class  TranstatusCheckDTO
    {
        [Column("tranrefno")]
        public string tranrefno { get; set; }

        [Column("status")]
        public string  status { get; set; }
    }

    public class IsWhiteListedCheckDTO
    {
        [Column("tranrefno")]
        public string tranrefno { get; set; }

        [Column("iswhitelisted")]
        public string IsWhiteListed { get; set; }
    }
}
  
