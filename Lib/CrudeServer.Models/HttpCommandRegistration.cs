using System;

using CrudeServer.Enums;

namespace CrudeServer.Models
{
    public class HttpCommandRegistration
    {
        public string Path { get; set; }
        public HttpMethod HttpMethod { get; set; }
        public Type Command { get; set; }
    }
}
