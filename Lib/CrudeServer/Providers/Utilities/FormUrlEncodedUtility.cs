using System.Collections.Generic;
using System.Linq;
using System.Web;

using CrudeServer.Models.Contracts;

namespace CrudeServer.Providers.Utilities
{
    public static class FormUrlEncodedUtility
    {
        public static (Dictionary<string, object> fields, List<HttpFile> files) ParseFormData(string data)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();

            List<string> fieldData = data.Split('&').ToList();

            foreach (string field in fieldData)
            {
                HandleRegularField(fields, field);
            }

            return (fields, new List<HttpFile>());
        }

        private static void HandleRegularField(Dictionary<string, object> fields, string field)
        {
            string[] fieldParts = field.Split('=');

            string fieldName = HttpUtility.UrlDecode(fieldParts[0].Trim());
            string value = HttpUtility.UrlDecode(fieldParts[1].Trim());

            FormFieldUtility.ProcessRegularField(fields, fieldName, value);
        }
    }
}
