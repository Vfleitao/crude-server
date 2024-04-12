using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace CrudeServer.HttpCommands.Responses
{
    public class OkResponse : StatusCodeResponse
    {
        public override int StatusCode { get; set; } = 200;

        public void SetData(object data)
        {
            if (data.GetType().IsValueType || data is string)
            {
                string valueAsString = data.ToString();
                this.ResponseData = Encoding.UTF8.GetBytes(valueAsString);
            }
            else
            {
                string json = JsonConvert.SerializeObject(data);
                this.ResponseData = Encoding.UTF8.GetBytes(json);
            }
        }

        public void SetData(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                this.ResponseData = ms.ToArray();
            }
        }
    }
}
