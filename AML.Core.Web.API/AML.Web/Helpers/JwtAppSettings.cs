using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AML.Web.Helpers
{
    public class JwtAppSettings
    {
        public string Secret { get; set; }

        // Optional issuer / audience claims — when set, JwtMiddleware validates them.
        // Closes the JWT-validation gap from the 2026-05-02 audit (previously
        // ValidateIssuer = false / ValidateAudience = false meant any token signed
        // with the right secret was accepted regardless of source).
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
