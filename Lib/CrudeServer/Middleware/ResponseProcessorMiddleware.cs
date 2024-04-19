using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

namespace CrudeServer.Middleware
{
    public class ResponseProcessorMiddleware : IMiddleware
    {
        private readonly IDictionary<int, (string location, int redirectStatusCode)> defaultStatusCodePaths = new Dictionary<int, (string location, int redirectStatusCode)>();
        private readonly IServerConfig _serverConfiguraton;
        private readonly ILoggerProvider loggerProvider;

        public ResponseProcessorMiddleware(IServerConfig serverConfiguraton, ILoggerProvider loggerProvider)
        {
            this._serverConfiguraton = serverConfiguraton;
            this.loggerProvider = loggerProvider;
            if (!string.IsNullOrEmpty(this._serverConfiguraton.AuthenticationPath))
            {
                this.defaultStatusCodePaths.Add(401, (this._serverConfiguraton.AuthenticationPath, 302));
            }
            if (!string.IsNullOrEmpty(this._serverConfiguraton.NotFoundPath))
            {
                this.defaultStatusCodePaths.Add(404, (this._serverConfiguraton.NotFoundPath, 302));
            }
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            IHttpResponse httpResponse = context.Response;

            if (context.Response.StatusCode != 200 &&
                defaultStatusCodePaths.ContainsKey(context.Response.StatusCode) &&
                (!context.IsAjaxRequest ||
                    (this._serverConfiguraton.RedirectOnAjaxCalls && context.IsAjaxRequest)
                )
            )
            {

                loggerProvider.Log($"[6] Redirecting to {defaultStatusCodePaths[context.Response.StatusCode].location} for status code {context.Response.StatusCode}");

                (string location, int redirectStatusCode) redirectSetup = defaultStatusCodePaths[context.Response.StatusCode];
                httpResponse = new RedirectResponse(redirectSetup.location, redirectSetup.redirectStatusCode);
            }

            context.HttpListenerResponse.StatusCode = httpResponse.StatusCode;
            context.HttpListenerResponse.ContentType = httpResponse.ContentType;

            if (context.ResponseHeaders != null)
            {
                foreach (KeyValuePair<string, string> header in context.ResponseHeaders)
                {
                    context.HttpListenerResponse.AddHeader(header.Key, header.Value);
                }
            }

            if (httpResponse.Headers != null)
            {
                foreach (KeyValuePair<string, string> header in httpResponse.Headers)
                {
                    context.HttpListenerResponse.AddHeader(header.Key, header.Value);
                }
            }

            context.HttpListenerResponse.AddHeader("X-Powered-By", "CrudeServer");

            if (httpResponse is RedirectResponse)
            {
                context.HttpListenerResponse.RedirectLocation = Encoding.UTF8.GetString(httpResponse.ResponseData);

                loggerProvider.Log($"[7] Redirect Response {context.HttpListenerResponse.RedirectLocation} written");
            }
            else if (httpResponse.ResponseData != null && httpResponse.ResponseData.Length > 0)
            {
                loggerProvider.Log($"[8] Writting bytes");
                await context.HttpListenerResponse.OutputStream.WriteAsync(httpResponse.ResponseData, 0, httpResponse.ResponseData.Length);
                loggerProvider.Log($"[9] bytes written");
            }

            await next();
        }
    }
}
