using System.Threading.Tasks;

using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Integration.Commands
{
    public class MockViewHttpCommand : HttpCommand
    {
        private static int pageViews = 0;

        public MockViewHttpCommand(ICommandContext requestContext) : base(requestContext)
        {
        }

        protected override async Task<IHttpResponse> Process()
        {
            pageViews++;

            this.RequestContext.Items.Add("title", "Hey Vitor!");

            return await View("index.html", new
            {
                name = "Vitor",
                number = pageViews
            });
        }
    }
}
