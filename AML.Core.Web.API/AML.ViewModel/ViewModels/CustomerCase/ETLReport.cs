using AML.Core.DataContract.Enum;
using AML.ViewModel.ViewModels.EtlBatch;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.CustomerCase
{
    public class ETLReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Customer { get; set; }
        public MatchingType MatchType { get; set; }
        public SelectList MatchingTypeList { get; set; }
        public List<EtlBatchModel> DataList { get; set; }
        public List<CaseModel> CustomerData { get; set; }
        public int ClientId { get; set; }
        public SelectList Employees { set; get; }
    }
    public class ETLReportDownloadModel
    {
        public string UploadedBy { get; set; }
        public int TotalRows { get; set; }
        public int Matched { get; set; }
        public int UnMatched { get; set; }
        public List<CaseModel> Data { get; set; }
    }
}
