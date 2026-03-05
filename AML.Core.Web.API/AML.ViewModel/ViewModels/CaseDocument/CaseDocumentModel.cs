using AML.ViewModel.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.CaseDocument
{
    public class CaseDocumentModel
    {
        public int Id { get; set; }
        public string CaseId { get; set; }
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
        public int ClientId { get; set; }
        public DateTime? IssuedDateOnDB { get; set; }

        public DateTime? ExpiryDateOnDB { get; set; }

    }

    public class DocumentCategoryModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class DocumentTypeModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
