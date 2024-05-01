using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Principal;
using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;

namespace CrudeServer.Models.Contracts
{
    public interface ICommandContext
    {
        IDictionary<string, object> Items { get; set; }
        IList<HttpFile > Files { get; set; }

        IHttpResponse Response { get; set; }
        IPrincipal User { get; set; }

        HttpMethod RequestHttpMethod { get; }
        bool IsAjaxRequest { get; }
        Uri RequestUrl { get; }
        string RequestContentType { get; }
        string UserAgent { get; }
        string RequestHost { get; }
        HttpListenerContext HttpListenerContext { get; }
        HttpListenerRequest HttpListenerRequest { get; }
        HttpListenerResponse HttpListenerResponse { get; }
        NameValueCollection RequestHeaders { get; }
        IEnumerable<HttpCookie> RequestCookies { get; }

        HttpCommandRegistration HttpRegistration { get; set; }
        IServiceProvider Services { get; }
        IDictionary<string, string> ResponseHeaders { get; set; }
        IEnumerable<HttpCookie> ResponseCookies { get; set; }
    }
}