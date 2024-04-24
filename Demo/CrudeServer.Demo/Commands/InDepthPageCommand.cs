using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.Demo.Commands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;

namespace CrudeServer
{
    public class InDepthPageCommand : BaseCommand
    {
        private static Dictionary<string, string> titlesandTitleMap = new Dictionary<string, string>
        {
            { "start", "Getting Started" },
            { "commands", "HttpCommands" },
        };

        protected override async Task<IHttpResponse> Process()
        {
            if (!titlesandTitleMap.Keys.Contains(RequestContext.Items["page"].ToString()))
            {
                return new NotFoundResponse();
            }

            this.AddGenericItemData();

            string page = RequestContext.Items["page"].ToString();

            if(titlesandTitleMap.ContainsKey(page))
            {
                this.RequestContext.Items.Add("title", titlesandTitleMap[page]);
            }

            return await View($"in-depth/{RequestContext.Items["page"]}.html");
        }
    }
}
