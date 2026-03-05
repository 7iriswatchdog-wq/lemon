using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.DataContract.Authentication
{
    public class LoginUser
    {
        /// <summary>
        /// User Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Security token
        /// </summary>
        public string Token { get; set; }
    }
}
