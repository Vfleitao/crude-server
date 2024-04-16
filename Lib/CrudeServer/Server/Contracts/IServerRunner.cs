using System.Threading.Tasks;

namespace CrudeServer.Server.Contracts
{
    public interface IServerRunner
    {
        Task Run();
        Task Stop();
    }
}