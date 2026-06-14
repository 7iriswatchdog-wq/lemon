using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.ViewModel.ViewModels.UserAccess
{
    public class ModuleModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
