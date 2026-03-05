using Microsoft.AspNetCore.Http;
using System;

namespace AML.ViewModel.ViewModels.TransactionScreening
{
    public class TransactionCaseDocumentModel
    {

        public int Id { get; set; }

        public string TranRefNo { get; set; }
        public int CaseId { get; set; }
        public string DocumentName { get; set; }
        public int DocumentCategory { get; set; }
        public int DocumentType { get; set; }
        public string DocumentFullPath { get; set; }
        public string DocumentFileName { get; set; }
        public string DocumentDetails { get; set; }
        public string Remarks { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public IFormFile Document { get; set; }
        public DateTime? CreatedOnDB { get; set; }
        public DateTime? IssuedDateOnDB { get; set; }

        public int ClientId { get; set; }
    }

    public class TransactionDocumentCategoryModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class TransactionDocumentTypeModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
