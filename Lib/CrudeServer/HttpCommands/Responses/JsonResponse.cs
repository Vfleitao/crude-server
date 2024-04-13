using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace CrudeServer.HttpCommands.Responses
{
    public class JsonResponse : OkResponse
    {
        public override string ContentType { get; set; } = "application/json";
    }
}
