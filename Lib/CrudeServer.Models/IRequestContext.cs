using System;
using System.Collections.Generic;
using System.Net;

namespace CrudeServer.Models
{
    public class RequestContext
    {
        public readonly HttpListenerContext Context;
        public readonly HttpListenerRequest Request;
        public readonly HttpListenerResponse Response;
        public readonly IServiceProvider Services;

        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public RequestContext(
            HttpListenerContext context, 
            HttpListenerRequest request, 
            HttpListenerResponse response, 
            IServiceProvider services
        )
        {
            this.Context = context;
            this.Request = request;
            this.Response = response;
            this.Services = services;
        }
    }
}
