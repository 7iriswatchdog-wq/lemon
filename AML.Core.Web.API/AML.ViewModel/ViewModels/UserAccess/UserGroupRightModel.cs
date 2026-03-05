using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.ViewModel.ViewModels.UserAccess
{
    public class UserGroupRightModel
    {
        public int UserGroupId { get; set; }
        public int ModuleId { get; set; }
        public int FunctionalityId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
