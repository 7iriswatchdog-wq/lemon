using AML.Core.Common.StaticResource;
using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace AML.DTO.DTO.TransactionScreening
{
    public class TransactionCaseDocumentDTO
    {
     

            [Column("id")]
            public int Id { get; set; }
            [Column("case_id")]
            public string CaseId { get; set; }

            [Column("tranrefno")]
            public string TranRefNo { get; set; }
            [Column("document_name")]
            public string DocumentName { get; set; }
            [Column("document_category")]
            public int DocumentCategory { get; set; }
            [Column("document_type")]
            public int DocumentType { get; set; }
            [Column("document_full_path")]
            public string DocumentFullPath { get; set; }
            [Column("document_file_name")]
            public string DocumentFileName { get; set; }
            [Column("documents_details")]
            public string DocumentDetails { get; set; }
            [Column("remarks")]
            public string Remarks { get; set; }
            [Column("created_by")]
            public int CreatedBy { get; set; }
            [Column("created_on")]
            public DateTime? CreatedOnDB { get; set; }
            public DateTime? IssuedDateOnDB { get; set; }
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
            [Column("issued_date")]
            public string IssuedDate
            {
                get
                {
                    return IssuedDateOnDB.ToUIDDateFormat();
                }
                set
                {
                    IssuedDateOnDB = value.ParseDB();
                }
            }
            [Column("expiry_date")]
            public DateTime? ExpiryDateOnDB { get; set; }
            public string ExpiryDate
            {
                get
                {
                    return ExpiryDateOnDB.ToUIDDateFormat();
                }
                set
                {
                    ExpiryDateOnDB = value.ParseDB();
                }
            }
        }
        public class TransactionDocumentCategoryDTO
        {

            [Column("id")]
            public int Id { get; set; }
            [Column("code")]
            public string Code { get; set; }
            [Column("name")]
            public string Name { get; set; }
        }
        public class TransactionDocumentTypeDTO
        {

            [Column("id")]
            public int Id { get; set; }
            [Column("code")]
            public string Code { get; set; }
            [Column("name")]
            public string Name { get; set; }
        }
    }
