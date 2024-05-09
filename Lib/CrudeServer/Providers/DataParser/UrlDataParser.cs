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
            HttpRequestData httpRequestData = new HttpRequestData();

            HttpCommandRegistration commandRegistration = request.HttpRegistration;
            if (commandRegistration == null)
            {
                return Task.FromResult(httpRequestData);
            }

            if (commandRegistration.UrlParameters == null || !commandRegistration.UrlParameters.Any())
            {
                return Task.FromResult(httpRequestData);
            }

            Match match = commandRegistration.PathRegex.Match(request.RequestUrl.AbsolutePath);
            if (match.Success)
            {
                for (int i = 0; i < commandRegistration.UrlParameters.Count; i++)
                {
                    httpRequestData.Data.Add(commandRegistration.UrlParameters[i].Key, match.Groups[i + 1].Value);
                }
            }

            return Task.FromResult(httpRequestData);
        }
    }
}
