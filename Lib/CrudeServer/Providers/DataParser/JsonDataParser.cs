using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

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

                if (string.IsNullOrEmpty(requestBody))
                {
                    return httpRequestData;
                }

                if (requestBody.StartsWith("[") || requestBody.StartsWith("{"))
                {
                    JsonElement deserializedData = JsonSerializer.Deserialize<JsonElement>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    ConvertJsonElementToDictionary(deserializedData, httpRequestData.Data);
                }
                else
                {
                    httpRequestData.Data.Add("body", requestBody);
                }
            }

            return httpRequestData;
        }

        private void ConvertJsonElementToDictionary(JsonElement element, Dictionary<string, object> properties)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        Dictionary<string, object> subProperties = new Dictionary<string, object>();
                        ConvertJsonElementToDictionary(property.Value, subProperties);
                        properties.Add(property.Name, subProperties);
                    }
                    else if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        properties.Add(property.Name, GetListFromJArray(property.Value));
                    }
                    else
                    {
                        properties.Add(property.Name, JsonElementToObject(property.Value));
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                List<object> list = new List<object>();
                foreach (JsonElement arrayElement in element.EnumerateArray())
                {
                    list.Add(JsonElementToObject(arrayElement));
                }
            }
            else
            {
                properties.Add("body", JsonElementToObject(element));
            }
        }

        private List<object> GetListFromJArray(JsonElement element)
        {
            List<object> list = new List<object>();

            foreach (JsonElement arrayElement in element.EnumerateArray())
            {
                list.Add(JsonElementToObject(arrayElement));
            }

            return list;
        }

        private object JsonElementToObject(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    string number = element.GetRawText();
                    if (number.Contains(".") || number.Contains(","))
                    {
                        return element.GetDouble();
                    }

                    if (element.TryGetInt32(out int i32_value))
                    {
                        return i32_value;
                    }

                    if (element.TryGetInt64(out long i64_value))
                    {
                        return i64_value;
                    }

                    return element.GetDouble();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return null;
                default:
                    return element.GetRawText();
            }
        }

    }
}
