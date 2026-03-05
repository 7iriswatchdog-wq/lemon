using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AML.Web.Entities
{
    public class ApiUser
    {
        public bool IsAuth { get; set; }
        public string Username { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

    }
}
