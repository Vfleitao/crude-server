using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Middleware
{
    public class CommandDataRetrieverMiddleware : IMiddleware
    {
        private readonly IHttpRequestDataProvider httpRequestDataProvider;

        public CommandDataRetrieverMiddleware(IHttpRequestDataProvider httpRequestDataProvider)
        {
            this.httpRequestDataProvider = httpRequestDataProvider;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            HttpRequestData data = await httpRequestDataProvider.GetDataFromRequest(context);
            UpdateRequestContext(data, context);

            await next();
        }

        private void UpdateRequestContext(HttpRequestData data, ICommandContext context)
        {
            if (data == null)
            {
                return;
            }

            foreach (KeyValuePair<string, object> item in data.Data)
            {
                context.Items.Add(item.Key, item.Value);
            }

            foreach (HttpFile file in data.Files)
            {
                context.Files.Add(file);
            }
        }
    }
}
