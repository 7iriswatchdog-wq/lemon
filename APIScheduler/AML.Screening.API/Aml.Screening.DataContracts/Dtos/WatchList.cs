using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Aml.Screening.DataContracts.Dtos
{
    public class WatchList
    {
        [Required(ErrorMessage = "FullName is required")]
        public string FULLNAME { get; set; }
        public string DOB { get; set; }
        public string NATIONALITY { get; set; }
        [Required(ErrorMessage = "List Type is required")]
        public string TYPE { get; set; }
        public string IDNUMBER { get; set; }
        public string UID { get; set; }
        public string CATEGORY { get; set; }
        public int CLIENTID { get; set; }

        public string REMARKS {get;set;}
    }
    public class WSearchDto
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Type { get; set; }

        public string UID { get; set; }

        public int client_id { get; set; }
    }
    public class WSearchResult
    {        
        public string FULLNAME { get; set; }
        public string NATIONALITY { get; set; }
        public string CATEGORY { get; set; }
        public string DOB { get; set; }
        
        public string TYPE { get; set; }
        public string IDNUMBER { get; set; }
        public string UID { get; set; }
        public string CREATEDON { get; set; }

        public string REMARKS { get; set; }



    }

    public class WatchlistRequestDto
    {
        [Required(ErrorMessage = "UID  Required")]
        public string UID { get; set; }

        public int client_id { get; set; }


    }

    public class WatchListModel
    {
        
        public string FULLNAME { get; set; }
        public string DOB { get; set; }
        public string NATIONALITY { get; set; }
        
        public string TYPE { get; set; }
        public string IDNUMBER { get; set; }
        [Required(ErrorMessage = "UID is required")]
        public string UID { get; set; }
        public string CATEGORY { get; set; }
        public int CLIENTID { get; set; }

        public string REMARKS { get; set; }

        public string UPDATEON { get; set; }
    }
}
