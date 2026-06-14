using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.LegalType
{
    public class LegalTypeModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? CreatedOnDB { get; set; }
    }
}
