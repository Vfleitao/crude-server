using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.Models.Contracts
{
    public class RequestContext
    {
        public readonly HttpListenerContext HttpContext;
        public readonly HttpListenerRequest HttpRequest;
        public readonly HttpListenerResponse HttpResponse;
        public readonly IServiceProvider Services;

        public IPrincipal User { get; set; }
        public IHttpResponse Response { get; set; }
        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public HttpCommandRegistration HttpRegistration { get; set; }

        public bool IsAjaxRequest
        {
            get { return HttpRequest.Headers["X-Requested-With"] == "XMLHttpRequest"; }
        }

        public RequestContext(
            HttpListenerContext context,
            HttpListenerRequest request,
            HttpListenerResponse response,
            IServiceProvider services
        )
        {
            HttpContext = context;
            HttpRequest = request;
            HttpResponse = response;
            Services = services;
        }
    }
}
