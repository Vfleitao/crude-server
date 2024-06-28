using System;
using System.Collections.Generic;
using System.Net;
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
        private readonly ILogger loggerProvider;

        public ResponseProcessorMiddleware(ILogger loggerProvider)
        {
            this.loggerProvider = loggerProvider;
        }

        public async Task Process(ICommandContext context, Func<Task> next)
        {
            IHttpResponse httpResponse = context.Response;

            // Should never happen but just in case
            if (httpResponse == null)
            {
                loggerProvider.Log("[ResponseProcessorMiddleware] No response found, returning 404");
                httpResponse = new NotFoundResponse();
            }

            await httpResponse.ProcessResponse();

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

            static void SetCookie(ICommandContext context, Models.HttpCookie cookie)
            {
                Cookie newCookie = new Cookie(cookie.Name, cookie.Value);
                if (cookie.ExpireTimeMinutes != 0)
                {
                    newCookie.Expires = DateTime.UtcNow.AddMinutes(cookie.ExpireTimeMinutes);
                }

                newCookie.HttpOnly = cookie.HttpOnly;
                newCookie.Secure = cookie.Secure;

                if (!string.IsNullOrEmpty(cookie.Domain))
                {
                    newCookie.Domain = cookie.Domain;
                }
                if (!string.IsNullOrEmpty(cookie.Path))
                {
                    newCookie.Path = cookie.Path;
                }
                else
                {
                    newCookie.Path = "/";
                }

                context.HttpListenerResponse.SetCookie(newCookie);
            }


            if (context.ResponseCookies != null)
            {
                foreach (Models.HttpCookie cookie in context.ResponseCookies)
                {
                    SetCookie(context, cookie);
                }
            }

            if (httpResponse.Cookies != null)
            {
                foreach (Models.HttpCookie cookie in httpResponse.Cookies)
                {
                    SetCookie(context, cookie);
                }
            }

            context.HttpListenerResponse.AddHeader("X-Powered-By", "CrudeServer");

            if (httpResponse is RedirectResponse)
            {
                context.HttpListenerResponse.RedirectLocation = Encoding.UTF8.GetString(httpResponse.ResponseData);

                loggerProvider.Log($"[ResponseProcessorMiddleware] Redirect Response {context.HttpListenerResponse.RedirectLocation} written");
            }
            else if (httpResponse.ResponseData != null && httpResponse.ResponseData.Length > 0)
            {
                loggerProvider.Log($"[ResponseProcessorMiddleware] Writting bytes");
                await context.HttpListenerResponse.OutputStream.WriteAsync(httpResponse.ResponseData, 0, httpResponse.ResponseData.Length);
                loggerProvider.Log($"[ResponseProcessorMiddleware] bytes written");
            }

            await next();
        }
    }
}
