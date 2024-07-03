using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.Attributes;
using CrudeServer.Demo.Commands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

namespace CrudeServer
{
    [Command("/in-depth/{page:\\w+}")]
    public class InDepthPageCommand : BaseCommand
    {
        private static Dictionary<string, string> titlesandTitleMap = new Dictionary<string, string>
        {
            { "start", "Getting Started" },
            { "commands", "HttpCommands" },
            { "files", "Files" },
            { "views", "Views" },
            { "middleware", "Middleware" },
            { "authentication", "Authentication" },
            { "examples", "Examples" },
        };

        private readonly IStandardResponseRegistry standardResponseRegistry;
        private readonly IServiceProvider serviceProvider;

        public InDepthPageCommand(
            IStandardResponseRegistry standardResponseRegistry,
            IServiceProvider serviceProvider,
            ICommandContext requestContext
        ) : base(requestContext)
        {
            this.standardResponseRegistry = standardResponseRegistry;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task<IHttpResponse> Process()
        {
            if (!titlesandTitleMap.Keys.Contains(RequestContext.Items["page"].ToString()))
            {
                Type responseType = standardResponseRegistry.GetResponseType(Enums.DefaultStatusCodes.NotFound);

                return (IHttpResponse)this.serviceProvider.GetService(responseType);
            }

            this.AddGenericItemData();

            string page = RequestContext.Items["page"].ToString();

            if (titlesandTitleMap.ContainsKey(page))
            {
                this.RequestContext.Items.Add("title", titlesandTitleMap[page]);
            }

            return await View($"in-depth/{RequestContext.Items["page"]}.html");
        }
    }
}
