using System.Threading.Tasks;

namespace CrudeServer.Providers.Contracts
{
    public interface ITemplatedViewProvider
    {
        public Task<string> GetTemplate(string templatePath, object data);
    }
}
