using System.Threading.Tasks;

using CrudeServer.Attributes;
using CrudeServer.Demo.Commands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

namespace CrudeServer
{
    [Command("/not-found")]
    public class NotFoundCommand : BaseCommand
    {
        public NotFoundCommand(ICommandContext requestContext) : base(requestContext)
        {
        }

        protected override async Task<IHttpResponse> Process()
        {
            this.AddGenericItemData();

            IHttpResponse viewResponse = await View("not-found.html");

            return viewResponse;
        }
    }
}
