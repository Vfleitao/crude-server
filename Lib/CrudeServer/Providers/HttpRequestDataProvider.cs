using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CrudeServer.Enums;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Providers
{
    public class HttpRequestDataProvider : IHttpRequestDataProvider
    {
        public HttpRequestDataProvider()
        {
        }

        public async Task<HttpRequestData> GetDataFromRequest(ICommandContext request)
        {
            HttpRequestData httpRequestData = new HttpRequestData();

            await GetDataFromUrl(request, httpRequestData);

            if (request.RequestHttpMethod == HttpMethod.OPTIONS ||
                request.RequestHttpMethod == HttpMethod.GET ||
                request.RequestHttpMethod == HttpMethod.HEAD ||
                request.RequestHttpMethod == HttpMethod.DELETE
            )
            {
                return httpRequestData;
            }

            await GetDataFromBody(request, httpRequestData);

            return httpRequestData;
        }

        private static async Task GetDataFromBody(ICommandContext request, HttpRequestData httpRequestData)
        {
            if(string.IsNullOrEmpty(request.RequestContentType))
            {
                return;
            }

            string contentType = request.RequestContentType.Split(";")[0];
            if (string.IsNullOrEmpty(contentType))
            {
                return;
            }

            IRequestDataParser data_parser = request.Services.GetKeyedService<IRequestDataParser>($"dataparser_{contentType.ToLower()}");

            if (data_parser == null)
            {
                throw new InvalidOperationException("Unsupported content type");
            }

            HttpRequestData bodyData = await data_parser.GetData(request);

            if (bodyData == null)
            {
                return;
            }

            if (bodyData != null && bodyData.Data.Any())
            {
                foreach (KeyValuePair<string, object> item in bodyData.Data)
                {
                    httpRequestData.Data.TryAdd(item.Key, item.Value);
                }
            }

            if (bodyData.Files != null && bodyData.Files.Any())
            {
                httpRequestData.Files.AddRange(bodyData.Files);
            }
        }

        private static async Task GetDataFromUrl(ICommandContext request, HttpRequestData httpRequestData)
        {
            IRequestDataParser urlDataParser = request.Services.GetKeyedService<IRequestDataParser>("dataparser_urlDataParser");

            if (urlDataParser == null)
            {
                return;
            }

            HttpRequestData urlData = await urlDataParser.GetData(request);
            if (urlData != null && urlData.Data.Any())
            {
                foreach (KeyValuePair<string, object> item in urlData.Data)
                {
                    httpRequestData.Data.TryAdd(item.Key, item.Value);
                }
            }
        }
    }
}
