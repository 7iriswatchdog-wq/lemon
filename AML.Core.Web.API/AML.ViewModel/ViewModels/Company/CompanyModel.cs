using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.Company
{
    public class CompanyModel
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; }
        public string LegalName { get; set; }
        public int LegalType { get; set; }
        public string LegalTypeName { get; set; }
        public string Mobile { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
