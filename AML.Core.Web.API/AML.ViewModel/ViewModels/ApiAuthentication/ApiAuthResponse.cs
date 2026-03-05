using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.ApiAuthentication
{
    public class ApiAuthResponse
    {
        public bool IsAuth { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }

        public ApiAuthResponse(bool isAuth, string userName, string jwtoken)
        {
            IsAuth = isAuth;
            Username = userName;
            Token = jwtoken;
        }
    }
}
