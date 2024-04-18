using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.HttpCommands
{
    public abstract class HttpCommand
    {
        public ICommandContext RequestContext { get; private set; }

        public void SetContext(ICommandContext context)
        {
            this.RequestContext = context;
        }

        public async virtual Task<IHttpResponse> ExecuteRequest()
        {
            return await Process();
        }

        protected abstract Task<IHttpResponse> Process();

        protected async Task<IHttpResponse> View(string path, object data)
        {
            IHttpViewResponse viewResponse = this.RequestContext.Services.GetService<IHttpViewResponse>();

            viewResponse.ViewModel = data;
            viewResponse.Items = new Dictionary<string, object>();
            viewResponse.Items.Add("User", RequestContext.User);

            if (RequestContext.Items != null) { 
                foreach (var item in RequestContext.Items)
                {
                    viewResponse.Items.Add(item.Key, item.Value);
                }
            }

            viewResponse.SetTemplatePath(path);

            await viewResponse.ProcessResponse();

            return viewResponse;
        }
    }
}
