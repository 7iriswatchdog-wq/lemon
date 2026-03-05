using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.ViewModel.ViewModels.UserAccess
{
    public class FunctionalityModel
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
