using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;
using CrudeServer.Providers.Utilities;

namespace CrudeServer.Providers.DataParser
{
    public class JsonDataParser : IRequestDataParser
    {
        public async Task<HttpRequestData> GetData(ICommandContext request)
        {
            HttpRequestData httpRequestData = new HttpRequestData();

            using (StreamReader reader = new StreamReader(request.HttpListenerRequest.InputStream, request.HttpListenerRequest.ContentEncoding))
            {
                string requestBody = await reader.ReadToEndAsync();
                Dictionary<string, object> data = JsonUtilities.DictionaryFromString(requestBody);

                foreach (KeyValuePair<string, object> item in data)
                {
                    httpRequestData.Data.TryAdd(item.Key, item.Value);
                }
            }

            return httpRequestData;
        }
    }
}
