using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.Common
{
    public class ErrorLogModel
    {
        public int id { get; set; }

        public string module { get; set; }

        public string description { get; set; }

        public string comments { get; set; }

        public int createdBy { get; set; }

        public DateTime created_on { get; set; }
    }
}
