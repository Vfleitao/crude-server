using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Principal;

using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Contracts;

namespace CrudeServer.Models
{
    public class RequestContext : IRequestContext
    {
        public HttpListenerContext HttpListenerContext { get; }
        public HttpListenerRequest HttpListenerRequest { get; }
        public HttpListenerResponse HttpListenerResponse { get; }
        public IServiceProvider Services { get; }

        public HttpCommandRegistration HttpRegistration { get; set; }
        public IPrincipal User { get; set; }
        public IHttpResponse Response { get; set; }
        public IDictionary<string, object> Items { get; set; } = new Dictionary<string, object>();

        public Uri Url => HttpListenerRequest.Url;
        public HttpMethod HttpMethod => HttpMethodExtensions.FromHttpString(HttpListenerRequest.HttpMethod);
        public string ContentType => HttpListenerRequest.ContentType;
        public string UserAgent => HttpListenerRequest.UserAgent;
        public string Host => HttpListenerRequest.UserHostName;
        public bool IsAjaxRequest => HttpListenerRequest.Headers["X-Requested-With"] == "XMLHttpRequest";
        public NameValueCollection Headers => HttpListenerRequest.Headers;

        public RequestContext(
            HttpListenerContext context,
            HttpListenerRequest request,
            HttpListenerResponse response,
            IServiceProvider serviceProvider
        )
        {
            HttpListenerContext = context;
            HttpListenerRequest = request;
            HttpListenerResponse = response;
            Services = serviceProvider;
        }
    }
}
