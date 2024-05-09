using System.Threading.Tasks;

using CrudeServer.Demo.Commands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

namespace CrudeServer
{
    public class HomeCommand : BaseCommand
    {
        public HomeCommand(ICommandContext requestContext) : base(requestContext)
        {
        }

        protected override async Task<IHttpResponse> Process()
        {
            this.RequestContext.Items.Add("title", "Crude Server - A rough HTTP Server");

            this.AddGenericItemData();

            return await View("index.html");
        }
    }
}
