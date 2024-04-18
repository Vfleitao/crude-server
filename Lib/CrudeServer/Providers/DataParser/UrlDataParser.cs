using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using HandlebarsDotNet;

namespace CrudeServer.Providers.DataParser
{
    public class UrlDataParser : IRequestDataParser
    {
        public Task<HttpRequestData> GetData(ICommandContext request)
        {
            return Task.Run<HttpRequestData>(() =>
            {
                HttpRequestData httpRequestData = new HttpRequestData();

                HttpCommandRegistration commandRegistration = request.HttpRegistration;

                Dictionary<string, string> urlParameters = new Dictionary<string, string>();

                if (commandRegistration.UrlParameters == null || !commandRegistration.UrlParameters.Any())
                {
                    return httpRequestData;
                }

                string regexPattern = "^";

                foreach (KeyValuePair<string, string> param in commandRegistration.UrlParameters)
                {
                    regexPattern += $"/({param.Value})";
                }
                regexPattern += "$";

                Match match = Regex.Match(request.RequestUrl.AbsolutePath, regexPattern);
                if (match.Success)
                {
                    for (int i = 0; i < commandRegistration.UrlParameters.Count; i++)
                    {
                        httpRequestData.Data.Add(commandRegistration.UrlParameters[i].Key, match.Groups[i + 1].Value);
                    }
                }

                return httpRequestData;
            });
        }
    }
}
