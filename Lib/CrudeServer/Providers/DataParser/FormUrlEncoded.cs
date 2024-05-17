using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;
using CrudeServer.Providers.Utilities;

namespace CrudeServer.Providers.DataParser
{
    public class FormUrlEncoded : IRequestDataParser
    {
        public async Task<HttpRequestData> GetData(ICommandContext request)
        {
            HttpRequestData httpRequestData = new HttpRequestData();

            HttpCommandRegistration commandRegistration = request.HttpRegistration;
            if (commandRegistration == null)
            {
                return httpRequestData;
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await request.HttpListenerRequest.InputStream.CopyToAsync(memoryStream);
                string data = Encoding.UTF8.GetString(memoryStream.ToArray());

                (Dictionary<string, object> fields, List<HttpFile> files) parsedMultipart = FormUrlEncodedUtility.ParseFormData(data);

                foreach (KeyValuePair<string, object> item in parsedMultipart.fields)
                {
                    httpRequestData.Data.TryAdd(item.Key, item.Value);
                }
            }

            return httpRequestData;
        }
    }
}
