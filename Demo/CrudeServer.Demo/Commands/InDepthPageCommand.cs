using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.Demo.Commands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;

namespace CrudeServer
{
    public class InDepthPageCommand : BaseCommand
    {
        private static Dictionary<string, string> titles = new Dictionary<string, string>
        {
            { "start", "Getting Started" }
        };

        private static List<string> availablePages = new List<string>
        {
            "start"
        };

        protected override async Task<IHttpResponse> Process()
        {
            if (!availablePages.Contains(RequestContext.Items["page"].ToString()))
            {
                return new NotFoundResponse();
            }

            this.AddGenericItemData();

            string page = RequestContext.Items["page"].ToString();

            if(titles.ContainsKey(page))
            {
                this.RequestContext.Items.Add("title", titles[page]);
            }

            return await View($"in-depth/{RequestContext.Items["page"]}.html");
        }
    }
}
