using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CrudeServer.Enums;

namespace CrudeServer.Models
{
    public class HttpCommandRegistration
    {
        public string Path { get; set; }
        public Regex PathRegex { get; set; }
        public List<KeyValuePair<string, string>> UrlParameters { get; set; } = new List<KeyValuePair<string, string>>();
        public HttpMethod HttpMethod { get; set; }
        public bool RequiresAuthentication { get; set; }
        public bool RequiresAntiforgeryToken { get; set; }
        public IEnumerable<string> AuthenticationRoles { get; set; } = new List<string>();
        public Type Command { get; set; }
        public bool SkipAuthenticationFetching { get; private set; }

        public HttpCommandRegistration RequireAuthentication(IEnumerable<string> roles = null)
        {
            this.RequiresAuthentication = true;
            if (roles != null)
            {
                this.AuthenticationRoles = roles;
            }

            return this;
        }

        public HttpCommandRegistration AllowAnonymous()
        {
            this.RequiresAuthentication = false;
            this.AuthenticationRoles = new List<string>();

            return this;
        }

        public HttpCommandRegistration RequireAntiforgeryToken()
        {
            this.RequiresAntiforgeryToken = true;

            return this;
        }

        public void SkipAuthenticationFetch()
        {
            this.SkipAuthenticationFetching = true;
        }
    }
}
