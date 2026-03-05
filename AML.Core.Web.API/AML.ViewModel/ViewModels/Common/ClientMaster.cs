using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AML.ViewModel.ViewModels.Common
{
    public class ClientMaster
    {
        public int ClientId { get; set; }
        [Required(ErrorMessage = "Client Name is required.")]
        public string ClientName { get; set; }
        [Required(ErrorMessage = "Prefix is required.")]
        public string Prefix { get; set; }
        [Required(ErrorMessage = "C6 UserName is required.")]
        public string C6Username { get; set; }
        [Required(ErrorMessage = "C6Threshold is required.")]
        public int C6Threshold { get; set; }
        [Required(ErrorMessage = "Threshold is required.")]
        public int Threshold { get; set; }
        public int isActive { get; set; }
        public int CreatedBy { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }
        public string E_From { get; set; }
        public string E_Alias { get; set; }
        public string E_User { get; set; }
        public string E_Pwd { get; set; }
        public string E_Host { get; set; }
        public string E_Port { get; set; }
        [Required(ErrorMessage = "Compliance Officer Email is required.")]
        public string Complem { get; set; }
        [Required(ErrorMessage = "C6BaseUrl is required.")]
        public string C6BaseUrl { get; set; }
        public IFormFile Document { get; set; }
        public string DocumentFullPath { get; set; }
        public string DocumentFileName { get; set; }
        public string DocumentDetails { get; set; }

        
        public string DocumentName { get; set; }
        public int type { get; set; }

    }
    public class ClientRightsModel
    {
        public int Menu_Id { get; set; }
    }
    public class MenuModel
    {
        public int Menu_Id { get; set; }
        public string Menu_Name { get; set; }
        public int is_active { get; set; }
        public List<ClientRightsModel> ClientMenus { get; set; }
        public List<MenuModel> MenuModels { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public bool isChecked { get; set; }
    }
    public class ClientMenuRights
    {
        public int[] MenuIds { get; set; }
        public int ClientId { get; set; }
    }
    public class ClientMenuRightsModel
    {
        public int Client_Id { get; set; }
        public int Menu_Id { get; set; }
        public int Created_By { get; set; }
        public int is_Active { get; set; }
    }
}
