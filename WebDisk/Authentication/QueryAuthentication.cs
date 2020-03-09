using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace WebDisk.Authentication
{
    public class QueryAuthentication : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public QueryAuthentication(
             IOptionsMonitor<AuthenticationSchemeOptions> options,
             ILoggerFactory logger,
             UrlEncoder encoder,
             ISystemClock clock)
            : base(options, logger, encoder, clock)
        {

        }
        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //var result = await this.AuthenticateAsync();

            var a =   this.Context?.User?.Identity?.IsAuthenticated;
            var claims = new Claim[]
            {
               // new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
               new Claim(ClaimTypes.Name,"张三"),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
