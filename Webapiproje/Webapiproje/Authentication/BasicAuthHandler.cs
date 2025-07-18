using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;

namespace Webapiproje.Authentication
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                ILoggerFactory logger,
                                UrlEncoder encoder,
                                ISystemClock clock) : base(options, logger, encoder, clock)
        {

        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (authHeader != null
             && authHeader.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            {
                var base64UserNameAndPassword = authHeader.Substring("Basic ".Length).Trim();

                var credentialstring = Encoding.UTF8.GetString(Convert.FromBase64String(base64UserNameAndPassword));

                var credentials = credentialstring.Split(':');

                if (credentials[0] == "azra"
                 && credentials[1] == "azra12")
                {
                    var claims = new[]
                    { 
                        new Claim(ClaimTypes.Name, credentials[0]),  // Kullanıcı adı
                        new Claim(ClaimTypes.Role, "Admin")   // Rol bilgisi
                    };

                    var identity = new ClaimsIdentity(claims, "Basic"); //ClaimsIdentity: Bir kimliği temsil eder. Hangi "şema" (örneğin Basic) ile geldiğini de belirtirsin.

                    var claimsPrincipal = new ClaimsPrincipal(identity); //ClaimsPrincipal: O kimliği barındıran kişi. ASP.NET bu nesneyi HttpContext.User içine koyar.

                    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
                }

                Response.StatusCode = 401;  // Unauthorized
                Response.Headers.Append("WWW-Authenticate", "Basic realm=\"azra.com\"");

                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }
            else
            {
                Response.StatusCode = 401;
                Response.Headers.Append("WWW-Authenticate", "Basic realm=\"azra.com\"");

                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }
        }
    }
}