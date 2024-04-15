using System.Security.Principal;
using System.Threading.Tasks;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Providers.Contracts
{
    public interface IAuthenticationProvider
    {
        Task<IPrincipal> GetUser(IRequestContext requestContext);
    }
}