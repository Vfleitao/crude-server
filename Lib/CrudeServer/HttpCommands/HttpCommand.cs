using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace CrudeServer.HttpCommands
{
    public abstract class HttpCommand
    {
        public ICommandContext RequestContext { get; private set; }

        protected HttpCommand(ICommandContext requestContext)
        {
            RequestContext = requestContext;
        }

        public async virtual Task<IHttpResponse> ExecuteRequest()
        {
            return await Process();
        }

        protected abstract Task<IHttpResponse> Process();

        protected Task<IHttpResponse> View(string path, object data = null)
        {
            return Task.Run<IHttpResponse>(() =>
            {
                IHttpViewResponse viewResponse = this.RequestContext.Services.GetService<IHttpViewResponse>();

                viewResponse.CommandContext = RequestContext;
                viewResponse.ViewModel = data ?? new { };
                viewResponse.Items = new Dictionary<string, object>();
                viewResponse.Items.Add("User", RequestContext.User);

                if (RequestContext.Items != null)
                {
                    foreach (var item in RequestContext.Items)
                    {
                        viewResponse.Items.Add(item.Key, item.Value);
                    }
                }

                viewResponse.SetTemplatePath(path);

                return viewResponse;
            });
        }

        protected Task<IHttpResponse> Redirect(string path, int statusCode = 302)
        {
            return Task.FromResult<IHttpResponse>(new RedirectResponse(path, statusCode));
        }

        protected T GetModelFromRequest<T>()
        {
            return this.RequestContext.GetModelFromRequest<T>();
        }

        protected object GetModelFromRequest(string key)
        {
            if (RequestContext.Items.ContainsKey(key))
            {
                return RequestContext.Items[key];
            }

            return null;
        }

        public void AddCookie(HttpCookie httpCookie)
        {
            this.RequestContext.ResponseCookies.Add(httpCookie);
        }

        public void RemoveCookie(string cookieName)
        {
            this.RequestContext.ResponseCookies.Add(new HttpCookie()
            {
                Name = cookieName,
                Value = "",
                ExpireTimeMinutes = -60 * 24 // minus one day
            });
        }
    }
}
