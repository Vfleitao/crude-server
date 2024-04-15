using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Principal;
using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.Models.Contracts
{
    public interface IRequestContext
    {
        IDictionary<string, object> Items { get; set; }
        IHttpResponse Response { get; set; }
        IPrincipal User { get; set; }

        HttpMethod HttpMethod { get; }
        bool IsAjaxRequest { get; }
        Uri Url { get; }
        string ContentType { get; }
        string UserAgent { get; }
        string Host { get; }
        HttpListenerContext HttpListenerContext { get; }
        HttpListenerRequest HttpListenerRequest { get; }
        HttpListenerResponse HttpListenerResponse { get; }
        NameValueCollection Headers { get; }

        HttpCommandRegistration HttpRegistration { get; set; }
        IServiceProvider Services { get; }
    }
}