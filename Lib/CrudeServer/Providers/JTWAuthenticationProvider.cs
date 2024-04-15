using System.Security.Principal;
using System.Threading.Tasks;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Providers
{
    public class JTWAuthenticationProvider : IAuthenticationProvider
    {
        public Task<IPrincipal> GetUser(IRequestContext requestContext)
        {
            return Task.Run(() =>
            {
                string token = requestContext.Headers["Authorization"];
                if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer"))
                {
                    return (IPrincipal)null;
                }

                // Validate token
                // If valid, return the user
                // If invalid, return null
                return (IPrincipal)null;
            });
        }
    }
}
