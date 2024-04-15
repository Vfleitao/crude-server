using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CrudeServer.Consts;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.HttpCommands
{
    public class FileHttpCommand : HttpCommand
    {
        private readonly IDictionary<string, byte[]> _cache;

        private readonly Assembly _fileAssembly;
        private readonly string _fileRoot;
        private readonly FileExtensionContentTypeProvider _fileExtentionProvider;

        public FileHttpCommand(
            [FromKeyedServices(ServerConstants.FILE_ASSEMBLY)] Assembly fileAssembly,
            [FromKeyedServices(ServerConstants.FILE_ROOT)] string fileRoot
        )
        {
            this._fileAssembly = fileAssembly;
            this._fileRoot = fileRoot;

            this._fileExtentionProvider = new FileExtensionContentTypeProvider();
            this._cache = new Dictionary<string, byte[]>();
        }

        protected async override Task<IHttpResponse> Process()
        {
            try
            {
                string resourceName = $"{this._fileRoot}.{this.RequestContext.Url.LocalPath.Substring(1).Replace("\\", ".")}";
                string wantedResource = this._fileAssembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(resourceName));

                if (string.IsNullOrEmpty(wantedResource))
                {
                    return new NotFoundResponse();
                }

                byte[] fileData;

                if (this._cache.ContainsKey(wantedResource))
                {
                    fileData = this._cache[wantedResource];
                }
                else
                {
                    fileData = await GetData(wantedResource);

                    if(fileData == null)
                    {
                        return new NotFoundResponse();
                    }

                    this._cache.TryAdd(wantedResource, fileData);
                }

                return new OkResponse()
                {
                    StatusCode = 200,
                    ResponseData = fileData,
                    ContentType = _fileExtentionProvider.TryGetContentType(resourceName, out string contentType) ?
                                            contentType :
                                            "application/octet-stream"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while reading the embedded resource: " + ex.Message);
                return new StatusCodeResponse()
                {
                    StatusCode = 500
                };
            }
        }

        private async Task<byte[]> GetData(string wantedResource)
        {
            using (Stream resourceStream = this._fileAssembly.GetManifestResourceStream(wantedResource))
            {
                if (resourceStream == null)
                {
                    return null;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    await resourceStream.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
