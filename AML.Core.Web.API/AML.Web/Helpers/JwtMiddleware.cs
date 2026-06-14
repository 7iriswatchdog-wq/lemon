using AML.Core.ServiceContract.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AML.Web.Helpers
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtAppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<JwtAppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, ICommonService commonService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                attachUserToContext(context, commonService, token);

            await _next(context);
        }

        private void attachUserToContext(HttpContext context, ICommonService commonService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

                // 2026-05-02 audit: validate issuer + audience whenever they're configured.
                // Backwards-compatible — if JwtAppSettings.Issuer / Audience are blank
                // (e.g., older deployments), the validation is skipped for those fields
                // only, preserving existing token compatibility while letting newer
                // deployments lock the token to a known issuer/audience.
                bool validateIssuer = !string.IsNullOrWhiteSpace(_appSettings.Issuer);
                bool validateAudience = !string.IsNullOrWhiteSpace(_appSettings.Audience);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = validateIssuer,
                    ValidIssuer = _appSettings.Issuer,
                    ValidateAudience = validateAudience,
                    ValidAudience = _appSettings.Audience,
                    ValidateLifetime = true,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                context.Items["User"] = jwtToken.Claims.First(x => x.Type == "unique_name").Value;

                // 2026-05-02 audit: surface the clientId claim so downstream tenant checks
                // can compare against the token's tenant binding (defence-in-depth — even
                // if session/clientId is wrong, the JWT carries the correct binding).
                var clientIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "clientId" || x.Type == "client_id");
                if (clientIdClaim != null)
                    context.Items["ClientId"] = clientIdClaim.Value;
            }
            catch (Exception ex)
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
