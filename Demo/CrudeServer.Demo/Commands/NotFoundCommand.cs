using System.Threading.Tasks;

using CrudeServer.Demo.Commands;
using CrudeServer.HttpCommands.Contract;

namespace CrudeServer
{
    public class NotFoundCommand : BaseCommand
    {

        protected override async Task<IHttpResponse> Process()
        {
            this.AddGenericItemData();

            IHttpResponse viewResponse = await View("not-found.html");

            return viewResponse;
        }
    }
}
