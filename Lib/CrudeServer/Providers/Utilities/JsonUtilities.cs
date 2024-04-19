using System.Collections.Generic;
using System.Text.Json;

namespace CrudeServer.Providers.Utilities
{
    public class JsonUtilities
    {
        public static Dictionary<string, object> DictionaryFromString(string input)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(input))
            {
                return data;
            }

            if (input.StartsWith("[") || input.StartsWith("{"))
            {
                JsonElement deserializedData = JsonSerializer.Deserialize<JsonElement>(input, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ConvertJsonElementToDictionary(deserializedData, data);
            }
            else
            {
                data.Add("body", input);
            }

            return data;
        }

        private static void ConvertJsonElementToDictionary(JsonElement element, Dictionary<string, object> properties)
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

        private static List<object> GetListFromJArray(JsonElement element)
        {
            List<object> list = new List<object>();

            foreach (JsonElement arrayElement in element.EnumerateArray())
            {
                list.Add(JsonElementToObject(arrayElement));
            }

            return list;
        }

        private static object JsonElementToObject(JsonElement element)
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
