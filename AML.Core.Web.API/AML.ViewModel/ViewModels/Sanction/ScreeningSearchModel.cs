
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.Sanction
{
    public class ScreeningSearchModel
    {
        public string Name { get; set; }
        public SelectList Nationalities { get; set; }
        public string Nationality { get; set; }
        public string DOB { get; set; }
        public string SelectionProperty { get; set; }
        public string[] SelectionProperties { get; set; }

        public List<ApiResultModel> DataList = new List<ApiResultModel>();
    }

    public class ApiResultModel
    {
        public string matchname { get; set; }
        public string matchscore { get; set; }
        public string matchuid { get; set; }
        public string matchcategory { get; set; }
        public string matchtype { get; set; }
        public string nationality { get; set; }
        public string matchidnumber { get; set; }
        public string matchdob { get; set; }
    }
}

