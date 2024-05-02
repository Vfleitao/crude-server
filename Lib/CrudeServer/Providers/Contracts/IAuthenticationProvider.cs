using System.Security.Principal;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Providers.Contracts
{
    public interface IAuthenticationProvider
    {
        Task<string> GenerateToken(IPrincipal principal);
        Task<HttpCookie> GenerateTokenCookie(IPrincipal principal);
        Task<IPrincipal> GetUserFromCookies(ICommandContext requestContext);
        Task<IPrincipal> GetUserFromHeaders(ICommandContext requestContext);
    }
}