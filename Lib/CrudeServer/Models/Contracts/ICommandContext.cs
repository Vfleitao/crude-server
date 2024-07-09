using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Principal;
using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Authentication;

namespace CrudeServer.Models.Contracts
{
    public interface ICommandContext
    {
        IDictionary<string, object> Items { get; set; }
        IList<HttpFile > Files { get; set; }

        IHttpResponse Response { get; set; }
        UserWrapper User { get; set; }

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
        IList<HttpCookie> ResponseCookies { get; set; }
        string IP { get; }

        void ConfigureContext(HttpListenerContext context, HttpListenerRequest request, HttpListenerResponse response);
        string GetCookie(string name);
        T GetModelFromRequest<T>();
        object GetModelFromRequest(Type objectType);
    }
}