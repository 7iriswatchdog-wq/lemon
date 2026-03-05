using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.LovMasterModel
{
   public class LovMasterModel
    {       
        public int Id { get; set; }
        public string LovRiskCatogory { get; set; }       
        public string LovTypeId { get; set; }      
        public string LovTypeName { get; set; }       
        public string LovRiskData { get; set; }       
        public string LovRiskScore { get; set; }
        public string OverrideScore { get; set; }
        public string IsActive { get; set; }
        public int ClientId { get; set; }

        public int percentage { get; set; }

    }
}
