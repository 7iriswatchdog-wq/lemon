using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.DataContract.Authentication
{
    public class Login
    {
        /// <summary>
        /// User name 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
    }
}
