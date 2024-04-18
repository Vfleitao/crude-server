using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Providers.Contracts
{
    public interface IRequestDataParser
    {
        Task<HttpRequestData> GetData(ICommandContext request);
    }
}
