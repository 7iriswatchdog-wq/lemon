using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.UserAccess
{
    public class UserGroupRightDetailsModel
    {
        public UserGroupRightDetailsModel()
        {
            UserGroupRightDetails = new List<UserGroupRightDetailsModel>();
        }
        public int UserGroupId { get; set; }
        public string UserGroupCode { get; set; }
        public string UserGroupName { get; set; }
        public int ModuleId { get; set; }
        public bool IsModule { get; set; }
        public bool IsFunctionality { get; set; }
        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }
        public int FunctionalityId { get; set; }
        public string FunctionalityCode { get; set; }
        public string FunctionalityName { get; set; }
        public SelectList UserGroups { get; set; }
        public SelectList Modules { get; set; }
        public SelectList Functionalities { get; set; }
        public int[] FunctionalityIds { get; set; }
        public int[] ModuleIds { get; set; }

        public List<UserGroupRightDetailsModel> UserGroupRightDetails { get; set; }
        public UserRightsSaveModel UserRightData { get; set; }
    }

    public class UserRights
    {
        public int ModuleId { get; set; }
        public List<int> FunctionalityIds { get; set; }
    }
    public class UserRightsSaveModel
    {
        public int GroupId { get; set; }
        public List<UserRights> Modules { get; set; }
    }
    public class UserGroupRights
    {
        public int[] FunctionalityIds { get; set; }
        public int UserGroupId { get; set; }
    }
    public class UserModules
    {
        [Column("module_id")]
        public int ModuleId { get; set; }
        [Column("func_id")]
        public int FuncId { get; set; }
    }
  }
