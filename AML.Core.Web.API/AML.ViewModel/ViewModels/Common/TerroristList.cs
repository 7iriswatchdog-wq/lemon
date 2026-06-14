using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.Common
{
    public class TerroristList
    {
        /*
         "DOB": "01-01-1965",
        "idNumber": "000000",
        "customerId": "UAE-IECIND01",
        "fullName": "KHALIFA MOHD T AL SUBAEY ",
        "Type": "INDIVIDUAL",
        "Source": "UAE IEC LIST",
        "Nationality": "QATAR",
        "id": "639c189e88f85f416d15cb33"
         * */
        public string CustomerID { get; set; }
        public string FullName { get; set; }
        public string Nationality { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public string DOB { get; set; }
        public string id { get; set; }

        public string idNumber { get; set; }
    }
}
