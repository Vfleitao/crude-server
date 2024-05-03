using System.Threading.Tasks;

using CrudeServer.Models.Contracts;

namespace CrudeServer.Providers.Contracts
{
    public interface ITemplatedViewProvider
    {
        public Task<string> GetTemplate(string templatePath, object data, ICommandContext commandContext);
    }
}
