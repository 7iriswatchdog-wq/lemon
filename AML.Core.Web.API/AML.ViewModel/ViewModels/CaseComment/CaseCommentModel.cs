using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.CaseComment
{
    public class CaseCommentModel
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string Comment { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedUser { get; set; }
        public string Duration { get; set; }

        public string CommentType { get; set; }
    }
    public class CaseCloseModel
    {
        public int CaseId { get; set; }
        public string Comment { get; set; }
        public int Action { get; set; }

        public int NoMatch { get; set; }

        public int TrueDomesticpep { get; set; }

    public int TrueForeignpep { get;set;}

        public int TrueAdverseMedia { get; set; }

        public int PartialDomesticpep { get; set; }

        public int PartialForeignpep { get; set; }

        public int Partialadversemedia { get; set; }

        public int TrueUAEUNSanction { get; set; }
        public int TrueOtherSanction { get; set; }

        public string CaseChangeStatus { get; set; }

    }

}
