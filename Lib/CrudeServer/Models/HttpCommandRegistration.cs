using System;
using System.Text.RegularExpressions;

using CrudeServer.Enums;

namespace CrudeServer.Models
{
    public class HttpCommandRegistration
    {
        public string Path { get; set; }
        public HttpMethod HttpMethod { get; set; }
        public Type Command { get; set; }
        public bool RequiresAuthentication { get; set; }
        public Regex PathRegex { get; set; }

        public void RequireAuthentication()
        {
            this.RequiresAuthentication = true;
        }

        public void AllowAnonymous()
        {
            this.RequiresAuthentication = false;
        }
    }
}
