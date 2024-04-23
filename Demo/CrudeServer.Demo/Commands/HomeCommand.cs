using System.Threading.Tasks;

using CrudeServer.Demo.Commands;
using CrudeServer.HttpCommands.Contract;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CrudeServer
{
    public class HomeCommand : BaseCommand
    {

        protected override async Task<IHttpResponse> Process()
        {
            this.RequestContext.Items.Add("title", "Crude Server - A rough HTTP Server");

            this.AddGenericItemData();

            return await View("index.html");
        }
    }
}
