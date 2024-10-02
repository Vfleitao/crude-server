using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;

using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Models.Authentication;
using CrudeServer.Models.Contracts;

using Newtonsoft.Json;

namespace CrudeServer.Models
{
    public class CommandContext : ICommandContext
    {
        public HttpListenerContext HttpListenerContext { get; private set; }
        public HttpListenerRequest HttpListenerRequest { get; private set; }
        public HttpListenerResponse HttpListenerResponse { get; private set; }
        public IServiceProvider Services { get; private set; }

        public HttpCommandRegistration HttpRegistration { get; set; }
        public UserWrapper User { get; set; }
        public IHttpResponse Response { get; set; }
        public IDictionary<string, object> Items { get; set; } = new Dictionary<string, object>();
        public IList<HttpFile> Files { get; set; } = new List<HttpFile>();
        public IDictionary<string, string> ResponseHeaders { get; set; } = new Dictionary<string, string>();
        public IList<HttpCookie> ResponseCookies { get; set; } = new List<HttpCookie>();

        public Uri RequestUrl => HttpListenerRequest.Url;
        public HttpMethod RequestHttpMethod => HttpMethodExtensions.FromHttpString(HttpListenerRequest.HttpMethod);
        public string RequestContentType => HttpListenerRequest.ContentType;
        public string UserAgent => HttpListenerRequest.UserAgent;
        public string IP => HttpListenerRequest.UserHostAddress;
        public string RequestHost => HttpListenerRequest.UserHostName;
        public bool IsAjaxRequest => HttpListenerRequest.Headers["X-Requested-With"] == "XMLHttpRequest";
        public NameValueCollection RequestHeaders => HttpListenerRequest.Headers;
        public IEnumerable<HttpCookie> RequestCookies { get; protected set; }

        public CommandContext(IServiceProvider serviceProvider)
        {
            Services = serviceProvider;
        }

        public void ConfigureContext(
            HttpListenerContext context,
            HttpListenerRequest request,
            HttpListenerResponse response
        )
        {
            HttpListenerContext = context;
            HttpListenerRequest = request;
            HttpListenerResponse = response;

            this.RequestCookies = HttpListenerRequest.Cookies.Select(x => new HttpCookie()
            {
                Domain = x.Domain,
                Path = x.Path,
                Value = x.Value,
                HttpOnly = x.HttpOnly,
                Secure = x.Secure,
                Name = x.Name
            });
        }

        public string GetCookie(string name)
        {
            if (RequestCookies == null && ResponseCookies == null)
            {
                return null;
            }

            // Check if its a cookie that already exists in the request
            HttpCookie cookie = RequestCookies.FirstOrDefault(x => x.Name == name);
            if (cookie != null)
            {
                return cookie.Value;
            }

            // Check if its a cookie that was set in the response (ie: setting an antiforgery token for the first time)
            cookie = ResponseCookies.FirstOrDefault(x => x.Name == name);
            if (cookie != null)
            {
                return cookie.Value;
            }

            return null;
        }

        public T GetModelFromRequest<T>()
        {
            string stringedItems = JsonConvert.SerializeObject(Items);
            return JsonConvert.DeserializeObject<T>(stringedItems, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public object GetModelFromRequest(Type objectType)
        {
            string stringedItems = JsonConvert.SerializeObject(Items);

            return JsonConvert.DeserializeObject(stringedItems, objectType);
        }
    }
}
