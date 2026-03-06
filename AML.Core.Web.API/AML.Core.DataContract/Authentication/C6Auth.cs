using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.DataContract.Authentication
{
    public class TokenRQ
    {
        public string username { get; set; }
    }
    public class TokenRS
    {
        public User user { get; set; }
        public int status { get; set; }

        public string message { get; set; }
    }

    public class User
    {
        public string username { get; set; }
        public string userid { get; set; }
        public string id { get; set; }
        public string token { get; set; }
        public DateTime createdDate { get; set; }

        public string userLimit { get; set; }

        public string individualCount { get; set; }

        public string corporateCount { get; set; }
    }
}
