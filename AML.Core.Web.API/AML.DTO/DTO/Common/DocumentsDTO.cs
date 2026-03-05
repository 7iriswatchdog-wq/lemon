using System;
using System.Collections.Generic;
using System.Text;

namespace AML.DTO.DTO.Common
{
    public class DocumentsDTO
    {
        public int Id { get; set; }
        public int ItemTypeId { get; set; }
        public int ItemIdDoc { get; set; }
        public string DocName { get; set; }
        public string DocPath { get; set; }
        public string DocFullPath { get; set; }
        public string DocType { get; set; }
        public string DocPathEncrypted { get; set; }
        public int AddedBy { get; set; }
        public string AddDate { get; set; }
        public string IsDeleted { get; set; }
        public string Notes { get; set; }
        public string ItemTypeName { get; set; }
        public string DocOriginalName { get; set; }
        public int ClientId { get; set; }
    }
}
