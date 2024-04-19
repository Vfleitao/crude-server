using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace CrudeServer.HttpCommands.Responses
{
    public class JsonResponse : OkResponse
    {
        public JsonResponse() { }

        public JsonResponse(object data) : base(data) { }

        public JsonResponse(Stream stream) : base(stream) { }

        public override string ContentType { get; set; } = "application/json";
    }
}
