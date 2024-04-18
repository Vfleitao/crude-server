using System.Collections.Generic;

using CrudeServer.Models.Contracts;

namespace CrudeServer.Models
{
    public class HttpRequestData
    {
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public List<HttpFile> Files { get; set; } = new List<HttpFile>();
    }
}
