using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;
using CrudeServer.Providers.Utilities;

namespace CrudeServer.Providers.DataParser
{
    public class MultiPartFormDataParser : IRequestDataParser
    {
        public MultiPartFormDataParser()
        {
        }

        public async Task<HttpRequestData> GetData(ICommandContext request)
        {
            HttpRequestData httpRequestData = new HttpRequestData();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await request.HttpListenerRequest.InputStream.CopyToAsync(memoryStream);
                byte[] data = memoryStream.ToArray();

                string contentType = request.HttpListenerRequest.ContentType;
                (Dictionary<string, object> fields, List<HttpFile> files) parsedMultipart = MultiPartFormDataUtility.ParseFormData(data, contentType);

                foreach (KeyValuePair<string, object> item in parsedMultipart.fields)
                {
                    httpRequestData.Data.TryAdd(item.Key, item.Value);
                }

                foreach (HttpFile file in parsedMultipart.files)
                {
                    httpRequestData.Files.Add(file);
                }
            }

            return httpRequestData;
        }
    }
}
