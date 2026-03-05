using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.Parameter
{
    public class ParameterTableModel
    {

        
        public string paraType { get; set; }
        public string paraTypeDesc { get; set; }
        public string paraCode { get; set; }
        public string paraCodeDesc { get; set; }
        public string paraTypeDescL2 { get; set; }
        public string paraCodeDescL2 { get; set; }
        public string paraValue { get; set; }
        public string paraValueL2 { get; set; }
        public string paraNotes { get; set; }
        public string paraNotesL2 { get; set; }
        public int paraActiveYn { get; set; }
        public int paraCrBy { get; set; }
        public int paraCrDt { get; set; }
        public int paraUpBy { get; set; }
        public string paraUpDt { get; set; }

    }
}
