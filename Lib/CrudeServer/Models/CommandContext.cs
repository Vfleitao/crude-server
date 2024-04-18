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
    public class CommandContext : ICommandContext
    {
        public HttpListenerContext HttpListenerContext { get; }
        public HttpListenerRequest HttpListenerRequest { get; }
        public HttpListenerResponse HttpListenerResponse { get; }
        public IServiceProvider Services { get; }

        public HttpCommandRegistration HttpRegistration { get; set; }
        public IPrincipal User { get; set; }
        public IHttpResponse Response { get; set; }
        public IDictionary<string, object> Items { get; set; } = new Dictionary<string, object>();
        public IList<HttpFile> Files { get; set; } = new List<HttpFile>();
        public IDictionary<string, string> ResponseHeaders { get; set; } = new Dictionary<string, string>();

        public Uri RequestUrl => HttpListenerRequest.Url;
        public HttpMethod RequestHttpMethod => HttpMethodExtensions.FromHttpString(HttpListenerRequest.HttpMethod);
        public string RequestContentType => HttpListenerRequest.ContentType;
        public string UserAgent => HttpListenerRequest.UserAgent;
        public string RequestHost => HttpListenerRequest.UserHostName;
        public bool IsAjaxRequest => HttpListenerRequest.Headers["X-Requested-With"] == "XMLHttpRequest";
        public NameValueCollection RequestHeaders => HttpListenerRequest.Headers;


        public CommandContext(
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
