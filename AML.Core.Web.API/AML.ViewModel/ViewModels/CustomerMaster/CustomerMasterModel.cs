using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.CustomerMaster
{
    public class CustomerMasterModel
    {
        public int Id { get; set; }
        public string CustomerReferenceId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public string Nationality { get; set; }
        public SelectList Nationalities { get; set; }
        public string CustomerType { get; set; }
        public SelectList CustomerCategories { get; set; }
        public string CustomerIdType { get; set; }
        public SelectList IdTypes { get; set; }
        public string CustomerIdNumber { get; set; }
        public string Mobile { get; set; }
        public int BatchId { get; set; }
        public int IsDeleted { get; set; }
        public int Status { get; set; }
        public int CreatedBy { get; set; }
        //public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        //public DateTime UpdatedOn { get; set; }
        public string CustomerFinalRiskScore { get; set; }
        public int CustomerScreenMatchScore { get; set; }
        public int RecordCount { get; set; }

        public string WhiteListed { get; set; }
        public int ClientId { get; set; }
        public string DateofAssessment { get; set; }
        public string DateofAssessments { get; set; }

        public string FullName { get; set; }
        public string UserName { get; set; }
        public bool IsDuplicate { get; set; }
    }
}
