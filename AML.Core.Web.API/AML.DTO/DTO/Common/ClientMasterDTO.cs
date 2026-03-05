using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Common
{
    public class ClientMasterDTO
    {
        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("Client_Name")]
        public string ClientName { get; set; }
        [Column("Prefix")]
        public string Prefix { get; set; }
        [Column("C6Username")]
        public string C6Username { get; set; }
        [Column("C6Threshold")]
        public int C6Threshold { get; set; }
        [Column("Threshold")]
        public int Threshold { get; set; }
        [Column("is_active")]
        public int isActive { get; set; }
        [Column("Created_By")]
        public int CreatedBy { get; set; }
        [Column("Description")]
        public string Description { get; set; }
        [Column("catogory_value")]
        public string Complem { get; set; }
        [Column("C6BaseUrl")]
        public string C6BaseUrl { get; set; }
        public IFormFile Document { get; set; }
        public string DocumentFullPath { get; set; }
        [Column("document_file_name")]
        public string DocumentFileName { get; set; }
        public string DocumentDetails { get; set; }
        public string DocumentName { get; set; }
        public int type { get; set; }
    }
    public class ClientRightsDTO
    {
        [Column("Menu_Id")]
        public int Menu_Id { get; set; }
    }
    public class MenuModelDTO
    {
        [Column("Menu_Id")]
        public int Menu_Id { get; set; }
        [Column("Menu_Name")]
        public string Menu_Name { get; set; }
        [Column("is_active")]
        public int is_active { get; set; }
    }
    public class ClientMenuRightsModelDTO
    {
        [Column("Client_Id")]
        public int Client_Id { get; set; }
        [Column("Menu_Id")]
        public int Menu_Id { get; set; }
        [Column("Created_By")]
        public int Created_By { get; set; }
        [Column("is_Active")]
        public int is_Active { get; set; }

        [Column("Menu_Name")]
        public string Menu_Name { get; set; }
    }
}
